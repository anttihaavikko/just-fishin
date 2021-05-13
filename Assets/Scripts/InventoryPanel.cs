using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryPanel : MonoBehaviour
{
    public Transform container;
    public TMP_Text title;
    public MyAppearer appearer;

    private List<GameObject> _items;
    
    private void Awake()
    {
        _items = new List<GameObject>();
    }

    public void Add(GameObject item)
    {
        _items.Add(item);
    }

    public void Clear()
    {
        title.text = "";
        _items.ForEach(Destroy);
        _items.Clear();
    }
}
