using System;
using UnityEngine;
using LuaInterface;

public partial class Lua_Reg
{
    public interface IVarConverter
    {
        bool Check(int i);
        void PutVar(object v);
        object GetVar(int i);
    }
    public interface IConverter<T> : IVarConverter
    {
        void Put(T v);
        T Get(int i);
    }
    class Converter_bool : IConverter<bool>
    {
        void IConverter<bool>.Put(bool v)
        {
            LuaDLL.lua_pushboolean(m_L, v);
        }
        bool IConverter<bool>.Get(int i)
        {
            var v = LuaDLL.lua_toboolean(m_L, i) == 1;
            return v;
        }
        bool IVarConverter.Check(int i)
        {
            bool v = LuaDLL.lua_isboolean(m_L, i);
            return v;
        }
        void IVarConverter.PutVar(object v)
        {
            LuaDLL.lua_pushboolean(m_L, (bool)v);
        }
        object IVarConverter.GetVar(int i)
        {
            return LuaDLL.lua_toboolean(m_L, i) == 1;
        }
    }
    class Converter_sbyte : IConverter<sbyte>
    {
        void IConverter<sbyte>.Put(sbyte v)
        {
            LuaDLL.lua_pushinteger(m_L, v);
        }
        sbyte IConverter<sbyte>.Get(int i)
        {
            var v = (sbyte)LuaDLL.lua_tonumber(m_L, i);
            return v;
        }
        bool IVarConverter.Check(int i)
        {
            bool v = LuaDLL.lua_isnumber(m_L, i) == 1;
            return v;
        }
        void IVarConverter.PutVar(object v)
        {
            LuaDLL.lua_pushinteger(m_L, (sbyte)v);
        }
        object IVarConverter.GetVar(int i)
        {
            var v = (sbyte)LuaDLL.lua_tonumber(m_L, i);
            return v;
        }
    }
    class Converter_byte : IConverter<byte>
    {
        void IConverter<byte>.Put(byte v)
        {
            LuaDLL.lua_pushinteger(m_L, v);
        }
        byte IConverter<byte>.Get(int i)
        {
            var v = (byte)LuaDLL.lua_tointeger(m_L, i);
            return v;
        }
        bool IVarConverter.Check(int i)
        {
            bool v = LuaDLL.lua_isnumber(m_L, i) == 1;
            return v;
        }
        void IVarConverter.PutVar(object v)
        {
            LuaDLL.lua_pushinteger(m_L, (byte)v);
        }
        object IVarConverter.GetVar(int i)
        {
            return (byte)LuaDLL.lua_tointeger(m_L, i);
        }
    }
    class Converter_short : IConverter<short>
    {
        void IConverter<short>.Put(short v)
        {
            LuaDLL.lua_pushinteger(m_L, v);
        }
        short IConverter<short>.Get(int i)
        {
            return (short)LuaDLL.lua_tointeger(m_L, i);
        }
        bool IVarConverter.Check(int i)
        {
            return LuaDLL.lua_isinteger(m_L, i) == 1;
        }
        void IVarConverter.PutVar(object v)
        {
            LuaDLL.lua_pushinteger(m_L, (short)v);
        }
        object IVarConverter.GetVar(int i)
        {
            return (short)LuaDLL.lua_tointeger(m_L, i);
        }
    }
    class Converter_ushort : IConverter<ushort>
    {
        void IConverter<ushort>.Put(ushort v)
        {
            LuaDLL.lua_pushinteger(m_L, v);
        }
        ushort IConverter<ushort>.Get(int i)
        {
            return (ushort)LuaDLL.lua_tointeger(m_L, i);
        }
        bool IVarConverter.Check(int i)
        {
            return LuaDLL.lua_isinteger(m_L, i) == 1;
        }
        void IVarConverter.PutVar(object v)
        {
            LuaDLL.lua_pushinteger(m_L, (ushort)v);
        }
        object IVarConverter.GetVar(int i)
        {
            return (short)LuaDLL.lua_tointeger(m_L, i);
        }
    }
    class Converter_int : IConverter<int>
    {
        void IConverter<int>.Put(int v)
        {
            LuaDLL.lua_pushinteger(m_L, v);
        }
        int IConverter<int>.Get(int i)
        {
            return LuaDLL.lua_tointeger(m_L, i);
        }
        bool IVarConverter.Check(int i)
        {
            return LuaDLL.lua_isinteger(m_L, i) == 1;
        }
        void IVarConverter.PutVar(object v)
        {
            LuaDLL.lua_pushinteger(m_L, (int)v);
        }
        object IVarConverter.GetVar(int i)
        {
            return LuaDLL.lua_tointeger(m_L, i);
        }
    }
    class Converter_uint : IConverter<uint>
    {
        void IConverter<uint>.Put(uint v)
        {
            LuaDLL.lua_pushinteger(m_L, v);
        }
        uint IConverter<uint>.Get(int i)
        {
            return (uint)LuaDLL.lua_tointeger(m_L, i);
        }
        bool IVarConverter.Check(int i)
        {
            return LuaDLL.lua_isinteger(m_L, i) == 1;
        }
        void IVarConverter.PutVar(object v)
        {
            LuaDLL.lua_pushinteger(m_L, (uint)v);
        }
        object IVarConverter.GetVar(int i)
        {
            return (uint)LuaDLL.lua_tointeger(m_L, i);
        }
    }
    
    class Converter_long : IConverter<long>
    {
        void IConverter<long>.Put(long v)
        {
            LuaDLL.lua_pushinteger(m_L, v);
        }
        long IConverter<long>.Get(int i)
        {
            return LuaDLL.lua_tolong(m_L, i);
        }
        bool IVarConverter.Check(int i)
        {
            return LuaDLL.lua_isinteger(m_L, i) == 1;
        }
        void IVarConverter.PutVar(object v)
        {
            LuaDLL.lua_pushinteger(m_L, (long)v);
        }
        object IVarConverter.GetVar(int i)
        {
            return LuaDLL.lua_tointegerx(m_L, i, IntPtr.Zero);
        }
    }
    class Converter_ulong : IConverter<ulong>
    {
        void IConverter<ulong>.Put(ulong v)
        {
            LuaDLL.lua_pushinteger(m_L, (long)v);
        }
        ulong IConverter<ulong>.Get(int i)
        {
            return (ulong)LuaDLL.lua_tolong(m_L, i);
        }
        bool IVarConverter.Check(int i)
        {
            return LuaDLL.lua_isinteger(m_L, i) == 1;
        }
        void IVarConverter.PutVar(object v)
        {
            LuaDLL.lua_pushinteger(m_L, (long)v);
        }
        object IVarConverter.GetVar(int i)
        {
            return LuaDLL.lua_tointegerx(m_L, i, IntPtr.Zero);
        }
    }
    class Converter_float : IConverter<float>
    {
        void IConverter<float>.Put(float v)
        {
            LuaDLL.lua_pushnumber(m_L, v);
        }
        float IConverter<float>.Get(int i)
        {
            return (float)LuaDLL.lua_tonumber(m_L, i);
        }
        bool IVarConverter.Check(int i)
        {
            return LuaDLL.lua_isnumber(m_L, i) == 1;
        }
        void IVarConverter.PutVar(object v)
        {
            LuaDLL.lua_pushnumber(m_L, (float)v);
        }
        object IVarConverter.GetVar(int i)
        {
            return (float)LuaDLL.lua_tonumber(m_L, i);
        }
    }
    class Converter_double : IConverter<double>
    {
        void IConverter<double>.Put(double v)
        {
            LuaDLL.lua_pushnumber(m_L, v);
        }
        double IConverter<double>.Get(int i)
        {
            return LuaDLL.lua_tonumber(m_L, i);
        }
        bool IVarConverter.Check(int i)
        {
            return LuaDLL.lua_isnumber(m_L, i) == 1;
        }
        void IVarConverter.PutVar(object v)
        {
            LuaDLL.lua_pushnumber(m_L, (double)v);
        }
        object IVarConverter.GetVar(int i)
        {
            return LuaDLL.lua_tonumber(m_L, i);
        }
    }
    class Converter_string : IConverter<string>
    {
        void IConverter<string>.Put(string v)
        {
            LuaDLL.lua_pushstring(m_L, v);
        }
        string IConverter<string>.Get(int i)
        {
            var v = LuaDLL.lua_tostring(m_L, i);
            return v;
        }
        bool IVarConverter.Check(int i)
        {
            var v = LuaDLL.lua_isstring(m_L, i);
            return v == 1;
        }
        void IVarConverter.PutVar(object v)
        {
            LuaDLL.lua_pushstring(m_L, (string)v);
        }
        object IVarConverter.GetVar(int i)
        {
            var v = LuaDLL.lua_tostring(m_L, i);
            return v;
        }
    }
    class Converter_Vector2 : IConverter<Vector2>
    {
        void IConverter<Vector2>.Put(Vector2 v){
            LuaDLL.luaS_pushVector2(m_L, v.x, v.y);
        }
        Vector2 IConverter<Vector2>.Get(int i)
        {
            LuaDLL.luaS_checkVector2(m_L, i, out float x, out float y);
            return new Vector2(x, y);
        }
        bool IVarConverter.Check(int i)
        {
            return LuaDLL.luaS_checkVector2(m_L, i, out float x, out float y) == 1;
        }
        void IVarConverter.PutVar(object v)
        {
            Vector2 vector2 = (Vector2)v;
            LuaDLL.luaS_pushVector2(m_L, vector2.x, vector2.y);
        }
        object IVarConverter.GetVar(int i)
        {
            LuaDLL.luaS_checkVector2(m_L, i, out float x, out float y);
            return new Vector2(x, y);
        }
    }
    class Converter_Vector3 : IConverter<Vector3>
    {
        void IConverter<Vector3>.Put(Vector3 v)
        {
            LuaDLL.luaS_pushVector3(m_L, v.x, v.y, v.z);
        }
        Vector3 IConverter<Vector3>.Get(int i)
        {
            LuaDLL.luaS_checkVector3(m_L, i, out float x, out float y, out float z);
            return new Vector3(x, y, z);
        }
        bool IVarConverter.Check(int i)
        {
            return LuaDLL.luaS_checkVector3(m_L, i, out float x, out float y, out float z) == 1;
        }
        void IVarConverter.PutVar(object v)
        {
            Vector3 value = (Vector3)v;
            LuaDLL.luaS_pushVector3(m_L, value.x, value.y, value.z);
        }
        object IVarConverter.GetVar(int i)
        {
            LuaDLL.luaS_checkVector3(m_L, i, out float x, out float y, out float z);
            return new Vector3(x, y, z);
        }
    }
    class Converter_Vector4 : IConverter<Vector4>
    {
        void IConverter<Vector4>.Put(Vector4 v)
        {
            LuaDLL.luaS_pushVector4(m_L, v.x, v.y, v.z, v.w);
        }
        Vector4 IConverter<Vector4>.Get(int i)
        {
            LuaDLL.luaS_checkVector4(m_L, i, out float x, out float y, out float z, out float w);
            return new Vector4(x, y, z, w);
        }
        bool IVarConverter.Check(int i)
        {
            return LuaDLL.luaS_checkVector4(m_L, i, out float x, out float y, out float z, out float w) == 1;
        }
        void IVarConverter.PutVar(object v)
        {
            Vector4 value = (Vector4)v;
            LuaDLL.luaS_pushVector4(m_L, value.x, value.y, value.z, value.w);
        }
        object IVarConverter.GetVar(int i)
        {
            LuaDLL.luaS_checkVector4(m_L, i, out float x, out float y, out float z, out float w);
            return new Vector4(x, y, z, w);
        }
    }
    class Converter_Color : IConverter<Color>
    {
        void IConverter<Color>.Put(Color v)
        {
            LuaDLL.luaS_pushColor(m_L, v.r, v.g, v.b, v.a);
        }
        Color IConverter<Color>.Get(int i)
        {
            LuaDLL.luaS_checkColor(m_L, i, out float x, out float y, out float z, out float w);
            return new Color(x, y, z, w);
        }
        bool IVarConverter.Check(int i)
        {
            return LuaDLL.luaS_checkColor(m_L, i, out float x, out float y, out float z, out float w) == 1;
        }
        void IVarConverter.PutVar(object v)
        {
            Color value = (Color)v;
            LuaDLL.luaS_pushColor(m_L, value.r, value.g, value.b, value.a);
        }
        object IVarConverter.GetVar(int i)
        {
            LuaDLL.luaS_checkColor(m_L, i, out float x, out float y, out float z, out float w);
            return new Color(x, y, z, w);
        }
    }
    class Converter_Quaternion : IConverter<Quaternion>
    {
        void IConverter<Quaternion>.Put(Quaternion v)
        {
            LuaDLL.luaS_pushQuaternion(m_L, v.x, v.y, v.z, v.w);
        }
        Quaternion IConverter<Quaternion>.Get(int i)
        {
            LuaDLL.luaS_checkQuaternion(m_L, i, out float x, out float y, out float z, out float w);
            return new Quaternion(x, y, z, w);
        }
        bool IVarConverter.Check(int i)
        {
            return LuaDLL.luaS_checkQuaternion(m_L, i, out float x, out float y, out float z, out float w) == 1;
        }
        void IVarConverter.PutVar(object v)
        {
            Quaternion value = (Quaternion)v;
            LuaDLL.luaS_pushQuaternion(m_L, value.x, value.y, value.z, value.w);
        }
        object IVarConverter.GetVar(int i)
        {
            LuaDLL.luaS_checkQuaternion(m_L, i, out float x, out float y, out float z, out float w);
            return new Quaternion(x, y, z, w);
        }
    }
    static void NewConverter(Type type, object obj)
    {
        m_converters[type] = obj;
        m_varConverters[type] = obj as IVarConverter;
    }
    static void NewConverter<T, C>() where C : class, new()
    {
        NewConverter(typeof(T), new C());
    }
    static Dictionary<Type, IVarConverter> m_varConverters = new Dictionary<Type, IVarConverter>(100);
}
