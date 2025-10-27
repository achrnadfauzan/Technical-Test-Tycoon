using System;
using System.Collections.Generic;

[Serializable]
public class AchievementData
{
    // Using HashSet for efficient checking if an ID exists
    public HashSet<string> unlockedAchievementIDs = new HashSet<string>();
}