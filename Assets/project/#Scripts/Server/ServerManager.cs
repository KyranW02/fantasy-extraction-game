// Scripts/Server/ServerManager.cs
using UnityEngine;
using FishNet.Object;
using FishNet.Managing;
using System.Collections;
using System.Collections.Generic;

public class GameServerManager : MonoBehaviour
{
    public static GameServerManager Instance { get; private set; }

    [Header("Server Config")]
    [SerializeField] private float raidDuration = 1800f;      // 30 minutes
    [SerializeField] private float closingWarning = 300f;     // 5 min warning
    [SerializeField] private int maxPlayers = 8;

    private ServerState _currentState = ServerState.Idle;
    private float _raidTimer;
    private Dictionary<string, ServerPlayerData> _activePlayers = new();
    private string _dungeonSeed;
    private NetworkManager _networkManager;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _networkManager = FindObjectOfType<NetworkManager>();

        // Parse command line args when running as headless server
        ParseCommandLineArgs();
    }

    private void ParseCommandLineArgs()
    {
        string[] args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-port":
                    if (i + 1 < args.Length)
                        SetPort(int.Parse(args[i + 1]));
                    break;
                case "-seed":
                    if (i + 1 < args.Length)
                        _dungeonSeed = args[i + 1];
                    break;
                case "-maxplayers":
                    if (i + 1 < args.Length)
                        maxPlayers = int.Parse(args[i + 1]);
                    break;
            }
        }
    }

    private void SetPort(int port)
    {
        // Set FishNet transport port
        var transport = _networkManager.TransportManager.Transport;
        transport.SetPort((ushort)port);
    }

    public void StartRaid()
    {
        _currentState = ServerState.Active;
        _raidTimer = raidDuration;
        StartCoroutine(RaidTimerRoutine());

        EventBus.Publish(new RaidStartedEvent
        {
            Seed = _dungeonSeed,
            Duration = raidDuration
        });
    }

    private IEnumerator RaidTimerRoutine()
    {
        yield return new WaitForSeconds(raidDuration - closingWarning);

        // Warn players
        EventBus.Publish(new RaidClosingEvent
        {
            TimeRemaining = closingWarning
        });

        yield return new WaitForSeconds(closingWarning);

        // Force close — kill remaining players
        CloseRaid();
    }

    public void RegisterPlayer(string playerId, ServerPlayerData data)
    {
        if (_activePlayers.Count >= maxPlayers)
        {
            // Reject connection
            return;
        }
        _activePlayers[playerId] = data;
    }

    public void RemovePlayer(string playerId, RemovalReason reason)
    {
        if (!_activePlayers.ContainsKey(playerId)) return;

        var playerData = _activePlayers[playerId];

        if (reason == RemovalReason.Extracted)
            ProcessExtraction(playerId, playerData);
        else if (reason == RemovalReason.Died)
            ProcessDeath(playerId, playerData);

        _activePlayers.Remove(playerId);

        // If server is empty, shut down early
        if (_activePlayers.Count == 0)
            StartCoroutine(ShutdownAfterDelay(30f));
    }

    private void ProcessExtraction(string playerId, ServerPlayerData data)
    {
        // Report to backend API
        BackendAPI.Instance.ReportExtraction(playerId, data.CurrentInventory);

        EventBus.Publish(new PlayerExtractedEvent
        {
            PlayerId = playerId,
            ItemIds = data.CurrentInventory,
            GoldExtracted = data.CurrentGold
        });
    }

    private void ProcessDeath(string playerId, ServerPlayerData data)
    {
        // Drop loot into world — handled by loot system
        EventBus.Publish(new PlayerDiedEvent
        {
            PlayerId = playerId,
            KillerId = data.LastDamageSourceId,
            Position = data.LastKnownPosition
        });
    }

    private void CloseRaid()
    {
        _currentState = ServerState.Closing;

        // Kill any remaining players
        foreach (var player in _activePlayers)
            ProcessDeath(player.Key, player.Value);

        _activePlayers.Clear();
        StartCoroutine(ShutdownAfterDelay(10f));
    }

    private IEnumerator ShutdownAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        BackendAPI.Instance.ReportInstanceClosed();
        Application.Quit();
    }
}

public enum ServerState
{
    Idle,
    Assigned,
    Loading,
    Active,
    Closing,
    Terminated
}

public enum RemovalReason
{
    Extracted,
    Died,
    Disconnected,
    Kicked
}

public struct ServerPlayerData
{
    public string PlayerId;
    public List<string> CurrentInventory;
    public int CurrentGold;
    public string LastDamageSourceId;
    public Vector3 LastKnownPosition;
}