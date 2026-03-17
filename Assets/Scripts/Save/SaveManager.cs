using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "pet_save.json");

    public static void Save(PetState petState, Transform petTransform)
    {
        SaveData data = new SaveData
        {
            affection = petState.affection,
            energy = petState.energy,
            mood = petState.mood,
            posX = petTransform.position.x,
            posY = petTransform.position.y,
            posZ = petTransform.position.z
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Saved to: " + SavePath);
        Debug.Log(json);
    }

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public static SaveData Load()
    {
        if (!HasSave())
        {
            Debug.Log("No save file found.");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        Debug.Log("Loaded from: " + SavePath);
        Debug.Log(json);

        return data;
    }

    public static void DeleteSave()
    {
        if (HasSave())
        {
            File.Delete(SavePath);
            Debug.Log("Save file deleted.");
        }
    }
}