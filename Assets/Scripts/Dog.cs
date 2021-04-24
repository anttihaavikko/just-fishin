using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Dog : MonoBehaviour
{
    public Inventory inventory;

    private Vector3 _movePos;
    private Container _bag;
    private bool _waiting;
    private HasContainer _targetContainer;
    private bool _selling;

    private void Start()
    {
        _bag = new Container();
        _movePos = transform.position;
        // Invoke(nameof(GetTarget), 1f);
    }

    private void Update()
    {
        Move();
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
            _movePos = _targetContainer.transform.position;
            return;
        }

        if (_bag.GetCount() > 0)
        {
            GoSell();
            return;
        }

        _movePos = transform.position + new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
    }

    private void GoSell()
    {
        _movePos = inventory.storeSpot.position;
        _selling = true;
    }

    private void Move()
    {
        var t = transform;
        var position = t.position;
        var moving = (_movePos - position).magnitude > 0.1f;
        
        // anim.SetBool(Moving, moving);

        if (!moving && !_waiting)
        {
            if (_selling)
            {
                Sell();
                return;
            }
            
            if (_targetContainer != null && (_targetContainer.transform.position - position).magnitude < 0.1f)
            {
                var fish = _targetContainer.GetFish();
                if (fish != null)
                {
                    _bag.Add((Fish)fish);   
                }
            }
            Invoke(nameof(GetTarget), 1f);
            _waiting = true;
            return;
        }
        
        position = Vector3.MoveTowards(position, _movePos, 10f * Time.deltaTime);
        t.position = position;
        TurnTowards(_movePos);
    }

    private void Sell()
    {
        Debug.Log("Sold " + _bag.GetCount() + " fish.");
        _bag.Clear();
        _selling = false;
        Invoke(nameof(GetTarget), 1f);
    }

    private void TurnTowards(Vector3 position)
    {
        var t = transform;
        t.localScale = new Vector3(t.position.x < position.x ? 1 : -1, 1, 1);
    }
}
