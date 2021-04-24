using System;
using System.Collections;
using System.Collections.Generic;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Managers;
using Pathfinding;
using UnityEngine;
using AnttiStarterKit.Extensions;
using Random = UnityEngine.Random;

public class Fisher : HasContainer
{
    public Camera cam;
    public Animator anim;
    public LayerMask lakeMask, trapMask;
    public SpriteRenderer shirtSprite, hatSprite, rodSprite;
    public List<Sprite> hatSprites;
    public Seeker seeker;
    public Transform faceHolder;
    public Transform marker;
    public Transform bagSprite;
    public GameObject splash;
    public SpriteRenderer heldTrapSprite;
    public Trap trapPrefab;
    
    public Shop shop;
    public Inventory inventory;

    private Vector3 _movePos;
    private bool _isFishing;

    private Container _bag;

    private int _pathIndex;
    private Vector3 _markerRest;
    private Transform _markerParent;

    private bool _needsPathFinding;
    private bool holding;
    
    private static readonly int Moving = Animator.StringToHash("moving");
    private static readonly int Fishing = Animator.StringToHash("fishing");
    private static readonly int Holding = Animator.StringToHash("holding");

    private void Start()
    {
        _bag = new Container();
        _movePos = transform.position;
        _markerRest = marker.localPosition;
        _markerParent = marker.parent;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            shop.Toggle(_bag);
        }
        
        Move();
        
        if (shop.IsOpen) return;
        CheckClick();
    }

    private void Move()
    {
        var curPath = seeker.GetCurrentPath();
        
        if (shop.IsOpen)
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
        var moving = (_movePos - position).magnitude > 0.1f;

        if (_needsPathFinding && !moving && _pathIndex < curPath.vectorPath.Count - 1)
        {
            _pathIndex++;
            return;
        }
        
        anim.SetBool(Moving, moving);

        if (!moving) return;
        position = Vector3.MoveTowards(position, _movePos, 7f * Time.deltaTime);
        t.position = position;
        TurnTowards(_movePos);
        anim.SetBool(Fishing, false);
    }

    private void TurnTowards(Vector3 position)
    {
        var t = transform;
        var mirrored = t.position.x < position.x;
        t.localScale = new Vector3(mirrored ? 1 : -1, 1, 1);
        faceHolder.localScale = t.localScale;
    }

    public static Fish GetRandomFish()
    {
        var fish = new[]
        {
            new Fish("Bass", "Lorem bass ipsum", 1),
            new Fish("Trout", "Lorem trout ipsum", 2),
            new Fish("Salmon", "Lorem salmon ipsum", 3)
        };

        return fish[Random.Range(0, fish.Length)];
    }

    private void CheckClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (_isFishing)
        {
            _isFishing = false;
            anim.SetBool(Fishing, false);

            EffectManager.Instance.AddEffect(1, marker.transform.position);
            Invoke(nameof(ResetMarker), 0.1f);

            _bag.Add(GetRandomFish());

            return;
        }

        var mp = Input.mousePosition;
        mp.z = 10f;
        var mouseInWorld = cam.ScreenToWorldPoint(mp);

        if (!CanStartFishing(mouseInWorld))
        {
            var p = transform.position;
            var dir = mouseInWorld - p;
            _needsPathFinding = Physics2D.Raycast(p, dir, dir.magnitude, lakeMask);
            
            if (_needsPathFinding)
            {
                seeker.StartPath(p, mouseInWorld);
                _pathIndex = 0;
                return;
            }

            _movePos = mouseInWorld;
        }
    }

    private void ResetMarker()
    {
        marker.parent = _markerParent;
        Tweener.Instance.MoveLocalTo(marker, _markerRest, 0.2f, 0, TweenEasings.QuadraticEaseIn);
        Tweener.Instance.RotateTo(marker, Quaternion.identity, 0.2f, 0f, TweenEasings.QuadraticEaseIn);
        splash.SetActive(false);
        var p = marker.transform.position;
        EffectManager.Instance.AddEffect(0, p);
    }

    private bool PickingTrap(Vector3 pos)
    {
        var hit = Physics2D.OverlapCircle(pos, 0.1f, trapMask);
        var picking = hit && (transform.position - pos).magnitude < 2f;
        if (picking)
        {
            var trap = hit.GetComponent<Trap>();
            inventory.traps.Remove(trap);
            Destroy(trap.gameObject);
        }
        return picking;
    }

    private bool CanStartFishing(Vector3 pos)
    {
        if (PickingTrap(pos))
        {
            Hold(true);
            return false;
        }
        var hit = Physics2D.OverlapCircle(pos, 0.1f, lakeMask);
        var hitTrap = Physics2D.OverlapCircle(pos, 0.1f, trapMask);
        if (!hit || (transform.position - pos).magnitude > 5f || hitTrap) return false;

        if (holding)
        {
            var t = transform;
            var p = t.position;
            var dir = pos - p;
            var edge = Physics2D.Raycast(p, dir, 10f, lakeMask);
            if (edge)
            {
                Hold(false);
                var trap = Instantiate(trapPrefab, edge.point, Quaternion.identity);
                inventory.traps.Add(trap);
            }
            return false;
        }

        splash.transform.parent = null;
        splash.transform.position = pos + Vector3.down * 0.1f;
        splash.SetActive(true);

        this.StartCoroutine(() => EffectManager.Instance.AddEffect(0, pos), 0.2f);

        marker.parent = null;
        Tweener.Instance.MoveTo(marker, pos, 0.2f, 0, TweenEasings.QuadraticEaseIn);
        Tweener.Instance.RotateTo(marker, Quaternion.identity, 0.2f, 0f, TweenEasings.QuadraticEaseIn);
        
        anim.SetBool(Moving, false);
        anim.SetBool(Fishing, true);
        
        _movePos = transform.position;
        _isFishing = true;
        
        TurnTowards(pos);

        return true;
    }

    public void Equip(EquipItem item)
    {
        switch (item.Slot)
        {
            case EquipSlot.Shirt:
                shirtSprite.color = item.Color;
                break;
            case EquipSlot.Hat:
                hatSprite.color = item.Color;
                hatSprite.sprite = hatSprites[item.SpriteIndex];
                break;
            case EquipSlot.Hook:
                break;
            case EquipSlot.Rod:
                break;
        }
    }

    public void ScaleBag()
    {
        var level = inventory.GetLevel(Upgrade.BagSpace);
        var size = Mathf.Pow(1.2f, level);
        Tweener.Instance.ScaleTo(bagSprite, Vector3.one * size, 0.2f, 0f, TweenEasings.BounceEaseOut);
        _bag.SetMaxSize(5 + 5 * level);
    }

    public bool HasFish()
    {
        return _bag.GetCount() > 0;
    }

    public override Fish? GetFish()
    {
        return _bag.GetFish();
    }

    private void Hold(bool state)
    {
        holding = state;
        heldTrapSprite.gameObject.SetActive(state);
        anim.SetBool(Holding, state);
    }
}