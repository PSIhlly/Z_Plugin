using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_DesignStyle;
using Z_UnitSystem;

namespace Item
{
    public enum ItemEventType
    {
        Add,
        Remove
    }
    public class ItemEvent : Z_Event
    {
        public ItemEventType type;
        public int delta;
    }
    public class ItemManager:Z_Singleton<ItemManager>
    {
        public void AddItem(int id,int count)
        {
            ItemForm.DataById[id].count += count;
            SaveAndLoad.Save(ItemDefines.SAVE_NAME, ItemForm.GetJaByDatas().ToString());
            Z_EventHelper.Invoke<ItemEvent>(new ItemEvent()
            {
                type = count > 0 ? ItemEventType.Add : ItemEventType.Remove,
                delta = count
            });
        }

    }
}
