﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Nicolas I
public class NewGame : MonoBehaviour
{
    [SerializeField] private PlayerAsset playerData;
    [SerializeField] private UnlockedItemsAsset unlockedItems;

    public PlayerAsset PlayerData { get => playerData; set => playerData = value; }
    public UnlockedItemsAsset UnlockedItems { get => unlockedItems; set => unlockedItems = value; }

    public void StartNewGame()    
    {
        Saver.SavePlayerData(playerData, unlockedItems);
    }
}
