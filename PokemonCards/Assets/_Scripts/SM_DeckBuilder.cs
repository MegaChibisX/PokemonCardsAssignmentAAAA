using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;
using System.Linq;

public class SM_DeckBuilder : MonoBehaviour
{

    public static SM_DeckBuilder instance;

    public RectTransform cardPrefab;

    public RectTransform deckCorner;
    public RectTransform folderCorner;

    [HideInInspector]
    public static List<Button> buttonDeck;
    [HideInInspector]
    public static List<Button> buttonFolder;

    public Button btAnim;

    [HideInInspector]
    public static int deckPage;
    [HideInInspector]
    public static int folderPage;

    private int selectedButton = 0;

    public TMP_InputField search;
    public TMP_Dropdown sort;
    public RawImage display;

    public TMP_Dropdown decks;
    public TMP_InputField deckName;

    [HideInInspector]
    public bool isAnimating = false;
    private string lastSearch;

    public Image antiInput;
    public RectTransform errorBoxTooFewCards;
    public RectTransform errorBoxNoName;

    public static void SetScene_Home()
    {
        SceneManager.LoadScene("Home");
    }
    public static void PageNext()
    {
        folderPage++;
        if (folderPage >= Mathf.CeilToInt(GameManager.deckFolder.cards.Count / (float)buttonFolder.Count))
            folderPage = 0;
        instance.selectedButton = 0;

        UpdateDeckVisuals();
    }
    public static void PagePrev()
    {
        folderPage--;
        if (folderPage < 0)
            folderPage = Mathf.CeilToInt(GameManager.deckFolder.cards.Count / (float)buttonFolder.Count) - 1;
        instance.selectedButton = 0;

        UpdateDeckVisuals();
    }

    public static void Save()
    {
        if (GameManager.savedDecks == null)
            GameManager.savedDecks = new Dictionary<string, string[]>();
        if (GameManager.deckDeck == null)
            GameManager.deckDeck = new Deck();

        string deckName = instance.deckName.text;
        if (string.IsNullOrEmpty(deckName))
        {
            instance.antiInput.gameObject.SetActive(true);
            instance.errorBoxNoName.gameObject.SetActive(true);
        }
        else if (GameManager.deckDeck.cards.Count() < 30)
        {
            instance.antiInput.gameObject.SetActive(true);
            instance.errorBoxTooFewCards.gameObject.SetActive(true);
        }

        if (GameManager.savedDecks.ContainsKey(deckName))
            GameManager.savedDecks.Remove(deckName);
        GameManager.savedDecks.Add(deckName, GameManager.deckDeck.cards.ToArray());

        SaveData.Save();
    }
    public static void Load()
    {
        SaveData.Load();
    }

    public static void DeleteCurrentDeck()
    {
        string deckName = instance.decks.options[instance.decks.value].text;
        if (GameManager.savedDecks.ContainsKey(deckName))
            GameManager.savedDecks.Remove(deckName);

        instance.decks.ClearOptions();
        instance.decks.AddOptions(GameManager.savedDecks.Keys.ToList());

        Save();
    }
    public static void ClearDeck()
    {
        GameManager.deckDeck.cards.Clear();
        UpdateDeckVisuals();
    }
    public static void EndErrors()
    {
        instance.antiInput.gameObject.SetActive(false);
        instance.errorBoxNoName.gameObject.SetActive(false);
        instance.errorBoxTooFewCards.gameObject.SetActive(false);
    }



    public void Start()
    {
        if (GameManager.instance == null)
            SceneManager.LoadScene("Loading");

        instance = this;

        // Sets up deck
        buttonDeck = new List<Button>();
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                RectTransform tr = Instantiate(cardPrefab);
                tr.SetParent(deckCorner);
                tr.position = deckCorner.position + new Vector3(x * (tr.rect.width + tr.rect.height * 0.2f)* 0.3f, (y * tr.rect.height * 1.0f) * -1f * 0.3f);
                tr.localScale = cardPrefab.localScale;

                Button b = tr.GetComponent<Button>();
                int index = y * 5 + x;

                b.onClick.AddListener(delegate {
                    MoveCardToFolder(this, index);
                });

                buttonDeck.Add(b);
            }
        }

        // Sets up folder
        buttonFolder = new List<Button>();
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                RectTransform tr = Instantiate(cardPrefab);
                tr.SetParent(folderCorner);
                tr.position = folderCorner.position + new Vector3((x - 3) * (tr.rect.width  + tr.rect.height * 0.2f) * 0.3f, ((y - 2.5f) * tr.rect.height * 1.2f) * -1f * 0.3f);
                tr.localScale = cardPrefab.localScale;

                Button b = tr.GetComponent<Button>();
                int index = y * 3 + x;

                b.onClick.AddListener(delegate {
                    MoveCardToDeck(this, index);
                });

                buttonFolder.Add(b);
            }
        }

        // Updates visuals
        UpdateDeckVisuals();

        // Sets up other buttons
        sort.ClearOptions();
        List<string> opt = new List<string>();
        for (int i = 0; i < (int)Deck.SortTypes.Length; i++)
            opt.Add(((Deck.SortTypes)i).ToString());
        sort.AddOptions(opt);

        sort.onValueChanged.AddListener(delegate
        {
            GameManager.deckFolder.Sort((Deck.SortTypes)sort.value);
            GameManager.deckFolder.maxSortIndex = -1;
            UpdateDeckVisuals();
        });

        SM_DeckBuilder.instance.decks.ClearOptions();
        SM_DeckBuilder.instance.decks.AddOptions(GameManager.savedDecks.Keys.ToList());
        decks.onValueChanged.AddListener(delegate
        {
            GameManager.deckDeck.cards = GameManager.savedDecks[decks.options[decks.value].text].ToList();
            UpdateDeckVisuals();
        });

        if (GameManager.savedDecks.ContainsKey(decks.options[decks.value].text))
        {
            GameManager.deckDeck.cards = GameManager.savedDecks[decks.options[decks.value].text].ToList();
        }
        if (GameManager.savedDecks.ContainsKey(instance.deckName.text))
            GameManager.savedDecks.Remove(instance.deckName.text);
        GameManager.savedDecks.Add(instance.deckName.text, GameManager.deckDeck.cards.ToArray());

        // Animation Button
        RectTransform trAnim = Instantiate(cardPrefab);
        trAnim.SetParent(deckCorner);
        trAnim.localScale = cardPrefab.localScale;

        btAnim = trAnim.GetComponent<Button>();
        btAnim.gameObject.SetActive(false);
    }
    public void Update()
    {
        if (GameManager.instance == null)
            return;

        string sr = search.text;
        if (sr != lastSearch)
        {
            GameManager.deckFolder.Search(sr);
            UpdateDeckVisuals();
        }
        lastSearch = sr;
    }


    public static void MoveCardToFolder(SM_DeckBuilder deck, int index)
    {
        if (index < 0 || index >= buttonDeck.Count)
            return;
        if (index >= GameManager.deckDeck.cards.Count)
            return;

        Vector3 startPos = buttonDeck[index].transform.position;
        Vector3 endPos = buttonFolder[buttonFolder.Count - 1].transform.position;

        string removeId = GameManager.deckDeck.cards[index];
        Texture txt = GameManager.GetCard(removeId).imageSm;
        
        GameManager.deckDeck.cards.RemoveAt(index);
        UpdateDeckVisuals();

        deck.StartCoroutine(deck.MoveCardAnim(startPos, endPos, txt, () =>
        {
            UpdateDeckVisuals();
        }));
    }

    public static void MoveCardToDeck(SM_DeckBuilder deck, int index)
    {
        if (index < 0 || index >= buttonFolder.Count)
            return;

        int lastB = instance.selectedButton;
        instance.selectedButton = index;
        UpdateDeckVisuals();
        if (lastB != index)
            return;

        int gmIndex = folderPage * buttonFolder.Count + index;

        if (GameManager.deckDeck.cards.Count >= buttonDeck.Count || gmIndex >= GameManager.deckFolder.cardCountSorted)
            return;

        UpdateDeckVisuals();
        Vector3 startPos = buttonFolder[index].transform.position;
        Vector3 endPos = buttonDeck[GameManager.deckDeck.cards.Count].transform.position;

        deck.StartCoroutine(deck.MoveCardAnim(startPos, endPos, GameManager.GetCard(GameManager.deckFolder.cards[gmIndex]).imageSm,() =>
        {
            GameManager.deckDeck.cards.Add(GameManager.deckFolder.cards[gmIndex]);
            UpdateDeckVisuals();
        }));
    }

    private IEnumerator MoveCardAnim(Vector3 startPos, Vector3 endPos, Texture img, Action onEnd)
    {
        if (isAnimating)
            yield break;

        isAnimating = true;

        btAnim.transform.position = startPos;
        btAnim.gameObject.SetActive(true);
        btAnim.transform.Find("Sprite").GetComponent<RawImage>().texture = img;

        btAnim.transform.DOMove(endPos, 0.2f, false);
        yield return new WaitForSeconds(0.2f);

        onEnd.Invoke();

        btAnim.gameObject.SetActive(false);
        isAnimating = false;
    }


    public static void UpdateDeckVisuals()
    {
        // For debug, working on the scene without the decks loaded.
        if (GameManager.deckDeck == null || GameManager.deckFolder == null)
            return;
        if (!instance)
            return;

        for (int i = 0; i < buttonDeck.Count; i++)
        {
            PokemonCard card = null;
            int index = deckPage * buttonDeck.Count + i;

            if (index < GameManager.deckDeck.cards.Count)
                card = GameManager.GetCard(GameManager.deckDeck.cards[index]);

            if (card == null)
                buttonDeck[i].transform.Find("Sprite").GetComponent<RawImage>().texture = null;
            else
                buttonDeck[i].transform.Find("Sprite").GetComponent<RawImage>().texture = card.imageSm;
        }

        for (int i = 0; i < buttonFolder.Count; i++)
        {
            PokemonCard card = null;
            int index = folderPage * buttonFolder.Count + i;
            
            if (index < GameManager.deckFolder.cardCountSorted)
                card = GameManager.GetCard(GameManager.deckFolder.cards[index]);

            if (card == null)
                buttonFolder[i].transform.Find("Sprite").GetComponent<RawImage>().texture = null;
            else
                buttonFolder[i].transform.Find("Sprite").GetComponent<RawImage>().texture = card.imageSm;
        }

        PokemonCard selectCard = GameManager.GetCard(GameManager.deckFolder.cards[folderPage * buttonFolder.Count + SM_DeckBuilder.instance.selectedButton]);
        if (selectCard != null)
        {
            instance.display.gameObject.SetActive(true);
            instance.display.texture = selectCard.imageSm;
        }
        else
            instance.display.gameObject.SetActive(false);
    }


}
