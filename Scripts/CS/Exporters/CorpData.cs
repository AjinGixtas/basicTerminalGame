using Godot;

public partial class CorpData : Node
{
    CorpNode[] corps = [
        new("alpine", "Alpine International", "240.3.82.179", 0, 0, NetworkNodeType.CORP, null),
        new("kaguo", "Kaguo Energy Inc", "12.18.149.16", 0, 0, NetworkNodeType.CORP, null),
        new("infeido", "Infeido Trade & Finance LLC", "6.214.81.163", 0, 0, NetworkNodeType.CORP, null),
        new("matok", "Matok Gloud Works", "198.210.181.7", 0, 0, NetworkNodeType.CORP, null),
        new("focuron", "Focuron Technology Inc.", "73.95.187.240", 0, 0, NetworkNodeType.CORP, null),
        new("veltri", "Veltrionics Global Systems", "91.188.173.198", 0, 0, NetworkNodeType.CORP, null),
        new("zunexa", "Zunexa Capital Partners", "253.117.16.25", 0, 0, NetworkNodeType.CORP, null),
        new("aure", "Aure Photonics Ltd", "88.173.216.88", 0, 0, NetworkNodeType.CORP, null),
        new("braylith", "Braylith Industries Corp", "66.95.238.48", 0, 0, NetworkNodeType.CORP, null),
        new("grovia", "NetGrovia Solutions Inc", "194.151.70.91", 0, 0, NetworkNodeType.CORP, null)
        ];
    public override void _Ready() {
        for (int i = 0; i < corps.Length; i++) {
            
        }
    }
}
/**
117	201	59	93
30	162	75	19
7	68	53	15
2	130	194	111
70	212	12	232
189	187	141	103
58	131	164	140
147	132	185	151
186	102	87	168
198	63	119	203
138	89	101	242
9	223	172	218
148	248	84	128
1	148	206	53
66	114	76	198
 * **/
