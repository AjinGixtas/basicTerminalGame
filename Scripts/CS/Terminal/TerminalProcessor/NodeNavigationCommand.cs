using System.Collections.Generic;
using System.Linq;
using System.Net;
public static partial class TerminalProcessor {
	static void Home(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
		ToHome();
	}
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
			(NetworkNode node, int depth, bool[] depthMarks) = stack.Pop();
			if (depth > MAX_DEPTH || visited.Contains(node)) continue;
			NodeAnalysis analyzeResult = node.Analyze();
			output += $"{Util.Format(getTreePrefix(depthMarks), StrType.DECOR)}{Util.Format(analyzeResult.IP, StrType.IP), -39}{Util.Format(analyzeResult.HostName, StrType.HOSTNAME)}\n";
			const int padLength = -40;
			if (parsedArgs.ContainsKey("-v")) {
				string descPrefix = getDescPrefix(depthMarks) + ((depth == MAX_DEPTH ? 0 : ((node.ParentNode != null ? 0 : -1) + node.ChildNode.Count)) > 0 ? " │" : "  ");
				output += $"{Util.Format(descPrefix, StrType.DECOR)}  {Util.Format("Display name:", StrType.DECOR),padLength}{Util.Format(analyzeResult.DisplayName, StrType.DISPLAY_NAME)}\n";
				output += $"{Util.Format(descPrefix, StrType.DECOR)}  {Util.Format("Firewall rating:",StrType.DECOR),padLength}{Util.Format($"{analyzeResult.DefLvl}", StrType.DEF_LVL),-2}  {Util.Format("Security:", StrType.DECOR)} {Util.Format($"{analyzeResult.SecType}", StrType.SEC_TYPE)}\n";
				if (!string.IsNullOrWhiteSpace(descPrefix))
					output += $"{Util.Format(descPrefix, StrType.DECOR)}\n";
			}
			visited.Add(node);

			// Use k==1 due to FILO algorithm.
			List<NetworkNode> nextOfQueue = [];
			if (node.ParentNode != null && !visited.Contains(node.ParentNode) && depth <= MAX_DEPTH) nextOfQueue.Add(node.ParentNode);
			foreach (NetworkNode child in node.ChildNode) {
				if (visited.Contains(child) || depth > MAX_DEPTH) continue;
				nextOfQueue.Add(child);
			}

			int k = 0; foreach (NetworkNode child in nextOfQueue) {
				++k; stack.Push((child, depth + 1, [.. depthMarks, k == 1]));
			}
		}
		Say("-n", output);
	}
	static void Connect(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
		if (positionalArgs.Length == 0) { Say("-r", $"No hostname provided."); return; }
		
		if (IsIPv4(positionalArgs[0])) { // Check if the input is a valid IPv4 address
			NetworkNode nodeDNS = NetworkManager.QueryDNS(positionalArgs[0]);
			if (nodeDNS != null) {
				if (!nodeDNS.OwnedByPlayer) {
					Say("-r", "Node not owned by you. Cannot connect to it."); return;
				}
				CurrNode = NetworkManager.QueryDNS(positionalArgs[0]); return;
			} else { Say("-r", $"IP not found: {Util.Format(positionalArgs[0], StrType.HOSTNAME)}"); return;  }
		}
		
		if (!CurrNode.OwnedByPlayer) {
			Say("-r", $"{Util.Format(CurrNode.HostName, StrType.HOSTNAME)} node is not owned by you. Cannot hop beyond it."); return;
		}
		NetworkNode parentNode = CurrNode.ParentNode;
		if (parentNode != null && parentNode.HostName == positionalArgs[0]) {

			CurrNode = parentNode; return; 
		}

		NetworkNode node = CurrNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]);
		if (node == null) { Say("-r", $"Host not found: {Util.Format(positionalArgs[0], StrType.HOSTNAME)}"); return; }
		
		CurrNode = node;
		static bool IsIPv4(string ip) {
			return IPAddress.TryParse(ip, out IPAddress address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
		}
	}
	static void Analyze(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
		if (positionalArgs.Length == 0) { positionalArgs = [CurrNode.HostName]; }
		if (CurrNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]) == null
			&& (CurrNode.ParentNode != null && CurrNode.ParentNode.HostName != positionalArgs[0])
			&& CurrNode.HostName != positionalArgs[0]) {
			Say("-r", $"Host not found: {positionalArgs[0]}");
			return;
		}
		NetworkNode analyzeNode = null;
		if (CurrNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]) != null) analyzeNode = CurrNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]);
		else if (CurrNode.ParentNode != null && CurrNode.ParentNode.HostName == positionalArgs[0]) analyzeNode = CurrNode.ParentNode;
		else if (CurrNode.HostName == positionalArgs[0]) analyzeNode = CurrNode;
		const int padLength = -40;
		Say("-tl", $@"
{Util.Format("▶ Node Info", StrType.HEADER)}
{Util.Format("Host name:", StrType.DECOR),padLength}{Util.Format(analyzeNode.HostName, StrType.HOSTNAME)}
{Util.Format("IP address:", StrType.DECOR),padLength}{Util.Format(analyzeNode.IP, StrType.IP)}
{Util.Format("Display name:", StrType.DECOR),padLength}{Util.Format(analyzeNode.DisplayName, StrType.DISPLAY_NAME)}
{Util.Format("▶ Classification", StrType.HEADER)}
{Util.Format("Firewall rating:", StrType.DECOR),padLength}{Util.Format($"{analyzeNode.DefLvl}", StrType.DEF_LVL)}
{Util.Format("Security level:", StrType.DECOR),padLength}{Util.Format($"{analyzeNode.SecType}", StrType.SEC_TYPE)}
");
		if (analyzeNode is DriftNode) {
			Say("This node is part of a temporal sector. There is nothing useful long term.");
			return;
		}
		if (!analyzeNode.OwnedByPlayer) {
			Say($"Crack this node security system to get further access.");
			return;
		}
		if (analyzeNode is ScriptedNetworkNode) {
			ScriptedNetworkNode sNode = analyzeNode as ScriptedNetworkNode;
			Say("-tl", $@"
{Util.Format("▶ GC miner detail", StrType.HEADER)}
{Util.Format("Transfer batch:", StrType.DECOR),padLength}{Util.Format($"{sNode.HackFarm.HackLvl}", StrType.NUMBER)} ({Util.Format($"{sNode.HackFarm.CurHack}", StrType.MONEY)})
{Util.Format("Transfer speed:", StrType.DECOR),padLength}{Util.Format($"{sNode.HackFarm.TimeLvl}", StrType.NUMBER)} ({Util.Format($"{sNode.HackFarm.CurTime}", StrType.UNIT, "s")})
{Util.Format("Mining speed:", StrType.DECOR),padLength}{Util.Format($"{sNode.HackFarm.GrowLvl}", StrType.NUMBER)} ({Util.Format(Util.Format($"{sNode.HackFarm.CurGrow}", StrType.MONEY), StrType.UNIT, "/s")})
");
		}
	}
	static void Sector(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
		bool connectedOnly = parsedArgs.ContainsKey("-c") || parsedArgs.ContainsKey("--connected");
		string[] sectorNames = connectedOnly ? NetworkManager.GetConnectedSectorNames() : NetworkManager.GetSectorNames();
		string output = "";
		for (int i = 0; i < sectorNames.Length; ++i) {
			if (sectorNames[i] == null) { continue; }
			output += $"{Util.Format(sectorNames[i], StrType.SECTOR), -40}";
		}
		Say(output);
	}
	static void Link(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
		for (int i = 0; i < positionalArgs.Length; ++i) { 
			int status = NetworkManager.ConnectToSector(positionalArgs[i]);
			string msg = status switch {
				0 => $"Linkto successfully: {Util.Format(positionalArgs[i], StrType.SECTOR)}",
				1 => $"Linkto failed: {Util.Format(positionalArgs[i], StrType.SECTOR)}. Sector is null. This behavior is unexpected and should be reported to the developer.",
				2 => $"Linkto failed: {Util.Format(positionalArgs[i], StrType.SECTOR)}. Already linked.",
				3 => $"Linkto failed: {Util.Format(positionalArgs[i], StrType.SECTOR)}. Sector not found",
				_ => $"Unknown error code: {status}. Failed to linkto sector: {Util.Format(positionalArgs[i], StrType.SECTOR)}",
			};
			if (status == 0) { Say(msg); } else { Say("-r", msg); }
		}
	}
	static void Unlink(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
		if (positionalArgs[0] == "*") {
			// Disconnect from all sectors
			while (NetworkManager.GetConnectedSectorNames().Length > 0) {
				int i = 0;
				int status = NetworkManager.DisconnectFromSector(NetworkManager.GetConnectedSectorNames()[i]);
				string msg = status switch {
					0 => $"Disconnected from sector: {Util.Format(NetworkManager.GetConnectedSectorNames()[i], StrType.SECTOR)}",
					1 => $"Failed to disconnect from sector: {Util.Format(NetworkManager.GetConnectedSectorNames()[i], StrType.SECTOR)}. Sector not found.",
					2 => $"Failed to disconnect from sector: {Util.Format(NetworkManager.GetConnectedSectorNames()[i], StrType.SECTOR)}. Not connected.",
					_ => $"Unknown error code: {status}. Failed to disconnect from sector: {Util.Format(NetworkManager.GetConnectedSectorNames()[i], StrType.SECTOR)}",
				};
                if (status == 0) { Say(msg); } else { Say("-r", msg); }
            }
			return;
		}
		for (int i = 0; i < positionalArgs.Length; ++i) {
			int status = NetworkManager.DisconnectFromSector(positionalArgs[i]);
			string msg = status switch {
				0 => $"Disconnected from sector: {Util.Format(positionalArgs[i], StrType.SECTOR)}",
				1 => $"Failed to disconnect from sector: {Util.Format(positionalArgs[i], StrType.SECTOR)}. Sector not found.",
				2 => $"Failed to disconnect from sector: {Util.Format(positionalArgs[i], StrType.SECTOR)}. Not connected.",
				_ => $"Unknown error code: {status}. Failed to disconnect from sector: {Util.Format(positionalArgs[i], StrType.SECTOR)}",
			};
			if (status == 0) { Say(msg); } else { Say("-r", msg); }
		}
	}
	public static void ToHome() {
		CurrNode = NetworkManager.PlayerNode;
	}
}
