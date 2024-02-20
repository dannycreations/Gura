using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class RestrictionHandler : MonoBehaviour
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
		IEnumerable<InventoryItem> enumerable = PhotonConnectionFactory.Instance.Profile.Inventory.Where((InventoryItem x) => x.Storage == StoragePlaces.Equipment);
		IList<InventoryItem> list = (enumerable as IList<InventoryItem>) ?? enumerable.ToList<InventoryItem>();
		this.ReelValue.text = string.Format("{0}/{1}", PhotonConnectionFactory.Instance.Profile.Inventory.GetCurrentItemsCount(ItemTypes.Reel), PhotonConnectionFactory.Instance.Profile.Inventory.EquipConstraintsCache()[ItemSubTypes.Reel].Count);
		this.LineValue.text = string.Format("{0}/{1}", PhotonConnectionFactory.Instance.Profile.Inventory.GetCurrentItemsCount(ItemTypes.Line), PhotonConnectionFactory.Instance.Profile.Inventory.EquipConstraintsCache()[ItemSubTypes.Line].Count);
		this.TackleValue.text = string.Format("{0}/{1}", PhotonConnectionFactory.Instance.Profile.Inventory.GetCurrentItemsCount(ItemTypes.Bait), PhotonConnectionFactory.Instance.Profile.Inventory.EquipConstraintsCache()[ItemSubTypes.Bait].Count);
		this.ChumValue.text = string.Format("{0}/{1}", PhotonConnectionFactory.Instance.Profile.Inventory.GetCurrentItemsCount(ItemTypes.Chum), PhotonConnectionFactory.Instance.Profile.Inventory.EquipConstraintsCache()[ItemSubTypes.Chum].Count);
	}

	internal void OnGotItemCategories(List<InventoryCategory> category)
	{
		this.Refresh();
	}

	private void OnInventoryUpdated()
	{
		this.Refresh();
	}

	internal void OnEnable()
	{
		this.Refresh();
		PhotonConnectionFactory.Instance.OnInventoryUpdated += this.OnInventoryUpdated;
	}

	internal void OnDisable()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.OnInventoryUpdated;
	}

	public Text ReelValue;

	public Text LineValue;

	public Text TackleValue;

	public Text ChumValue;
}
