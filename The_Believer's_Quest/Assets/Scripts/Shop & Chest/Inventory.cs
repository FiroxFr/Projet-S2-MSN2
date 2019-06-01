﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    void Awake()
    {
        if (instance != null)
        {
            print("should not do that" ); //DEBUG
            return;
        }

        instance = this;
    }

    // Callback which is triggered when
    // an item gets added/removed.
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public int space = 4;  // Amount of slots in inventory

    // Current list of items in inventory
    public List<GameObject> items = new List<GameObject>();

    // Add a new item. If there is enough room we
    // return true. Else we return false.
    public bool Add(GameObject item)
    {
        foreach(GameObject O in items) //Check if the item is already in items or not
        {
            if (O == item) return false;
        }

        // Check if out of space
        if (items.Count < space)
        {
            items.Add(item);    // Add item to list
            // Trigger callback
            if (onItemChangedCallback != null)
                onItemChangedCallback.Invoke();

            return true;
        }

        return false;
    }

    // Remove an item
    public void Remove(GameObject item)
    {
        items.Remove(item);     // Remove item from list

        // Trigger callback
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

}