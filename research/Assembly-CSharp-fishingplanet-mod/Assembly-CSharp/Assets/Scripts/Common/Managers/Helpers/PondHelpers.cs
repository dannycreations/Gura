using System;
using UnityEngine;

namespace Assets.Scripts.Common.Managers.Helpers
{
	public class PondHelpers
	{
		public PondControllers PondControllerList
		{
			get
			{
				if (this._pondControllerList == null)
				{
					this.PondControllerListRefresh();
				}
				return this._pondControllerList;
			}
		}

		public void PondControllerListRefresh()
		{
			GameObject gameObject = GameObject.Find(StaticUserData.GameObjectCommonDataName);
			if (gameObject == null)
			{
				return;
			}
			this._pondControllerList = gameObject.GetComponent<PondControllers>();
		}

		public MenuPrefabsList PondPrefabsList
		{
			get
			{
				if (this._pondPrefabsList == null)
				{
					this.PondPrefabsListRefresh();
				}
				return this._pondPrefabsList;
			}
		}

		public void PondPrefabsListRefresh()
		{
			GameObject gameObject = GameObject.Find(StaticUserData.GameObjectCommonDataName);
			if (gameObject == null)
			{
				return;
			}
			this._pondPrefabsList = gameObject.GetComponent<MenuPrefabsList>();
		}

		public void ShowLoading()
		{
			if (this.PondPrefabsList.helpPanel != null)
			{
				this.PondPrefabsList.helpPanelAS.Hide(false);
			}
			this.PondPrefabsList.globalMapFormAS.Hide(false);
			this.PondPrefabsList.topMenuFormAS.Hide(false);
			this.PondPrefabsList.loadingFormAS.Show(false);
		}

		private PondControllers _pondControllerList;

		private MenuPrefabsList _pondPrefabsList;
	}
}
