using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test1 : Singleton<Test1>
{
    public string text = "222";
}
public class Test : MonoBehaviour
{
    void Start()
    {
        Debug.Log(Test1.Instance.text);
    }
}
