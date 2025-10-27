using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI incomeText;
    [SerializeField] private TextMeshProUGUI currentUserText;
    [SerializeField] private StoreUIButton[] storeUIButtons;
    [SerializeField] private GameObject buttonGroup;
    [SerializeField] private GameObject hintTextObject;
    [SerializeField] private GameObject pausePanel;

    private void Awake()
    {
        // Singleton Initializaiton
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void UpdateUI()
    {
        moneyText.text = $"$ {CurrencyUtility.CurrencyFormat(GameManager.Instance.GetCurrentMoney())}";
        incomeText.text = $"+ {CurrencyUtility.CurrencyFormat(GameManager.Instance.GetTotalIncome())}/s";

        // Set buttons interactable state based on currentMoney
        foreach (var button in storeUIButtons)
        {
            button.SetButtonInteractable(GameManager.Instance.GetCurrentMoney() >= button.GetBuildCost());
        }
    }

    public void SetButtonGroupActive(bool value)
    {
        buttonGroup.SetActive(value);
    }

    public void SetHintTextActive(bool value)
    {
        hintTextObject.SetActive(value);
    }

    public void  SetPausePanelActive(bool value)
    {
        pausePanel.SetActive(value);
    }
}
