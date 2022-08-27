using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class SM_Loading : MonoBehaviour
{

    public TextMeshProUGUI tmp_loading;

    public Transform pokeBall;
    public Transform pokeEnd;

    public void Start()
    {
        pokeBall.DOMove(pokeEnd.position, 10f, false).SetLoops(-1, LoopType.Yoyo);
    }


}
