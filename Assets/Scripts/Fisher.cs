using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Managers;
using Pathfinding;
using UnityEngine;
using AnttiStarterKit.Extensions;
using TMPro;
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
    public SpriteRenderer fishSprite;
    public List<Fish> fishList;

    public Shop shop;
    public Inventory inventory;
    
    public List<EquipItem> Gear { get; private set; }

    private Vector3 _movePos;
    private bool _isFishing;

    private Container _bag;

    private int _pathIndex;
    private Vector3 _markerRest;
    private Transform _markerParent;

    private bool _needsPathFinding;
    private bool _holding;
    private bool _biteActive;
    private bool _fishingDone = true;
    
    private Coroutine _fishingCoroutine;
    private Fish _currentFish;
    private EquipItem _heldTrap;

    private static readonly int Moving = Animator.StringToHash("moving");
    private static readonly int Fishing = Animator.StringToHash("fishing");
    private static readonly int Holding = Animator.StringToHash("holding");
    private static readonly int Pull = Animator.StringToHash("pull");

    private void Start()
    {
        Gear = new List<EquipItem>();
        _bag = new Container();
        _movePos = transform.position;
        _markerRest = marker.localPosition;
        _markerParent = marker.parent;

        _bag.onUpdate = UpdateCountText;
        
        AddStarterGear();
    }

    private void AddStarterGear()
    {
        Gear.Add(new EquipItem
        {
            Name = "Twig",
            Description = "Barely functions as a fishing rod",
            Slot = EquipSlot.Rod,
            Equipped = true,
            Color = shop.equipColors[6].color
        });

        Gear.Add(new EquipItem
        {
            Name = "Rusty Hook",
            Description = "Might actually deter fish from biting",
            Slot = EquipSlot.Hook,
            Equipped = true
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.I) || 
            Input.GetKeyDown(KeyCode.E) ||
            Input.GetMouseButtonDown(1))
        {
            var close = (transform.position - inventory.storeSpot.position).magnitude < 1f;
            shop.Toggle(_bag, close);
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
        var scl = new Vector3(mirrored ? 1 : -1, 1, 1);
        t.localScale = scl;
        faceHolder.localScale = scl;
        countText.transform.localScale = scl;
        countText.alignment = mirrored ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
    }

    public Fish GetRandomFish()
    {
        return fishList[Random.Range(0, fishList.Count)];
    }

    private void CheckClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (_isFishing)
        {
            StopCoroutine(_fishingCoroutine);
            
            _isFishing = false;
            _fishingDone = false;

            var pos = marker.transform.position;
            EffectManager.Instance.AddEffect(1, pos);
            SplashSound(pos);
            
            Invoke(nameof(ResetMarker), 0.1f);

            this.StartCoroutine(() => _fishingDone = true, _biteActive ? 1.2f : 0.1f);
            this.StartCoroutine(() => anim.SetBool(Fishing, false), _biteActive ? 0.2f : 0f);

            if (_biteActive)
            {
                anim.SetTrigger(Pull);
                _bag.Add(_currentFish);
            }

            _biteActive = false;

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
        var p = marker.transform.position;
        EffectManager.Instance.AddEffect(0, p);
        this.StartCoroutine(() => splash.SetActive(false), 0.2f);
    }

    private bool PickingTrap(Vector3 pos)
    {
        var hit = Physics2D.OverlapCircle(pos, 0.1f, trapMask);
        var picking = hit && (transform.position - pos).magnitude < 2f;
        if (picking)
        {
            var trap = hit.GetComponent<Trap>();
            var all = trap.GetAllFish();
            all.ForEach(f => _bag.Add(f));
            inventory.traps.Remove(trap);
            Destroy(trap.gameObject);
        }
        return picking;
    }
    
    // ReSharper disable once FunctionRecursiveOnAllPaths
    private IEnumerator FishingCoroutine()
    {
        _currentFish = GetRandomFish();
        fishSprite.color = _currentFish.color;
        fishSprite.transform.localScale = _currentFish.size * Vector3.one;
        var waitTime = Random.Range(1f, 10f);
        yield return new WaitForSeconds(waitTime);
        _biteActive = true;
        var pos = marker.position;
        Bite();
        var delay = Random.Range(0.5f, 1f);
        yield return new WaitForSeconds(delay / _currentFish.speed);
        ResetBite(pos);
        _biteActive = false;
        
        _fishingCoroutine = StartCoroutine(FishingCoroutine());
    }

    private void Bite()
    {
        var pos = marker.position;
        Tweener.Instance.MoveFor(marker, Vector3.down * 0.2f, 0.2f, 0, TweenEasings.QuadraticEaseIn);
        EffectManager.Instance.AddEffect(0, pos);
        EffectManager.Instance.AddEffect(1, pos + Vector3.down * 0.2f);
        SplashSound(pos);
    }

    public static void SplashSound(Vector3 pos)
    {
        AudioManager.Instance.PlayEffectAt(Random.Range(0, 4), pos, 6f);
    }

    private void ResetBite(Vector3 pos)
    {
        SplashSound(pos);
        Tweener.Instance.MoveTo(marker, pos, 0.2f, 0, TweenEasings.QuadraticEaseIn);
    }

    private bool CanStartFishing(Vector3 pos)
    {
        if (!_fishingDone) return false;
        
        if (PickingTrap(pos))
        {
            Hold(true);
            return false;
        }
        var hit = Physics2D.OverlapCircle(pos, 0.1f, lakeMask);
        var hitTrap = Physics2D.OverlapCircle(pos, 0.1f, trapMask);
        if (!hit || (transform.position - pos).magnitude > 5f || hitTrap) return false;

        if (_holding)
        {
            var t = transform;
            var p = t.position;
            var dir = pos - p;
            var edge = Physics2D.Raycast(p, dir, 10f, lakeMask);
            if (edge)
            {
                Hold(false);
                var trap = Instantiate(trapPrefab, edge.point, Quaternion.identity);
                Gear.Remove(_heldTrap);
                trap.inventory = inventory;
                if (inventory.HasUpgrade(Upgrade.BigTraps))
                {
                    trap.SetMaxSize(3);
                }
                inventory.traps.Add(trap);
            }
            return false;
        }

        splash.transform.parent = null;
        splash.transform.position = pos + Vector3.down * 0.1f;
        splash.SetActive(true);

        this.StartCoroutine(() =>
        {
            SplashSound(pos);
            EffectManager.Instance.AddEffect(0, pos);
        }, 0.2f);

        marker.parent = null;
        Tweener.Instance.MoveTo(marker, pos, 0.2f, 0, TweenEasings.QuadraticEaseIn);
        Tweener.Instance.RotateTo(marker, Quaternion.identity, 0.2f, 0f, TweenEasings.QuadraticEaseIn);
        
        anim.SetBool(Moving, false);
        anim.SetBool(Fishing, true);
        
        _movePos = transform.position;
        _isFishing = true;
        
        TurnTowards(pos);

        _fishingCoroutine = StartCoroutine(FishingCoroutine());

        return true;
    }

    public void Equip(EquipItem item)
    {
        Gear.Where(e => e.Slot == item.Slot).ToList().ForEach(e => e.Equipped = false);
        item.Equipped = true;
        
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
                rodSprite.color = item.Color;
                break;
            case EquipSlot.Trap:
                _heldTrap = item;
                Hold(true);
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
        _holding = state;
        heldTrapSprite.gameObject.SetActive(state);
        anim.SetBool(Holding, state);
    }
}