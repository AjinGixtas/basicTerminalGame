using Godot;

public partial class BotnetDashboard : MarginContainer {
	[Export] MenuDirector menuDirector;
	[Export] PackedScene slideScene;
	[Export] HFlowContainer slideContainer;
	[Export] private NetworkManager.HackFarmSortType sortType = NetworkManager.HackFarmSortType.LIFETIME;

	BotSlide[] BotSlides;
	int[] pageLength;
	public override void _Ready() {
		pageLength = [10, 20, 50, 100];
		BotSlides = new BotSlide[pageLength[^1]];
		for (int i = 0; i < pageLength[^1]; ++i) {
			BotSlides[i] = slideScene.Instantiate<BotSlide>();
			slideContainer.AddChild(BotSlides[i]);
		}
		for (int i = 0; i < _pageLength; ++i) {
			BotSlides[i].Visible = true; // Show new slides
		}
		for (int i = _pageLength; i < BotSlides.Length; ++i) {
			BotSlides[i].Visible = false; // Hide slides that are not needed
		}
		RenderDashboardPage();
	}

	[Export] int _pageLength; int PageLength {
		get => _pageLength;
		set {
			value = Mathf.Clamp(value, pageLength[0], pageLength[^1]);
			if (value == _pageLength) return; // No change
			_pageLength = value;
		}
	}
	int _curPage; [Export] int CurPage {
		get => _curPage;
		set {
			_curPage = Mathf.Clamp(value, 0, Mathf.Max(0, NetworkManager.GetBotFarms().Length / PageLength));
		}
	}
	public override void _Process(double delta) {
		base._Process(delta);
	}
	public void OnRefreshTimerTimeout() {
		if (menuDirector.MenuWindowIndex != MenuDirector.BOTNET_INDEX) return;
		RenderDashboardPage();
	}
	public void RenderDashboardPage() {
		NetworkManager.SortHackFarm(sortType);
		BotFarm[] botnet = NetworkManager.GetBotFarms();
		int startingIndex = CurPage * PageLength;
		for (int index = 0; index < PageLength; ++index) {
			int i = startingIndex + index;
			if (i >= botnet.Length) {
				BotSlides[index].Visible = false;
				continue;
			}
			BotSlides[index].Update(botnet[i]);
			BotSlides[index].Visible = true;
		}
	}

	public void SetPageLength(int length) {
		PageLength = length;
	}
}
