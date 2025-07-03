using Godot;

public class DriftSector : Sector {
	static readonly string[] DRIFT_NODE_NAMES = StringExtensions.Split(FileAccess.Open("res://Utilities/TextFiles/ServerNames/DriftNode.txt", FileAccess.ModeFlags.Read).GetAsText(), "\n", false);
	static readonly string[] DRIFT_SECTOR_NAMES = StringExtensions.Split(FileAccess.Open("res://Utilities/TextFiles/ServerNames/DriftSector.txt", FileAccess.ModeFlags.Read).GetAsText(), "\n", false);

	SecLvl _sectorLevel = SecLvl.NOSEC;
	public SecLvl SectorLevel => _sectorLevel;

    bool _lockedDown = false;
	public bool LockedDown { 
		get => _lockedDown; 
		set { 
			if (_lockedDown) return; // Prevents re-locking
			_lockedDown = value; 
		} 
	}

	static bool read = false;
	public DriftSector() {
        Name = GenSectorName();
		int type = GD.RandRange(0, 3);
		_sectorLevel = GD.Randf() < .02 ? SecLvl.NOSEC : (SecLvl)GD.RandRange(1, 4);
		switch (type) {
			case 0: GenerateBusNetwork(_sectorLevel); break;
			case 1: GenerateStarNetwork(_sectorLevel); break;
			case 2: GenerateVineNetwork(_sectorLevel); break;
			case 3: GenerateTreeNetwork(_sectorLevel); break;
			default: GD.PrintErr("Invalid network type"); break;
		}
		MarkIntializationCompleted();
	}
	~DriftSector() {
		if (Util.HaveFinalWord)
			GD.Print($"DriftSector {Name} is being destroyed");
	}
	
	int AddSurfaceNode(DriftNode node) { if (_isIntialized) return 1; SurfaceNodes.Add(node); return 0; }
	public int MarkIntializationCompleted() { _isIntialized = true; return 0; }
	void GenerateBusNetwork(SecLvl secLvl) {
		if (_isIntialized) return;
		int layer = 3, node = 3;
		(string displayName, string hostName) = GenNodeName();
		DriftNode chainNode = new(hostName, displayName, NetworkManager.GetRandomIP(), null, this, secLvl);
		AddSurfaceNode(chainNode);
		for (int i = 0; i < node-1; ++i) {
			(displayName, hostName) = GenNodeName();
			AddSurfaceNode(new(hostName, displayName, NetworkManager.GetRandomIP(), null, this, secLvl));
		}
		for (int i = 0; i < layer-1; i++) {
			for (int j = 0; j < node; j++) {
				(displayName, hostName) = GenNodeName();
				new DriftNode(hostName, displayName, NetworkManager.GetRandomIP(), chainNode, this, secLvl);
			}
			chainNode = chainNode.ChildNode[0] as DriftNode;
		}
	}
	void GenerateStarNetwork(SecLvl secLvl) {
		if (_isIntialized) return;
		int node = 5;
		for (int i = 0; i < node; ++i) {
			(string displayName, string hostName) = GenNodeName();
			AddSurfaceNode(new(hostName, displayName, NetworkManager.GetRandomIP(), null, this, secLvl));
		}
	}
	void GenerateVineNetwork(SecLvl secLvl) {
		if (_isIntialized) return;
		int vine = 2, node = 3;
		for(int i = 0; i < vine; ++i) {
			(string displayName, string hostName) = GenNodeName();
			DriftNode chainNode = new(hostName, displayName, NetworkManager.GetRandomIP(), null, this, secLvl);
			AddSurfaceNode(chainNode);
			for (int j = 0; j < node-1; ++j) {
				(displayName, hostName) = GenNodeName();
				chainNode = new(hostName, displayName, NetworkManager.GetRandomIP(), chainNode, this, secLvl);
			}
		}
	}
	void GenerateTreeNetwork(SecLvl secLvl) {
		if (_isIntialized) return;
		int layer = 3, node = 2;
		for(int i = 0; i < 1; ++i) {
			(string displayName, string hostName) = GenNodeName();
			DriftNode surfaceNode = new(hostName, displayName, NetworkManager.GetRandomIP(), null, this, secLvl);
			GenerateTree(surfaceNode, 1, layer, node);
			AddSurfaceNode(surfaceNode);
		}
		void GenerateTree(DriftNode node, int depth, int maxDepth, int childCount) {
			if (depth >= maxDepth) return;
			for (int i = 0; i < childCount; ++i) {
				(string displayName, string hostName) = GenNodeName();
				DriftNode childNode = new(hostName, displayName, NetworkManager.GetRandomIP(), node, this, secLvl);
				GenerateTree(childNode, depth + 1, maxDepth, childCount);
			}
		}
	}
	
	static string GenSectorName() {
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string sb = DRIFT_SECTOR_NAMES[GD.RandRange(0, DRIFT_SECTOR_NAMES.Length - 1)] + "_";
		for (int i = 0; i < 6; i++) {
			sb += chars[GD.RandRange(0, chars.Length - 1)];
		}
		return sb;
	}
	static (string, string) GenNodeName() {
		string baseName = DRIFT_NODE_NAMES[GD.RandRange(0, DRIFT_NODE_NAMES.Length - 1)], suffix = Util.GenerateRandomString(6);
		return ($"{char.ToUpper(baseName[0])}{baseName[1..]} {suffix.ToUpper()}", $"{baseName}_{suffix}");
	}
}
