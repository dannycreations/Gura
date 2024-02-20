using System;

public class LeaderboardInit : ActivityStateControlled
{
	protected override void SetHelp()
	{
		this.PlayersPanel.Refresh();
	}

	public TabControl PlayersPanel;

	public LeaderboardFishInit FishPanel;
}
