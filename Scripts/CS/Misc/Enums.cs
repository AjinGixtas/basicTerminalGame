using System;
[Flags]
public enum NodeType {
    PLAYER =   1<<0, //Player
    BUSINESS = 1<<1, //Money+Stock
    CORP =     1<<2, //Money+Stock+Alliance
    FACTION =  1<<3, //Money+Alliance
    DRIFT =    1<<4, //One-time-hack
    VM =       1<<5, //Virtual Machine
}
// NOSEC = [0, 1)
// LOSEC = [1, 3)
// MISEC = [3, 6)
// HISEC = [7, 10)
// MASEC = [10, inf)
// security_point is dressed in these enum to introduce ambugitiy
// 10-security_point = retaliation level.
[Flags]
public enum SecurityType {
    NOSEC = 0, LOSEC = 1, MISEC = 2, HISEC = 3, MASEC = 4
}
public enum Cc {
    R = 2,  G = 3,  B = 4,
    r = 5,  g = 6,  b = 7,
    c = 8,  m = 9,  y = 10,
    C = 17, M = 18, Y = 19,
    gB = 11, rB = 12, rG = 13,
    bG = 14, bR = 15, gR = 16,

    _ = 0,
    w = 20,
    LB = 21,
    LG = 22,
    LC = 23,
    LR = 24,
    LM = 25,
    LY = 26,
    W = 1,
}
public enum StrType {
    DECOR = 0,
    HOSTNAME = 1,
    DISPLAY_NAME = 2,
    DESC = 3,
    SYMBOL = 4,
    UNIT = 5,
    SEC_LVL = 6,
    DEF_LVL = 7,
    NUMBER = 8,
    IP = 9,
    DIR = 10,
    FILE = 11,
    CMD_FUL = 12,
    ERROR = 13,
    HEADER = 14,
    MONEY = 15,
    USERNAME = 16,
    WARNING = 17,
    PART_SUCCESS = 18,
    FULL_SUCCESS = 19,
    CMD_FLAG = 20,
    UNKNOWN_ERROR = 21,
    SEC_TYPE = 22,
    CMD_ARG = 23,
    CMD_ACT = 24,
    SECTOR = 25,
    MINERAL = 26
}
[Flags]
public enum LockType {
    I4X = 1<<0,
    F8X = 1<<1,
    I16 = 1<<2,
    P16X= 1<<3,
    P90 = 1<<4,
    M2  = 1<<5,
    M3  = 1<<6,
    C0  = 1<<7,
    C1  = 1<<8,
    C3  = 1<<9
}
public enum CError {
    OK = 0,
    NOT_FOUND = 1,
    DUPLICATE = 2,
    INVALID = 3,
    REDUCDANT = 4,
    UNKNOWN = 5,
    INCORRECT = 6,
    MISSING = 7,
    NO_PERMISSION = 8,
}