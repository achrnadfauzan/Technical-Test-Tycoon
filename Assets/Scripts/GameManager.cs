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
        InvokeRepeating("AddMoney", 0, 1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) PauseGame(!isGamePaused);
    }

    private void AddMoney()
    {
        currentMoney += totalIncomePerSecond;
        GameUI.Instance.UpdateUI();
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

    // Called when new store is built
    public void AddIncome(int amount)
    {
        totalIncomePerSecond += amount;
        GameUI.Instance.UpdateUI();
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
}
