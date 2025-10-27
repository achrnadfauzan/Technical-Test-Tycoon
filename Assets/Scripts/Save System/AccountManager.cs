using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Class for handling creation, deletion, and listing of user profiles.
public static class AccountManager
{
    // the name of the root folder in Application.persistentDataPath
    private const string RootSaveDirectory = "SaveData";

    // Gets the absolute path to the root save directory
    public static string GetRootSavePath()
    {
        return Path.Combine(Application.persistentDataPath, RootSaveDirectory);
    }

    // Gets the absolute path to a specific user's save folder (e.g., ".../SaveData/PlayerName/").
    public static string GetUserSavePath(string username)
    {
        return Path.Combine(GetRootSavePath(), username);
    }

    // Creates a new user account by creating a new folder for them.
    public static bool CreateUser(string username)
    {
        // Negative Case: Handle invalid username (empty or containing invalid path characters)
        if (string.IsNullOrEmpty(username) || username.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            Debug.LogError($"[AccountManager] CreateUser failed: Username '{username}' is invalid.");
            return false;
        }

        string userPath = GetUserSavePath(username);

        // Negative Case: User folder already exists
        if (Directory.Exists(userPath))
        {
            Debug.LogWarning($"[AccountManager] CreateUser failed: User '{username}' already exists.");
            return false;
        }

        try
        {
            // Create the root folder (".../SaveData/") if it doesn't exist
            Directory.CreateDirectory(GetRootSavePath());
            
            // Create the user's folder (".../SaveData/PlayerName/")
            Directory.CreateDirectory(userPath);
            
            Debug.Log($"[AccountManager] Created new user: {username}");
            return true;
        }
        catch (System.Exception e)
        {
            // Negative Case: Handle other errors (like permissions or disk full)
            Debug.LogError($"[AccountManager] Error creating user directory '{userPath}': {e.Message}");
            return false;
        }
    }

    // Deletes a user account and all associated save files
    public static bool DeleteUser(string username)
    {
        string userPath = GetUserSavePath(username);

        // Negative Case: User folder does not exist
        if (!Directory.Exists(userPath))
        {
            Debug.LogWarning($"[AccountManager] DeleteUser failed: User '{username}' not found.");
            return false;
        }

        try
        {
            // This recursively deletes the user's folder and all files inside it.
            Directory.Delete(userPath, true);
            Debug.Log($"[AccountManager] Deleted user: {username}");
            return true;
        }
        catch (System.Exception e)
        {
            // Negative Case: File is in use, permissions error, etc.
            Debug.LogError($"[AccountManager] Error deleting user directory '{userPath}': {e.Message}");
            return false;
        }
    }

    // Gets a list of all existing user accounts (folder names).
    public static List<string> GetAllUsers()
    {
        string rootPath = GetRootSavePath();
        List<string> users = new List<string>();

        // Negative Case: The root save directory doesn't even exist yet.
        if (!Directory.Exists(rootPath))
        {
            return users; // Return an empty list
        }

        try
        {
            // Get all subdirectories in the root save path
            string[] userPaths = Directory.GetDirectories(rootPath);

            foreach (string path in userPaths)
            {
                // Add just the folder name (the username) to the list
                users.Add(Path.GetFileName(path));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AccountManager] Error reading user directories: {e.Message}");
            // Return whatever we found, which might be an empty list
        }
        
        return users;
    }
}