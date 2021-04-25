using System;
using System.Collections;
using System.Collections.Generic;
using AnttiStarterKit.Utils;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class Inventory : MonoBehaviour
{
    public Fisher fisher;
    public List<Trap> traps;
    public Transform storeSpot;
    public Dog dogPrefab;
    public Score scoreDisplay;
    
    private int _money;
    private Dictionary<Upgrade, int> _upgrades;
    private List<Dog> _dogs;

    private void Start()
    {
        traps = new List<Trap>();
        _dogs = new List<Dog>();
        _upgrades = new Dictionary<Upgrade, int>();
    }

    private void Update()
    {
        if (!Application.isEditor) return;
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            AddDog();
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Time.timeScale = 1f;
        }
        
        if (Input.GetKeyDown(KeyCode.W))
        {
            Time.timeScale = 5f;
        }
    }

    private void AddDog()
    {
        var color = Color.HSVToRGB(Random.value, 0.4f, 0.95f);
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
        scoreDisplay.Add(amount);
        Debug.Log("Cash: " + _money);
    }
    
    public void ApplyUpgrade(Upgrade item)
    {
        InitOrIncrement(item);

        switch (item)
        {
            case Upgrade.Dog:
                AddDog();
                break;
            case Upgrade.BagSpace:
                fisher.ScaleBag();
                break;
            case Upgrade.DogBag:
                _dogs.ForEach(d => d.AddBag());
                break;
            case Upgrade.BigTraps:
                traps.ForEach(t => t.SetMaxSize(3));
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
