using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Gal.GalUI;

namespace Z_Gal.Core
{
    public class InputManager : MonoBehaviour
    {
        public GameManager gameManager;


        public GalUIInputController galUIInputController;
        // Update is called once per frame
        public void Click(Vector3 pos)
        {
              if(galUIInputController.ClickNoAction(pos))
                {
                    gameManager.NextClip();
                }
        }
    }
}
