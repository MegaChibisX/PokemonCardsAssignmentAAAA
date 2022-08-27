using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class SaveData
{

    public string[] deckNames;
    public string[][] deckIDs;


    public static void Save()
    {
        SaveData data = GenerateData();

        string path = Application.dataPath + "/Data";
        System.IO.Directory.CreateDirectory(path);
        path += "/saveData.data";

        var binaryFormatter = new BinaryFormatter();
        using (var fileStream = File.Create(path))
        {
            binaryFormatter.Serialize(fileStream, data);
        }

        if (SM_DeckBuilder.instance)
        {
            SM_DeckBuilder.instance.decks.ClearOptions();
            SM_DeckBuilder.instance.decks.AddOptions(GameManager.savedDecks.Keys.ToList());
        }

        Debug.Log("Saved!");
    }
    public static void Load()
    {

        string path = Application.dataPath + "/Data/saveData.data";
        if (File.Exists(path))
        {
            SaveData data;

            var binaryFormatter = new BinaryFormatter();
            using (var fileStream = File.Open(path, FileMode.Open))
            {
                data = (SaveData)binaryFormatter.Deserialize(fileStream);
            }

            LoadFromData(data);
        }

        if (SM_DeckBuilder.instance)
        {
            SM_DeckBuilder.instance.decks.ClearOptions();
            SM_DeckBuilder.instance.decks.AddOptions(GameManager.savedDecks.Keys.ToList());
        }

        Debug.Log("Loaded!");
    }




    public static SaveData GenerateData()
    {
        SaveData data = new SaveData();

        List<string> deckNames = new List<string>();
        List<string[]> deckIDs = new List<string[]>();

        foreach (var v in GameManager.savedDecks)
        {
            deckNames.Add(v.Key);
            deckIDs.Add(v.Value);
        }

        data.deckIDs = deckIDs.ToArray();
        data.deckNames = deckNames.ToArray();

        return data;
    }

    public static void LoadFromData(SaveData data)
    {
        GameManager.savedDecks = new Dictionary<string, string[]>();
        for (int i = 0; i < data.deckNames.Length; i++)
        {
            GameManager.savedDecks.Add(data.deckNames[i], data.deckIDs[i]);
        }
    }

}
