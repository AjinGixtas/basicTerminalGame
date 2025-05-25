using System;
using System.Collections.Generic;

public abstract class Sector {
    public string Name;
    protected bool _isIntialized = false;
    protected readonly List<NetworkNode> SurfaceNodes = [];
}