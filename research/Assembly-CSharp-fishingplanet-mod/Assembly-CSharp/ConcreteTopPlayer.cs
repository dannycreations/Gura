using System;
using Assets.Scripts.UI._2D.PlayerProfile;
using ObjectModel;
using UnityEngine.UI;

public class ConcreteTopPlayer : ConcreteTop
{
	private void Awake()
	{
		this._hlg = base.GetComponent<HorizontalLayoutGroup>();
		this._hlg.enabled = false;
	}

	internal void InitItem(TopPlayers player, int number, bool isWeeklyExp)
	{
		if ((number - 1) % 2 != 0)
		{
			base.GetComponent<Image>().enabled = true;
		}
		this.Number.text = number.ToString();
		this.Name.text = player.UserName;
		this.Level.text = PlayerProfileHelper.GetPlayerLevelRank(player);
		if (isWeeklyExp)
		{
			this.Experience.text = ((int)player.WeeklyExpGain).ToString();
		}
		else
		{
			this.Experience.text = ((int)player.Experience).ToString();
		}
		this.Fish.text = player.FishCount.ToString();
		this.Trophy.text = player.TrophyCount.ToString();
		this.Unique.text = player.UniqueCount.ToString();
		this.UserId = player.UserId.ToString();
	}

	private void DisableHLG()
	{
		this._hlg.enabled = false;
	}

	public Text Number;

	public Text Name;

	public Text Level;

	public Text Experience;

	public Text Fish;

	public Text Trophy;

	public Text Unique;

	private HorizontalLayoutGroup _hlg;
}
