using System;
using System.Collections;
using System.Collections.Generic;
using AnttiStarterKit.Utils;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Inventory : MonoBehaviour
{
    public Fisher fisher;
    public List<Trap> traps;
    public Transform storeSpot;
    public Dog dogPrefab;
    
    private int _money;
    private Dictionary<Upgrade, int> _upgrades;
    private List<Dog> _dogs;

    private void Start()
    {
        _dogs = new List<Dog>();
        _upgrades = new Dictionary<Upgrade, int>();
    }

    private void Update()
    {
        if (!Application.isEditor) return;
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            AddDog(Color.yellow);
        }
    }

    private void AddDog(Color color)
    {
        var dog = Instantiate(dogPrefab, fisher.transform.position + Dog.GetRandomOffset(), Quaternion.identity);
        dog.inventory = this;
        dog.SetColor(color);
        _dogs.Add(dog);
        if (HasUpgrade(Upgrade.DogBag))
        {
            dog.AddBag();
        }
    }

    public void AddMoney(int amount)
    {
        _money += amount;
        Debug.Log("Cash: " + _money);
    }
    
    public void ApplyUpgrade(Upgrade item)
    {
        InitOrIncrement(item);

        switch (item)
        {
            case Upgrade.Dog:
                AddDog(Color.white);
                break;
            case Upgrade.BagSpace:
                fisher.ScaleBag();
                break;
            case Upgrade.DogBag:
                _dogs.ForEach(d => d.AddBag());
                break;
        }
    }

    private void InitOrIncrement(Upgrade item)
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
