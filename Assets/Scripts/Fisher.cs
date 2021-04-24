using System;
using System.Collections;
using System.Collections.Generic;
using AnttiStarterKit.Animations;
using UnityEngine;

public class Fisher : MonoBehaviour
{
    public Camera cam;
    public Animator anim;

    private Vector3 movePos;
    private static readonly int Moving = Animator.StringToHash("moving");

    private void Start()
    {
        movePos = transform.position;
    }

    private void Update()
    {
        SetMovePos();
        Move();
    }

    private void Move()
    {
        var t = transform;
        var position = t.position;
        var moving = (movePos - position).magnitude > 0.1f;
        anim.SetBool(Moving, moving);
        
        if (!moving) return;
        position = Vector3.MoveTowards(position, movePos, 10f * Time.deltaTime);
        t.position = position;
        t.localScale = new Vector3(movePos.x > position.x ? 1 : -1, 1, 1);
    }

    private void SetMovePos()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        var t = transform;

        var mp = Input.mousePosition;
        mp.z = 10f;
        var mouseInWorld = cam.ScreenToWorldPoint(mp);

        movePos = mouseInWorld;
    }
}
