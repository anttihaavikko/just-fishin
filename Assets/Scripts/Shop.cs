using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public InventoryButton buttonPrefab;
    public Inventory inventory;
    public GameObject wrapper;

    public InventoryPanel categoryPanel;
    public InventoryPanel sellPanel;
    
    public bool IsOpen { get; private set; }

    private void Start()
    {
        Close();
    }

    private void UpdateSellMenu(Container items)
    {
        sellPanel.Clear();
        
        if (items.GetCount() == 0) return;
        
        sellPanel.title.text = "Sell fish";
        
        items.GetContents().ForEach(item =>
        {
            var btn = Instantiate(buttonPrefab, sellPanel.container);
            sellPanel.Add(btn.gameObject);
            btn.nameText.text = item.Name;
            btn.descText.text = "Lorem ipsum";
            btn.priceText.text = item.Price.ToString();
            btn.button.onClick.AddListener(() =>
            {
                Destroy(btn.gameObject);
                items.Remove(item);
                inventory.AddMoney(item.Price);
            });
        });
    }

    private void Open(Container bag)
    {
        IsOpen = true;
        wrapper.SetActive(true);
        
        categoryPanel.Clear();
        PopulateCategories(bag);
    }

    private void PopulateCategories(Container bag)
    {
        categoryPanel.title.text = "Shop";
        CreateCategory("Sell all", "Sell all fish", () => SellAll(bag));
        CreateCategory("Sell", "Something", () => UpdateSellMenu(bag));
        CreateCategory("Close", "Leave shop", Close);
    }

    private void SellAll(Container bag)
    {
        var total = bag.GetContents().Sum(item => item.Price);
        inventory.AddMoney(total);
        bag.Clear();
        sellPanel.Clear();
    }

    private void CreateCategory(string title, string desc, Action action)
    {
        var btn = Instantiate(buttonPrefab, categoryPanel.container);
        categoryPanel.Add(btn.gameObject);
        btn.nameText.text = title;
        btn.descText.text = desc;
        btn.priceText.text = "";
        btn.button.onClick.AddListener(action.Invoke);
    }

    private void Close()
    {
        IsOpen = false;
        wrapper.SetActive(false);
        sellPanel.Clear();
    }

    public void Toggle(Container bag)
    {
        if (IsOpen)
        {
            Close();
            return;
        }
        
        Open(bag);
    }
}
