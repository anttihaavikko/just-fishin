using System.Collections.Generic;

public class Container
{
    private int _maxSize = 5;
    private readonly List<Fish> _contents;

    public Container()
    {
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
}