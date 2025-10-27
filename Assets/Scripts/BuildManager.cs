using UnityEngine;
using UnityEngine.EventSystems;

// Class for handling player input & tracking selected store to build.
public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    [SerializeField] private GameObject buttonGroup;
    [SerializeField] private GameObject hintTextObject;

    private StoreData selectedStoreToBuild;
    private int storeCount = 0;

    private void Awake()
    {
        // Singleton initialization
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }

    void Update()
    {
        // If player clicks the left mouse button
        if (Input.GetMouseButtonDown(0))
        {
            // Handle negative case: Player has not selected a store to build
            if (selectedStoreToBuild == null)
            {
                Debug.LogWarning("Build failed: No store selected");
                return;
            }

            // Handle negative case: Player clicked on a UI element
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.LogWarning("Build failed: Click invalid");
                return;
            }
            
            // Get mouse position in the game world
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPoint.z = 0;

            // Convert world position to a grid cell
            Vector2Int gridPos = GridManager.Instance.GetGridPosition(worldPoint);

            // Try to build the store
            bool buildSuccess = GridManager.Instance.TryBuildStore(gridPos, selectedStoreToBuild);

            if (buildSuccess)
            {
                DeselectStore();
                storeCount++;
                if (storeCount >= GridManager.Instance.GetGridCapacity()) buttonGroup.SetActive(false);
            }

            // If build failed, keep the store selected so the player can try clicking on a different, valid tile.
        }

        // Input for build cancel
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            if(selectedStoreToBuild != null)
            {
                DeselectStore();
                Debug.Log("Build canceled.");
            }
        }
    }

    // Called by UI buttons (StoreUIButton) to set the active store.
    public void SelectStoreType(StoreData storeData)
    {
        // Handle negative case: Data from scriptable object is empty
        if (storeData == null)
        {
            Debug.LogError("StoreData Scriptable Objects was not found");
            return;
        }

        selectedStoreToBuild = storeData;
        Debug.Log($"Selected: {selectedStoreToBuild.storeName}");

        // Hide buttons and show hint
        buttonGroup.SetActive(false);
        hintTextObject.SetActive(true);
    }

    // Reset the current store selection.
    public void DeselectStore()
    {
        selectedStoreToBuild = null;

        // Show buttons and hide hint
        buttonGroup.SetActive(true);
        hintTextObject.SetActive(false);
    }
}
