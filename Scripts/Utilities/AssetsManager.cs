using System.IO;
using System;
using UnityEngine;

namespace Entropy.Assets.Scripts.Assets.Scripts.Utilities
{
    public static class AssetsManager
    {
        private static AssetBundleCreateRequest _request;
        private static AssetBundle _bundle;
        private static bool _isLoaded;
        private static bool _unloaded;
        static AssetsManager()
        {
            var codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            var directory = Path.GetDirectoryName(path);
            var pathToBundle = Path.Combine(directory, "Content", "entropy.assets");
            _request = AssetBundle.LoadFromFileAsync(pathToBundle);
            _request.completed += AssetsLoaded;
            if (_request == null)
                throw new ApplicationException("Unable to load \"Content\\entropy.assets\" file!");
        }
        public static void Init()
        {
            // This is just to call the static constructor
        }
        public static void Unload()
        {
            if (_bundle != null)
            {
                _bundle.Unload(true);
                _bundle = null;
            }
            _isLoaded = true;
            _unloaded = true;
        }

        public static T LoadAsset<T>(string assetName) where T : UnityEngine.Object
        {
            if(_unloaded)
                throw new ApplicationException("Mod assets have been unloaded!");
            if (_isLoaded && _bundle == null)
                throw new ApplicationException("Unable to load \"Content\\entropy.assets\" file!");
            if (_bundle == null)
            {
                _bundle = _request.assetBundle;
                if (_bundle == null)
                    throw new ApplicationException("Unable to load \"Content\\entropy.assets\" file!");
            }
            return _bundle.LoadAsset<T>(assetName);
        }

        private static void AssetsLoaded(AsyncOperation obj)
        {
            _request.completed -= AssetsLoaded;
            _isLoaded = true;
            _bundle = _request.assetBundle;
            if (_bundle == null)
                throw new ApplicationException("Unable to load \"Content\\entropy.assets\" file!");
        }
    }
}
