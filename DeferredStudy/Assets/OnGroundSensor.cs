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
        #region debug
#if UNITY_EDITOR
        if (capcol == null)
        {
            Debug.LogError("AnimPart +" + this.name + "上没有指定胶囊体");
        }
#endif
        #endregion
    }

    void FixedUpdate()
    {
        point1 = transform.position - transform.up * 0.5f * (capcol.height + offset) + capcol.center;
        point2 = transform.position + transform.up * 0.5f * (capcol.height + offset) + capcol.center;

        #region debug 方阵，查看地面检测用的
#if UNITY_EDITOR
        Debug.DrawLine(point1, point2, Color.red);
        Debug.DrawLine(point1 + new Vector3(0.2f, 0, 0), point2 + new Vector3(0.2f, 0, 0), Color.red);
        Debug.DrawLine(point1 + new Vector3(-0.2f, 0, 0), point2 + new Vector3(-0.2f, 0, 0), Color.red);
        Debug.DrawLine(point1 + new Vector3(0, 0, -0.2f), point2 + new Vector3(0, 0, -0.2f), Color.red);
        Debug.DrawLine(point1 + new Vector3(0, 0, 0.2f), point2 + new Vector3(0, 0, 0.2f), Color.red);
#endif
        #endregion

        Collider[] outputCols = Physics.OverlapCapsule(point1, point2, radius, LayerMask.GetMask("Ground"));
        if (outputCols.Length != 0)
        {
            //foreach (var col in outputCols)
            //{    // debug都跟谁碰撞了
            //    print("当前碰撞的collision:" + col.name);
            //}
            SendMessageUpwards("IsGround");
        }
        else {
            SendMessageUpwards("IsNotGround");
        }
    }
}
