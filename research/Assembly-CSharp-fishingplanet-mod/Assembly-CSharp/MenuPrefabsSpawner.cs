using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;

public class MenuPrefabsSpawner : MonoBehaviour
{
	public void Awake()
	{
		if (!this._inited)
		{
			this.Init();
		}
	}

	public void Init()
	{
		if (this._inited)
		{
			MonoBehaviour.print("_inited, exiting");
			return;
		}
		Transform parent = base.transform.parent;
		if (this.MenuPrefabsList == null)
		{
			this.MenuPrefabsList = MenuHelpers.Instance.MenuPrefabsList;
		}
		if (this.MenuPrefabsList.missions == null)
		{
			this.MenuPrefabsList.missions = Object.Instantiate<QuestsListInit>(this.MissionsScreenPrefab, Vector3.zero, Quaternion.identity, parent).gameObject;
		}
		this.MenuPrefabsList.missionsAS = this.MenuPrefabsList.missions.GetComponent<ActivityState>();
		this.MenuPrefabsList.missions.SetActive(false);
		bool flag = StaticUserData.CurrentPond != null;
		if (flag)
		{
			if (this._topFormPrefab != null)
			{
				this.MenuPrefabsList.topMenuFormAS = this.InitMenuOnPond(this._topFormPrefab, ref this.MenuPrefabsList.topMenuForm, parent);
			}
			GameStateChanger component = this.MenuPrefabsList.topMenuForm.GetComponent<GameStateChanger>();
			if (component != null)
			{
				component.SetState = GameStates.PondUIState;
			}
			if (StaticUserData.IS_IN_TUTORIAL)
			{
				this.MenuPrefabsList.topMenuForm.SetActive(false);
			}
			this.MenuPrefabsList.globalMapFormAS = this.InitMenuOnPond(this.PondMapPrefab, ref this.MenuPrefabsList.globalMapForm, parent);
			this.MenuPrefabsList.inventoryFormAS = this.InitMenuOnPond(this.InventoryPrefab, ref this.MenuPrefabsList.inventoryForm, parent);
			this.MenuPrefabsList.SportAS = this.InitMenuOnPond(this._sportFormPrefab, ref this.MenuPrefabsList.SportForm, parent);
			this.MenuPrefabsList.CreateTournamentFormAS = this.InitMenuOnPond(this._createTournamentFormPrefab, ref this.MenuPrefabsList.CreateTournamentForm, parent);
			if (this._ugcRoomFormPrefab != null)
			{
				this.MenuPrefabsList.UgcRoomFormAS = this.InitMenuOnPond(this._ugcRoomFormPrefab, ref this.MenuPrefabsList.UgcRoomForm, parent);
			}
			if (this._shopPrefab != null)
			{
				this.MenuPrefabsList.shopFormAS = this.InitMenuOnPond(this._shopPrefab, ref this.MenuPrefabsList.shopForm, parent);
			}
			this.MenuPrefabsList.PremiumShopRetailFormAS = this.InitMenuOnPond(this._premiumShopRetailFormPrefab, ref this.MenuPrefabsList.PremiumShopRetailForm, parent);
		}
		if (this._shopPremiumPrefab != null)
		{
			this.MenuPrefabsList.shopPremiumFormAS = this.InitMenuOnPond(this._shopPremiumPrefab, ref this.MenuPrefabsList.shopPremiumForm, parent);
		}
		this._inited = true;
	}

	private ActivityState InitMenuOnPond(GameObject prefab, ref GameObject globalForm, Transform root)
	{
		if (globalForm != null)
		{
			Object.Destroy(globalForm);
		}
		globalForm = Object.Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, root);
		return globalForm.GetComponent<ActivityState>();
	}

	[SerializeField]
	private MenuPrefabsList MenuPrefabsList;

	public QuestsListInit MissionsScreenPrefab;

	public GameObject InventoryPrefab;

	public GameObject PondMapPrefab;

	[SerializeField]
	private GameObject _shopPrefab;

	[SerializeField]
	private GameObject _shopPremiumPrefab;

	[SerializeField]
	private GameObject _topFormPrefab;

	[SerializeField]
	private GameObject _sportFormPrefab;

	[SerializeField]
	private GameObject _createTournamentFormPrefab;

	[SerializeField]
	private GameObject _ugcRoomFormPrefab;

	[SerializeField]
	private GameObject _premiumShopRetailFormPrefab;

	private bool _inited;
}
