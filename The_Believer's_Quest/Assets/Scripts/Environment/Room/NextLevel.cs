﻿using UnityEngine;
using UnityEditor;
using System.Collections;

public class NextLevel : MonoBehaviour
{
    private Board board;
    [SerializeField] private PlayerAsset playerAsset;
    private bool canGo;
    private bool firstTrigger;              //just in case

    private GameObject boardManager;

    public PlayerAsset PlayerAsset { get => playerAsset; set => playerAsset = value; }

    private void Start()
    {
        board = Board.instance;
        boardManager = GameObject.Find("BoardManager");
        firstTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.CompareTag("Player"))
        {
            if(firstTrigger)
            {
                board.Init();
                if (playerAsset.Floor > 0)
                {
                    GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>().ChangeBO(playerAsset.Floor + 3);
                    Destroy(GameObject.Find("Board"));
                    MapController.mapInstance.ResetMap();
                    Saver.SavePlayerData(playerAsset, Player.instance.UnlockedItems.Unlocked);
                }
            }
            firstTrigger = false;
        }
    }
}