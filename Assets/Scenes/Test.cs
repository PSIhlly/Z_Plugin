using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject go;
    // Update is called once per frame

    void Update()
    {

            go?.SetActive(true);
        if (Input.GetKeyDown(KeyCode.A))
            Destroy(go);
    }
}
