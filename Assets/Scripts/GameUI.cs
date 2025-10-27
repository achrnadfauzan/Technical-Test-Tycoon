using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private GameObject savePanel;
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private GameObject[] saveSlotButtons;
    [SerializeField] private GameObject[] loadSlotButtons;


    private void Awake()
    {
        // Singleton Initializaiton
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        currentUserText.text = SessionManager.Instance.CurrentUsername;
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

    public void SetPausePanelActive(bool value)
    {
        pausePanel.SetActive(value);
    }

    // called when player see save slot list while pausing
    public void OnShowSaveSlots()
    {
        string username = SessionManager.Instance.CurrentUsername;

        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            GameData slotData;
            SaveLoadStatus status = SaveSystem.LoadGame(username, i, out slotData);

            Button button = saveSlotButtons[i].GetComponent<Button>();
            TMP_Text buttonText = saveSlotButtons[i].GetComponentInChildren<TMP_Text>();

            if (status == SaveLoadStatus.Success)
            {
                // File exists, make button unsuable
                button.interactable = false;
                buttonText.text = $"Slot {i + 1} - {slotData.lastSaveTime.ToLocalTime()}\n$ {CurrencyUtility.CurrencyFormat(slotData.currentMoney)}";
            }
            else
            {
                // No file, make button usable
                button.interactable = true;
                buttonText.text = $"Slot {i + 1}\n- Empty -";

                int capturedSlotIndex = i;
                button.onClick.RemoveAllListeners(); // Clear old listeners
                button.onClick.AddListener(() => OnSaveSlotClicked(capturedSlotIndex));
            }
        }
    }

    // Called when user click an empty save slot
    public void OnSaveSlotClicked(int slotIndex)
    {
        // Get the current username
        string username = SessionManager.Instance.CurrentUsername;
        if (string.IsNullOrEmpty(username)) return; // Negative case

        // Get the current game data from GameManager
        GameData dataToSave = GameManager.Instance.GetCurrentGameData();

        // Call the SaveSystem
        SaveLoadStatus status = SaveSystem.SaveGame(username, slotIndex, dataToSave);

        // Handle negative cases and give feedback
        if (status == SaveLoadStatus.Success)
        {
            Debug.Log("Game Saved!");
            GameManager.Instance.PauseGame(false);
            savePanel.SetActive(false);
        }
        else if (status == SaveLoadStatus.Failure_DiskFull)
        {
            Debug.LogWarning("Save Failed: Disk is full!");
        }
        else
        {
            Debug.LogWarning("Save Failed: An unknown error occurred.");
        }
    }

    // CAlled when player see load slot list while pausing
    public void OnShowLoadSlots()
    {
        string username = SessionManager.Instance.CurrentUsername;

        for (int i = 0; i < loadSlotButtons.Length; i++)
        {
            GameData slotData;
            SaveLoadStatus status = SaveSystem.LoadGame(username, i, out slotData);

            Button button = loadSlotButtons[i].GetComponent<Button>();
            TMP_Text buttonText = loadSlotButtons[i].GetComponentInChildren<TMP_Text>();

            if (status == SaveLoadStatus.Success)
            {
                // File exists, make button usable
                button.interactable = true;
                buttonText.text = $"Slot {i + 1}\n{slotData.lastSaveTime.ToLocalTime()}\n$ {CurrencyUtility.CurrencyFormat(slotData.currentMoney)}";

                int capturedSlotIndex = i;
                button.onClick.RemoveAllListeners(); // Clear old listeners
                button.onClick.AddListener(() => OnLoadSlotClicked(capturedSlotIndex));
            }
            else
            {
                // No file, make button unusable
                button.interactable = false;
                buttonText.text = $"Slot {i + 1}\n- Empty -";
            }
        }
    }

    // Called when a load slot is clicked
    public void OnLoadSlotClicked(int slotIndex)
    {
        string username = SessionManager.Instance.CurrentUsername;

        GameData loadedData;
        SaveLoadStatus status = SaveSystem.LoadGame(username, slotIndex, out loadedData);

        if (status == SaveLoadStatus.Success)
        {
            // Tell GameManager to apply this data
            GameManager.Instance.LoadGameData(loadedData);

            // Close the pause menu and unpause
            GameManager.Instance.PauseGame(false);
            loadPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Failed to load file, but button was active. This shouldn't happen.");
        }
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
