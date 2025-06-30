using Godot;
using System;

public partial class BotnetDashboard : MarginContainer {
	[Export] MenuDirector menuDirector;
	[Export] PackedScene slideScene;
	[Export] HFlowContainer slideContainer;
	[Export] BotStatusBoard botStatusBoard;
	[Export] NetworkManager.HackFarmSortType sortType = NetworkManager.HackFarmSortType.LIFETIME;

	BotSlide[] BotSlides;
	int[] pageLength;
	public override void _Ready() {
		pageLength = [10, 20, 50, 100];
		BotSlides = new BotSlide[pageLength[^1]];
		for (int i = 0; i < pageLength[^1]; ++i) {
			BotSlides[i] = slideScene.Instantiate<BotSlide>();
			slideContainer.AddChild(BotSlides[i]);
			BotSlides[i].Visible = false;
			BotSlides[i].Director = this;
			BotSlides[i].ID = i;
		}
		RenderDashboardPage();
		LifeCycleDirector.FinishScene += FinishSetup;
    }
	void FinishSetup() {

    }
    [Export] int _pageLength; int PageLength {
		get => _pageLength;
		set {
			value = Mathf.Clamp(value, pageLength[0], pageLength[^1]);
			if (_pageLength == value) return;
			for (int i = value; i < _pageLength; ++i) BotSlides[i].Visible = false;
			_pageLength = value;
            pageAmountLabel.Text = $"/{Mathf.CeilToInt((double)NetworkManager.BotNet.Count / PageLength)}";
			CurPage = 0;
        }
    }
	int _curPage; [Export] int CurPage {
		get => _curPage;
		set {
			_curPage = Mathf.Clamp(value, 0, Mathf.Max(0, Mathf.FloorToInt(NetworkManager.BotNet.Count / (double)PageLength)));
			curPageIO.Text = $"{_curPage+1}";
		}
	}
	public override void _Process(double delta) {
        OnRefreshTimerTimeout();
    }
    public void OnRefreshTimerTimeout() {
		if (menuDirector.MenuWindowIndex != MenuDirector.BOTNET_INDEX) return;
		RenderDashboardPage();
        pageAmountLabel.Text = $"/{Mathf.CeilToInt((double)NetworkManager.BotNet.Count / PageLength)}";
    }
    public void RenderDashboardPage() {
		NetworkManager.SortHackFarm(sortType);
		BotFarm[] botnet = NetworkManager.GetBotFarms();
		int startingIndex = CurPage * PageLength;
		if (botStatusBoard.isRefNull() && NetworkManager.BotNet.Count > 0) botStatusBoard.ChangeFocusedBotFarm(NetworkManager.BotNet[0]);
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
	public void OnBotSlidePressed(int index) {
		int entryIndex = CurPage * PageLength + index;
		if (entryIndex >= NetworkManager.BotNet.Count) return;
		botStatusBoard.ChangeFocusedBotFarm(NetworkManager.BotNet[entryIndex]);
	}
	[Export] RichTextLabel pageAmountLabel;
	[Export] LineEdit curPageIO;
	public void SetPageLength(int length) {
		PageLength = length;
	}
	public void NextPage() { CurPage++; }
	public void PrevPage() { CurPage--; }
}
