using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_DesignStyle;

namespace Character
{
    public static class GlobalSettings
    {
        public const int CHARACTER_AVATA_MAX = 10;
        public const int CHARACTER_ANIM_MAX = 10;
        public const int CHARACTER_PART_MAX = 2;
    }
    public static class GlobalHelper
    {
        public static string GetCharacterAvatarName(string nickName, int id)
        {
            return "character_a$" + nickName + "$" + id;
        }
        public static string GetCharacterAnimName(string nickName,string animName,int part, int id)
        {
            return (part==0?"character_b_up$": "character_b_down$") + nickName + "$"+ animName+"$" + id;
        }


        public static CharacterAnimForm.Data GetAnim(this CharacterProductForm.Data data, int id)
        {
            return CharacterAnimForm.GetDataByJo(JObject.Parse(data.animJo[id]));
        }
        public static void SaveAnim(this CharacterProductForm.Data data, int id, CharacterAnimForm.Data info)
        {
            data.animJo[id] = CharacterAnimForm.GetJoByData(info).ToString();
        }

    }
    public class CharacterManager : Z_MonoManager<CharacterManager>
    {
       
    }
}
