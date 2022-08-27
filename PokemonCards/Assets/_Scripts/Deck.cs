using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Deck
{

    public List<string> cards;
    // Length should always be last!!
    public enum SortTypes { Type, Hp, Rarity, Length }

    public int maxSortIndex = -1;
    public int cardCountSorted
    {
        get
        {
            if (maxSortIndex < 0)
                return cards.Count;
            return Mathf.Min(maxSortIndex, cards.Count);
        }
    }

    public Deck()
    {
        cards = new List<string>();
    }

    public void Sort(SortTypes type)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            PokemonCard bestC = GameManager.GetCard(cards[i]);
            int bestIndex = i;
            for (int j = i + 1; j < cards.Count; j++)
            {
                PokemonCard curC = GameManager.GetCard(cards[j]);
                if (curC == null)
                    continue;

                switch (type)
                {
                    case SortTypes.Type:
                        if (bestC == null || bestC.types[0].CompareTo(curC.types[0]) >= 0)
                        {
                            bestIndex = j;
                            bestC = GameManager.GetCard(cards[bestIndex]);
                        }
                        break;
                    case SortTypes.Hp:
                        Debug.Log(int.Parse(curC.hp));
                        if (bestC == null || int.Parse(curC.hp) < int.Parse(bestC.hp))
                        {
                            bestIndex = j;
                            bestC = GameManager.GetCard(cards[bestIndex]);
                        }
                        break;
                    case SortTypes.Rarity:
                        if (bestC == null || bestC.rarity.CompareTo(curC.rarity) >= 0)
                        {
                            bestIndex = j;
                            bestC = GameManager.GetCard(cards[bestIndex]);
                        }
                        break;
                }
            }
            if (bestIndex != i)
            {
                string temp = cards[i];
                cards[i] = cards[bestIndex];
                cards[bestIndex] = temp;
            }
        }
    }

    public void Search(string term)
    {
        if (string.IsNullOrEmpty(term))
        {
            maxSortIndex = -1;
            return;
        }

        int lastSorted = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            PokemonCard card = GameManager.GetCard(cards[i]);
            if (card == null)
                continue;

            // Swaps cards
            if (card.name.ToLower().Contains(term.ToLower()))
            {
                string temp = cards[i];
                cards[i] = cards[lastSorted];
                cards[lastSorted] = temp;

                lastSorted++;
            }
        }

        maxSortIndex = lastSorted;
    }
}
