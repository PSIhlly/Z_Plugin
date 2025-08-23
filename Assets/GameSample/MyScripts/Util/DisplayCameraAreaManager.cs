using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

public class DisplayCameraAreaManager : Z_MonoManager<DisplayCameraAreaManager>
{
    public Camera displayCamera;
    public List<GameObject> shows=new List<GameObject>();
    public float normalized2scene=>displayCamera.orthographicSize*2;
    public Vector3 pos=> transform.position;
    public override void Init()
    {
        base.Init();
        Hide();
    }
    public void Show()
    {
        Clear();
        gameObject.SetActive(true);
        
    }
    public void Add(GameObject go,Vector3 relaPos)
    {
       go.transform.position = pos+ relaPos;
       shows.Add(go);
    }

    public void Hide()
    {
        Clear();
        gameObject.SetActive(false);
    }
    public void Clear()
    {
        foreach (var go in shows)
        {
            Destroy(go);
        }
        shows.Clear();
    }
}
