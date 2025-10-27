using UnityEngine;

/// This is a data container for different store types.
[CreateAssetMenu(fileName = "NewStoreData", menuName = "Tycoon/Store Data")]
public class StoreData : ScriptableObject
{
    [Header("Store Info")]
    public string storeName;
    public int buildCost;
    public int incomePerSecond;

    [Header("Visuals")]
    public Sprite storeSprite;
}
