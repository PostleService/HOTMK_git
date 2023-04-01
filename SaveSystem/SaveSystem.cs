using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveSystem
{
    // Current location of save files is in C:\Users\<username>\AppData\LocalLow\DefaultCompany\<project_name>
    public static void SaveGame(GameManager aGameManager)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/gamestate.sav";
        FileStream stream = new FileStream(path, FileMode.Create);

        SaveData data = new SaveData(aGameManager);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static SaveData LoadGame()
    {
        string path = Application.persistentDataPath + "/gamestate.sav";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SaveData data = formatter.Deserialize(stream) as SaveData;
            stream.Close();

            return data;
        }
        else
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Create);
            SaveData data = new SaveData();
            formatter.Serialize(stream, data);
            stream.Close();

            Debug.LogWarning("save was not found under " + path + ". Creating new savefile.");
            return data;
        }
    }
}
