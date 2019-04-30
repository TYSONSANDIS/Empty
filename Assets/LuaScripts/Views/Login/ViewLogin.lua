--[[
	@desc:ViewLogin
]]

local ViewLogin = Class("ViewLogin", ViewBase)

-- 构造 数据初始化
local function Ctor(self, gameObject)
	ViewLogin.super.Ctor(self, gameObject)
end

-- 创建 UI控件初始化
local function Create(self, data)
	ViewLogin.super.Create(self)
end

-- 关闭
local function Close(self)
	ViewLogin.super.Close(self)
end

-- BtnLogin的监听事件
local function OnBtnLogin(self)
	ViewManager:Instance():Add("Views.Common.ViewCommonTips", 1000)
end

ViewLogin.Ctor = Ctor
ViewLogin.Create = Create
ViewLogin.Close = Close
ViewLogin.OnBtnLogin = OnBtnLogin

return ViewLogin
