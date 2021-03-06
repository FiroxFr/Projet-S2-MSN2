﻿using UnityEngine;
//Nicolas L

public class NextLevel : MonoBehaviour
{
    private Board board;
    private PlayerAsset playerAsset;
    private bool canGo;
    private bool firstTrigger;              //just in case

    private GameObject boardManager;

    public PlayerAsset PlayerAsset { get => playerAsset; set => playerAsset = value; }

    private void Start()
    {
        board = Board.instance;
        boardManager = GameObject.Find("BoardManager");
        firstTrigger = true;
        playerAsset = Player.instance.PlayerAsset;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.CompareTag("Player"))
        {
            if(firstTrigger)
            {
                playerAsset.Floor++;
                Saver.SavePlayerData(playerAsset, Player.instance.UnlockedItems);
                if (playerAsset.Floor > 0)
                {
                    GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>().ChangeBO(playerAsset.Floor + 3);
                    MapController.mapInstance.ResetMap();
                    Player.instance.PlayerAsset.EffectValue = 0;
                    Destroy(GameObject.Find("Board"));
                }

                board.Init();

                Destroy(gameObject);
            }

            firstTrigger = false;
        }
    }
}