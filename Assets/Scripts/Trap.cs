using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Managers;
using UnityEngine;

public class Trap : HasContainer
{
    public SpriteRenderer fish;
    
    private Container _contents;

    private void Start()
    {
        _contents = new Container(1);
        Invoke(nameof(AddFish), 5f);
        _contents.onUpdate = UpdateCountText;
    }
    
    public bool HasFish()
    {
        return _contents.GetCount() > 0;
    }

    public override Fish? GetFish()
    {
        var f = _contents.GetFish();
        UpdateVisuals();
        return f;
    }

    public List<Fish> GetAllFish()
    {
        var all = _contents.GetContents().ToList();
        _contents.Clear();
        return all;
    }

    private void AddFish()
    {
        Invoke(nameof(AddFish), 5f);
        
        if (_contents.IsFull()) return;
        
        var quaternion = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
        Tweener.Instance.RotateTo(transform, quaternion, 0.2f, 0, TweenEasings.BounceEaseOut);
        var p = fish.transform.position;
        EffectManager.Instance.AddEffect(0, p);
        EffectManager.Instance.AddEffect(1, p);
        _contents.Add(Fisher.GetRandomFish());
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        fish.gameObject.SetActive(HasFish());
    }
}
