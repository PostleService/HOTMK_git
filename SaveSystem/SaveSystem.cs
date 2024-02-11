using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveSystem
{
    // Current location of save files is 
    // Win: C:\Users\<username>\AppData\LocalLow\DefaultCompany\<project_name>
    // iOS: /var/mobile/Containers/Data/Application/<guid>/Documents.
    public static void SaveGame(GameManager aGameManager)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/gamestate_" + aGameManager.GameVersion + ".sav";
        FileStream stream = new FileStream(path, FileMode.Create);

        SaveData data = new SaveData(aGameManager);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static SaveData LoadGame()
    {
        string path = Application.persistentDataPath + "/gamestate_" + GameObject.Find("GameManager").GetComponent<GameManager>().GameVersion + ".sav";
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
