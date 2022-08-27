using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.IO;
using System;
using Newtonsoft.Json.Linq;

[System.Serializable]
public class PokemonCard
{

    public string id;
    public string name;
    public string[] types;
    public string hp;
    public string rarity;

    public Texture imageSm;


    [System.Serializable]
    public class Ability
    {
        public string name;
        public string text;
        public string type;
    }
    [System.Serializable]
    public class Attacks
    {
        public string name;
        public string text;
        public string damage;
    }

    public static PokemonCard GetCard(string id)
    {
        if (!GameManager.cardDict.ContainsKey(id))
            return null;

        return GameManager.cardDict[id];
    }
}
