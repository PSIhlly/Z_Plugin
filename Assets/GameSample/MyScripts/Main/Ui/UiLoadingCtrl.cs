using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.Loading
{
    public partial class UiLoadingCtrl :
        IZ_Listener<LoadingEvent>
    {
       public override void OnCreate()
        {
            this.Register<LoadingEvent>();
        }

        public void OnEvent(LoadingEvent evt)
        {
            Close();
        }

        public override void OnShow()
        {
            Refresh();
        }
        public void Refresh()
        {
          
        }

       

    }
   
}
