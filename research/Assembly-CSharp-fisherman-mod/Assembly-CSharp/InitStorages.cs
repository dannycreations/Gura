using System;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class InitStorages : MonoBehaviour
{
	internal void Awake()
	{
		InitStorages.Instance = this;
	}

	internal void OnEnable()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated += this.OnInventoryUpdated;
		PhotonConnectionFactory.Instance.OnProductDelivered += this.OnInventoryUpdated;
		PhotonConnectionFactory.Instance.OnProductBought += this.OnProductBought;
		if (this.ActiveStorage == null)
		{
			this.ActiveStorage = base.GetComponent<ActiveStorage>();
		}
		this.OnInventoryUpdated();
		bool flag = StaticUserData.CurrentPond != null;
		if (this.ActiveStorage.storage != StoragePlaces.Equipment && flag)
		{
			this.ActiveStorage.Setup(StoragePlaces.Equipment);
		}
	}

	internal void OnDisable()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.OnInventoryUpdated;
		PhotonConnectionFactory.Instance.OnProductDelivered -= this.OnInventoryUpdated;
		PhotonConnectionFactory.Instance.OnProductBought -= this.OnProductBought;
	}

	internal void OnInventoryUpdated(ProfileProduct p, int count, bool announce)
	{
		this.OnInventoryUpdated();
	}

	private void OnProductBought(ProfileProduct product, int count)
	{
		this.OnInventoryUpdated();
	}

	protected virtual void OnInventoryUpdated()
	{
		if (StaticUserData.DISABLE_MISSIONS && this.ToggleSpecials.gameObject.activeSelf)
		{
			this.TabsNavigation.RemoveSelectable(this.ToggleSpecials);
			this.ToggleSpecials.gameObject.SetActive(false);
		}
		bool isStorageOverloaded = PhotonConnectionFactory.Instance.Profile.Inventory.IsStorageOverloaded;
		bool flag = StaticUserData.CurrentPond != null;
		this.ToggleExceed.gameObject.SetActive(isStorageOverloaded && !flag);
		this.ToggleTemplates.gameObject.SetActive(!flag);
		this.ToggleStorage.gameObject.SetActive(!flag);
		this.ToggleKeepnet.gameObject.SetActive(flag);
		if (this.ToggleExceed.isOn && !isStorageOverloaded && this.ToggleStorage != null && !flag)
		{
			this.ToggleStorage.isOn = true;
		}
		if (!isStorageOverloaded && this.ExceedContent.activeSelf)
		{
			this.ExceedContent.SetActive(false);
		}
		if (!flag)
		{
			this.TabsNavigation.AddSelectable(this.ToggleStorage);
		}
		else
		{
			this.TabsNavigation.RemoveSelectable(this.ToggleStorage);
		}
		if (isStorageOverloaded && !flag)
		{
			this.TabsNavigation.AddSelectable(this.ToggleExceed);
		}
		else
		{
			this.TabsNavigation.RemoveSelectable(this.ToggleExceed);
		}
		if (!flag)
		{
			this.TabsNavigation.AddSelectable(this.ToggleTemplates);
		}
		else
		{
			this.TabsNavigation.RemoveSelectable(this.ToggleTemplates);
		}
		if (flag)
		{
			this.TabsNavigation.AddSelectable(this.ToggleKeepnet);
		}
		else
		{
			this.TabsNavigation.RemoveSelectable(this.ToggleKeepnet);
		}
	}

	[SerializeField]
	private GameObject ExceedContent;

	[SerializeField]
	private GameObject KeepnetContent;

	public Toggle ToggleExceed;

	public Toggle ToggleKeepnet;

	public Toggle ToggleStorage;

	public Toggle ToggleTemplates;

	public Toggle ToggleSpecials;

	public CircleNavigation TabsNavigation;

	public Image CarImage;

	public ActiveStorage ActiveStorage;

	public static InitStorages Instance;
}
