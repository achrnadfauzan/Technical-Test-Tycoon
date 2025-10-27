using System;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public long currentMoney;
    public long totalIncomePerSecond;
    public DateTime lastSaveTime;

    public List<StoreSaveData> stores;
}