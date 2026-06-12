using UnityEngine;
using Z_DesignStyle;
namespace Z_DataSystem.Form
{
    public partial class GameObjectAssetForm
    {
        public partial class Data
        {
            private Sprite _sprite;
            private GameObject _go => (GameObject)asset;

            public GameObject GetGo()
            {
                return _go;
            }
        }
    }
}
namespace Z_DataSystem
{
    public class GameObjectController : Z_Controller<AssetManager>, IAssetController
    {
        public bool IsAsset(string name)
        {
            var parts = name.Split(GetMark());
            return parts.Length == 3 && string.IsNullOrEmpty(parts[0]) && string.IsNullOrEmpty(parts[2]);
        }
        public virtual string GetName(int id=-1)
        {
            return $"{GetMark()}{id}{GetMark()}";
        }
        public string GetMark() => "$g$";
        public string[] GetSupportedExtensions() => new string[]
        {
        };

        public GameObjectController(AssetManager super) : base(super)
        {
        }


    }
}
