using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("碰了"+other.gameObject.name);
    }
}
