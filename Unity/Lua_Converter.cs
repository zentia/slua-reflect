using System;
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
