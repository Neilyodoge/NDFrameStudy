using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorController : MonoBehaviour
{
    public GameObject model;
    public PlayerInput pi;
    [SerializeField]
    private Animator anim;

    void Awake()        // Awake里面赋值好比较方面且符合unity gameplay框架设计原则
    {
        pi = GetComponent<PlayerInput>();
        anim = model.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // print(pi.Dup);
        anim.SetFloat("forward",pi.Dup);
    }
}
