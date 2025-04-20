using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTest : MonoBehaviour
{
    public CapsuleCollider capcol;
    private Vector3 point1;
    private Vector3 point2;
    private float radius;
    // Start is called before the first frame update
    void Start()
    {
        radius = capcol.radius; // 胶囊体组件上的radius,好像是半径
    }

    // Update is called once per frame
    void Update()
    {
        point1 = transform.position + transform.up * radius;
        point2 = transform.position + transform.up * capcol.height - transform.up * radius;
        Collider[] outputCols = Physics.OverlapCapsule(point1, point2, radius, LayerMask.GetMask("Ground"));
        if (outputCols.Length != 0)
        {
            foreach (var col in outputCols)
            {    // debug都跟谁碰撞了
                print("当前碰撞的collision:" + col.name);
            }
            //SendMessageUpwards("IsGround");
            print("is ground");
        }
    }
}
