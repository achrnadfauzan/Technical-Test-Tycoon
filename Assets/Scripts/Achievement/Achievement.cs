using UnityEngine;

public enum AchievementType
{
    MoneyReached,
    IncomeReached
}

[CreateAssetMenu(fileName = "New Achievement", menuName = "SO/Achievement")]
public class Achievement : ScriptableObject
{
    // unique ID for this achievement
    public string achievementID; // example: "MONEY_1M", "INCOME_100K"

    // display name shown to the player
    public string displayName;

    // requirement shown to the player
    [TextArea] public string description;

    // achievement type (money / income)
    public AchievementType unlockType;
    
    // value requirement
    public long unlockValue;
}