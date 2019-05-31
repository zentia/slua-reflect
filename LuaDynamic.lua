local q_getmetatable = getmetatable
local q_setmetatable = setmetatable
local q_GetMember = Lua2CS.GetMember
local q_Typeof = Lua2CS.Typeof
LuaDynamic = LuaDynamic or {}

function LuaDynamic.DynamicInvoke(obj, name, ...)
	local t,f,s = q_GetMember(obj, name)
	if s then
		return f(...)
	else
		return f(obj, ...)
	end
end

function LuaDynamic.Invoke(className, memberName, ...)
	local tp = q_Typeof(className)
	if tp ~= nil then
		local q, f, s = q_GetMember(tp, memberName)
		if q == 1 then
			return f(...)
		end
	end
end

function LuaDynamic.PropertyOrField(obj, name, get, ...)
	local getter,setter = q_GetMember(obj, name)
	if get then
		return getter(obj,...)
	else
		return setter(obj,...)
	end
end

return LuaDynamic
--[[
测试用例：
-- 静态函数调用和读取返回值
LuaDynamic.Invoke("UnityEngine.Debug,UnityEngine", "Log", "11111111111111111111111111111111")
local tb = LuaDynamic.Invoke("Lua2CS", "IsTagMatchScene")
if tb ~= nil then
	print("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")
end
-- Property读取和写入
local tb = LuaDynamic.PropertyOrField(self.gameObject, "name", true)
if tb ~= nil then
	print(tb)
end
LuaDynamic.PropertyOrField(self.gameObject, "name", false, "wahaha")
-- Field读取和写入
local bind = UIHelper.GetComponent(self.gameObject, "LuaBind")
local tb = LuaDynamic.PropertyOrField(bind, "ScriptPath", false, "test")
if tb ~= nil then
	print(tb)
end
]]