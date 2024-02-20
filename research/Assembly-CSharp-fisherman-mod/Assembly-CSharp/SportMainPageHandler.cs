using System;
using UnityEngine;
using UnityEngine.UI;

public class SportMainPageHandler : MainPageHandler<SportCategories>
{
	protected override void InitUi()
	{
		this.Category = SportCategories.AutoCompetitions;
		for (int i = 0; i < this.CategoriesContainers.Length; i++)
		{
			SportMainPageHandler.CategoriesContainer categoriesContainer = this.CategoriesContainers[i];
			if (categoriesContainer.Category == SportCategories.UserCompetitions)
			{
				categoriesContainer.Header.gameObject.SetActive(PhotonConnectionFactory.Instance.IsUgcOn);
			}
			this.CategoriesGo[categoriesContainer.Category] = categoriesContainer.Go;
			this.CreateHeaderData(categoriesContainer.Header, categoriesContainer.Category);
			this.InitHeaderToggleClickLogic(categoriesContainer.Header, categoriesContainer.Category);
		}
	}

	[SerializeField]
	protected SportMainPageHandler.CategoriesContainer[] CategoriesContainers;

	[Serializable]
	public class CategoriesContainer
	{
		public SportCategories Category;

		public MainPageItem Go;

		public Toggle Header;
	}
}
