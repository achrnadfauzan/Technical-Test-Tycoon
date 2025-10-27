using UnityEngine;

// This class is attached to the Store prefab. It holds a reference to its data.
public class Store : MonoBehaviour
{
    public StoreData Data { get; private set; }

    // Called by the GridManager after instantiation.
    public void Initialize(StoreData data)
    {
        Data = data;
    }
}
