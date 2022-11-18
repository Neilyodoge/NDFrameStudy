using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 相当于控制角色模型父级PlayerHandle的旋转来控制角色的旋转
/// </summary>
public class CamRotController : MonoBehaviour
{
    
    public PlayerInput pi;
    public float camRotValue = 0.2f;    // 镜头平滑

    private GameObject playerHandle;
    private GameObject characterCamera;
    private Vector3 cameraRotVelocity;
    void Awake()
    {
        #region debug
#if UNITY_EDITOR
        if (pi == null)
        {
            Debug.LogError("**Anim部分" + this.name + "未设置 PlayerHandle 脚本");
        }
#endif
        #endregion
        playerHandle = transform.parent.gameObject;
        characterCamera = Camera.main.gameObject;
    }

    public void SetCamQuaternion()
    {
        Vector3 camForward = characterCamera.transform.forward;
        camForward.y = 0;
        if (Input.GetKey(pi.KeyUp) && pi.inputEnable)
        {
            playerHandle.transform.forward = Vector3.SmoothDamp(playerHandle.transform.forward, camForward, ref cameraRotVelocity, camRotValue);
        }
    }
}
