local q_GetMember = Lua_Reg.GetMember
local q_Typeof = Lua_Reg.Typeof
local q_type = type
local q_rawset = rawset

local metatable = {
	__index = function(tb, name)
		local info = tb.__info
		local fn = info[name]
		if fn then
			if q_type(fn) == "function" then
				return fn
			end
			return fn[1](tb.__data)
		end
		local t, g, s = q_GetMember(info.__type, name)
		if t == 0 then
			error("bad member name"..name)
		end
		if t == 1 then
			info[name] = g
			return g
		end

		info[name] = {g,s}
		return g(tb.__data)
	end,
	__newindex = function(tb, name, value)
		local info = tb.__info
		local fn = info[name]
		if fn then
			fn[2](tb.__data, value)
			return
		end
		local t, g, s = q_GetMember(info.__type, name)
		if t == 0 then
			error("bad member name"..name)
		end
		if t == 1 then
			error("bad property name "..name)
		end

		q_rawset(info, name, {g, s})
		return s(nil, value)
	end
}

function DynamicObject(obj)
	if not obj then return end

	local tp = q_Typeof(obj)
	if not tp then return end

	local name = tp.AssemblyQualifiedName
	local info = infos[name]
	if not info then
		info = {__type = tp}
		infos[name] = info
	end

	local tb = {
		__data = obj,
		__info = info,
	}
	q_setmetatable(tb, metatable)
end

local class_metatable = {
	__index = function(info, name)
		local fn = q_rawget(info, name)
		if fn then
			if q_type(fn) == "function" then
				return fn
			end
			return fn[1]()
		end
		local t, g, s = q_GetMember(info.__type, name)
		if t == 0 then
			error("bad member name " .. name)
		end
		if t == 1 then
			q_rawset(info, name, g)
			return g
		end

		q_rawset(info, name, {g, s})
		return g()
	end,
	__newindex = function(info, name, value)
		local fn = q_rawget(info, name)
		if fn then
			fn[2](nil, value)
			return
		end
		local t, g, s = q_GetMember(info.__type, name)
		if t == 0 then
			error("bad member name " .. name)
		end
		if t == 1 then
			error("bad property name" .. name)
		end

		q_rawset(info, name, {g, s})
		return s(nil, value)
	end,
}

local class_infos = {}

function DynamicClass(cls)
	if not cls then return end

	local tp = q_Typeof(cls)
	if not tp then return end

	local name = tp.AssemblyQualifiedName
	local tb = class_infos[name]
	if tb ~= nil then return tb end

	tb = {__type = tp}
	class_infos[name] = tb
	q_setmetatable(tb, class_metatable)
end

function DynamicInvoke(obj, name, ...)
	local t,f,s = q_GetMember(obj, name)
	if s then
		return f(...)
	else
		return f(obj, ...)
	end
end

function DynamicSet(obj, name, value)
	local t, g, s = q_GetMember(obj, name)
	s(obj, value)
end

function DynamicGet(obj, name)
	local t, g, s = q_GetMember(obj, name)
	return g(obj)
end
--[[
测试用例：
-- 静态函数调用和读取返回值
LuaDynamic.Invoke("UnityEngine.Debug,UnityEngine", "Log", "11111111111111111111111111111111")
local tb = DynamicInvoke("Lua2CS", "IsTagMatchScene")
if tb ~= nil then
	print("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")
end
-- Field和Property读取和写入
print(DynamicSet(self.gameObject, "name"))
DynamicSet(self.gameObject, "name", "wahaha")
-- 成员方法的读取有写入
local bind = UIHelper.GetComponent(self.gameObject, "Lua_DelegateTest")
local tb = DynamicInvoke(bind, "TestMethod", 1, 2)
if tb ~= nil then
	print(tb)
end
local bind = UIHelper.GetComponent(self.gameObject, "Lua_DelegateTest")
local tb = DynamicInvoke(bind, "TestMethod", "没有返回值",100, {10,20})
if tb ~= nil then
	print(tb)
end
]]