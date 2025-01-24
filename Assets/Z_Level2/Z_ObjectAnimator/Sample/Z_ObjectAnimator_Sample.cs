using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ObjectAnimator.Core;
using Z_ObjectAnimator.Base;
public class Z_ObjectAnimator_Sample : MonoBehaviour
{
    public Transform tar;
    private Controller con;
    void Start()
    {
        //need to set fps
        Application.targetFrameRate = 60;

        var act=new Action(this);
        act.Add(new LocalPositionSetEvent(tar, Vector3.zero, Vector3.down * 5, 2));
        act.Add(new WaitSecondEvent(3));
        act.Add(new LocalPositionSetEvent(tar, Vector3.down * 5, Vector3.zero, 2));
        var acts = new List<Action>() { act };
        var group = new ActionGroup(acts, StopType.BackToStart,true);
        var groups = new List<ActionGroup>() { group };
        con=new Controller(groups);
        con.Play(0);
    }
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            con.Pause(0);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            con.Play(0);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            con.Stop(0);
        }
    }
}
