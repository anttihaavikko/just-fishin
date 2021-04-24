using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Fisher fisher;
    public List<Trap> traps;
    public Transform storeSpot;
    
    private int _money;
    private Dictionary<Upgrade, int> _upgrades;

    private void Start()
    {
        _upgrades = new Dictionary<Upgrade, int>();
    }

    public void AddMoney(int amount)
    {
        _money += amount;
        Debug.Log("Cash: " + _money);
    }
    
    public void ApplyUpgrade(Upgrade item)
    {
        if (!_upgrades.ContainsKey(item))
        {
            _upgrades.Add(item, 1);
            return;
        }
        
        _upgrades[item]++;
    }

    public int GetLevel(Upgrade item)
    {
        return _upgrades.ContainsKey(item) ? _upgrades[item] : 0;
    }

    public bool HasUpgrade(Upgrade item)
    {
        return _upgrades.ContainsKey(item) && _upgrades[item] > 0;
    }
}
