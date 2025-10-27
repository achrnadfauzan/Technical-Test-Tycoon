using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

// Class for managing the game's economy (money and income).
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private long currentMoney = 1000; // Start with some money
    [SerializeField] private long totalIncomePerSecond = 0;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI incomeText;
    [SerializeField] private StoreUIButton[] storeUIButtons;

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

    private void AddMoney()
    {
        currentMoney += totalIncomePerSecond;
        UpdateUI();
    }

    private void UpdateUI()
    {
        moneyText.text = $"$ {CurrencyUtility.CurrencyFormat(currentMoney)}";
        incomeText.text = $"+ {CurrencyUtility.CurrencyFormat(totalIncomePerSecond)}/s";

        // Set buttons interactable state based on currentMoney
        foreach (var button in storeUIButtons)
        {
            button.SetButtonInteractable(currentMoney >= button.GetBuildCost());
        }
    }

    // Tries to spend money. Returns true if successful, false if not.
    public bool TrySpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateUI();
            return true;
        }

        Debug.LogWarning("Not enough money to build!");
        return false;
    }

    // Called when new store is built
    public void AddIncome(int amount)
    {
        totalIncomePerSecond += amount;
        UpdateUI();
    }

    public long GetCurrentMoney()
    {
        return currentMoney;
    }
    
    public long GetTotalIncome()
    {
        return totalIncomePerSecond;
    }
}
