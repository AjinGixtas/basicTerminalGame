public class Faction {
    public string Name { get; init; }
    public string Desc { get; init; }
    public int Favor { get; private set; }
    public int Reputation { get; init; }
    public Faction(string Name, string Desc, int Favor, int Reputation) {
        this.Name = Name; this.Desc = Desc;
        this.Favor = Favor; this.Reputation = Reputation;
    }
}