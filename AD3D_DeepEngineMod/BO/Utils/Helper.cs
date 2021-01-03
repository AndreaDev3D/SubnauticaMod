using System.IO;
using System.Reflection;
using UnityEngine;

namespace AD3D_DeepEngineMod.BO.Utils
{
    public class Helper
    {
        private static AssetBundle _bundle;
        public static AssetBundle Bundle
        {
            get
            {
                if (_bundle == null)
                {
                    var assetsFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");
                    _bundle = AssetBundle.LoadFromFile(Path.Combine(assetsFolder, QPatch._AssetName));
                }
                return _bundle;
            }
            set
            {
                _bundle = value;
            }
        }
    }
}
