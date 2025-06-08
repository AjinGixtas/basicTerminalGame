---@class FileModule
local FileModule = {}


---@param path string
---@return string[]
function FileModule.ListFiles(path) end


---@param path string
---@return number
function FileModule.MkF(path) end


---@param path string
---@return number
function FileModule.RmF(path) end


---@param path string
---@return boolean
function FileModule.ExistF(path) end


---@param path string
---@return number
function FileModule.MkDir(path) end


---@param path string
---@return number
function FileModule.RmDir(path) end


---@param path string
---@return boolean
function FileModule.ExistDir(path) end


---@param path string
---@param content string
---@param mode any
---@return any
function FileModule.WriteF(path, content, mode) end


---@param path string
---@return string
function FileModule.ReadF(path) end


---@param path string
---@return any
function FileModule.OpenF(path) end

return FileModule
