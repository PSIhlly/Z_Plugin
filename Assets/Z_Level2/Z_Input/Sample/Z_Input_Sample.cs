using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using Z_Input;

public class Z_Input_Sample : MonoBehaviour, IZ_Listener<InputKeyEvent>
{

    public Transform tr;



    // Update is called once per frame
    void Start()
    {
        this.Register();
    }
    public void OnEvent(InputKeyEvent evt)
    {
        switch (evt.key)
        {
            case KeyCode.W:
                tr.position += Time.deltaTime * Vector3.forward * 2;
                break;
            case KeyCode.S:
                tr.position += Time.deltaTime * Vector3.back * 2;
                break;

            case KeyCode.A:
                tr.position += Time.deltaTime * Vector3.left * 2;
                break;
            case KeyCode.D:
                tr.position += Time.deltaTime * Vector3.right * 2;
                break;
        }
    }
}
