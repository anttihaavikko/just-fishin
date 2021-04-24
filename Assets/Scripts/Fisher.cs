using System;
using System.Collections;
using AnttiStarterKit.Animations;
using UnityEngine;
using Random = UnityEngine.Random;

public class Fisher : MonoBehaviour
{
    public Camera cam;
    public Animator anim;
    public LayerMask lakeMask;
    
    public Shop shop;
    public Inventory inventory;

    private Vector3 _movePos;
    private bool _isFishing;

    private Container _bag;
    
    private static readonly int Moving = Animator.StringToHash("moving");
    private static readonly int Fishing = Animator.StringToHash("fishing");

    private void Start()
    {
        _bag = new Container();
        _movePos = transform.position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            shop.Toggle(_bag);
        }
        
        if (shop.IsOpen) return;
        
        CheckClick();
        Move();
    }

    private void Move()
    {
        var t = transform;
        var position = t.position;
        var moving = (_movePos - position).magnitude > 0.1f;
        
        anim.SetBool(Moving, moving);

        if (!moving) return;
        position = Vector3.MoveTowards(position, _movePos, 10f * Time.deltaTime);
        t.position = position;
        TurnTowards(_movePos);
        anim.SetBool(Fishing, false);
    }

    private void TurnTowards(Vector3 position)
    {
        var t = transform;
        t.localScale = new Vector3(t.position.x < position.x ? 1 : -1, 1, 1);
    }

    public Fish GetFish()
    {
        var fish = new[]
        {
            new Fish("Bass", 1),
            new Fish("Trout", 2),
            new Fish("Salmon", 3)
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

            _bag.Add(GetFish());
            
            // Debug.Log("Bag has " + _bag.GetCount());
            // bag.GetContents().ForEach(fish => Debug.Log(fish.name));

            return;
        }

        var mp = Input.mousePosition;
        mp.z = 10f;
        var mouseInWorld = cam.ScreenToWorldPoint(mp);

        if (!CanStartFishing(mouseInWorld))
        {
            _movePos = mouseInWorld;   
        }
    }

    private bool CanStartFishing(Vector3 pos)
    {
        var hit = Physics2D.OverlapCircle(pos, 0.1f, lakeMask);
        if (!hit || (transform.position - pos).magnitude > 5f) return false;
        
        anim.SetBool(Moving, false);
        anim.SetBool(Fishing, true);
        
        _movePos = transform.position;
        _isFishing = true;
        
        TurnTowards(pos);

        return true;
    }
}
