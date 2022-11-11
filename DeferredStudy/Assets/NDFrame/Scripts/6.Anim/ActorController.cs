using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorController : MonoBehaviour
{
    public GameObject model;  // 当前控制的模型
    [HideInInspector] public PlayerInput pi;    // PlayerInput脚本
    public float walkSpeed = 1.0f;
    public float runMultiplier = 2.0f;
    public float rotSpeed = 0.2f;
    public float jumpVelocity = 3f;
    public float rollVelocity = 3f;

    [SerializeField]
    private Animator anim;
    private Rigidbody rigid;
    private Vector3 planarVec; // 跟玩家相关的移动信息，给rigid用
    private Vector3 thrustVec;

    private bool lockPlanar = false;    // 锁死平面移动

    void Awake()        // Awake里面赋值好比较方面且符合unity gameplay框架设计原则
    {
        pi = GetComponent<PlayerInput>();
        anim = model.GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        //------------------配置Debug------------------
        if (pi == null) Debug.LogError(this.name + "缺少 PlayerInput 组件");
        if (anim == null) Debug.LogError(model.name + "缺少 Animator 组件");
        if (rigid == null) Debug.LogError(this.name + "缺少 Rigidbody 组件");
    }

    void Update()           // Time.deltaTime         1/60
    {
        ///*! 这里确实有变化，但并不明显，感觉走合跑应该是用不同值来lerp。0.01会合适，但从idle切walk会滑步
        anim.SetFloat("forward", pi.Dmag * Mathf.Lerp(anim.GetFloat("forward"), ((pi.run) ? runMultiplier : 1.0f), 0.2f));
        if (rigid.velocity.magnitude > 1.0f)    // 刚体速度
        {
            anim.SetTrigger("roll");
        }
        if (pi.jump)
        {   // PlayerInput那边按下了这边才变
            anim.SetTrigger("jump");
        }
        if (pi.Dmag > 0.1f) // 如果没有松手。原因是长度0的向量没法指派给forward向量
        {   // 就修改模型的正方向，这样松手后就不会发生旋转了。并且用Slerp函数进行旋转插值控制转向速度
            model.transform.forward = Vector3.Slerp(model.transform.forward, pi.Dvec, rotSpeed);
        }
        if (lockPlanar == false)
        {
            planarVec = pi.Dmag * model.transform.forward * walkSpeed * ((pi.run) ? runMultiplier : 1.0f);  // 计算移动量,后面的是run是两倍速
        }

    }

    // 移动，加速，减速，旋转，都要在FixedUpdate里算
    void FixedUpdate()      // Time.fixedDeltaTime    1/50
    {
        // rigid.position += planarVec * Time.fixedDeltaTime;
        rigid.velocity = new Vector3(planarVec.x, rigid.velocity.y, planarVec.z) + thrustVec;   // 两种方法都行
        thrustVec = Vector3.zero;
    }

    /// <summary>
    /// Message processing block
    /// </summary>
    public void OnJumpEnter()
    {
        thrustVec = new Vector3(0, jumpVelocity, 0);
        pi.inputEnable = false;
        lockPlanar = true;
    }
    public void IsGround()
    {
        //print("is on ground");
        anim.SetBool("isGround", true);
    }
    public void IsNotGround()
    {
        //print("is not on ground!!!");
        anim.SetBool("isGround", false);
    }
    public void OnGroundEnter()
    {
        pi.inputEnable = true;
        lockPlanar = false;
    }
    public void OnFallEnter()   // 解决掉落没有弧线的
    {
        pi.inputEnable = false;
        lockPlanar = true;
    }
    public void OnRollEnter()
    {
        thrustVec = new Vector3(0, rollVelocity, 0);
        pi.inputEnable = false;
        lockPlanar = true;
    }
    public void OnJabEnter()
    {
        pi.inputEnable = false;
        lockPlanar = true;
    }
    public void OnJabUpdate()
    {
        thrustVec = model.transform.forward * anim.GetFloat("jabVelocity"); // 得用模型的前方向的反方向
    }

}
