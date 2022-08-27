using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    [SerializeField]
    public List<PokemonCard> cardList;
    public static Dictionary<string, PokemonCard> cardDict;

    public static Dictionary<string, string[]> savedDecks;

    public static Deck deckDeck;
    public static Deck deckFolder;

    public void Start()
    {
        if (SceneManager.GetActiveScene().name != "Loading")
            SceneManager.LoadScene("Loading");

        DontDestroyOnLoad(gameObject);
        instance = this;

        cardList = new List<PokemonCard>();
        cardDict = new Dictionary<string, PokemonCard>();

        deckDeck = new Deck();
        deckFolder = new Deck();

        savedDecks = new Dictionary<string, string[]>();

        StartCoroutine(LoadingCoroutine());
    }


    public IEnumerator LoadingCoroutine()
    {
        //yield return AddCardFromAPI("xy1-1");
        //yield return AddCardFromAPI("swsh1-1");
        //yield return AddCardFromAPI("bw1-1");

        string[] games = new string[] { "bw", "swsh", "xy" };

        SM_Loading loading = FindObjectOfType<SM_Loading>();

        yield return null;

        for (int i = 0; i < games.Length; i++)
        {
            for (int j = 1; j < 5; j++)
            {
                for (int k = 3; k < 8; k++)
                {
                    Debug.Log("AAAAAAAAAAAAAAAAAAAAA");

                    string id = "";
                    id += games[i];
                    id += j.ToString();
                    id += "-";
                    id += (k * 10).ToString();

                    yield return AddCardFromAPI(id);

                    string loadTxt = "LOADING...\n";
                    loadTxt += Mathf.RoundToInt(cardList.Count / 59f * 100);
                    loadTxt += "%";

                    loading.tmp_loading.text = loadTxt;
                }
            }
        }

        SaveData.Load();

        SceneManager.LoadScene("Home");
    }

    public IEnumerator AddCardFromAPI(string id)
    {
        Debug.Log("Prerequest");
        // Gets the card info from the API
        // Example: https://api.pokemontcg.io/v2/cards/xy1-1
        HttpWebRequest request = null;
        try
        {
            request = (HttpWebRequest)WebRequest.Create("https://api.pokemontcg.io/v2/cards/" + id);
        }
        catch (Exception e)
        {
            Debug.Log("Prerfail" + e.Message + "https://api.pokemontcg.io/v2/cards/" + id);
        }
        HttpWebResponse response = null;

        Debug.Log("Preresponse");
        // Catches error in case the request doesn't work.
        try
        {
            response = (HttpWebResponse)request.GetResponse();
        }
        catch
        {
            Debug.Log("No response");
            yield break;
        }
        Debug.Log("Pregotit");

        // Reads the info and converts to json
        StreamReader r = new StreamReader(response.GetResponseStream());
        string jsonStr = r.ReadToEnd();
        JObject jsonObj = JObject.Parse(jsonStr);

        // Makes the card and adds the basic string info
        PokemonCard card = new PokemonCard();

        JToken jsonToken = jsonObj.SelectToken("data");
        card.id = jsonToken.SelectToken("id").Value<string>();
        card.name = jsonToken.SelectToken("name").Value<string>();
        card.hp = jsonToken.SelectToken("hp").Value<string>();
        card.rarity = jsonToken.SelectToken("rarity").Value<string>();

        // Adds the enumerable data to the card
        List<string> l = new List<string>();
        foreach (var v in jsonToken.SelectTokens("types").Values<string>())
            l.Add(v);
        card.types = l.ToArray();

        Debug.Log("Prewww");
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(jsonToken.SelectToken("images").SelectToken("small").Value<string>());
        www.timeout = 1;
        yield return www.SendWebRequest();

        Texture t = DownloadHandlerTexture.GetContent(www);
        card.imageSm = t;

        cardList.Add(card);
        if (!cardDict.ContainsKey(id))
            cardDict.Add(id, card);

        deckFolder.cards.Add(id);

        Debug.Log("Preitworkled");
    }


    public static PokemonCard GetCard(string id)
    {
        if (!cardDict.ContainsKey(id))
            return null;

        return cardDict[id];
    }

}
