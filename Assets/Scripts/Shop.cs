using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Shop : MonoBehaviour
{
    public InventoryButton buttonPrefab;
    public GameObject spacerPrefab;
    public Inventory inventory;
    public GameObject wrapper;

    public InventoryPanel categoryPanel;
    public InventoryPanel sellPanel;
    public Fisher fisher;

    public List<EquipColor> equipColors;
    
    public bool IsOpen { get; private set; }

    private Dictionary<string, List<ShopItem>> _itemPool;

    private const string UpgradeCategory = "UpgradeCategory";
    private const string EquipCategory = "EquipCategory";
    private const string PetCategory = "PetCategory";
    private const string ClothesCategory = "ClothesCategory";

    private void Start()
    {
        _itemPool = new Dictionary<string, List<ShopItem>>();
        
        _itemPool[UpgradeCategory] = new List<ShopItem>
        {
            new UpgradeItem {Name = "Haggler", Description = "Increases sell prices by 10%", Price = 50, Repeatable = true, Type = Upgrade.Haggle},
            new UpgradeItem {Name = "Patron", Description = "Decreases buy prices by 10%", Price = 50, Repeatable = true, Type = Upgrade.Patron},
            new UpgradeItem {Name = "Bigger Bag", Description = "+5 bag space", Price = 100, Repeatable = true, Type = Upgrade.BagSpace},
            new UpgradeItem {Name = "Bulk Trader", Description = "Ability to sell all fish", Price = 500, Repeatable = false, Type = Upgrade.BulkTrader}
        };
        
        _itemPool[EquipCategory] = new List<ShopItem>
        {
            new EquipItem
            {
                Name = "Newbie Rod",
                Description = "A decent rod for beginners",
                Price = 50,
                Repeatable = false,
                Slot = EquipSlot.Rod
            },
            new EquipItem
            {
                Name = "Newbie Hook",
                Description = "A decent hook for beginners",
                Price = 50,
                Repeatable = false,
                Slot = EquipSlot.Hook
            }
        };

        _itemPool[PetCategory] = new List<ShopItem>
        {
            new UpgradeItem
            {
                Name = "Dog",
                Description = "Lovely helper pet",
                Price = 300,
                Repeatable = true,
                Type = Upgrade.Dog
            },
            new UpgradeItem
            {
                Name = "Doggy Bag",
                Description = "A bag for your dog",
                Price = 300,
                Repeatable = false,
                Type = Upgrade.DogBag
            }
        };

        _itemPool[ClothesCategory] = new List<ShopItem>();
        equipColors.ForEach(c =>
        {
            _itemPool[ClothesCategory].Add(new EquipItem
            {
                Name = c.name + " Shirt",
                Description = "Comfortable " + c.name.ToLower() + " t-shirt",
                Price = 50,
                Slot = EquipSlot.Shirt,
                Color = c.color
            });
            
            _itemPool[ClothesCategory].Add(new EquipItem
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

    private void UpdateSellMenu(Container items, bool canSell = true)
    {
        sellPanel.Clear();
        
        if (items.GetCount() == 0) return;
        
        sellPanel.title.text = canSell ? "Sell fish" : "Held fish";
        
        items.GetContents().ForEach(item =>
        {
            var btn = Instantiate(buttonPrefab, sellPanel.container);
            sellPanel.Add(btn.gameObject);
            btn.nameText.text = item.Name;
            btn.descText.text = item.Description;
            var multi = Mathf.Pow(1.1f, inventory.GetLevel(Upgrade.Haggle));
            var adjustedPrice = Mathf.RoundToInt(item.Price * multi);
            btn.priceText.text = adjustedPrice.ToString();
            if (canSell)
            {
                btn.button.onClick.AddListener(() =>
                {
                    Destroy(btn.gameObject);
                    items.Remove(item);
                    inventory.AddMoney(adjustedPrice);
                });   
            }
        });
    }

    private void Open(Container bag, bool shopping)
    {
        IsOpen = true;
        wrapper.SetActive(true);

        if (shopping)
        {
            categoryPanel.Clear();
            PopulateCategories(bag);
            return;
        }

        PopulateInventory();
        UpdateSellMenu(bag, false);
    }
    
    private void PopulateInventory()
    {
        categoryPanel.Clear();
        categoryPanel.title.text = "Inventory";
        
        fisher.Gear.ForEach(e =>
        {
            var btn = CreateCategory(e.Name, e.Description, () =>
            {
                if (!e.Equipped)
                {
                    fisher.Equip(e);
                    PopulateInventory();   
                }
            });
            if (e.Equipped)
            {
                btn.priceText.text = "E";
            }
        });
        
        CreateSpacer(categoryPanel.container);
        CreateCategory("Close", "Done for now", Close);
        CreateSpacer(categoryPanel.container);
        CreateCategory("Quit", ":(", PopulateConfirmation);
    }

    private void PopulateConfirmation()
    {
        sellPanel.Clear();
        sellPanel.title.text = "Are you sure?";
        CreateInventoryItem("Yes", "So sad...", Application.Quit, sellPanel);
        CreateInventoryItem("No", "Oopsie doopsie!", sellPanel.Clear, sellPanel);
    }

    private void PopulateCategories(Container bag)
    {
        categoryPanel.title.text = "Shop of Donald Behr";
        if (inventory.HasUpgrade(Upgrade.BulkTrader))
        {
            CreateCategory("Sell all", "Sell all fish", () => SellAll(bag));   
        }
        CreateCategory("Sell", "Sell to D. Behr", () => UpdateSellMenu(bag));
        CreateCategory("Browse Upgrades", "Buy from D. Behr", () => UpdateBuyMenu(UpgradeCategory));
        CreateCategory("Browse Equipment", "Buy from D. Behr", () => UpdateBuyMenu(EquipCategory));
        CreateCategory("Browse Pet Stuff", "Buy from D. Behr", () => UpdateBuyMenu(PetCategory));
        CreateCategory("Browse Clothes", "Buy from D. Behr", () => UpdateBuyMenu(ClothesCategory));
        CreateCategory("Close", "Leave shop", Close);
        
        CreateSpacer(categoryPanel.container);
        CreateCategory("Quit", ":(", PopulateConfirmation);
    }

    private void CreateSpacer(Transform panel)
    {
        var spacer = Instantiate(spacerPrefab, panel);
        categoryPanel.Add(spacer);
    }

    private void UpdateBuyMenu(string category)
    {
        sellPanel.Clear();
        
        sellPanel.title.text = "In stock";
        
        _itemPool[category].ForEach(item =>
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
                    _itemPool[category].Remove(item);
                    Destroy(btn.gameObject);                    
                }
                
                inventory.AddMoney(-item.GetRealPrice(fisher));
                item.Buy(fisher);
                
                UpdateBuyMenu(category);
            });
        });
    }

    public void SellAll(Container bag)
    {
        var total = bag.GetContents().Sum(item => item.Price);
        var multi = Mathf.Pow(1.1f, inventory.GetLevel(Upgrade.Haggle));
        inventory.AddMoney(Mathf.RoundToInt(total * multi));
        bag.Clear();
        sellPanel.Clear();
    }
    
    private InventoryButton CreateInventoryItem(string title, string desc, Action action, InventoryPanel panel)
    {
        var btn = Instantiate(buttonPrefab, panel.container);
        panel.Add(btn.gameObject);
        btn.nameText.text = title;
        btn.descText.text = desc;
        btn.priceText.text = "";
        btn.button.onClick.AddListener(action.Invoke);
        return btn;
    }

    private InventoryButton CreateCategory(string title, string desc, Action action)
    {
        return CreateInventoryItem(title, desc, action, categoryPanel);
    }

    private void Close()
    {
        IsOpen = false;
        wrapper.SetActive(false);
        sellPanel.Clear();
    }

    public void Toggle(Container bag, bool shopping)
    {
        if (IsOpen)
        {
            Close();
            return;
        }
        
        Open(bag, shopping);
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
    public bool Equipped { get; set; } = false;
    public Color Color { get; set; } = Color.white;
    public EquipSlot Slot;
    public int SpriteIndex { get; set; } = -1;

    public override void Buy(Fisher fisher)
    {
        base.Buy(fisher);
        fisher.Gear.Add(this);
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
    Dog,
    DogBag
}