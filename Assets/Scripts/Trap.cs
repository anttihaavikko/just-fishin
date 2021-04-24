using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : HasContainer
{
    private Container _contents;

    private void Start()
    {
        _contents = new Container(1);
        
        Invoke(nameof(AddFish), 5f);
    }
    
    public bool HasFish()
    {
        return _contents.GetCount() > 0;
    }

    public override Fish? GetFish()
    {
        return _contents.GetFish();
    }

    private void AddFish()
    {
        _contents.Add(Fisher.GetRandomFish());
        Invoke(nameof(AddFish), 5f);
    }
}
