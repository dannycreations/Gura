using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateTournamentPageHandler : MainPageHandler<CreationsCategories>
{
	public CreationsCategories CurrentCategory
	{
		get
		{
			return this.Category;
		}
	}

	protected UgcMenuStateManager MenuMgr
	{
		get
		{
			return MenuHelpers.Instance.MenuPrefabsList.UgcMenuManager;
		}
	}

	protected virtual void Update()
	{
		DateTime now = DateTime.Now;
		this._dateTime.text = string.Format("{0} {1}", MeasuringSystemManager.GetFullDayCaption(now), MeasuringSystemManager.DateTimeString(now));
	}

	public void Init()
	{
		CreateTournamentPageHandler.CategoriesContainer categoriesContainer = this.CategoriesContainers.FirstOrDefault((CreateTournamentPageHandler.CategoriesContainer c) => c.Category == CreationsCategories.Templates);
		if (categoriesContainer != null)
		{
			categoriesContainer.Header.isOn = true;
		}
		else
		{
			this.OnClickCategory(2);
		}
	}

	public void LoadCompetition(UserCompetitionPublic c)
	{
		CreationsCategories cat = ((c.Type != UserCompetitionType.Custom) ? CreationsCategories.Templates : CreationsCategories.Advanced);
		CreateTournamentPageHandler.CategoriesContainer categoriesContainer = this.CategoriesContainers.FirstOrDefault((CreateTournamentPageHandler.CategoriesContainer cont) => cont.Category == cat);
		if (categoriesContainer != null)
		{
			categoriesContainer.Header.isOn = true;
		}
		else
		{
			this.OnClickCategory((int)cat);
		}
		CreateTournamentManager createTournamentManager = this.CategoriesGo[this.Category] as CreateTournamentManager;
		if (createTournamentManager != null)
		{
			createTournamentManager.LoadCompetition(c);
		}
	}

	protected override void InitUi()
	{
		this.Category = CreationsCategories.Templates;
		for (int i = 0; i < this.CategoriesContainers.Length; i++)
		{
			CreateTournamentPageHandler.CategoriesContainer categoriesContainer = this.CategoriesContainers[i];
			this.CategoriesGo[categoriesContainer.Category] = categoriesContainer.Go;
			this.CreateHeaderData(categoriesContainer.Header, categoriesContainer.Category);
			this.InitHeaderToggleClickLogic(categoriesContainer.Header, categoriesContainer.Category);
		}
	}

	protected override void SetActiveCategory(Dictionary<CreationsCategories, MainPageItem> categoriesGo)
	{
		base.SetActiveCategory(categoriesGo);
		if (this.Category == CreationsCategories.Back)
		{
			this.MenuMgr.SetActiveFormSport(UgcMenuStateManager.UgcStates.Create, false, false);
			this.MenuMgr.SetActiveFormSport(UgcMenuStateManager.UgcStates.Sport, true, false);
		}
	}

	[SerializeField]
	private TextMeshProUGUI _dateTime;

	[SerializeField]
	protected CreateTournamentPageHandler.CategoriesContainer[] CategoriesContainers;

	[Serializable]
	public class CategoriesContainer
	{
		public CreationsCategories Category;

		public MainPageItem Go;

		public Toggle Header;
	}
}
