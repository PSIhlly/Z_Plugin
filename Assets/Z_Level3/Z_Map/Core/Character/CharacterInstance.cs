using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_Map
{
    public class CharacterInstance : Instance
    {
        public CharacterUnit unit
        {
            set { base.unit = value; }
            get { return (CharacterUnit)base.unit; }
        }
    }
}