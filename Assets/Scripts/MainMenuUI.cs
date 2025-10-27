using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public static MainMenuUI Instance { get; private set; }
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Transform userListContentPanel;
    [SerializeField] private GameObject userButtonPrefab;
    [SerializeField] private GameObject achievementItemPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private List<Achievement> allAchievementSOs;

    private void Awake()
    {
        // Singleton Initialization
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        PopulateAchievements();
    }

    public void OnNewGameClicked()
    {
        string username = nameInputField.text;
        if (string.IsNullOrEmpty(username))
        {
            // Negative Case: No username
            Debug.LogWarning("Username cannot be empty.");
            return;
        }

        // Create the user account (this just creates the folder)
        AccountManager.CreateUser(username); 

        //"Log in" as this new user
        SessionManager.Instance.Login(username);

        // Clear any cached data (it's a new game)
        SessionManager.Instance.SetDataForNextScene(null);

        // Load the Game Scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public void OnLoadGameMenuClicked()
    {
        // Clear any old buttons
        foreach (Transform child in userListContentPanel)
        {
            Destroy(child.gameObject);
        }

        // Get all user accounts
        List<string> users = AccountManager.GetAllUsers();

        // Create a button for each user
        foreach (string username in users)
        {
            GameObject buttonGO = Instantiate(userButtonPrefab, userListContentPanel);
            buttonGO.GetComponentInChildren<TMP_Text>().text = username;

            // Add a listener to the button
            string capturedUsername = username;
            buttonGO.GetComponent<Button>().onClick.AddListener(() => OnUserSelected(capturedUsername));
        }
    }

    private void OnUserSelected(string username)
    {
        // "Log in" as this user
        SessionManager.Instance.Login(username);

        // Find their latest save file
        GameData latestData = null;
        DateTime latestTime = DateTime.MinValue;

        // We check all possible slots (5)
        for (int i = 0; i < 5; i++)
        {
            GameData slotData;
            SaveLoadStatus status = SaveSystem.LoadGame(username, i, out slotData);

            if (status == SaveLoadStatus.Success && slotData.lastSaveTime > latestTime)
            {
                latestTime = slotData.lastSaveTime;
                latestData = slotData;
            }
        }

        // If latestData is null (no saves found), the game will start fresh.
        SessionManager.Instance.SetDataForNextScene(latestData);

        // Load the Game Scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    void PopulateAchievements()
    {
        // Clear old items first
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Check if AchievementManager exists
        if (AchievementManager.Instance == null)
        {
            Debug.LogError("AchievementManager not found!");
            return;
        }

        // Create UI elements for each defined achievement
        foreach (Achievement achDef in allAchievementSOs)
        {
            GameObject itemGO = Instantiate(achievementItemPrefab, contentParent);
            
            // Get references to UI components
            TextMeshProUGUI nameText = itemGO.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descText = itemGO.transform.Find("Description").GetComponent<TextMeshProUGUI>(); 
            TextMeshProUGUI compText = itemGO.transform.Find("Completion").GetComponent<TextMeshProUGUI>();

            bool isUnlocked = AchievementManager.Instance.IsUnlocked(achDef.achievementID);

            nameText.text = achDef.displayName;
            descText.text = achDef.description;
            
            if (isUnlocked)
            {
                nameText.color = Color.green;
                descText.color = Color.green;
                compText.color = Color.green;
                compText.text = "( v Completed )";
            }
            else
            {
                nameText.color = Color.red;
                descText.color = Color.red;
                compText.color = Color.red;
                compText.text = "( x Incomplete )";
            }
        }
    }

    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
