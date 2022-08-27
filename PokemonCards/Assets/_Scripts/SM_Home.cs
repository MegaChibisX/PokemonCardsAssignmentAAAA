using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SM_Home : MonoBehaviour
{

    public static void SetScene_Builder()
    {
        SceneManager.LoadScene("DeckBuilder");
    }
    public static void SetScene_About()
    {
        SceneManager.LoadScene("About");
    }
}
