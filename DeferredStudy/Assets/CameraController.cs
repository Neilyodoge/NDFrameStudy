using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerInput pi;
    public float horizontalSpeed = 50.0f;
    public float verticalSpeed = 50.0f;
    public float cameraDampValue = 0.2f;    // 镜头平滑
    public float cameraZOffset = 2f;        // 镜头偏移

    private GameObject playerHandle;
    private GameObject cameraHandle;
    private float tempEulerX; // 解决角度限制没法有负值的问题gameCamera
    private GameObject model;
    private GameObject characterCamera;

    private Vector3 cameraDampVelocity;

    void Awake()
    {
        #region debug
#if UNITY_EDITOR
        if (pi == null)
        {
            Debug.LogError("AnimPart +" + this.name + "PlayerHandle没有放入！！！");
        }
#endif
        #endregion
        cameraHandle = transform.parent.gameObject;
        playerHandle = cameraHandle.transform.parent.gameObject;
        tempEulerX = cameraHandle.transform.eulerAngles.x;          // 拿到 cameraHandle 的默认角度
        model = playerHandle.GetComponent<ActorController>().model; // 拿到 模型
        characterCamera = Camera.main.gameObject;
    }

    void Update()
    {
        var angle = Vector3.Angle(transform.forward, characterCamera.transform.position - gameObject.transform.position) / 10.0f;//差角的1/10
        var lookRo = Quaternion.LookRotation(characterCamera.transform.position - gameObject.transform.position, Vector3.up);
        var sAngle = horizontalSpeed * Time.deltaTime;
        //gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, lookRo, sAngle < angle ? sAngle : angle);
        //这里记得计算差角，并且永远不要用1帧直接完成全部旋转，防止短距移动的跳镜（比如人物上楼梯的时候）
        //

        Vector3 tempModelEuler = model.transform.eulerAngles;

        // characterCamera 水平方向旋转
        playerHandle.transform.Rotate(Vector3.up, pi.Jright * horizontalSpeed * Time.deltaTime);
        // characterCamera 垂直方向旋转。这里的 -verticalSpeed是为了按上的时候是抬头                               
        tempEulerX -= pi.Jup * -verticalSpeed * Time.deltaTime;
        tempEulerX = Mathf.Clamp(tempEulerX, -40, 30);                            // 角度限制范围
        cameraHandle.transform.localEulerAngles = new Vector3(tempEulerX, 0, 0);  // set 回去。一定是local才管用

        model.transform.eulerAngles = tempModelEuler;  // 设置回去，这样模型的就不会跟着父级转了

        transform.localPosition = new Vector3(0, 0, -cameraZOffset); // 手动调整 cameraPos 位置或者在这里做offset，我选择后者
        // characterCamera.transform.position = Vector3.Lerp(characterCamera.transform.position,transform.position,0.2f);    // 插值切换
        characterCamera.transform.position = Vector3.SmoothDamp(characterCamera.transform.position, transform.position, ref cameraDampVelocity, cameraDampValue);
        //characterCamera.transform.eulerAngles = transform.eulerAngles;
        characterCamera.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, lookRo, sAngle < angle ? sAngle : angle);
    }
}
