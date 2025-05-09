using System;
[Flags]
public enum NetworkNodeType {
    PLAYER =   0, //Player
    PERSON =   1, //Money
    BUSINESS = 2, //Money+Stock
    CORP =     3, //Money+Stock+Alliance
    FACTION =  4, //Money+Alliance
    HONEYPOT = 5, //Lore
    MINER =    6, //Money+Contestable
    ROUGE =    7  //Money+Retaliation+Constestor
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

    ___ = 0,
    rgb = 20,
    LB = 21,
    LG = 22,
    LC = 23,
    LR = 24,
    LM = 25,
    LY = 26,
    RGB = 1,
}