using System;
using System.Collections;
using System.Collections.Generic;
using AnttiStarterKit.Managers;
using AnttiStarterKit.Utils;
using UnityEngine;
using UnityEngine.PlayerLoop;
using AnttiStarterKit.Extensions;
using Random = UnityEngine.Random;

public class Inventory : MonoBehaviour
{
    public Fisher fisher;
    public List<Trap> traps;
    public Transform storeSpot;
    public Dog dogPrefab;
    public Score scoreDisplay;
    public Animator booth;

    public List<MyAppearer> logoStuffs;
    
    private int _money;
    private Dictionary<Upgrade, int> _upgrades;
    private List<Dog> _dogs;

    private bool _hasStarted;
    private bool _canStart;
    
    private static readonly int Activate = Animator.StringToHash("activate");
    
    public const double ShopRange = 1.5f;

    private void Start()
    {
        traps = new List<Trap>();
        _dogs = new List<Dog>();
        _upgrades = new Dictionary<Upgrade, int>();

        this.StartCoroutine(() => _canStart = true, 2f);
    }

    private void Update()
    {
        if (!_hasStarted && Input.anyKeyDown && _canStart)
        {
            _hasStarted = true;
            fisher.DoStart();
            logoStuffs.ForEach(a => a.HideWithDelay());
        }
        
        if (!Application.isEditor) return;
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneChanger.Instance.ChangeScene("Main");
        }
        
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
        
        if (Input.GetKeyDown(KeyCode.B))
        {
            ApplyUpgrade(Upgrade.BagSpace);
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
        AudioManager.Instance.PlayEffectAt(Random.Range(24, 26), storeSpot.position, 2f);
        _money += amount;
        scoreDisplay.Add(amount);
        AnimateBooth();
    }

    public void AnimateBooth()
    {
        booth.SetTrigger(Activate);
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

    public bool CanAfford(int price)
    {
        return _money >= price;
    }
}
