﻿using UnityEngine;

public class InventoryUI : MonoBehaviour
{

    public Transform itemsParent;   
    public GameObject inventoryUI;
    public GameObject chest;

    Inventory inventory;    

    InventorySlot[] slots; 

    void Start()
    {
        inventory = Inventory.instance;
        inventory.onItemChangedCallback += UpdateUI;    
        
        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Interact"))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
        }
    }
    
    void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.items.Count)  
            {
                slots[i].AddItem(inventory.items[i]);   
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }
}