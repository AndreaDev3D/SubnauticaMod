using AD3D_HabitatSolutionMod.BO.Utils;
using SMLHelper.V2.Utility;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace AD3D_HabitatSolutionMod.BO.Utils
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
                    var mainDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var assetsFolder = Path.Combine(mainDirectory, "Assets");
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
