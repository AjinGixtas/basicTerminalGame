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
    // Short codes
    R = 0, G = 1, B = 2, C = 3, M = 4, Y = 5, ___ = 6, RGB = 7,
    // Named colors
    ORANGE = 10, PINK = 11, LIME = 12, AQUA = 13,
    VIOLET = 14, PURPLE = 15, TEAL = 16, INDIGO = 17,
    GOLD = 18, BROWN = 19, GRAY = 20, NAVY = 21,
    SKY = 22,
    // Fun extras
    HOTPINK = 30, LAVENDER = 31, MINT = 32
}