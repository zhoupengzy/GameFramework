using UnityEngine;
using System.Collections;
using LuaInterface;
using GameFramework;
using System.IO;
using GameFramework.Resource;
using System;

namespace GameFramework.Lua
{
    public class LuaLoader : LuaFileUtils
    {
        private IResourceManager resourceManager = null;

        private readonly LoadAssetCallbacks m_LoadAssetCallbacks;

        private bool IsDebug = false;

        public EventHandler<LoadLuaBoundleSuccessEventArgs> OnLoadLuaBoundleSuccess
        { get; set; }

        public LuaLoader()
        {
            instance = this;

            m_LoadAssetCallbacks = new LoadAssetCallbacks(LoadLuaBoundleSuccessCallback,
                LoadLuaBoundleFailureCallback,
                LoadLuaBoundleUpdateCallback,
                LoadLuaBoundleDependencyAssetCallback);
        }

        public void SetResourceManager(IResourceManager _resourceManager)
        {
            if (_resourceManager == null)
            {
                throw new GameFrameworkException("invalid ResourceManager");
            }

            resourceManager = _resourceManager;
        }

        public void SetDebug(bool _IsDebug) 
        {
            this.IsDebug = _IsDebug;
        }

        public override byte[] ReadFile(string fileName)
        {
            //Log.Info(fileName);

            if (!beZip || IsDebug)
            {
                string path = FindFile(fileName);
                byte[] str = null;

                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
#if !UNITY_WEBPLAYER
                    str = File.ReadAllBytes(path);
#else
                    throw new LuaException("can't run in web platform, please switch to other platform");
#endif
                }
                else if(IsDebug)
                {
                    return _GetLuaCode(fileName);
                }

                return str;
            }
            else
            {
                return _GetLuaCode(fileName);
            }
        }

        private byte[] _GetLuaCode(string fileName)
        {
            if (resourceManager == null)
            {
                throw new GameFrameworkException("invalid ResourceManager");
            }

            if (!fileName.EndsWith(".lua")) fileName += ".lua";
            fileName = "Assets/ProjectResources/LuaBuilds/" + fileName + ".bytes";

            return (resourceManager.LoadAsset(fileName) as TextAsset).bytes;
        }

        private void LoadLuaBoundleSuccessCallback(string luaAssetName, object luaAsset, float duration, object userData)
        {
            AssetBundle bundle = luaAsset as AssetBundle;
            if (bundle != null)
            {
                luaAssetName = luaAssetName.Replace("lua/", "").Replace(".unity3d", "");
                LuaFileUtils.Instance.AddSearchBundle(luaAssetName.ToLower(), bundle);

                if (OnLoadLuaBoundleSuccess != null)
                {
                    OnLoadLuaBoundleSuccess(this, new LoadLuaBoundleSuccessEventArgs(luaAssetName, userData));
                }
            }
        }

        private void LoadLuaBoundleFailureCallback(string luaAssetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            string appendErrorMessage = string.Format("Load lua boundle failure, asset name '{0}', status '{1}', error message '{2}'.", luaAssetName, status.ToString(), errorMessage);
            throw new GameFrameworkException(appendErrorMessage);
        }

        private void LoadLuaBoundleUpdateCallback(string luaAssetName, float progress, object userData)
        {
            throw new GameFrameworkException("not need update");
        }

        private void LoadLuaBoundleDependencyAssetCallback(string luaAssetName, string dependencyAssetName, int loadedCount, int totalCount, object userData)
        {
            throw new GameFrameworkException("not need dependency");
        }
    }
}