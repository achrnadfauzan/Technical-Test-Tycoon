using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A simple helper class to put on UI Buttons.
[RequireComponent(typeof(Button))]
public class StoreUIButton : MonoBehaviour
{
    [SerializeField] private StoreData storeData;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI incomeText;

    private Button button;

    void Awake()
    {
        // Initialize button properties
        button = GetComponent<Button>();
        
        if (storeData == null)
        {
            Debug.LogError($"Button '{gameObject.name}' has no StoreData assigned!");
            button.interactable = false;
            return;
        }
        
        button.onClick.AddListener(OnButtonClick);

        if (costText != null)
        {
            costText.text = $"$ {CurrencyUtility.CurrencyFormat(storeData.buildCost)}";
        }
        if (incomeText != null)
        {
            incomeText.text = $"+ {CurrencyUtility.CurrencyFormat(storeData.incomePerSecond)}/s";
        }
    }

    private void OnButtonClick()
    {
        // Tell the BuildManager to select this store
        BuildManager.Instance.SelectStoreType(storeData);
    }

    public int GetBuildCost()
    {
        return storeData.buildCost;
    }

    public void SetButtonInteractable(bool value)
    {
        button.interactable = value;
    }
}
