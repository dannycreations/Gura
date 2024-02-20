using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class CapacityHandler : MonoBehaviour
{
	internal void Start()
	{
		PhotonConnectionFactory.Instance.OnGotItemCategories += this.OnGotItemCategories;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotItemCategories -= this.OnGotItemCategories;
	}

	public void Refresh()
	{
		List<InventoryItem> list = PhotonConnectionFactory.Instance.Profile.Inventory.Where((InventoryItem x) => x.Storage == StoragePlaces.Storage).ToList<InventoryItem>();
		this.Indicator.SetCapacity(list.Count, PhotonConnectionFactory.Instance.Profile.Inventory.CurrentInventoryCapacity);
	}

	internal void OnGotItemCategories(List<InventoryCategory> category)
	{
		this.Refresh();
	}

	private void OnProductBought(ProfileProduct product, int count)
	{
		base.Invoke("Refresh", 0.1f);
	}

	private void OnInventoryUpdated()
	{
		this.Refresh();
	}

	internal void OnEnable()
	{
		this.Refresh();
		PhotonConnectionFactory.Instance.OnInventoryUpdated += this.OnInventoryUpdated;
		PhotonConnectionFactory.Instance.OnProductBought += this.OnProductBought;
	}

	internal void OnDisable()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.OnInventoryUpdated;
		PhotonConnectionFactory.Instance.OnProductBought -= this.OnProductBought;
	}

	public CapacityIndicator Indicator;
}
