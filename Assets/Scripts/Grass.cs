using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Grass : MonoBehaviour
{
    public Animator anim;
    public Transform sprite;
    
    private static readonly int Disturb = Animator.StringToHash("disturb");

    private void Start()
    {
        var orig = sprite.localScale.x;
        var mir = Random.value < 0.5f ? 1f : -1f;
        var ratio = Random.Range(0.9f, 1.1f) * orig;
        sprite.localScale = new Vector3(mir * ratio, ratio, ratio);
        anim.speed = Random.Range(0.95f, 1.05f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        anim.SetTrigger(Disturb);
    }
}
