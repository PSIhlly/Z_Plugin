using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

public class InputManager : Z_MonoManager<InputManager>
{
    public bool enabled=true;
    public GameObject tarGo;
    public float speed;
    public override void Init()
    {
        (int, int) gg = (2, 3);

    }

    public void Update()
    {
        if (!enabled)
            return;
#if UNITY_ANDROID && !UNITY_EDITOR

#else
        if (Input.GetKey(KeyCode.W))
        {
            tarGo.transform.Translate(Vector3.forward * speed*Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            tarGo.transform.Translate(-Vector3.forward * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            tarGo.transform.Translate(Vector3.left * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            tarGo.transform.Translate(-Vector3.left * speed * Time.deltaTime);
        }
#endif

    }

}
