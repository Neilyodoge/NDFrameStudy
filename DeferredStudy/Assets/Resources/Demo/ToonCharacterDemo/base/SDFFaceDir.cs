using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDFFaceDir : MonoBehaviour
{
    public GameObject faceDirObj;
    public Material faceMat;
    private Vector3 faceUpDir;
    private Vector3 faceFrontDir;
    private Vector3 faceRightDir;
    void Start()
    {
        #region Debug
#if UNITY_EDITOR
        if (faceDirObj == null)
        {
            Debug.LogError("Scene中的" + this.name + "缺少 faceDirObj");
        }
        if (faceMat == null)
        {
            Debug.LogError("Scene中的" + this.name + "缺少 faceMat");
        }
#endif
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        faceUpDir = faceDirObj.transform.up;
        faceFrontDir = faceDirObj.transform.forward;
        faceRightDir = faceDirObj.transform.right;
        
        faceMat.SetVector("_FaceUpDir", Vector3.Normalize(faceUpDir));
        faceMat.SetVector("_FaceFrontDir", Vector3.Normalize(faceFrontDir));
        faceMat.SetVector("_FaceRightDir", Vector3.Normalize(faceRightDir));

        #region Debug
#if UNITY_EDITOR
        // GREEN up Dir
        Debug.DrawRay(faceDirObj.transform.position, faceUpDir, Color.green);
        // BLUE forward Dir
        Debug.DrawRay(faceDirObj.transform.position, faceFrontDir, Color.blue);
        // RED right Dir
        Debug.DrawRay(faceDirObj.transform.position, faceRightDir, Color.red);
        // sun Dir
        // Debug.DrawRay(RenderSettings.sun.transform.position, RenderSettings.sun.transform.forward, Color.green);
#endif
        #endregion
    }
}