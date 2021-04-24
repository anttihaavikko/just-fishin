using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : HasContainer
{
    private Container _contents;

    private void Start()
    {
        _contents = new Container();
    }
    
    public bool HasFish()
    {
        return _contents.GetCount() > 0;
    }

    public override Fish? GetFish()
    {
        return _contents.GetFish();
    }
}
