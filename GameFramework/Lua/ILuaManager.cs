using UnityEngine;
using System.Collections;
using LuaInterface;
using GameFramework;
using GameFramework.Resource;
using System.IO;
using System;

namespace GameFramework.Lua
{
    public interface ILuaManager
    {
        LuaState GetLuaState();
        void Initialize();

        void DoFile(string filename);

        LuaTable DoString(string luaString);

        object[] CallFunction(string funcName, params object[] args);

        void LuaGC();

        byte[] ReadFile(string fileName);

        void Start(GameObject target, MonoBehaviour componment);

        void SetResourceManager(IResourceManager resourceManager);

        void AddSearchPath(string path);

        LuaFunction GetFunction(string funcName);

        T Require<T>(string luaModule);

        void SetBeZip(bool beZip);

        void SetDebug(bool _IsDebug);
    }
}