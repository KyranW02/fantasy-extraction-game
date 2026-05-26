// Scripts/Core/Bootstrap/BootstrapLoader.cs
// Scripts/Core/Bootstrap/BootstrapLoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string firstScene = "MainMenu";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InitialiseCoreServices();
    }

    private void InitialiseCoreServices()
    {
        ServiceLocator.Initialise();
        EventBus.Initialise();

        // AudioManager will be added later
        // ServiceLocator.Get<AudioManager>().Initialise();

        SceneManager.LoadScene(firstScene);
    }
}