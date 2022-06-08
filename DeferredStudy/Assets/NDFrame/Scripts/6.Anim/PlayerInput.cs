using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    // Variable
    [Header("====== Key Settings =====")]
    public string KeyUp = "w";
    public string KeyDown = "s";
    public string KeyLeft = "a";
    public string KeyRight = "d";

    public string keyA;
    public string keyB;
    public string keyC;
    public string keyD;

    [Header("====== Output Signals =====")]
    public float Dup;
    public float Dright;
    public float DirBlendSpeed = 0.1f;
    public float Dmag;                  // 坐标系中的距离长度
    public Vector3 Dvec;                // 坐标系中的方向向量

    // 1. pressing signal
    public bool run;
    // 2. trigger once signal
    // 3. double trigger

    [Header("====== Others =====")]
    [HideInInspector]public bool inputEnable = true;     // 关掉这个模块

    private float targetDup;            // 平滑前分辨方向的参数
    private float targetDright;
    private float velocityDup;          // 会自动算，无需参数
    private float velocityDright;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        targetDup = (Input.GetKey(KeyUp)? 1.0f:0) - (Input.GetKey(KeyDown)? 1.0f:0);          // ()?():()
        targetDright = (Input.GetKey(KeyRight)? 1.0f:0) - (Input.GetKey(KeyLeft)? 1.0f:0);
        if (inputEnable == false)
        {
            targetDup = 0;
            targetDright = 0;
        }
        // 平滑
        Dup = Mathf.SmoothDamp(Dup, targetDup, ref velocityDup, DirBlendSpeed);
        Dright = Mathf.SmoothDamp(Dright, targetDright, ref velocityDright, DirBlendSpeed);
        Dmag = Mathf.Sqrt((Dup * Dup) + (Dright * Dright));
        Dvec = Dright * transform.right + Dup * transform.forward;

        run = Input.GetKey(keyA);
    }
}
