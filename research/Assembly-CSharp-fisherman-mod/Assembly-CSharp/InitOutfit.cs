using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class InitOutfit : ActivityStateControlled
{
	protected override void SetHelp()
	{
		this.Subscribe();
		this.Setup(PhotonConnectionFactory.Instance.Profile, false);
		this.RequestBoatDesc();
	}

	protected override void HideHelp()
	{
		this.Unsubscribe();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (this.BoatDescSubscribed)
		{
			this.UnsubscribeFromBoats();
		}
	}

	[HideInInspector]
	public bool Subscribed { get; set; }

	public void Subscribe()
	{
		if (!this.Subscribed)
		{
			this.Subscribed = true;
			PhotonConnectionFactory.Instance.OnInventoryUpdated += this.OnInventoryUpdated;
		}
	}

	public void Unsubscribe()
	{
		if (this.Subscribed)
		{
			PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.OnInventoryUpdated;
		}
		this.Subscribed = false;
	}

	[HideInInspector]
	public bool BoatDescRequested { get; set; }

	public bool BoatDescSubscribed { get; set; }

	private void RequestBoatDesc()
	{
		if (!this.BoatDescRequested)
		{
			this.BoatDescRequested = true;
			this.BoatDescSubscribed = true;
			CacheLibrary.MapCache.OnGetBoatDescs += this.OnGotBoatDesc;
			CacheLibrary.MapCache.GetBoatDescs();
		}
	}

	private void UnsubscribeFromBoats()
	{
		if (this.BoatDescRequested && this.BoatDescSubscribed)
		{
			CacheLibrary.MapCache.OnGetBoatDescs -= this.OnGotBoatDesc;
			this.BoatDescSubscribed = false;
		}
	}

	private void OnGotBoatDesc(object sender, GlobalMapBoatDescCacheEventArgs e)
	{
		this.UnsubscribeFromBoats();
		if (StaticUserData.CurrentPond != null)
		{
			IEnumerable<BoatDesc> pondBoatPrices = e.Items.GetPondBoatPrices(StaticUserData.CurrentPond.State.StateId);
			if (pondBoatPrices.Count<BoatDesc>() == 0)
			{
				this.Kayak.GetComponent<Image>().color = new Color(0.39215687f, 0.39215687f, 0.39215687f, 1f);
			}
		}
	}

	protected virtual void OnInventoryUpdated()
	{
		this.Setup(PhotonConnectionFactory.Instance.Profile, true);
	}

	public void Setup(Profile profile, bool shouldCallChangeHandler = false)
	{
		if (profile == null)
		{
			return;
		}
		this.RodCase.Set(profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Doll && x.ItemSubType == ItemSubTypes.RodCase), shouldCallChangeHandler);
		this.TackleBox.Set(profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Doll && x.ItemSubType == ItemSubTypes.LuresBox), shouldCallChangeHandler);
		this.FishKeepnet.Set(profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Doll && (x.ItemSubType == ItemSubTypes.Keepnet || x.ItemSubType == ItemSubTypes.Stringer)), shouldCallChangeHandler);
		this.Kayak.Set(profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Doll && x.ItemType == ItemTypes.Boat), shouldCallChangeHandler);
		this.Hat.Set(profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Doll && x.ItemSubType == ItemSubTypes.Hat), shouldCallChangeHandler);
		this.Coat.Set(profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Doll && x.ItemSubType == ItemSubTypes.Waistcoat), shouldCallChangeHandler);
		this.RodStand.Set(profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Doll && x.ItemSubType == ItemSubTypes.RodStand), shouldCallChangeHandler);
		if (this.Chum != null)
		{
			this.Chum.Set(profile.Inventory.FirstOrDefault((InventoryItem x) => (x.Storage == StoragePlaces.Doll || x.Storage == StoragePlaces.Hands) && x.ItemSubType == ItemSubTypes.Chum), shouldCallChangeHandler);
		}
	}

	public InventoryItemDollComponent RodCase;

	public InventoryItemDollComponent TackleBox;

	public InventoryItemDollComponent Hat;

	public InventoryItemDollComponent Boots;

	public InventoryItemDollComponent Glasses;

	public InventoryItemDollComponent Coat;

	public InventoryItemDollComponent Vest;

	public InventoryItemDollComponent Clothing;

	public InventoryItemDollComponent Gloves;

	public InventoryItemDollComponent Talisman;

	public InventoryItemDollComponent FishKeepnet;

	public InventoryItemDollComponent Kayak;

	public InventoryItemDollComponent RodStand;

	public InventoryItemDollComponent Chum;
}
