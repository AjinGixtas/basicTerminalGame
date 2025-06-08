---@class NetworkModule
local NetworkModule = {}


---@param sectorName string
---@return any
function NetworkModule.LinkSector(sectorName) end


---@param sectorName string
---@return any
function NetworkModule.UnlinkSector(sectorName) end


---@param verbose boolean
---@param IP string
---@param MAX_DEPTH number
---@return string[]
function NetworkModule.Scan(verbose, IP, MAX_DEPTH) end


---@param connectedOnly boolean
---@return string[]
function NetworkModule.Sector(connectedOnly) end

return NetworkModule
