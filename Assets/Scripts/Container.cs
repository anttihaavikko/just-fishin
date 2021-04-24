using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Container
{
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
        return true;
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
    }

    public void Clear()
    {
        _contents.Clear();
    }

    public Fish? GetFish()
    {
        if (!_contents.Any()) return null;
        
        var fish = _contents.First();
        _contents.Remove(fish);
        return fish;
    }

    public bool IsFull()
    {
        return _contents.Count >= _maxSize;
    }
}