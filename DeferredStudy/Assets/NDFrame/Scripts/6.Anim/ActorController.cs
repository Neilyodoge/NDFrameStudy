using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorController : MonoBehaviour
{
    public GameObject model;  // 当前控制的模型
    [HideInInspector] public PlayerInput pi;    // PlayerInput脚本
    public float walkSpeed = 1.0f;
    public float runMultiplier = 2.0f;
    private Animator anim;
    private Rigidbody rigid;
    private Vector3 movingVec; // 跟玩家相关的移动信息，给rigid用

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
        // print(pi.Dup);
        anim.SetFloat("forward", pi.Dmag * ((pi.run) ? 2.0f : 1.0f));   // 因为run就是值为2，所以直接*2就行
        if (pi.Dmag > 0.1f) // 如果没有松手。原因是长度0的向量没法指派给forward向量
        {
            // 将方向 球面插值 到另一方向
            Vector3 targetForward = Vector3.Slerp(model.transform.forward, pi.Dvec, 0.25f);
            // 就修改模型的正方向，这样松手后就不会发生旋转了
            model.transform.forward = targetForward;
        }
        movingVec = pi.Dmag * model.transform.forward * walkSpeed * ((pi.run) ? runMultiplier : 1.0f);  // 计算移动量,后面的是run是两倍速
    }

    // 移动，加速，减速，旋转，都要在FixedUpdate里算
    void FixedUpdate()      // Time.fixedDeltaTime    1/50
    {
        // rigid.position += movingVec * Time.fixedDeltaTime;
        rigid.velocity = new Vector3(movingVec.x, rigid.velocity.y, movingVec.z);   // 两种方法都行
    }
}
