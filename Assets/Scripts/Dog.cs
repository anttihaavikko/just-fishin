using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnttiStarterKit.Animations;
using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

public class Dog : HasContainer
{
    public Inventory inventory;
    public Animator anim;
    public List<SpriteRenderer> colorSprites;
    public Seeker seeker;
    public LayerMask lakeMask;
    public Transform faceHolder;
    public GameObject fishHolder;
    public SpriteRenderer fish;

    private Vector3 _movePos;
    private Container _bag;
    private bool _waiting;
    private HasContainer _targetContainer;
    private bool _selling;
    private static readonly int Moving = Animator.StringToHash("moving");
    private bool _needsPathFinding;
    private int _pathIndex;
    

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
            FindPath(_targetContainer.transform.position + GetRandomOffset());
            return;
        }

        if (_bag.GetCount() > 0)
        {
            GoSell();
            return;
        }

        FindPath(transform.position + new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0));
    }

    private void FindPath(Vector3 target)
    {
        var p = transform.position;
        var dir = target - p;
        _needsPathFinding = Physics2D.Raycast(p, dir, dir.magnitude, lakeMask);
            
        if (_needsPathFinding)
        {
            seeker.StartPath(p, target);
            _pathIndex = 0;
            return;
        }

        _movePos = target;
    }

    public static Vector3 GetRandomOffset(float amount = 1f)
    {
        return new Vector3(Random.Range(-amount, amount), Random.Range(-amount, amount), 0);
    }

    private void GoSell()
    {
        _waiting = false;
        FindPath(inventory.storeSpot.position + GetRandomOffset(0.2f));
        _selling = true;
    }

    private void Move()
    {
        var curPath = seeker.GetCurrentPath();
        
        if (inventory.fisher.shop.IsOpen)
        {
            anim.SetBool(Moving, false);
            return;
        }
        
        if (_needsPathFinding)
        {
            if (!seeker.IsDone() || curPath == null)
            {
                anim.SetBool(Moving, false);
                return;
            }
        
            _movePos = curPath.vectorPath[_pathIndex];
        }
        
        var t = transform;
        var position = t.position;
        var moving = (_movePos - position).magnitude > 0.6f;
        
        if (_needsPathFinding && !moving && _pathIndex < curPath.vectorPath.Count - 1)
        {
            _pathIndex++;
            return;
        }

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
                var f = _targetContainer.GetFish();
                if (f != null)
                {
                    fishHolder.SetActive(true);
                    _bag.Add((Fish)f);   
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
        // Debug.Log("Sold " + _bag.GetCount() + " fish.");
        inventory.fisher.shop.SellAll(_bag);
        _selling = false;
        Invoke(nameof(GetTarget), GetDelay());
        fishHolder.SetActive(false);
    }

    private static float GetDelay()
    {
        return Random.Range(1f, 2f);
    }

    private void TurnTowards(Vector3 position)
    {
        var t = transform;
        var mirrored = t.position.x < position.x;
        t.localScale = new Vector3(mirrored ? 1 : -1, 1, 1);
        faceHolder.localScale = t.localScale;
    }

    public override Fish? GetFish()
    {
        return _bag.GetFish();
    }
}
