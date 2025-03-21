using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_Text;
using Z_Ui.Base;

namespace Ui.ModStoryCharacter.ModStoryCharacterArguments
{
    public partial class UiModStoryCharacterArgumentsCtrl
    {
        UiScrViewContainer<UiArgCtrl> con;

        public override void OnCreate()
        {
            con = new UiScrViewContainer<UiArgCtrl>(view.go_arg, view.scr_items);

        }

        public override void OnShow()
        {
            Refresh();
        }
        public override void OnDisable()
        {
        }
        public void Refresh()
        {
            con.Clear();
            foreach (var data in CharacterParamForm.DataByUid.Values)
            {
                if (data.uid > MapBaseForm.autoIdCnt)
                    continue;
                con.Add(new UiArgParam
                {
                    id = data.uid
                });
            }
            con.Add(new UiArgParam
            {
                id = -1
            });
            con.Refresh();

        }


    }





    public partial class UiArgModel
    {
        public int id;
    }
    public partial class UiArgParam
    {
        public int id;
    }
    public partial class UiArgCtrl
    {
        public override void OnCreate()
        {
            view.btn_new.onClick.AddListener(() =>
            {
                
                    int max = 1;
                    foreach (var nm in CharacterParamForm.DataByName.Keys)
                    {
                        var splt = nm.Split("newArg");
                        if (splt.Length > 1)
                        {
                            if (int.TryParse(splt[1], out int v))
                            {
                                max = Mathf.Max(max, v + 1);
                            }
                        }
                    }
                    ModManager.instance.assetCtrl.CreateCharacterArg("newArg" + max);
                    parent.Refresh();
            });

            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteCharacterArg(CharacterParamForm.DataByUid[model.id].name);
                parent.Refresh();
            });
            view.ipt_name.onFinishInput += (s) =>
            {
                CharacterParamForm.DataByUid[model.id].name = s;
                parent.Refresh();
            };

        }
        public override void OnShow()
        {
            model.id = param.id;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_arg.ChangeState(model.id ==-1?0:1);
            if (model.id != -1)
            {
                view.ipt_name.text = CharacterParamForm.DataByUid[model.id].name;
            }
            else
            {
                view.txt_new.text = TextManager.instance.GetTxt("new");
            }
        }


    }
}
