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
    public Fisher fisher;

    public List<EquipColor> equipColors;
    
    public bool IsOpen { get; private set; }

    private List<ShopItem> itemPool;

    private void Start()
    {
        Close();
        itemPool = new List<ShopItem>();
        equipColors.ForEach(c =>
        {
            itemPool.Add(new EquipItem
            {
                Name = c.name + " Shirt",
                Price = 50,
                Slot = EquipSlot.Shirt,
                Color = c.color
            });
            
            itemPool.Add(new EquipItem
            {
                Name = c.name + " Bucket Hat",
                Price = 50,
                Slot = EquipSlot.Hat,
                Color = c.color,
                SpriteIndex = 0
            });
        });
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
        CreateCategory("Buy", "Stuff", UpdateBuyMenu);
        CreateCategory("Close", "Leave shop", Close);
    }

    private void UpdateBuyMenu()
    {
        itemPool.ForEach(item =>
        {
            var btn = Instantiate(buttonPrefab, sellPanel.container);
            sellPanel.Add(btn.gameObject);
            btn.nameText.text = item.Name;
            btn.descText.text = "Lorem ipsum";
            btn.priceText.text = item.Price.ToString();
            btn.button.onClick.AddListener(() =>
            {
                if (!item.Repeatable)
                {
                    itemPool.Remove(item);
                    Destroy(btn.gameObject);                    
                }
                
                inventory.AddMoney(-item.Price);
                item.Buy(fisher);
            });
        });
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

public class ShopItem
{
    public string Name { get; set; }
    public int Price { get; set; }
    
    public bool Repeatable { get; set; }

    public virtual void Buy(Fisher fisher)
    {
    }
}

public class EquipItem : ShopItem
{
    public Color Color { get; set; } = Color.white;
    public EquipSlot Slot;
    public int SpriteIndex { get; set; } = -1;

    public override void Buy(Fisher fisher)
    {
        base.Buy(fisher);
        Equip(fisher);
    }

    protected virtual void Equip(Fisher fisher)
    {
        fisher.Equip(this);
    }
}

public enum EquipSlot
{
    Shirt,
    Hat,
    Rod,
    Hook
}

[Serializable]
public struct EquipColor
{
    public string name;
    public Color color;
}