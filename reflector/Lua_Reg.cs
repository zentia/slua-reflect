// Copyright (C) Kingsoft
// All rights reserved.
//
// Author   : Liyanfeng
// Date     : 2019-05-31
// Comment  : 实在不喜欢slua的那一套不考虑成本代价的wrap，需要自行自行注册的可以参考本代码
using SLua;
using System;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;

public partial class Lua_Reg : LuaObject
{
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static public int constructor(IntPtr l)
    {
        var o = new Lua_Reg();
        pushValue(l, true);
        pushValue(l, o);
        return 2;
    }
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static public int LoadDataText_s(IntPtr l)
    {
        pushValue(l, true);
        string name = LuaDLL.lua_tostring(l, 1);
        if (name == null || name.Length == 0)
        {
            Debug.LogError("bad file name");
            LuaDLL.lua_pushnil(l);
        }
        else
        {
            string file;
            if (LuaDLL.lua_toboolean(l, 2) == 1)
            {
                file = name;
            }
            else
            {
                file = Application.persistentDataPath + "/" + name;
            }
            if (!System.IO.File.Exists(file))
            {
                LuaDLL.lua_pushnil(l);
            }
            else
            {
                try
                {
                    byte[] data = System.IO.File.ReadAllBytes(file);
                    LuaDLL.lua_pushlstring(l, data, data.Length);
                }
                catch (Exception err)
                {
                    Debug.LogException(err);
                    LuaDLL.lua_pushnil(l);
                }
            }
        }
        return 2;
    }
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static public int SaveDataText_s(IntPtr l)
    {
        pushValue(l, true);
        string name = LuaDLL.lua_tostring(l, 1);
        if (name == null || name.Length == 0)
        {
            Debug.LogError("bad file name");
            LuaDLL.lua_pushnil(l);
        }
        else
        {
            if (LuaDLL.lua_isstring(l, 2) == 0)
            {
                Debug.LogError("param is not a text");
                LuaDLL.lua_pushnil(l);
            }
            else
            {
                string data = LuaDLL.lua_tostring(l, 2);
                string file;
                if (LuaDLL.lua_toboolean(l, 3) == 1)
                {
                    file = name;
                }
                else
                {
                    file = Application.persistentDataPath + "/" + name;
                }
                try
                {
                    System.IO.File.WriteAllText(file, data, System.Text.Encoding.ASCII);//我也是被逼的，以后有时间在改吧。
                    LuaDLL.lua_pushboolean(l, true);
                }
                catch (Exception err)
                {
                    Debug.LogException(err);
                    LuaDLL.lua_pushnil(l);
                }
            }
        }
        return 2;
    }
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static public int SaveTexture_s(IntPtr l)
    {
        checkType(l, 1, out GameObject lefttop);
        checkType(l, 2, out GameObject rightbottom);
        checkType(l, 3, out GameObject obj);
        pushValue(l, true);
        CaptureAndSaveManager.CaptureCameraExt(UINavigationManager.Instance.UIMainCamera, lefttop, rightbottom, obj);
        return 1;
    }
    public static void Reg(IntPtr l)
    {
        getTypeTable(l, "Lua_Reg");
        addMember(l, SaveDataText_s);
        addMember(l, LoadDataText_s);
        addMember(l, GetMember_s);
        addMember(l, Typeof_s);
        addMember(l, SaveTexture_s);
        createTypeMetatable(l, constructor, typeof(Lua_Reg));
    }
}