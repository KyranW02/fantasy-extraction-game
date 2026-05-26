// Scripts/Server/BackendAPI.cs
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class BackendAPI : MonoBehaviour
{
    public static BackendAPI Instance { get; private set; }

    [SerializeField] private string apiBaseUrl = "http://localhost:3000";
    private string _serverSecret;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // Secret key injected via environment variable
        // Never hardcoded
        _serverSecret = System.Environment.GetEnvironmentVariable(
            "SERVER_SECRET") ?? "dev-secret";
    }

    public void ReportExtraction(string playerId, List<string> items)
    {
        StartCoroutine(PostExtraction(playerId, items));
    }

    private IEnumerator PostExtraction(string playerId, List<string> items)
    {
        var payload = new ExtractionPayload
        {
            playerId = playerId,
            itemIds = items,
            serverSecret = _serverSecret
        };

        string json = JsonUtility.ToJson(payload);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using var request = new UnityWebRequest(
            $"{apiBaseUrl}/api/extraction/complete", "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("X-Server-Secret", _serverSecret);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError($"[API] Extraction report failed: {request.error}");
        else
            Debug.Log($"[API] Extraction reported for {playerId}");
    }

    public void ReportInstanceClosed()
    {
        StartCoroutine(PostInstanceClosed());
    }

    private IEnumerator PostInstanceClosed()
    {
        using var request = new UnityWebRequest(
            $"{apiBaseUrl}/api/server/closed", "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("X-Server-Secret", _serverSecret);

        yield return request.SendWebRequest();
        Debug.Log("[API] Instance closed reported");
    }
}

[System.Serializable]
public class ExtractionPayload
{
    public string playerId;
    public List<string> itemIds;
    public string serverSecret;
}