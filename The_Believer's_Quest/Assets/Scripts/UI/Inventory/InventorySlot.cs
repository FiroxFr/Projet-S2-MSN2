﻿using UnityEngine;
using UnityEngine.UI;

/* Sits on all InventorySlots. */

public class InventorySlot : MonoBehaviour
{

    public Image icon;          // Reference to the Icon image

    GameObject item;  // Current item in the slot

    // Add item to the slot
    public void AddItem(GameObject newItem)
    {
        item = newItem;
        icon.sprite = item.GetComponent<Object>().ObjectsAsset.Sprite;
        icon.enabled = true;
    }

    // Clear the slot
    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false;
    }

    // Called when the item is pressed
    public void UseItem()
    {
        if (GameObject.Find("Player").GetComponent<Player>().RoomType == Board.Type.Chest)
        {
            if (item != null)
            {
                if (Chest.instance.Add(item))
                {
                    Inventory.instance.Remove(item);
                }
            }
        }

        if (GameObject.Find("Player").GetComponent<Player>().RoomType == Board.Type.Shop)
        {
            if (item != null)
            {
                if (Shop.instance.Add(item))
                {
                    Inventory.instance.Remove(item);
                }
            }
        }
    }

}