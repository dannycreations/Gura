using System;
using Assets.Scripts.Common.Managers.Helpers;
using ObjectModel;
using UnityEngine.UI;

public class ChangeFormByName : ChangeForm
{
	public void Change(FormsEnum form, bool immediate = false)
	{
		if (this.helpers.GetFormByName(form) == null || (base.GetComponent<Toggle>() != null && !base.GetComponent<Toggle>().isOn))
		{
			return;
		}
		if (!ChangeForm.IsChangeOverrriden)
		{
			this.SendGameState(form);
			this.helpers.MenuPrefabsList.currentActiveForm = this.helpers.GetFormByName(form);
		}
		base.Change(this.helpers.GetFormActivityStateByName(form), immediate);
	}

	public void Change()
	{
		if (this.helpers.GetFormByName(this.ChangeToForm) == null || (base.GetComponent<Toggle>() != null && !base.GetComponent<Toggle>().isOn))
		{
			return;
		}
		if (!ChangeForm.IsChangeOverrriden)
		{
			this.SendGameState(this.ChangeToForm);
			this.helpers.MenuPrefabsList.currentActiveForm = this.helpers.GetFormByName(this.ChangeToForm);
		}
		base.Change(this.helpers.GetFormActivityStateByName(this.ChangeToForm), false);
	}

	public void ChangeWithHideDashboard()
	{
		if (this.helpers.GetFormByName(this.ChangeToForm) == null)
		{
			return;
		}
		this.SendGameState(this.ChangeToForm);
		this.helpers.MenuPrefabsList.currentActiveForm = this.helpers.GetFormByName(this.ChangeToForm);
		base.ChangeWithHideDashboard(this.helpers.GetFormActivityStateByName(this.ChangeToForm));
	}

	public void ChangeWithShowDashboard()
	{
		if (this.helpers.GetFormByName(this.ChangeToForm) == null)
		{
			return;
		}
		this.SendGameState(this.ChangeToForm);
		this.helpers.MenuPrefabsList.currentActiveForm = this.helpers.GetFormByName(this.ChangeToForm);
		base.ChangeWithShowDashboard(this.helpers.GetFormActivityStateByName(this.ChangeToForm));
	}

	private void SendGameState(FormsEnum form)
	{
		switch (form)
		{
		case FormsEnum.Stats:
			UIStatsCollector.ChangeGameScreen(GameScreenType.Stats, GameScreenTabType.Undefined, null, null, null, null, null);
			break;
		default:
			switch (form)
			{
			case FormsEnum.Options:
				UIStatsCollector.ChangeGameScreen(GameScreenType.Options, GameScreenTabType.Undefined, null, null, null, null, null);
				break;
			case FormsEnum.GlobalMap:
				UIStatsCollector.ChangeGameScreen(GameScreenType.GlobalMap, GameScreenTabType.Undefined, null, null, null, null, null);
				break;
			case FormsEnum.LocalMap:
				UIStatsCollector.ChangeGameScreen(GameScreenType.LocalMap, GameScreenTabType.Undefined, null, null, null, null, null);
				break;
			}
			break;
		case FormsEnum.FishKeepnet:
			UIStatsCollector.ChangeGameScreen(GameScreenType.Fishkeeper, GameScreenTabType.Undefined, null, null, null, null, null);
			break;
		case FormsEnum.Leaderboards:
			UIStatsCollector.ChangeGameScreen(GameScreenType.Leaderboards, GameScreenTabType.Undefined, null, null, null, null, null);
			break;
		case FormsEnum.Friends:
			UIStatsCollector.ChangeGameScreen(GameScreenType.Friends, GameScreenTabType.Undefined, null, null, null, null, null);
			break;
		case FormsEnum.TutorialsList:
			UIStatsCollector.ChangeGameScreen(GameScreenType.Tutorials, GameScreenTabType.Undefined, null, null, null, null, null);
			break;
		case FormsEnum.Tournaments:
		case FormsEnum.Sport:
			UIStatsCollector.ChangeGameScreen(GameScreenType.Tournaments, GameScreenTabType.Undefined, null, null, null, null, null);
			break;
		case FormsEnum.Quests:
			UIStatsCollector.ChangeGameScreen(GameScreenType.Missions, GameScreenTabType.Undefined, null, null, null, null, null);
			break;
		case FormsEnum.PremiumShopRetail:
			UIStatsCollector.ChangeGameScreen(GameScreenType.PremiumShop, GameScreenTabType.Undefined, null, null, null, null, null);
			break;
		}
	}

	public FormsEnum ChangeToForm;

	private MenuHelpers helpers = new MenuHelpers();
}
