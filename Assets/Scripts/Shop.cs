using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

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

    private List<ShopItem> _itemPool;

    private void Start()
    {
        _itemPool = new List<ShopItem>
        {
            new UpgradeItem {Name = "Haggler", Description = "Increases sell prices by 10%", Price = 50, Repeatable = true, Type = Upgrade.Haggle},
            new UpgradeItem {Name = "Patron", Description = "Decreases buy prices by 10%", Price = 50, Repeatable = true, Type = Upgrade.Patron},
            new UpgradeItem {Name = "Bigger Bag", Description = "+5 bag space", Price = 100, Repeatable = true, Type = Upgrade.BagSpace},
            new UpgradeItem {Name = "Bulk Trader", Description = "Ability to sell all fish", Price = 500, Repeatable = false, Type = Upgrade.BulkTrader},
            new UpgradeItem {Name = "Dog", Description = "Lovely helper pet", Price = 300, Repeatable = true, Type = Upgrade.Dog}
        };

        equipColors.ForEach(c =>
        {
            _itemPool.Add(new EquipItem
            {
                Name = c.name + " Shirt",
                Description = "Comfortable " + c.name.ToLower() + " t-shirt",
                Price = 50,
                Slot = EquipSlot.Shirt,
                Color = c.color
            });
            
            _itemPool.Add(new EquipItem
            {
                Name = c.name + " Bucket Hat",
                Description = "Practical " + c.name.ToLower() + " hat",
                Price = 50,
                Slot = EquipSlot.Hat,
                Color = c.color,
                SpriteIndex = 0
            });
        });
        
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
            btn.descText.text = item.Description;
            var multi = Mathf.Pow(1.1f, inventory.GetLevel(Upgrade.Haggle));
            var adjustedPrice = Mathf.RoundToInt(item.Price * multi);
            btn.priceText.text = adjustedPrice.ToString();
            btn.button.onClick.AddListener(() =>
            {
                Destroy(btn.gameObject);
                items.Remove(item);
                inventory.AddMoney(adjustedPrice);
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
        if (inventory.HasUpgrade(Upgrade.BulkTrader))
        {
            CreateCategory("Sell all", "Sell all fish", () => SellAll(bag));   
        }
        CreateCategory("Sell", "Something", () => UpdateSellMenu(bag));
        CreateCategory("Buy", "Stuff", UpdateBuyMenu);
        CreateCategory("Close", "Leave shop", Close);
    }

    private void UpdateBuyMenu()
    {
        sellPanel.Clear();
        
        _itemPool.ForEach(item =>
        {
            var btn = Instantiate(buttonPrefab, sellPanel.container);
            sellPanel.Add(btn.gameObject);
            btn.nameText.text = item.Name;
            btn.descText.text = item.Description;
            btn.priceText.text = item.GetRealPrice(fisher).ToString();
            btn.button.onClick.AddListener(() =>
            {
                if (!item.Repeatable)
                {
                    _itemPool.Remove(item);
                    Destroy(btn.gameObject);                    
                }
                
                inventory.AddMoney(-item.GetRealPrice(fisher));
                item.Buy(fisher);
                
                UpdateBuyMenu();
            });
        });
    }

    private void SellAll(Container bag)
    {
        var total = bag.GetContents().Sum(item => item.Price);
        var multi = Mathf.Pow(1.1f, inventory.GetLevel(Upgrade.Haggle));
        inventory.AddMoney(Mathf.RoundToInt(total * multi));
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
    public string Description { get; set; }
    public int Price { protected get; set; }
    public bool Repeatable { get; set; } = false;

    protected int DiscountedPrice(Fisher fisher)
    {
        var multi = Mathf.Pow(0.9f, fisher.inventory.GetLevel(Upgrade.Patron));
        return Mathf.RoundToInt(Price * multi);
    }

    public virtual int GetRealPrice(Fisher fisher)
    {
        return DiscountedPrice(fisher);
    }

    public virtual void Buy(Fisher fisher)
    {
    }
}

public class UpgradeItem : ShopItem
{
    public Upgrade Type { get; set; }
    
    public override int GetRealPrice(Fisher fisher)
    {
        var level = fisher.inventory.GetLevel(Type);
        return Mathf.RoundToInt(DiscountedPrice(fisher) + Mathf.FloorToInt(Price * level * level));
    }
    
    public override void Buy(Fisher fisher)
    {
        base.Buy(fisher);
        fisher.inventory.ApplyUpgrade(Type);
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

public enum Upgrade
{
    Haggle,
    Patron,
    BagSpace,
    BulkTrader,
    Dog
}