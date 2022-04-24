using UnityEngine;
using System.Collections;
using LuaInterface;
using GameFramework;
using System.IO;
using GameFramework.Resource;
using System;
using System.Threading;

namespace GameFramework.Lua
{
    internal sealed class LuaManager : GameFrameworkModule, ILuaManager
    {
        public LuaState lua
        { get; private set; }

        public LuaLooper loop
        { get; private set; }

        internal override int Priority


        {
            get
            {
                return 60;
            }
        }

        private LuaLoader loader = null;

        public LuaManager()
        {
            loader = new LuaLoader();
            lua = new LuaState();
            //this.OpenLibs();
            this.OpenLuaSocket();
            //this.OpenSprotoCore();
            lua.LuaSetTop(0);
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {

        }

        internal override void Shutdown()
        {
            if (loop != null)
            {
                loop.Destroy();
                loop = null;
            }

            lua.Dispose();
            lua = null;
            loader = null;
        }

        //cjson 比较特殊，只new了一个table，没有注册库，这里注册一下
        public void OpenCJson()
        {
            lua.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
            lua.OpenLibs(LuaDLL.luaopen_cjson);
            lua.LuaSetField(-2, "cjson");

            lua.OpenLibs(LuaDLL.luaopen_cjson_safe);
            lua.LuaSetField(-2, "cjson.safe");
        }

        private void OpenLuaSocket()
        {
            LuaConst.openLuaSocket = true;
            lua.BeginPreLoad();
            lua.RegFunction("socket.core", LuaOpen_Socket_Core);
            lua.EndPreLoad();
        }

        private void OpenSprotoCore()
        {
            lua.BeginPreLoad();
            //lua.RegFunction("sproto.core", LuaOpen_Sproto_Core);
            lua.EndPreLoad();
        }


        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int LuaOpen_Socket_Core(IntPtr L)
        {
            return LuaDLL.luaopen_socket_core(L);
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
//         static int LuaOpen_Sproto_Core(IntPtr L)
//         {
//             return LuaDLL.luaopen_sproto_core(L);
//         }

        // private void OpenLuaPb()
        // {
        //    lua.OpenLibs(LuaDLL.luaopen_pb);
        //    lua.LuaSetGlobal("pb");

        //     lua.OpenLibs(LuaDLL.luaopen_pb_buffer);
        //     lua.LuaSetGlobal("pb.buffer");

        //      lua.OpenLibs(LuaDLL.luaopen_pb_slice);
        //     lua.LuaSetGlobal("pb.slice");

        //      lua.OpenLibs(LuaDLL.luaopen_pb_io);
        //     lua.LuaSetGlobal("pb.io");

        //      lua.OpenLibs(LuaDLL.luaopen_pb_conv);
        //     lua.LuaSetGlobal("pb.conv");
        // }

        /// <summary>
        /// 初始化加载第三方库
        /// </summary>
        private void OpenLibs()
        {
            //保持库名字与5.1.5库中一致
            lua.BeginPreLoad();                        
//             lua.AddPreLoadLib("struct", new LuaCSFunction(LuaDLL.luaopen_struct));
//             lua.AddPreLoadLib("lpeg", new LuaCSFunction(LuaDLL.luaopen_lpeg));
//             lua.AddPreLoadLib("cjson", new LuaCSFunction(LuaDLL.luaopen_cjson));
//             lua.AddPreLoadLib("cjson.safe", new LuaCSFunction(LuaDLL.luaopen_cjson_safe));
            lua.EndPreLoad();   
        }

        public void DoFile(string filename)
        {
            lua.DoFile(filename);
        }

        // Update is called once per frame
        public object[] CallFunction(string funcName, params object[] args)
        {
            LuaFunction func = lua.GetFunction(funcName);
            if (func != null)
            {
                return func.LazyCall(args);
            }
            return null;
        }

        public void LuaGC()
        {
            lua.LuaGC(LuaGCOptions.LUA_GCCOLLECT);
        }

        public byte[] ReadFile(string fileName)
        {
            return LuaFileUtils.Instance.ReadFile(fileName);
        }

        public void Start(GameObject target, MonoBehaviour componment)
        {
            this.lua.Start();

            loop = target.AddComponent<LuaLooper>();
            loop.luaState = lua;

            LuaCoroutine.Register(lua, componment);
        }

        public void SetResourceManager(IResourceManager resourceManager)
        {
            if (resourceManager == null)
            {
                throw new GameFrameworkException("Resource manager is invalid.");
            }

            loader.SetResourceManager(resourceManager);
        }

        public void SetDebug(bool _IsDebug)
        {
            loader.SetDebug(_IsDebug);
        }

        public void AddSearchPath(string path)
        {
            lua.AddSearchPath(path);
        }

        public LuaFunction GetFunction(string funcName)
        {
            return lua.GetFunction(funcName);
        }

        public T Require<T>(string luaModule)
        {
#if UNITY_EDITOR
            try
            {
                return lua.Require<T>(luaModule);
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
                return default(T);
            }
#else
            return lua.Require<T>(luaModule);
#endif
        }

        public void SetBeZip(bool beZip)
        {
            LuaFileUtils.Instance.beZip = beZip;
        }

        public LuaTable DoString(string luaString)
        {
            return lua.DoString<LuaTable>(luaString);
        }

        public void Initialize()
        {
            LuaBinder.Bind(lua);
            DelegateFactory.Init();
        }

        public LuaState GetLuaState()
        {
            return lua;
        }
    }
}