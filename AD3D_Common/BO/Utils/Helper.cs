using SMLHelper.V2.Utility;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace AD3D_Common
{
    public static class Helper
    {
        private static AssetBundle _bundle;
        private static AssetBundle Bundle
        {
            get
            {
                if (_bundle == null)
                {
                    var mainDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var assetsFolder = Path.Combine(mainDirectory, "Assets");
                    _bundle = AssetBundle.LoadFromFile(Path.Combine(assetsFolder, "common"));
                }
                return _bundle;
            }
            set
            {
                _bundle = value;
            }
        }

        public static GameObject GetGameObjectFromBundle(AssetBundle bundle, string filename)
        {
            return bundle.LoadAsset<GameObject>(filename);
        }

        public static Atlas.Sprite GetSpriteFromBundle(AssetBundle bundle, string filename)
        {
            return ImageUtils.LoadSpriteFromTexture(bundle.LoadAsset<Texture2D>(filename));
        }

        public static Texture2D GetTextureFromBundle(AssetBundle bundle, string filename)
        {
            return bundle.LoadAsset<Texture2D>(filename);
        }

        public static Atlas.Sprite GetSprite(string modName, string filename, string format = "png")
        {
            return ImageUtils.LoadSpriteFromFile($"./QMods/{modName}/Assets/{filename}.{format}");
        }

        public static Texture2D GetTexture(string modName, string filename)
        {
            return ImageUtils.LoadTextureFromFile($"./QMods/{modName}/Assets/{filename}.png");
        }

        public static void Log(string text, bool showOnScreen = false, QModManager.Utility.Logger.Level loggerLevel = QModManager.Utility.Logger.Level.Info)
        {
            QModManager.Utility.Logger.Log(loggerLevel, $"{text} {Constant.Spacer}", showOnScreen: showOnScreen);
        }

        public static Atlas.Sprite GetPrefabKitSprite()
        {
            return ImageUtils.LoadSpriteFromTexture(Bundle.LoadAsset<Texture2D>("Icon_Kit"));
        }

        public static class ClipboardHelper
        {
            public static string ClipBoard
            {
                get
                {
                    return GUIUtility.systemCopyBuffer;
                }
                set
                {
                    GUIUtility.systemCopyBuffer = value;
                }
            }
        }
    }
}
