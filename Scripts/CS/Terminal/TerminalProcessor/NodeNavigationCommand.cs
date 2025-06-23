using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
public static partial class ShellCore {
	static void Home(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
		ToHome();
	}
	public static event Action<string[]> SCANrunCMD;
    static void Scan(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
		string L = " └─── ", M = " ├─── ", T = "      ", E = " │    ";
		System.Func<bool[], string> getTreePrefix = arr => string.Concat(arr.Select((b, i) => (i == arr.Length - 1 ? (b ? L : M) : (b ? T : E))));
		System.Func<bool[], string> getDescPrefix = arr => string.Concat(arr.Select((b, i) => (b ? T : E)));
		if (!parsedArgs.TryGetValue("-d", out string value)) { value = "1"; parsedArgs["-d"] = value; }
		if (!int.TryParse(value, out int MAX_DEPTH)) { Say("-r", $"Invalid depth: {Util.Format(value, StrType.NUMBER)}"); return; }

		Stack<(NetworkNode, int, bool[])> stack = new([(_currNode, 0, [])]);
		List<NetworkNode> visited = [];
		string output = "";
		while (stack.Count > 0) {
			(NetworkNode node, int tdepth, bool[] depthMarks) = stack.Pop();
			if (tdepth > MAX_DEPTH || visited.Contains(node)) continue;
			NodeAnalysis analyzeResult = node.Analyze();
			output += $"{Util.Format(getTreePrefix(depthMarks), StrType.DECOR)}{Util.Format(analyzeResult.IP, StrType.IP), -39}{Util.Format(analyzeResult.HostName, StrType.HOSTNAME)}\n";
			const int padLength = -40;
			if (parsedArgs.ContainsKey("-v")) {
				string descPrefix = getDescPrefix(depthMarks) + ((tdepth == MAX_DEPTH ? 0 : ((node.ParentNode != null ? 0 : -1) + node.ChildNode.Count)) > 0 ? " │" : "  ");
				output += $"{Util.Format(descPrefix, StrType.DECOR)}  {Util.Format("Display name:", StrType.DECOR),padLength}{Util.Format(analyzeResult.DisplayName, StrType.DISPLAY_NAME)}\n";
				output += $"{Util.Format(descPrefix, StrType.DECOR)}  {Util.Format("Firewall rating:",StrType.DECOR),padLength}{Util.Format($"{analyzeResult.DefLvl}", StrType.DEF_LVL),-2}  {Util.Format("Security:", StrType.DECOR)} {Util.Format($"{analyzeResult.SecType}", StrType.SEC_TYPE)}\n";
				if (!string.IsNullOrWhiteSpace(descPrefix))
					output += $"{Util.Format(descPrefix, StrType.DECOR)}\n";
			}
			visited.Add(node);

			// Use k==1 due to FILO algorithm.
			List<NetworkNode> nextOfQueue = [];
			if (node.ParentNode != null && !visited.Contains(node.ParentNode) && tdepth <= MAX_DEPTH) nextOfQueue.Add(node.ParentNode);
			foreach (NetworkNode child in node.ChildNode) {
				if (visited.Contains(child) || tdepth > MAX_DEPTH) continue;
				nextOfQueue.Add(child);
			}

			int k = 0; foreach (NetworkNode child in nextOfQueue) {
				++k; stack.Push((child, tdepth + 1, [.. depthMarks, k == 1]));
			}
		}
		SCANrunCMD?.Invoke(visited.Select(x => x.HostName).ToArray());
		Say("-n", output);
	}
	public static event Action<CError, string> CONNECTrunCMD;
    /// <summary>
    /// Connect to a node by its hostname or IP address.
    /// </summary>
    /// <param name="parsedArgs">Parsed arguments from the command line.</param>
    /// <param name="positionalArgs">Positional arguments, where the first argument is the target node.</param>
    static void Connect(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", "No hostname provided."); return; }
		string target = positionalArgs[0]; NetworkNode node = null;
        
		if (IsIPv4(target)) {
            node = NetworkManager.GetNodeByIP(target);
            if (node == null) { Say("-r", $"IP not found: {Util.Format(target, StrType.HOSTNAME)}"); CONNECTrunCMD?.Invoke(CError.NOT_FOUND, target); return; }
        } else if (CurrNode.ParentNode != null && CurrNode.ParentNode.HostName == target) {
            node = CurrNode.ParentNode;
        } else {
            node = CurrNode.ChildNode.FindLast(n => n.HostName == target);
            if (node == null) { Say("-r", $"Host not found: {Util.Format(target, StrType.HOSTNAME)}"); CONNECTrunCMD?.Invoke(CError.NOT_FOUND, target); return; }
        }

        if (!node.RequestConnectPermission()) { Say("-r", "Connection denied by the corresponding node."); CONNECTrunCMD?.Invoke(CError.NO_PERMISSION, target); return; }
        CurrNode = node;
        CONNECTrunCMD?.Invoke(CError.OK, target);
    }
    static void Sector(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
		bool connectedOnly = parsedArgs.ContainsKey("-c") || parsedArgs.ContainsKey("--connected");
		int sectorLevel = parsedArgs.ContainsKey("-l") ? int.Parse(parsedArgs["-l"]) : -1;
        string[] sectorNames = NetworkManager.GetSectorNames(connectedOnly, sectorLevel);
        string output = "";
		for (int i = 0; i < sectorNames.Length; ++i) {
			if (sectorNames[i] == null) { continue; }
            output += $"{Util.Format(sectorNames[i], StrType.SECTOR), -40}";
		}
		Say(output);
	}
	public static event Action<string, CError> LinkedToSector;
    static void Link(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", "No sector name provided."); return; }
        CError status = NetworkManager.ConnectToSector(positionalArgs[0]);
		string msg = status switch {
			CError.OK => $"Linkto successfully: {Util.Format(positionalArgs[0], StrType.SECTOR)}",
			CError.INVALID => $"Linkto failed: {Util.Format(positionalArgs[0], StrType.SECTOR)}. Sector is null. This behavior is unexpected and should be reported to the developer.",
			CError.REDUNDANT => $"Linkto failed: {Util.Format(positionalArgs[0], StrType.SECTOR)}. Already linked.",
			CError.NOT_FOUND => $"Linkto failed: {Util.Format(positionalArgs[0], StrType.SECTOR)}. Sector not found",
			_ => $"Unexpected error: {status}. Please report to the developer. Linkto failed: {Util.Format(positionalArgs[0], StrType.SECTOR)}",
		};
		if (status == 0) { Say(msg); LinkedToSector?.Invoke(positionalArgs[0], status); } else { Say("-r", msg); }
	}
	public static event Action<string, CError> UnlinkedToSector;
    static void Unlink(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
		string msg; CError err = CError.OK;
		if (positionalArgs[0] == "*") {
			// Disconnect from all sectors
			string[] secNames = NetworkManager.GetSectorNames(true);
			for (int i = 0; i < secNames.Length; ++i) {
				err = NetworkManager.DisconnectFromSector(secNames[i]);
				msg = err switch {
                    CError.OK => "",
                    CError.INVALID => $"Unlink failed: {Util.Format(secNames[i], StrType.SECTOR)}. Sector is null. This behavior is unexpected and should be reported to the developer.",
                    CError.NOT_FOUND => $"Unlink failed: {Util.Format(secNames[i], StrType.SECTOR)}. Sector not found.",
                    CError.REDUNDANT => $"Unlink failed: {Util.Format(secNames[i], StrType.SECTOR)}. Not connected.",
                    _ => $"Unexpected error: {err}. Please report to the developer. Unlink failed: {Util.Format(secNames[i], StrType.SECTOR)}",
                };
				if (err == CError.OK && msg != "") { Say(msg); } else { Say("-r", msg); }
            }
			UnlinkedToSector?.Invoke("*", CError.OK);
            return;
		}
		err = NetworkManager.DisconnectFromSector(positionalArgs[0]);
		msg = err switch {
			CError.OK => $"Disconnected from sector: {Util.Format(positionalArgs[0], StrType.SECTOR)}",
            CError.INVALID => $"Unlink failed: {Util.Format(positionalArgs[0], StrType.SECTOR)}. Sector is null. This behavior is unexpected and should be reported to the developer.",
			CError.NOT_FOUND => $"Unlink failed: {Util.Format(positionalArgs[0], StrType.SECTOR)}. Sector not found.",
			CError.REDUNDANT => $"Unlink failed: {Util.Format(positionalArgs[0], StrType.SECTOR)}. Not connected.",
			_ => $"Unexpected error: {err}. Please report to the developer. Unlink failed: {Util.Format(positionalArgs[0], StrType.SECTOR)}",
		};
		if (err == CError.OK && msg != "") { Say(msg); } else { Say("-r", msg); }
		UnlinkedToSector?.Invoke(positionalArgs[0], err);
    }

	public static void ToHome() {
		CurrNode = NetworkManager.PlayerNode;
	}
	public static CError ConnectNode(string IP="") {
        if (string.IsNullOrEmpty(IP)) return CError.MISSING;
		if (!IsIPv4(IP)) return CError.INVALID;
        NetworkNode node = NetworkManager.GetNodeByIP(IP);
        if (node == null) return CError.NOT_FOUND;
		if (!node.RequestConnectPermission()) return CError.NO_PERMISSION;
        CurrNode = node;
		return CError.OK;
    }
	public static string[] Scan(string IP="", int MAX_DEPTH=1) {
		if (string.IsNullOrEmpty(IP)) IP = NetworkManager.PlayerNode.IP;
        NetworkNode node = NetworkManager.GetNodeByIP(IP);
		if (node == null) return null;
        return node.ChildNode
			.Select(a => a.IP)
			.Concat(node.ParentNode != null ? [node.ParentNode.IP] : [])
			.ToArray();
    }
    static bool IsIPv4(string ip) {
        return IPAddress.TryParse(ip, out IPAddress address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
    }
}
