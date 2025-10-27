using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Class for managing the grid logic, converting world positions to grid coordinates, and tracking which grid cells are occupied.
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float cellSize = 1.0f;
    [SerializeField] private Transform gridOrigin; // Bottom left corner of the grid
    [SerializeField] private GameObject storePrefab; // A generic prefab to instantiate
    [SerializeField] private List<StoreData> allStoreDatas;

    private Dictionary<Vector2Int, Store> occupiedCells = new Dictionary<Vector2Int, Store>();

    private void Awake()
    {
        // Singleton Initialization
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Converts a world position from a mouse click to a grid cell coordinate.
    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        // Calculate offset from the grid's origin
        Vector3 relativePos = worldPosition - gridOrigin.position;

        // Determine grid coordinates
        int x = Mathf.FloorToInt(relativePos.x / cellSize);
        int y = Mathf.FloorToInt(relativePos.y / cellSize);

        return new Vector2Int(x, y);
    }

    // Calculates the center world position of a given grid cell.
    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        return gridOrigin.position + new Vector3(
            (gridPos.x * cellSize) + (cellSize * 0.5f),
            (gridPos.y * cellSize) + (cellSize * 0.5f),
            0
        );
    }

    // Checks if a grid cell is valid (within bounds) and not already occupied.
    private bool IsCellValidAndEmpty(Vector2Int gridPos)
    {
        // Check bounds
        if (gridPos.x < 0 || gridPos.x >= gridWidth || gridPos.y < 0 || gridPos.y >= gridHeight)
        {
            Debug.Log("Build failed: Outside grid bounds.");
            return false;
        }

        // Check if occupied
        if (occupiedCells.ContainsKey(gridPos))
        {
            Debug.Log("Build failed: Cell is already occupied.");
            return false;
        }

        return true;
    }

    private StoreData FindStoreDataByName(string storeName)
    {
        // Use Linq to find the first match.
        return allStoreDatas.FirstOrDefault(data => data.storeName == storeName);
    }

    public bool TryBuildStore(Vector2Int gridPos, StoreData storeData)
    {
        // Handle Negative Case: Cell invalid or occupied
        if (!IsCellValidAndEmpty(gridPos))
        {
            return false;
        }

        // Handle Negative Case: Not enough money
        if (!GameManager.Instance.TrySpendMoney(storeData.buildCost))
        {
            return false;
        }

        // All checks passed, build the store
        BuildStoreInternal(gridPos, storeData);
        
        Debug.Log($"Successfully built {storeData.storeName} at {gridPos}");
        return true;
    }

    private void BuildStoreInternal(Vector2Int gridPos, StoreData storeData)
    {
        // Instantiate the store
        Vector3 buildWorldPos = GetWorldPosition(gridPos);
        GameObject storeGO = Instantiate(storePrefab, buildWorldPos, Quaternion.identity);
        storeGO.name = storeData.storeName;

        // Configure the store
        Store storeComponent = storeGO.GetComponent<Store>();
        SpriteRenderer sr = storeGO.GetComponent<SpriteRenderer>();

        if (storeComponent != null)
        {
            storeComponent.Initialize(storeData);
        }
        if (sr != null)
        {
            sr.sprite = storeData.storeSprite;
        }

        // Update game state
        occupiedCells.Add(gridPos, storeComponent); // Mark cell as occupied
        GameManager.Instance.AddIncome(storeData.incomePerSecond); // Add to total income
    }

    private void ClearGrid()
    {
        foreach (KeyValuePair<Vector2Int, Store> pair in occupiedCells)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value.gameObject);
            }
        }
        
        occupiedCells.Clear();
    }

    // Return total number of tiles
    public int GetGridCapacity()
    {
        return gridWidth * gridHeight;
    }

    public List<StoreSaveData> GetStoreSaveData()
    {
        List<StoreSaveData> storeSaveList = new List<StoreSaveData>();

        foreach (KeyValuePair<Vector2Int, Store> pair in occupiedCells)
        {
            // use the StoreData's name as a unique ID
            string storeName = pair.Value.Data.storeName;
            storeSaveList.Add(new StoreSaveData(storeName, pair.Key));
        }

        return storeSaveList;
    }
    
    public void LoadStores(List<StoreSaveData> storesToLoad)
    {
        // Clear all existing stores
        ClearGrid();
        
        // Tell GameManager to reset income before we re-add it
        GameManager.Instance.ResetIncome();

        if (storesToLoad == null) return; // Handle new game (empty list)

        // Loop through save data and rebuild each store
        foreach (StoreSaveData savedStore in storesToLoad)
        {
            // Find the correct ScriptableObject using the saved name
            StoreData storeData = FindStoreDataByName(savedStore.storeDataName);

            if (storeData != null)
            {
                // Build the store *without* charging money
                BuildStoreInternal(savedStore.GetGridPosition(), storeData);
            }
            else
            {
                // Negative Case: A store was saved but its StoreData is no longer in 'allStoreDatas'
                Debug.LogWarning($"Load failed: Could not find StoreData for '{savedStore.storeDataName}'");
            }
        }
    }

    // Draw a visual gizmo in the editor to see grid from the scene view
    private void OnDrawGizmos()
    {
        if (gridOrigin == null) return;
        
        Gizmos.color = Color.green;
        Vector3 origin = gridOrigin.position;

        // Draw vertical lines
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = origin + new Vector3(x * cellSize, 0, 0);
            Vector3 end = origin + new Vector3(x * cellSize, gridHeight * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }
        
        // Draw horizontal lines
        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 start = origin + new Vector3(0, y * cellSize, 0);
            Vector3 end = origin + new Vector3(gridWidth * cellSize, y * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }
    }
}
