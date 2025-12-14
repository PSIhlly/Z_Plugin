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
        if (evt.key.Contains(KeyCode.W))
        {
            tr.position += Time.deltaTime * Vector3.forward * 2;
        }
        else if (evt.key.Contains(KeyCode.S))
        {

            tr.position += Time.deltaTime * Vector3.back * 2;
        }
        else if (evt.key.Contains(KeyCode.A))
        {

            tr.position += Time.deltaTime * Vector3.left * 2;
        }
        else if (evt.key.Contains(KeyCode.D))
        {

            tr.position += Time.deltaTime * Vector3.right * 2;
        }


    }
}
