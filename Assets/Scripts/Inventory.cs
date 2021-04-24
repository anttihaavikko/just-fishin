using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private int _money;

    public void AddMoney(int amount)
    {
        _money += amount;
        Debug.Log("Cash: " + _money);
    }
}
