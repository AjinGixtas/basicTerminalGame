---@class CrackModule
local CrackModule = {}


---@return number
function CrackModule.Begin() end


---@param flagKeyPairs table<string, string>
---@return any[][]
function CrackModule.AttackNode(flagKeyPairs) end


---@return nil
function CrackModule.End() end

return CrackModule
