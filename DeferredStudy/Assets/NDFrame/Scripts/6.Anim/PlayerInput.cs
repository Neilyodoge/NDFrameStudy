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
    public bool jump;
    private bool lastJump;
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

        Vector2 tempDAxis = SquareToCircle(new Vector2(Dright, Dup));   // 坐标轴转成圆形的，解决斜向移动问题
        float DrightCircle = tempDAxis.x;
        float DupCircle = tempDAxis.y;

        Dmag = Mathf.Sqrt((DupCircle * DupCircle) + (DrightCircle * DrightCircle));
        Dvec = DrightCircle * transform.right + DupCircle * transform.forward;

        run = Input.GetKey(keyA);
        // jump相关
        bool newJamp = Input.GetKey(keyB);
        //jump = newJamp;
        if (newJamp != lastJump && newJamp == true) {
            jump = true;
        }
        else {
            jump = false;
        }
        lastJump = newJamp;
    }

    private Vector2 SquareToCircle(Vector2 input) {
        Vector2 output = Vector2.zero;
        output.x = input.x * Mathf.Sqrt(1-(input.y*input.y)/2.0f);
        output.y = input.y * Mathf.Sqrt(1-(input.x*input.x)/2.0f);
        return output;
    }
}
