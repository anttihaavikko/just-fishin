using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public InventoryButton buttonPrefab;
    public Transform sellPanel;
    public Inventory inventory;
    public GameObject wrapper;
    
    public bool IsOpen { get; private set; }

    private List<GameObject> _sellItems;

    private void Start()
    {
        _sellItems = new List<GameObject>();
        Close();
    }

    public void UpdateSellMenu(Container items)
    {
        _sellItems.ForEach(Destroy);
        _sellItems.Clear();
        
        items.GetContents().ForEach(item =>
        {
            var btn = Instantiate(buttonPrefab, sellPanel);
            _sellItems.Add(btn.gameObject);
            btn.nameText.text = item.Name;
            btn.descText.text = "Lorem ipsum";
            btn.priceText.text = item.Price.ToString();
            btn.button.onClick.AddListener(() =>
            {
                Destroy(btn.gameObject);
                inventory.AddMoney(item.Price);
            });
        });
    }

    private void Open()
    {
        IsOpen = true;
        wrapper.SetActive(true);
    }

    private void Close()
    {
        IsOpen = false;
        wrapper.SetActive(false);
    }

    public void Toggle(Container bag)
    {
        if (IsOpen)
        {
            Close();
            return;
        }
        
        UpdateSellMenu(bag);
        Open();
    }
}
