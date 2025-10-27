// Create this script: Assets/Scripts/Achievements/AchievementManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Class for managinf loading, saving, checking, and unlocking achievements.
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [SerializeField] private List<Achievement> allAchievements;

    private AchievementData achievementData; // Holds the state (unlocked IDs)

    // event if an achievement is unlocked (for the future)
    public static event System.Action<Achievement> OnAchievementUnlocked;

    private void Awake()
    {
        // Singleton Initialization
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAchievements();
    }

    // read achievements data from file
    private void LoadAchievements()
    {
        SaveLoadStatus status = SaveSystem.LoadGlobalData(out achievementData);
        if (status != SaveLoadStatus.Success && status != SaveLoadStatus.Failure_FileNotFound) // FileNotFound is okay on first launch
        {
            Debug.LogWarning($"[AchievementManager] Failed to load achievement data. Status: {status}");
        }
        else if (achievementData == null) // double check in case LoadGlobalData logic changes
        {
            Debug.LogError("[AchievementManager] Achievement data is null after loading! Creating new data.");
            achievementData = new AchievementData();
        }
        Debug.Log($"[AchievementManager] Loaded {achievementData.unlockedAchievementIDs.Count} unlocked achievements.");
    }

    private void SaveAchievements()
    {
        SaveLoadStatus status = SaveSystem.SaveGlobalData(achievementData);
        if (status != SaveLoadStatus.Success)
        {
            Debug.LogWarning($"[AchievementManager] Failed to save achievement data. Status: {status}");
        }
    }

    public bool IsUnlocked(string achievementID)
    {
        if (achievementData == null || achievementData.unlockedAchievementIDs == null)
        {
             Debug.LogError("[AchievementManager] Achievement data is null when checking IsUnlocked!");
             return false;
        }
        return achievementData.unlockedAchievementIDs.Contains(achievementID);
    }

    private void TryUnlockAchievement(Achievement achievement)
    {
        if (achievement == null) return;
        
        string id = achievement.achievementID;

        // Requirement: Check the achievement state first
        if (IsUnlocked(id))
        {
            return; // Already unlocked, do nothing
        }
        
        // Unlock it
        achievementData.unlockedAchievementIDs.Add(id);
        Debug.Log($"[AchievementManager] Unlocked achievement: {achievement.displayName} ({id})");

        // Save progress immediately after unlocking
        SaveAchievements();

        // Requirement: Fire event (for future)
        OnAchievementUnlocked?.Invoke(achievement);
    }

    private Achievement GetAchievementByID(string id)
    {
        return allAchievements.FirstOrDefault(ach => ach.achievementID == id);
    }

    public void CheckMoneyAchievements(long currentMoney)
    {
        foreach (var achievement in allAchievements)
        {
            if (achievement.unlockType == AchievementType.MoneyReached && currentMoney >= achievement.unlockValue)
            {
                TryUnlockAchievement(achievement);
            }
        }
    }

    public void CheckIncomeAchievements(long currentIncome)
    {
        foreach (var achievement in allAchievements)
        {
            if (achievement.unlockType == AchievementType.IncomeReached && currentIncome >= achievement.unlockValue)
            {
                TryUnlockAchievement(achievement);
            }
        }
    }

    private void OnApplicationQuit()
    {
        SaveAchievements();
    }
}