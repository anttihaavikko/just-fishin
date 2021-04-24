using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Dog : HasContainer
{
    public Inventory inventory;
    public Animator anim;
    public List<SpriteRenderer> colorSprites;

    private Vector3 _movePos;
    private Container _bag;
    private bool _waiting;
    private HasContainer _targetContainer;
    private bool _selling;
    private static readonly int Moving = Animator.StringToHash("moving");

    private void Start()
    {
        _bag = new Container(1);
        _movePos = transform.position;
    }

    private void Update()
    {
        Move();
    }

    public void SetColor(Color color)
    {
        colorSprites.ForEach(s => s.color = color);
    }

    private void GetTarget()
    {
        if (_bag.IsFull())
        {
            GoSell();
            return;
        }
        
        _waiting = false;
        if (!inventory || !inventory.fisher) return;

        var points = new List<HasContainer>(inventory.traps.Where(t => t.HasFish()));

        if (inventory.fisher.HasFish())
        {
            points.Add(inventory.fisher);
        }

        _targetContainer = points.OrderBy(p => (transform.position - p.transform.position).magnitude).FirstOrDefault();
        SetMoveTarget();
    }

    private void SetMoveTarget()
    {
        if (_targetContainer)
        {
            _movePos = _targetContainer.transform.position + GetRandomOffset();
            return;
        }

        if (_bag.GetCount() > 0)
        {
            GoSell();
            return;
        }

        _movePos = transform.position + new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
    }

    public static Vector3 GetRandomOffset(float amount = 1f)
    {
        return new Vector3(Random.Range(-amount, amount), Random.Range(-amount, amount), 0);
    }

    private void GoSell()
    {
        _waiting = false;
        _movePos = inventory.storeSpot.position + GetRandomOffset(0.2f);
        _selling = true;
    }

    private void Move()
    {
        var t = transform;
        var position = t.position;
        var moving = (_movePos - position).magnitude > 0.6f;
        
        anim.SetBool(Moving, moving);

        if (!moving && !_waiting)
        {
            if (_selling)
            {
                Sell();
                return;
            }
            
            if (_targetContainer != null && (_targetContainer.transform.position - position).magnitude < 1.5f)
            {
                var fish = _targetContainer.GetFish();
                if (fish != null)
                {
                    _bag.Add((Fish)fish);   
                }
            }
            Invoke(nameof(GetTarget), GetDelay());
            _waiting = true;
            return;
        }

        if (!moving) return;
        position = Vector3.MoveTowards(position, _movePos, 4f * Time.deltaTime);
        t.position = position;
        TurnTowards(_movePos);
    }

    private void Sell()
    {
        Debug.Log("Sold " + _bag.GetCount() + " fish.");
        inventory.fisher.shop.SellAll(_bag);
        _selling = false;
        Invoke(nameof(GetTarget), GetDelay());
    }

    private static float GetDelay()
    {
        return Random.Range(1f, 2f);
    }

    private void TurnTowards(Vector3 position)
    {
        var t = transform;
        t.localScale = new Vector3(t.position.x < position.x ? 1 : -1, 1, 1);
    }

    public override Fish? GetFish()
    {
        return _bag.GetFish();
    }
}
