using UnityEngine;

// Class for keeping track of current user playing
public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    public string CurrentUsername { get; private set; }
    private GameData _dataToLoadOnSceneStart;

    private void Awake()
    {
        // Singleton Initializaiton
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Login(string username)
    {
        CurrentUsername = username;
    }

    // keep data to be loaded after the game scene starts
    public void SetDataForNextScene(GameData data)
    {
        _dataToLoadOnSceneStart = data;
    }

    // GameManager will call this in its Start() method
    public GameData ConsumeDataToLoad()
    {
        GameData data = _dataToLoadOnSceneStart;
        _dataToLoadOnSceneStart = null; // Consume the data so it's only used once
        return data;
    }
}