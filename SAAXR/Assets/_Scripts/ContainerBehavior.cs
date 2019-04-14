using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerBehavior : MonoBehaviour
{

    private Collider _collider;
    private Animator _anim;

    void Start()
    {
        _collider = GetComponent<Collider>();
        _anim = GetComponent<Animator>();
    }

    public void Open()
    {
        _anim.SetTrigger("Play");
        _collider.enabled = false;
    }
}
