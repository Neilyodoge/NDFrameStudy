using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HairUIController : MonoBehaviour
{
    public GameObject cameraFather;
    public Slider cameraRotate;
    public Slider cameraRotateSpeed;
    public Toggle isNear;
    /// <summary>
    /// 剔除强度
    /// </summary>
    public Slider CullIntensity;
    /// <summary>
    /// 主要高光强度
    /// </summary>
    public Slider MainSpecularIntensity;
    /// <summary>
    /// 主要高光范围
    /// </summary>
    public Slider MainSpecularRadius;
    /// <summary>
    /// 次要高光强度
    /// </summary>
    public Slider SecondSpecularIntensity;
    /// <summary>
    /// 次要高光范围
    /// </summary>
    public Slider SecondSpecularRadius;
    public GameObject Hair;
    Material hairMat;
    float cameraZ;
    // Start is called before the first frame update
    void Start()
    {
        hairMat = Hair.GetComponent<MeshRenderer>().materials[0];
        cameraRotate.value = 0f;
        cameraRotateSpeed.value = 0f;
        cameraZ = Camera.main.transform.localPosition.z;
        isNear.onValueChanged.AddListener(IsNear);
        CullIntensity.value = 0.7f;
        MainSpecularIntensity.value =1f;
        MainSpecularRadius.value = 5f;
        SecondSpecularIntensity.value = 1f;
        SecondSpecularRadius.value = 0.3f;

    }

    // Update is called once per frame
    void Update()
    {
        CameraRotate();
        ChangeMat();
    }

    void CameraRotate()
    {
        float rotateValue = Mathf.Lerp(-360, 360, cameraRotate.value);
        cameraFather.transform.rotation = Quaternion.Euler(new Vector3(0, rotateValue + cameraRotateSpeed.value * Time.time, 0));

    }

    void IsNear(bool isOn)
    {
        float cameraPosX = Camera.main.transform.localPosition.x;
        float cameraPosY = Camera.main.transform.localPosition.y;
        if (isOn)
        {
            Camera.main.transform.localPosition = new Vector3(cameraPosX, cameraPosY, cameraZ + 2f);
        }
        else
        {
            Camera.main.transform.localPosition = new Vector3(cameraPosX, cameraPosY, cameraZ);
        }
    }

    void ChangeMat()
    {
        hairMat.SetFloat("_CutOff", CullIntensity.value);
        hairMat.SetFloat("_PrimaryStrength", MainSpecularIntensity.value);
        hairMat.SetFloat("_PrimaryRadius", MainSpecularRadius.value);
        hairMat.SetFloat("_SecondaryStrength", SecondSpecularIntensity.value);
        hairMat.SetFloat("_SecondaryRadius", SecondSpecularRadius.value);
    }
}
