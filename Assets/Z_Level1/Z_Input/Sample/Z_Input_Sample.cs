using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Input;

public class Z_Input_Sample : MonoBehaviour
{

    public Transform tr;
    // Update is called once per frame
    void Start()
    {
        var config = new InputConfig();

        config.onButtonW = () =>
        {
            tr.position += Time.deltaTime * Vector3.forward * 2;
        };
        config.onButtonS = () =>
        {
            tr.position += Time.deltaTime * Vector3.back * 2;
        };

        config.onButtonA = () =>
        {
            tr.position += Time.deltaTime * Vector3.left * 2;
        };
        config.onButtonD = () =>
        {
            tr.position += Time.deltaTime * Vector3.right * 2;
        };
        InputManager.instance.Register(config);
    }
}
