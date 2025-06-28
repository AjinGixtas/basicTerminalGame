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
/// <summary>
/// Represents standard color codes used for UI formatting.
/// </summary>
public enum Cc {
    /// <summary>Default black: <c>#000000</c></summary>
    _ = 0,

    /// <summary>Bright white: <c>#ffffff</c></summary>
    W = 1,

    /// <summary>Bright red: <c>#ff0000</c></summary>
    R = 2,

    /// <summary>Bright green: <c>#00ff00</c></summary>
    G = 3,

    /// <summary>Bright blue: <c>#0000ff</c></summary>
    B = 4,

    /// <summary>Dark red: <c>#cc0000</c></summary>
    r = 5,

    /// <summary>Dark green: <c>#00cc00</c></summary>
    g = 6,

    /// <summary>Dark blue: <c>#0000cc</c></summary>
    b = 7,

    /// <summary>Dark teal (cyan): <c>#005C5C</c></summary>
    c = 8,

    /// <summary>Deep magenta: <c>#cc00cc</c></summary>
    m = 9,

    /// <summary>Olive yellow: <c>#cccc00</c></summary>
    y = 10,

    /// <summary>Aqua blue: <c>#00aaff</c></summary>
    gB = 11,

    /// <summary>Violet: <c>#aa00ff</c></summary>
    rB = 12,

    /// <summary>Lime green: <c>#aaff00</c></summary>
    rG = 13,

    /// <summary>Mint green: <c>#00ffaa</c></summary>
    bG = 14,

    /// <summary>Hot pink: <c>#ff00aa</c></summary>
    bR = 15,

    /// <summary>Amber (orange-yellow): <c>#ffaa00</c></summary>
    gR = 16,

    /// <summary>Bright cyan: <c>#00ffff</c></summary>
    C = 17,

    /// <summary>Bright magenta: <c>#ff00ff</c></summary>
    M = 18,

    /// <summary>Bright yellow: <c>#ffff00</c></summary>
    Y = 19,

    /// <summary>Dark gray: <c>#666666</c></summary>
    w = 20,

    /// <summary>Light blue (periwinkle): <c>#6666ff</c></summary>
    LB = 21,

    /// <summary>Light green: <c>#66ff66</c></summary>
    LG = 22,

    /// <summary>Light cyan: <c>#adffff</c></summary>
    LC = 23,

    /// <summary>Light coral (light red): <c>#ff6666</c></summary>
    LR = 24,

    /// <summary>Light magenta: <c>#ff66ff</c></summary>
    LM = 25,

    /// <summary>Light yellow: <c>#ffff66</c></summary>
    LY = 26
}
public enum StrSty {
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
    T_MINERAL = 26,
    G_MINERAL = 27,
    GAME_WINDOW = 28,
    CODE_MODULE = 29,
}
public enum LockType {
    I4X = 0,
    F8X = 1,
    I16 = 2,
    P16X= 3,
    P90 = 4,
    M2  = 5,
    M3  = 6,
    C0  = 7,
    C1  = 8,
    C3  = 9
}
public enum CError {
    OK = 0,
    NOT_FOUND = 1,
    DUPLICATE = 2,
    INVALID = 3,
    REDUNDANT = 4,
    UNKNOWN = 5,
    INCORRECT = 6,
    MISSING = 7,
    NO_PERMISSION = 8,
    INSUFFICIENT = 9
}