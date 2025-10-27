using System;
using UnityEngine;

[System.Serializable]
public class StoreSaveData
{
    public string storeDataName;
    public int gridPosX; // make separate int variable for x and y since Vector2 is not natively serializable in unity
    public int gridPosY;

    public StoreSaveData(string name, Vector2Int gridPos)
    {
        storeDataName = name;
        gridPosX = gridPos.x;
        gridPosY = gridPos.y;
    }
    
    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(gridPosX, gridPosY);
    }
}