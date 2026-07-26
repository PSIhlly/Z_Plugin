using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
using Z_Map;

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
        switch (DynamicGlobalSettings.cameraMode)
        {
            case CameraMode.Overhead:
                displayCamera.transform.localPosition = new Vector3(0, 4, 0);
                displayCamera.transform.eulerAngles = new Vector3(90, 0, 0);
                break;
            case CameraMode.Isometric:
                displayCamera.transform.localPosition = new Vector3(0, 4, -4);
                displayCamera.transform.eulerAngles = new Vector3(45, 0, 0);
                break;
        }
        Clear();
        gameObject.SetActive(true);
        
    }
    public void Add(GameObject go,Vector3 relaPos)
    {
       go.transform.position = pos+ relaPos;
/*       foreach(var per in go.GetComponentsInChildren<PerspectiveKeeper>())
        {
            per.UpdateModel();
        }*/
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
