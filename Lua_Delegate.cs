using UnityEngine;
using System;
using System.Collections.Generic;
using SLua;
using LuaInterface;
using System.Reflection;

public class Lua_Delegate
{
    public static List<Assembly> assemblies;
    private static bool _init;
    public static void Init()
    {
        if (_init)
            return;
        _init = true;
        assemblies = new List<Assembly>();
        assemblies.Add(Assembly.GetExecutingAssembly());
    }
    public interface IVarConverter
    {
        bool Check(int i);
        void PutVar(object v);
        object GetVar(int i);
    }
    static Dictionary<Type, TypeInfo> m_typeInfos = new Dictionary<Type, TypeInfo>(100);
    static Dictionary<Type, object> m_converters = new Dictionary<Type, object>(100);
    static Dictionary<Type, IVarConverter> m_varConverters = new Dictionary<Type, IVarConverter>(100);
    static Dictionary<string, Type> m_types = new Dictionary<string, Type>(100);
    public abstract class TypeInfo
    {
        public Type type;
        public int meta = -2;
        public abstract void Put(object obj);
        public abstract TypeInfo New(Type t);
    }
    public static IntPtr m_L;

    public interface IConverter<T> : IVarConverter
    {
        void Put(T v);
        T Get(int i);
    }
    public static T GetComponent<T>(int i)
    {
        object v = LuaObject.checkObj(m_L, i);
        if (v == null) return default;

        Component p = v as Component;
        if (p != null)
        {
            if (v is T)
            {
                return (T)v;
            }
            T c = p.gameObject.GetComponent<T>();
            return c;
        }
        GameObject g = v as GameObject;
        if (g != null)
        {
            T c = g.GetComponent<T>();
            return c;
        }

        return default;
    }
    class Converter_Component<T> : IConverter<T>
    {
        void IConverter<T>.Put(T v)
        {
            LuaObject.pushObject(m_L, v);
        }
        T IConverter<T>.Get(int i)
        {
            return GetComponent<T>(i);
        }
        bool IVarConverter.Check(int i)
        {
            object v = LuaObject.checkObj(m_L, i);
            if (v == null) return false;
            object p = (T)v;
            return p != null;
        }
        void IVarConverter.PutVar(object v)
        {
            LuaObject.pushObject(m_L, v);
        }
        object IVarConverter.GetVar(int i)
        {
            return GetComponent<T>(i);
        }
    }
    internal static IConverter<T> GetConverter<T>()
    {
        var type = typeof(T);
        m_converters.TryGetValue(type, out var converter);
        if (converter != null) return converter as IConverter<T>;

        IConverter<T> p = null;
        if (type.IsValueType)
        {
            if (type.IsEnum)
            {
                p = new ConverterE<T>();
            }
            else
            {
                p = new ConverterX<T>();
            }
        }
        else if (type.IsArray)
        {
            p = new ConverterArray<T>();
        }
        else if (typeof(Component).IsAssignableFrom(type))
        {
            p = new Converter_Component<T>();
        }
        else
        {
            p = new ConverterX<T>();
        }
        m_converters[type] = p;
#if UNITY_EDITOR
        if (m_varConverters.ContainsKey(type))
        {
            Debug.LogErrorFormat("type {0} already in var converters", type.Name);
        }
#endif
        m_varConverters[type] = p;
        return p;
    }
    public class ConvertionX<T>
    {
        public static IConverter<T> c = GetConverter<T>();
    }
    public static T Get<T>(int idx)
    {
        return ConvertionX<T>.c.Get(idx);
    }
    class ConverterE<T> : IConverter<T>
    {
        public static bool putString = true;
        void PutValue(object v)
        {
            if (putString)
            {
                LuaDLL.lua_pushstring(m_L, v.ToString());
            }
            else
            {
                LuaDLL.lua_pushinteger(m_L, (int)v);
            }
        }
        void IConverter<T>.Put(T v)
        {
            PutValue(v);
        }
        T GetValue(int idx)
        {
            return (T)GetEnum(idx, typeof(T));
        }
        T IConverter<T>.Get(int i)
        {
            return GetValue(i);
        }
        bool IVarConverter.Check(int i)
        {
            return IsEnum(i, typeof(T));
        }
        void IVarConverter.PutVar(object v)
        {
            PutVar(v);
        }
        object IVarConverter.GetVar(int i)
        {
            return GetValue(i);
        }
    }
    public class ConverterX<T> : IConverter<T>
    {
        void IConverter<T>.Put(T v)
        {
            LuaObject.pushValue(m_L, v);
        }
        T GetValue(int i)
        {
            LuaObject.checkType(m_L, i, out object v);
            if (v == null)
            {
                return default;
            }
            T p = (T)v;
            if (p == null)
            {
                Debug.LogErrorFormat("ConverterX: {0} expected got {1}", typeof(T).Name, v.GetType().Name);
            }
            return p;
        }
        T IConverter<T>.Get(int i)
        {
            return GetValue(i);
        }
        bool IVarConverter.Check(int i)
        {
            LuaObject.checkType(m_L, i, out object v);
            if (v == null) return false;
            T p = (T)v;
            return p != null;
        }
        void IVarConverter.PutVar(object v)
        {
            LuaObject.pushObject(m_L, v);
        }
        object IVarConverter.GetVar(int i)
        {
            return GetValue(i);
        }
    }
    public static Array GetTableArray(int idx, Type etype)
    {
        var L = m_L;
        IVarConverter cvt = GetVarConverter(etype);
        int tabIdx = GetAbsIndex(idx);
        int count = LuaDLL.lua_rawlen(L, tabIdx);
        var array = Array.CreateInstance(etype, count);
        int index = LuaDLL.lua_gettop(L);
        using (MakeGuard())
        {
            for(int i = 0; i < count; ++i)
            {
                LuaDLL.lua_rawgeti(L, tabIdx, i + 1);
                var val = cvt.GetVar(index + 1);
                LuaDLL.lua_settop(L, index);
                array.SetValue(val, i);
            }
        }
        return array;
    }
    static T GetArray<T>(int idx)
    {
        var L = m_L;
        var t = LuaDLL.lua_type(L, idx);
        if (t == LuaTypes.LUA_TTABLE)
        {
            var type = typeof(T);
            var etype = type.GetElementType();
            if (etype == null) return default;
            object obj = GetTableArray(idx, etype);
            return (T)obj;
        }
        
        return default;
    }
    class ConverterArray<T> : IConverter<T>
    {
        void IConverter<T>.Put(T v)
        {
            LuaObject.pushObject(m_L, (Array)(object)v);
        }
        T IConverter<T>.Get(int i)
        {
            return GetArray<T>(i);
        }
        bool IVarConverter.Check(int i)
        {
            object v = LuaObject.checkObj(m_L, i);
            if (v == null) return false;
            T p = (T)v;
            return p != null;
        }
        void IVarConverter.PutVar(object v)
        {
            LuaObject.pushObject(m_L, (Array)v);
        }
        object IVarConverter.GetVar(int i)
        {
            return GetArray<T>(i);
        }
    }
    static bool IsEnum(int idx, Type type)
    {
        try
        {
            object v = null;
            var t = LuaDLL.lua_type(m_L, idx);
            switch (t)
            {
                case LuaTypes.LUA_TSTRING:
                    {
                        string x = LuaDLL.lua_tostring(m_L, idx);
                        v = Enum.Parse(type, x);
                    }
                    break;
                case LuaTypes.LUA_TNUMBER:
                    {
                        int x = (int)LuaDLL.lua_tonumber(m_L, idx);
                        v = Enum.ToObject(type, x);
                    }
                    break;
                case LuaTypes.LUA_TUSERDATA:
                    {
                        v = LuaObject.checkObj(m_L, idx);
                        if (v != null)
                        {
                            if (!type.IsInstanceOfType(v))
                            {
                                v = null;
                            }
                        }
                    }
                    break;
            }
            return v != null;
        }
        catch
        {
            return false;
        }
    }
    static object GetEnum(int idx, Type type)
    {
        object v = null;
        var t = LuaDLL.lua_type(m_L, idx);
        switch (t)
        {
            case LuaTypes.LUA_TSTRING:
                {
                    string x = LuaDLL.lua_tostring(m_L, idx);
                    v = Enum.Parse(type, x);
                }
                break;
            case LuaTypes.LUA_TNUMBER:
                {
                    int x = (int)LuaDLL.lua_tonumber(m_L, idx);
                    v = Enum.ToObject(type, x);
                }
                break;
            case LuaTypes.LUA_TUSERDATA:
                {
                    v = LuaObject.checkObj(m_L, idx);
                    if (v != null)
                    {
                        if (!type.IsInstanceOfType(v))
                        {
                            v = null;
                        }
                    }
                }
                break;
        }
        return v;
    }
    class VarConverter_enum : IVarConverter
    {
        public Type type;
        bool IVarConverter.Check(int i)
        {
            return IsEnum(i, type);
        }
        void IVarConverter.PutVar(object v)
        {
            LuaDLL.lua_pushstring(m_L, v.ToString());
        }
        object IVarConverter.GetVar(int i)
        {
            return GetEnum(i, type);
        }
    }
    class VarConverter_class : IVarConverter
    {
        public Type type;
        bool IVarConverter.Check(int i)
        {
            object obj = LuaObject.checkObj(m_L, i);
            if (obj == null) return true;
            bool v = type.IsInstanceOfType(obj);
            return v;
        }
        void IVarConverter.PutVar(object v)
        {
            LuaObject.pushObject(m_L, v);
        }
        object IVarConverter.GetVar(int i)
        {
            LuaObject.checkType(m_L, i, out object obj);
            if (type.IsInstanceOfType(obj))
            {
                return obj;
            }
            return null;
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    internal static int Typeof_s(IntPtr l)
    {
        m_L = l;
        Type t = GetTypeof(1);
        LuaObject.pushValue(m_L, true);
        Put(t);
        return 2;
    }

    public static Type GetType(string name)
    {
        Type t = null;
        if (m_types.TryGetValue(name, out t))
        {
            return t;
        }
        t = Type.GetType(name);
        if (t == null)
        {
            foreach(var assembly in assemblies)
            {
                t = assembly.GetType(name);
                if (t != null)
                {
                    m_types[name] = t;
                    return t;
                }
            }
            return t;
        }
        m_types[name] = t;
        return t;
    }

    static Type GetTypeof(int i)
    {
        var obj = Get<object>(i);
        if (obj == null) return null;

        Type type = null;
        if (obj is string)
        {
            type = GetType(obj as string);
        }
        else if (obj is Type)
        {
            type = obj as Type;
        }
        else
        {
            type = obj.GetType();
        }
        return type;
    }
    class DynBase
    {
        internal DynType dynType;
    }
    class DynConstructor : DynBase
    {

    }
    public delegate int SafeFunction(IntPtr l);
    struct GuardL : IDisposable
    {
        public IntPtr L;
        public void Dispose()
        {
            m_L = L;
        }
    }
    internal class SafeCall
    {
        public class Info
        {
            public string name;
            public SafeFunction function;
        }
        static Info[] m_infos = new Info[200];
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int Call(IntPtr L)
        {
            int upidx = LuaDLL.lua_upvalueindex(1);
            int fnidx = (int)LuaDLL.lua_touserdata(L, upidx);
            if (fnidx <= 0 || fnidx > m_count)
            {
                Debug.LogErrorFormat("SafeCallApi : bad function index " + fnidx);
                return 0;
            }
            var info = m_infos[fnidx - 1];
            try
            {
                if (L != m_L)
                {
                    using (new GuardL { L = m_L })
                    {
                        m_L = L;
                        int v = info.function(L);
                        return v;
                    }
                }
                int r = info.function(L);
                return r;
            }
            catch (Exception err)
            {
                LuaDLL.lua_pushstring(L, "SafeCallApi : exception \n" + err.ToString());
            }
            return 0;
        }
        static int m_count = 0;
        static LuaCSFunction m_callFn = Call;
        static IntPtr m_callPtr = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(m_callFn);
        public static void PushClosure(int fnidx)
        {
            if (fnidx == 0)
            {
                LuaDLL.lua_pushnil(m_L);
                return;
            }
            LuaDLL.lua_pushlightuserdata(m_L, (IntPtr)fnidx);
            LuaDLL.lua_pushcclosure(m_L, m_callPtr, 1);
        }
        public static int Make(string name, SafeFunction fn)
        {
            Info info = new Info { name = name, function = fn };
            if (m_count >= m_infos.Length)
            {
                Info[] d = new Info[m_count * 2];
                System.Array.Copy(m_infos, 0, d, 0, m_count);
                m_infos = d;
            }
            m_infos[m_count] = info;
            int fnidx = ++m_count;
            return fnidx;
        }
    }

    class DynValue : DynMember
    {
        internal int getter;
        internal int setter;
        internal IVarConverter converter;
        internal override int Push()
        {
            LuaObject.pushValue(m_L, true);
            SafeCall.PushClosure(getter);
            SafeCall.PushClosure(setter);
            return 3;
        }
    }
    class DynProperty : DynValue
    {
        internal PropertyInfo info;
        internal MethodInfo getm;
        internal MethodInfo setm;
        internal int Getter(IntPtr l)
        {
            object self = null;
            if (!getm.IsStatic)
            {
                self = GetSelf(l);
            }
            object ret = getm.Invoke(self, null);
            converter.PutVar(ret);
            return 1;
        }
        internal int Setter(IntPtr l)
        {
            object self = null;
            if (!setm.IsStatic)
            {
                self = GetSelf(l);
            }
            object val = converter.GetVar(2);
            setm.Invoke(self, new object[] { val });
            return 0;
        }
        internal override string name { get { return info.Name; } }
    }
    public static object GetVar(int idx)
    {
        var L = m_L;
        LuaTypes type = LuaDLL.lua_type(L, idx);
        switch (type)
        {
            case LuaTypes.LUA_TNUMBER:
                return LuaDLL.lua_tonumber(L, idx);
            case LuaTypes.LUA_TSTRING:
                return LuaDLL.lua_tostring(L, idx);
            case LuaTypes.LUA_TBOOLEAN:
                return LuaDLL.lua_toboolean(L, idx);
            case LuaTypes.LUA_TTABLE:
                LuaObject.checkArray<float>(L, idx, out var ta);
                return ta;
            case LuaTypes.LUA_TFUNCTION:
                LuaDLL.lua_pushvalue(L, idx);
                return new LuaFunction(Lua.Instance.luaState, LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX));
        }
        return null;
    }
    class DynField : DynValue
    {
        internal FieldInfo info;
        internal int Getter(IntPtr l)
        {
            m_L = l;
            object self = null;
            if (!info.IsStatic)
            {
                self = GetSelf(l);
            }
            object ret = info.GetValue(self);
            converter.PutVar(ret);
            return 1;
        }
        internal int Setter(IntPtr l)
        {
            m_L = l;
            object self = null;
            if (!info.IsStatic)
            {
                self = GetSelf(l);
            }
            object val = converter.GetVar(2);
            info.SetValue(self, val);
            return 0;
        }
    }
    public static int Put<T>(T v)
    {
        ConvertionX<T>.c.Put(v);
        return 1;
    }
    class DynMember
    {
        internal DynType dynType;
        internal virtual int Push() { return 2; }
        internal virtual string name { get { return null; } }

        internal object GetSelf(IntPtr l)
        {
            m_L = l;
            return dynType.converter.GetVar(1);
        }
    }
    class DynType
    {
        internal DynType baseDynType;
        internal Type type;
        internal IVarConverter converter;
        internal Dictionary<string, DynMember> members = new Dictionary<string, DynMember>(8);
    }
    internal static IVarConverter GetVarConverter(Type type)
    {
        m_varConverters.TryGetValue(type, out var converter);
        if (converter != null) return converter;
        IVarConverter p = null;
        if (type.IsEnum)
        {
            p = new VarConverter_enum { type = type };
        }
        else
        {
            p = new VarConverter_class { type = type };
        }
        m_varConverters[type] = p;
        return p;
    }
    static Dictionary<Type, DynType> dynTypes = new Dictionary<Type, DynType>(10);
    static DynType GetDynType(Type type)
    {
        dynTypes.TryGetValue(type, out DynType dynType);
        if (dynType != null) return dynType;

        dynType = new DynType { type = type, converter = GetVarConverter(type) };
        dynTypes[type] = dynType;
        Type bt = type.BaseType;
        if (bt != null)
        {
            dynType.baseDynType = GetDynType(bt);
        }
        return dynType;
    }
    public static bool IsArg(int idx)
    {
        var t = LuaDLL.lua_type(m_L, idx);
        if (t == LuaTypes.LUA_TNONE) return false;
        return true;
    }
    internal struct GuardStack : IDisposable
    {
        internal int m_index;
        public void Dispose()
        {
            LuaDLL.lua_settop(m_L, m_index);
        }
    }
    static internal GuardStack MakeGuard()
    {
        return new GuardStack { m_index = LuaDLL.lua_gettop(m_L) };
    }
    public static int GetAbsIndex(int idx)
    {
        if (idx >= 0) return idx;
        int top = LuaDLL.lua_gettop(m_L);
        idx = top + 1 + idx;
        if (idx <= 0)
        {
            throw new Exception("GetAbsIndex : bad stack index " + idx);
        }
        return idx;
    }
    public static object GetVar(int idx, Type type)
    {
        var converter = GetVarConverter(type);
        return converter.GetVar(idx);
    }
    class VarConverter_ParamArray : IVarConverter
    {
        public Type arrayType;
        public Type elementType;
        bool IVarConverter.Check(int i)
        {
            bool v = IsArg(i);
            return v;
        }
        void IVarConverter.PutVar(object v)
        {

        }
        object IVarConverter.GetVar(int i)
        {
            var L = m_L;
            var t = LuaDLL.lua_type(L, i);
            if (t == LuaTypes.LUA_TNIL) return null;
            if (t == LuaTypes.LUA_TTABLE)
            {
                using (MakeGuard())
                {
                    int idx = GetAbsIndex(i);
                    int cnt = LuaDLL.lua_rawlen(m_L, idx);
                    var v = Array.CreateInstance(elementType, cnt);
                    for (int a = 0; a < cnt; ++a)
                    {
                        LuaDLL.lua_rawgeti(L, idx, a + 1);
                        object o = GetVar(-1, elementType);
                        v.SetValue(o, a);
                    }
                    return v;
                }
            }
            else
            {
                int idx = GetAbsIndex(i);
                int max = LuaDLL.lua_gettop(L);
                int cnt = max - idx + 1;
                var v = Array.CreateInstance(elementType, cnt);
                for (int a = 0; a < cnt; ++a)
                {
                    object o = GetVar(idx + a, elementType);
                    v.SetValue(o, a);
                }
                return v;
            }
        }
    }
    static IVarConverter GetArgConverter(ParameterInfo pi)
    {
        var type = pi.ParameterType;
        if (pi.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
        {
            var etype = type.GetElementType();
            var p = new VarConverter_ParamArray { arrayType = type, elementType = etype };
            return p;
        }
        var converter = GetVarConverter(type);
        return converter;
    }
    static IVarConverter[] emptyConverters = new IVarConverter[0];
    static IVarConverter[] GetArgConverters(MethodInfo m)
    {
        var paramters = m.GetParameters();
        int c = paramters.Length;
        if (c == 0) return emptyConverters;
        var converters = new IVarConverter[c];
        for (int i = 0; i < c; ++i)
        {
            converters[i] = GetArgConverter(paramters[i]);
        }
        return converters;
    }
    static DynMember BuildDynMember(DynType dynType, MemberInfo[] members)
    {
        var member = members[0];
        DynMember dm = null;
        switch (member.MemberType)
        {
            case MemberTypes.Field:
                {
                    var fi = member as FieldInfo;
                    var df = new DynField { dynType = dynType, info = fi };
                    dm = df;
                    df.converter = GetVarConverter(fi.FieldType);
                    df.getter = SafeCall.Make(df.name, df.Getter);
                    df.setter = SafeCall.Make(df.name, df.Setter);
                }
                break;
            case MemberTypes.Property:
                {
                    var pi = member as PropertyInfo;
                    var dp = new DynProperty { dynType = dynType, info = pi, getm = pi.GetGetMethod(), setm = pi.GetSetMethod() };
                    dm = dp;
                    dp.converter = GetVarConverter(pi.PropertyType);
                    if (pi.CanRead) dp.getter = SafeCall.Make(dp.name, dp.Getter);
                    if (pi.CanWrite) dp.setter = SafeCall.Make(dp.name, dp.Setter);
                }
                break;
            case MemberTypes.Method:
                {
                    int c = members.Length;
                    var infos = new MethodInfo[c];
                    IVarConverter[][] converters = new IVarConverter[c][];
                    for (int i = 0; i < c; ++i)
                    {
                        var m = members[i] as MethodInfo;
                        infos[i] = m;
                        converters[i] = GetArgConverters(m);
                    }
                    var md = new DynMethod { dynType = dynType, infos = infos, converters = converters };
                    dm = md;
                    md.call = SafeCall.Make(md.name, md.Invoke);
                }
                break;
        }
        return dm;
    }
    static object[] emptyParam = new object[0];
    static object[] GetArgs(int idx, IVarConverter[] converters)
    {
        int c = converters.Length;
        if (c == 0) return emptyParam;

        object[] args = new object[c];
        for(int i = 0; i < c; ++i)
        {
            args[i] = converters[i].GetVar(idx + i);
        }
        return args;
    }
    public static void PutVar(object v)
    {
        var L = m_L;
        if (v == null)
        {
            LuaDLL.lua_pushnil(L);
            return;
        }
        var type = v.GetType();
        var converter = GetVarConverter(type);
        converter.PutVar(v);
    }
    static bool CheckArgs(int idx, IVarConverter self, IVarConverter[] converters)
    {
        if(self != null)
        {
            if (!self.Check(idx)) return false;
            ++idx;
        }
        int c = converters.Length;
        for(int i = 0; i < c; ++i)
        {
            if (!converters[i].Check(idx + i)) return false;
        }
        return true;
    }
    class DynMethod : DynMember
    {
        internal MethodInfo[] infos;
        internal IVarConverter[][] converters;
        internal int call;
        internal int Invoke(IntPtr l)
        {
            m_L = l;
            int mc = infos.Length;
            if (mc == 1)
            {
                var m = infos[0];
                object self = null;
                int idx = 1;
                if (!m.IsStatic)
                {
                    self = GetSelf(l);
                    idx = 2;
                }
                var args = GetArgs(idx, converters[0]);
                object ret = m.Invoke(self, args);
                if (m.ReturnType == null) return 0;
                PutVar(ret);
                return 1;
            }
            else
            {
                for(int i = 0; i < mc; ++i)
                {
                    var m = infos[i];
                    int idx = 1;
                    IVarConverter selfCvt = null;
                    if (!m.IsStatic)
                    {
                        selfCvt = dynType.converter;
                        idx = 2;
                    }
                    if(!CheckArgs(idx, selfCvt, converters[0]))
                    {
                        continue;
                    }
                    object self = null;
                    if (!m.IsStatic)
                    {
                        self = GetSelf(l);
                    }
                    var args = GetArgs(idx, converters[0]);
                    object ret = m.Invoke(self, args);
                    if (m.ReturnType == null) return 0;
                    PutVar(ret);
                    return 1;
                }
            }
            Debug.LogErrorFormat("no match method " + infos[0].Name);
            return 0;
        }
        internal override int Push()
        {
            LuaObject.pushObject(m_L, true);
            Put(1);
            SafeCall.PushClosure(call);
            Put(infos[0].IsStatic);
            return 4;
        }
    }
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    internal static int GetMember_s(IntPtr l)
    {
        m_L = l;
        Type type = GetTypeof(1);
        DynType dynType = GetDynType(type);
        LuaObject.checkType(m_L, 2, out string name);
        DynType dp = dynType;
        for (; dp != null; dp = dp.baseDynType)
        {
            dp.members.TryGetValue(name, out var m);
            if (m != null)
            {
                return m.Push();
            }
        }
        MemberInfo[] members = null;
        dp = dynType;
        for (; dp != null; dp = dp.baseDynType)
        {
            members = dp.type.GetMember(name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            if (members != null && members.Length > 0)
            {
                var m = BuildDynMember(dp, members);
                return m.Push();
            }
        }
        return 2;
    }
}