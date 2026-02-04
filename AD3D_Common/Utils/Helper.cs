using System.IO;
using System.Reflection;
using UnityEngine;

namespace AD3D_Common
{
    public static class Helper
    {
        internal const string _CommonBundle = "common.asset";
        internal static string _Assemblylocation => Assembly.GetExecutingAssembly().Location;

        internal static AssetBundle _bundle;
        internal static AssetBundle Bundle => _bundle ??= GetAssetBundle(_Assemblylocation, _CommonBundle);

        public static AssetBundle GetAssetBundle(string assemblyLocation, string bundleName)
        {
            var mainDirectory = Path.GetDirectoryName(assemblyLocation);
            var assetsFolder = Path.Combine(mainDirectory, "Assets", bundleName);
            return AssetBundle.LoadFromFile(Path.Combine(assetsFolder));
        }

        public static GameObject GetGameObjectFromBundle(AssetBundle bundle, string filename)
        {
            return bundle.LoadAsset<GameObject>(filename);
        }

        public static Sprite GetSpriteFromBundle(AssetBundle bundle, string filename)
        {
            return null;
            //return ImageUtils.LoadSpriteFromTexture(bundle.LoadAsset<Texture2D>(filename));
        }

        public static Texture2D GetTextureFromBundle(AssetBundle bundle, string filename)
        {
            return bundle.LoadAsset<Texture2D>(filename);
        }

        public static Sprite GetSprite(string modName, string filename, string format = "png")
        {
            return null;
            //return ImageUtils.LoadSpriteFromFile($"{Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)}/Assets/{filename}.{format}");
        }

        public static Texture2D GetTexture(string modName, string filename, string format = "png")
        {
            return null;// ImageUtils.LoadTextureFromFile($"{Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)}/Assets/{filename}.{format}");
        }

        //public static void Log(string text, bool showOnScreen = false, QModManager.Utility.Logger.Level loggerLevel = QModManager.Utility.Logger.Level.Debug)
        //{
        //    QModManager.Utility.Logger.Log(loggerLevel, $"{text} {Constant.Spacer}", showOnScreen: showOnScreen);
        //}

        public static Sprite GetPrefabKitSprite()
        {
            return null;// ImageUtils.LoadSpriteFromTexture(Bundle.LoadAsset<Texture2D>("Icon_Kit"));
        }

        public static class ClipboardHelper
        {
            public static string ClipBoard { get => GUIUtility.systemCopyBuffer; set => GUIUtility.systemCopyBuffer = value; }
        }
    }
}