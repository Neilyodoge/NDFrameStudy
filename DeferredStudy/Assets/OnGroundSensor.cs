using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnGroundSensor : MonoBehaviour
{
    public CapsuleCollider capcol;
    public float offset = 0.1f;//下沉碰撞偏移量

    private Vector3 point1;
    private Vector3 point2;
    private float radius;

    void Awake()
    {
        radius = capcol.radius - 0.05f; // 胶囊体组件上的半径,做减法，是把整体所小往下移，便于检测
        if (capcol == null)
        {
            Debug.LogError(this.name + "上没有指定胶囊体");
        }
    }

    void FixedUpdate()
    {
        //point1 = transform.position - transform.up * 0.5f * capcol.height + capcol.center;
        //point2 = transform.position + transform.up * (capcol.height-offset) - transform.up * radius + capcol.center;

        point1 = transform.position - transform.up * 0.5f * (capcol.height + offset) + capcol.center;
        point2 = transform.position + transform.up * 0.5f * (capcol.height + offset) + capcol.center;
        Debug.DrawLine(point1, point2,Color.red);
        Debug.DrawLine(point1 + new Vector3(0.3f, 0, 0), point2 + new Vector3(0.3f, 0, 0), Color.red);
        Debug.DrawLine(point1 + new Vector3(-0.3f, 0, 0), point2 + new Vector3(-0.3f, 0, 0), Color.red);
        Debug.DrawLine(point1 + new Vector3(0, 0, -0.3f), point2 + new Vector3(0, 0, -0.3f), Color.red);
        Debug.DrawLine(point1 + new Vector3(0, 0, 0.3f), point2 + new Vector3(0, 0, 0.3f), Color.red);

        Collider[] outputCols = Physics.OverlapCapsule(point1, point2, radius, LayerMask.GetMask("Ground"));
        if (outputCols.Length != 0)
        {
            foreach (var col in outputCols)
            {    // debug都跟谁碰撞了
                print("当前碰撞的collision:" + col.name);
            }
            SendMessageUpwards("IsGround");
        }
        else {
            SendMessageUpwards("IsNotGround");
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
