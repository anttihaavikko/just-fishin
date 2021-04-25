using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Container
{
    public UnityAction<string> onUpdate;
    
    private int _maxSize = 5;
    private readonly List<Fish> _contents;

    public Container(int max = 5)
    {
        _maxSize = max;
        _contents = new List<Fish>();
    }

    public bool Add(Fish fish)
    {
        if (_contents.Count >= _maxSize) return false;
        _contents.Add(fish);
        UpdateCount();
        return true;
    }

    private void UpdateCount()
    {
        onUpdate?.Invoke(_contents.Any() ? _contents.Count + "/" + _maxSize : "");
    }

    public int GetCount()
    {
        return _contents.Count;
    }

    public List<Fish> GetContents()
    {
        return _contents;
    }

    public void Remove(Fish fish)
    {
        _contents.Remove(fish);
        UpdateCount();
    }

    public void Clear()
    {
        _contents.Clear();
        UpdateCount();
    }

    public Fish? GetFish()
    {
        if (!_contents.Any()) return null;
        
        var fish = _contents.First();
        _contents.Remove(fish);
        UpdateCount();
        return fish;
    }

    public bool IsFull()
    {
        return _contents.Count >= _maxSize;
    }

    public void SetMaxSize(int max)
    {
        _maxSize = max;
    }

    public Color GetColor()
    {
        return !_contents.Any() ? Color.white : _contents.First().color;
    }

    public float GetSize()
    {
        return !_contents.Any() ? 1f : _contents.First().size;
    }
}