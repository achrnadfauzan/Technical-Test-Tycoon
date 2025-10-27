using System;
using UnityEngine;

// Class for managing the game's economy (money and income).
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private long currentMoney = 1000; // Start with some money
    [SerializeField] private long totalIncomePerSecond = 0;

    private bool isGamePaused = false;

    private void Awake()
    {
        // Singleton initialization
        if (Instance != null)
        {
            Destroy(gameObject);
        }

        Instance = this;
    }

    private void Start()
    {
        // Check if the SessionManager has data for us to load
        GameData dataToLoad = SessionManager.Instance.ConsumeDataToLoad();

        if (dataToLoad != null)
        {
            LoadGameData(dataToLoad);
        }

        InvokeRepeating("AddMoney", 0, 1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) PauseGame(!isGamePaused);
    }

    // called once every seconds
    private void AddMoney()
    {
        currentMoney += totalIncomePerSecond;
        GameUI.Instance.UpdateUI();

        AchievementManager.Instance?.CheckMoneyAchievements(currentMoney);
    }
    
    // Called when new store is built
    public void AddIncome(int amount)
    {
        totalIncomePerSecond += amount;
        GameUI.Instance.UpdateUI();

        AchievementManager.Instance?.CheckIncomeAchievements(totalIncomePerSecond);
    }

    // Tries to spend money. Returns true if successful, false if not.
    public bool TrySpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            GameUI.Instance.UpdateUI();
            return true;
        }

        Debug.LogWarning("Not enough money to build!");
        return false;
    }

    public long GetCurrentMoney()
    {
        return currentMoney;
    }

    public long GetTotalIncome()
    {
        return totalIncomePerSecond;
    }

    public void PauseGame(bool value)
    {
        isGamePaused = value;
        GameUI.Instance.SetPausePanelActive(value);
        Time.timeScale = value ? 0 : 1;
    }

    public void ResetIncome()
    {
        totalIncomePerSecond = 0;
    }

    // gather all game's data from gameobjects for saving
    public GameData GetCurrentGameData()
    {
        GameData data = new GameData();
        data.currentMoney = currentMoney;
        data.totalIncomePerSecond = totalIncomePerSecond;
        data.lastSaveTime = DateTime.Now;

        data.stores = GridManager.Instance.GetStoreSaveData();

        return data;
    }

    // gather game data from file for loading
    public void LoadGameData(GameData data)
    {
        currentMoney = data.currentMoney;
        totalIncomePerSecond = data.totalIncomePerSecond;

        GridManager.Instance.LoadStores(data.stores);

        GameUI.Instance.UpdateUI();

        AchievementManager.Instance?.CheckMoneyAchievements(currentMoney);
        AchievementManager.Instance?.CheckIncomeAchievements(totalIncomePerSecond);
    }
}
