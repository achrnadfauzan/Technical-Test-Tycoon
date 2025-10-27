using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

// Enum for better error reporting
public enum SaveLoadStatus
{
    Success,
    Failure_DiskFull,
    Failure_AccessDenied,
    Failure_FileNotFound,
    Failure_CorruptFile,
    Failure_Unknown
}

// Class for handling the serialization/deserialization of GameData to binary filess
public static class SaveSystem
{
    private const string SaveFileExtension = ".dat";
    private const string SaveFileNamePrefix = "save_";

    private const long MinRequiredFreeSpace = 1024 * 1024; // Estimated minimum space to require for a save file
    private const string GlobalDataFileName = "global_achievements.dat"; // Fixed filename

    // Gets the full path for a specific save slot for a given user
    private static string GetSaveFilePath(string username, int slotIndex)
    {
        string userPath = AccountManager.GetUserSavePath(username);
        string saveFileName = $"{SaveFileNamePrefix}{slotIndex}{SaveFileExtension}";
        return Path.Combine(userPath, saveFileName);
    }

    // Saves the provided GameData to a specific user folder
    public static SaveLoadStatus SaveGame(string username, int slotIndex, GameData data)
    {
        string filePath = GetSaveFilePath(username, slotIndex);

        // 1. Requirement: Free space check
        try
        {
            // Get drive info for the persistentDataPath
            string drive = Path.GetPathRoot(Application.persistentDataPath);
            DriveInfo driveInfo = new DriveInfo(drive);
            if (driveInfo.AvailableFreeSpace < MinRequiredFreeSpace)
            {
                Debug.LogWarning($"[SaveSystem] Save failed: Not enough disk space. {driveInfo.AvailableFreeSpace} bytes remaining.");
                return SaveLoadStatus.Failure_DiskFull;
            }
        }
        catch (System.Exception e)
        {
            // This can fail (on some platforms or permissions issues)
            Debug.LogWarning($"[SaveSystem] Disk space check failed: {e.Message}. Proceeding with save attempt...");
        }

        // 2. Requirement: Binary serialization
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = null;

        try
        {
            // Ensure the user's directory exists (it should, but good to check)
            Directory.CreateDirectory(AccountManager.GetUserSavePath(username));

            // Create or overwrite the save file
            stream = new FileStream(filePath, FileMode.Create);
            formatter.Serialize(stream, data);
            
            return SaveLoadStatus.Success;
        }
        catch (IOException e)
        {
            // Negative Case: This triggers if the disk is actually full during write.
            Debug.LogError($"[SaveSystem] Save failed (IOException): {e.Message}");
            return SaveLoadStatus.Failure_DiskFull;
        }
        catch (System.UnauthorizedAccessException e)
        {
            // Negative Case: File is read-only, or insufficient permissions.
            Debug.LogError($"[SaveSystem] Save failed (Access Denied): {e.Message}");
            return SaveLoadStatus.Failure_AccessDenied;
        }
        catch (System.Exception e)
        {
            // Negative Case: Catch-all for any other serialization or I/O error.
            Debug.LogError($"[SaveSystem] Save failed (Unknown Error): {e.Message}");
            return SaveLoadStatus.Failure_Unknown;
        }
        finally
        {
            // Close the file stream
            if (stream != null)
            {
                stream.Close();
            }
        }
    }

    /// Loads GameData from a specific user slot
    public static SaveLoadStatus LoadGame(string username, int slotIndex, out GameData data)
    {
        string filePath = GetSaveFilePath(username, slotIndex);
        data = null;

        // 1. Negative Case: File doesn't exist.
        if (!File.Exists(filePath))
        {
            return SaveLoadStatus.Failure_FileNotFound;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = null;

        try
        {
            stream = new FileStream(filePath, FileMode.Open);

            // Deserialize the data
            data = formatter.Deserialize(stream) as GameData;

            return SaveLoadStatus.Success;
        }
        catch (System.Runtime.Serialization.SerializationException e)
        {
            // Negative Case: File is corrupt or not a valid GameData file.
            Debug.LogError($"[SaveSystem] Load failed: File is corrupt. {e.Message}");
            return SaveLoadStatus.Failure_CorruptFile;
        }
        catch (System.UnauthorizedAccessException e)
        {
            // Negative Case: Insufficient permissions to read the file.
            Debug.LogError($"[SaveSystem] Load failed (Access Denied): {e.Message}");
            return SaveLoadStatus.Failure_AccessDenied;
        }
        catch (System.Exception e)
        {
            // Negative Case: Catch-all for other I/O errors.
            Debug.LogError($"[SaveSystem] Load failed (Unknown Error): {e.Message}");
            return SaveLoadStatus.Failure_Unknown;
        }
        finally
        {
            if (stream != null)
            {
                stream.Close();
            }
        }
    }

    // Gets the full path for the global data file.
    private static string GetGlobalDataFilePath()
    {
        // Place it directly inside the RootSaveDirectory, not a user folder
        return Path.Combine(AccountManager.GetRootSavePath(), GlobalDataFileName);
    }

    // Saves global data (like achievements) to a dedicated file.
    public static SaveLoadStatus SaveGlobalData(AchievementData data)
    {
        string filePath = GetGlobalDataFilePath();

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = null;

        try
        {
            // Ensure the root save directory exists
            Directory.CreateDirectory(AccountManager.GetRootSavePath());

            stream = new FileStream(filePath, FileMode.Create);
            formatter.Serialize(stream, data);
            Debug.Log($"[SaveSystem] Global data saved to {filePath}");
            return SaveLoadStatus.Success;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] SaveGlobalData failed: {e.Message}");
            return SaveLoadStatus.Failure_Unknown;
        }
        finally
        {
            if (stream != null)
            {
                stream.Close();
            }
        }
    }
    
    // Loads global data (like achievements) from its dedicated file.
    public static SaveLoadStatus LoadGlobalData(out AchievementData data)
    {
        string filePath = GetGlobalDataFilePath();
        data = null;

        if (!File.Exists(filePath))
        {
            // If no global file exists, it's not an error, just means no achievements saved yet.
            // Return a new empty data object.
            data = new AchievementData();
            return SaveLoadStatus.Success; // Or Failure_FileNotFound if you prefer to treat it differently
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = null;

        try
        {
            stream = new FileStream(filePath, FileMode.Open);
            data = formatter.Deserialize(stream) as AchievementData;

            // Check if deserialization somehow failed or file was empty/corrupt
            if (data == null)
            {
                 Debug.LogWarning($"[SaveSystem] LoadGlobalData: File at {filePath} was corrupt or incompatible. Creating new data.");
                 data = new AchievementData(); // Return fresh data instead of null
                 return SaveLoadStatus.Failure_CorruptFile; // Indicate the issue
            }

            return SaveLoadStatus.Success;
        }
        catch (System.Runtime.Serialization.SerializationException e)
        {
            Debug.LogError($"[SaveSystem] LoadGlobalData failed (SerializationException): File is corrupt. {e.Message}");
            data = new AchievementData(); // Return fresh data
            return SaveLoadStatus.Failure_CorruptFile;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] LoadGlobalData failed (Unknown Error): {e.Message}");
            data = new AchievementData(); // Return fresh data
            return SaveLoadStatus.Failure_Unknown;
        }
        finally
        {
            if (stream != null)
            {
                stream.Close();
            }
        }
    }
}