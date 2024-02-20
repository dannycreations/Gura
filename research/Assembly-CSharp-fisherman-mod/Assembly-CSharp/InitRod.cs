using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Inventory;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class InitRod : MonoBehaviour
{
	private void Awake()
	{
		this.sizeSetters = base.GetComponentsInChildren<ContentPreferredSizeSetter>();
	}

	private void Start()
	{
		this.RebuildLayout();
	}

	private void RebuildLayout()
	{
		if (this.sizeSetters != null)
		{
			foreach (ContentPreferredSizeSetter contentPreferredSizeSetter in this.sizeSetters)
			{
				contentPreferredSizeSetter.Refresh();
			}
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		if (this.layout != null)
		{
			this.layout.enabled = false;
			this.layout.enabled = true;
		}
	}

	public void Setup(int slotId, Profile profile = null)
	{
		this.SlotId = slotId;
		profile = profile ?? PhotonConnectionFactory.Instance.Profile;
		Rod rod = RodHelper.FindRodInSlot(slotId, profile);
		this.ClearSlot();
		if (rod != null)
		{
			this.SetupRod(rod, profile);
		}
		else
		{
			this.RebuildLayout();
		}
	}

	public Feeder GetFeeder()
	{
		if (this.Rod.InventoryItem != null)
		{
			ItemSubTypes itemSubType = this.Rod.InventoryItem.ItemSubType;
			if (itemSubType != ItemSubTypes.FeederRod && itemSubType != ItemSubTypes.BottomRod && itemSubType != ItemSubTypes.CarpRod)
			{
				if (itemSubType == ItemSubTypes.SpodRod)
				{
					return this.SpodFeeder.InventoryItem as Feeder;
				}
			}
			return this.Feeder.InventoryItem as Feeder;
		}
		return null;
	}

	public void ClearSlot()
	{
		this.Rod.ClearSlot();
		if (this.Rod.ChangeHandler != null)
		{
			this.Rod.ChangeHandler.OnChange();
		}
		this.Reel.ClearSlot();
		if (this.Line != null)
		{
			this.Line.ClearSlot();
		}
		if (this.Tackle != null)
		{
			this.Tackle.ClearSlot();
		}
		if (this.LureHook != null)
		{
			this.LureHook.ClearSlot();
		}
		if (this.Bait != null)
		{
			this.Bait.ClearSlot();
		}
		if (this.Bell != null)
		{
			this.Bell.ClearSlot();
		}
		if (this.Feeder != null)
		{
			this.Feeder.ClearSlot();
		}
		if (this.SpodFeeder != null)
		{
			this.SpodFeeder.ClearSlot();
		}
		if (this.PVASinker != null)
		{
			this.PVASinker.ClearSlot();
		}
		if (this.SpinningSinker != null)
		{
			this.SpinningSinker.ClearSlot();
		}
		if (this.Chum != null)
		{
			this.Chum.ClearSlot();
		}
		if (this.SpodChumAdditional != null)
		{
			this.SpodChumAdditional.ClearSlot();
		}
		if (this.Leader != null)
		{
			this.Leader.ClearSlot();
		}
		if (this.Tackle != null && !this.Tackle.transform.parent.gameObject.activeInHierarchy)
		{
			this.Tackle.transform.parent.gameObject.SetActive(true);
		}
		if (this.Bait != null && !this.Bait.transform.parent.gameObject.activeInHierarchy)
		{
			this.Bait.transform.parent.gameObject.SetActive(true);
		}
		if (this.Feeder != null && this.Feeder.transform.parent.gameObject.activeInHierarchy)
		{
			this.Feeder.transform.parent.gameObject.SetActive(false);
		}
		if (this.Bell != null && this.Bell.transform.parent.gameObject.activeInHierarchy)
		{
			this.Bell.transform.parent.gameObject.SetActive(false);
		}
		if (this.Chum != null && this.Chum.transform.parent.gameObject.activeInHierarchy)
		{
			this.Chum.transform.parent.gameObject.SetActive(false);
		}
		if (this.Leader != null && this.Leader.transform.parent.gameObject.activeInHierarchy)
		{
			this.Leader.transform.parent.gameObject.SetActive(false);
		}
		if (this.SpodFeeder != null && this.SpodFeeder.transform.parent.gameObject.activeInHierarchy)
		{
			this.SpodFeeder.transform.parent.gameObject.SetActive(false);
		}
		if (this.PVASinker != null && this.PVASinker.transform.parent.gameObject.activeInHierarchy)
		{
			this.PVASinker.transform.parent.gameObject.SetActive(false);
		}
		if (this.SpinningSinker != null && this.SpinningSinker.transform.parent.gameObject.activeInHierarchy)
		{
			this.SpinningSinker.transform.parent.gameObject.SetActive(false);
		}
		if (this.SpodChumAdditional != null && this.SpodChumAdditional.transform.parent.gameObject.activeInHierarchy)
		{
			this.SpodChumAdditional.transform.parent.gameObject.SetActive(false);
		}
	}

	private void SetupRod(InventoryItem rod, Profile profile)
	{
		Inventory inventory = profile.Inventory;
		List<InventoryItem> rodEquipment = inventory.GetRodEquipment(rod as Rod);
		RodTemplate largestTemplate = InventoryHelper.GetLargestTemplate(rod as Rod);
		bool isRig = largestTemplate.IsSinkerRig();
		this.Rod.Set(rod, true);
		if (rod != null && rod.ItemSubType != ItemSubTypes.MatchRod && rod.ItemSubType != ItemSubTypes.TelescopicRod)
		{
			if (this.Tackle != null)
			{
				this.Tackle.transform.parent.gameObject.SetActive(false);
			}
			if (rod.ItemSubType == ItemSubTypes.FeederRod || rod.ItemSubType == ItemSubTypes.BottomRod)
			{
				if (this.LureHook != null)
				{
					this.LureHook.transform.parent.gameObject.SetActive(true);
				}
				if (this.Feeder != null)
				{
					this.Feeder.transform.parent.gameObject.SetActive(true);
				}
				if (this.SpodFeeder != null)
				{
					this.SpodFeeder.transform.parent.gameObject.SetActive(false);
				}
				if (this.Bell != null)
				{
					this.Bell.transform.parent.gameObject.SetActive(true);
				}
				if (this.Chum != null)
				{
					this.Chum.transform.parent.gameObject.SetActive(InventoryHelper.HasFeeder(rod));
				}
				if (this.Leader != null)
				{
					this.Leader.transform.parent.gameObject.SetActive(true);
				}
				if (this.PVASinker != null)
				{
					this.PVASinker.transform.parent.gameObject.SetActive(false);
				}
				if (this.SpodChumAdditional != null)
				{
					this.SpodChumAdditional.transform.parent.gameObject.SetActive(false);
				}
			}
			else if (rod.ItemSubType == ItemSubTypes.CarpRod)
			{
				if (this.LureHook != null)
				{
					this.LureHook.transform.parent.gameObject.SetActive(true);
				}
				if (this.Feeder != null)
				{
					this.Feeder.transform.parent.gameObject.SetActive(true);
				}
				if (this.SpodFeeder != null)
				{
					this.SpodFeeder.transform.parent.gameObject.SetActive(false);
				}
				if (this.Bell != null)
				{
					this.Bell.transform.parent.gameObject.SetActive(false);
				}
				bool flag = InventoryHelper.HasFeeder(rod);
				if (this.Chum != null)
				{
					this.Chum.transform.parent.gameObject.SetActive(flag);
				}
				if (this.PVASinker != null)
				{
					InventoryItem inventoryItem = inventory.FirstOrDefault(delegate(InventoryItem x)
					{
						Guid? parentItemInstanceId = x.ParentItemInstanceId;
						bool flag9 = parentItemInstanceId != null;
						Guid? instanceId = rod.InstanceId;
						return flag9 == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault()) && x.ItemSubType == ItemSubTypes.PvaFeeder;
					});
					this.PVASinker.transform.parent.gameObject.SetActive(inventoryItem != null);
				}
				if (this.Leader != null)
				{
					this.Leader.transform.parent.gameObject.SetActive(true);
				}
				if (this.SpodChumAdditional != null)
				{
					this.SpodChumAdditional.transform.parent.gameObject.SetActive(false);
				}
			}
			else if (rod.ItemSubType == ItemSubTypes.SpodRod)
			{
				if (this.Tackle != null)
				{
					this.Tackle.transform.parent.gameObject.SetActive(false);
				}
				if (this.Bait != null)
				{
					this.Bait.transform.parent.gameObject.SetActive(false);
				}
				if (this.LureHook != null)
				{
					this.LureHook.transform.parent.gameObject.SetActive(false);
				}
				if (this.Leader != null)
				{
					this.Leader.transform.parent.gameObject.SetActive(false);
				}
				if (this.Feeder != null)
				{
					this.Feeder.transform.parent.gameObject.SetActive(false);
				}
				if (this.SpodFeeder != null)
				{
					this.SpodFeeder.transform.parent.gameObject.SetActive(true);
				}
				if (this.Bell != null)
				{
					this.Bell.transform.parent.gameObject.SetActive(false);
				}
				InventoryItem inventoryItem2 = inventory.FirstOrDefault(delegate(InventoryItem x)
				{
					Guid? parentItemInstanceId2 = x.ParentItemInstanceId;
					bool flag10 = parentItemInstanceId2 != null;
					Guid? instanceId2 = rod.InstanceId;
					return flag10 == (instanceId2 != null) && (parentItemInstanceId2 == null || parentItemInstanceId2.GetValueOrDefault() == instanceId2.GetValueOrDefault()) && x.ItemSubType == ItemSubTypes.SpodFeeder;
				});
				bool flag2 = inventoryItem2 != null && (inventoryItem2 as SpodFeeder).SlotCount > 1;
				if (this.Chum != null)
				{
					this.Chum.transform.parent.gameObject.SetActive(true);
				}
				if (this.PVASinker != null)
				{
					this.PVASinker.transform.parent.gameObject.SetActive(false);
				}
				if (this.SpodChumAdditional != null)
				{
					this.SpodChumAdditional.transform.parent.gameObject.SetActive(flag2);
				}
			}
			else
			{
				if (this.LureHook != null)
				{
					this.LureHook.transform.parent.gameObject.SetActive(true);
				}
				if (this.Feeder != null)
				{
					this.Feeder.transform.parent.gameObject.SetActive(false);
				}
				if (this.SpodFeeder != null)
				{
					this.SpodFeeder.transform.parent.gameObject.SetActive(false);
				}
				if (this.Bell != null)
				{
					this.Bell.transform.parent.gameObject.SetActive(false);
				}
				if (this.Chum != null)
				{
					this.Chum.transform.parent.gameObject.SetActive(false);
				}
				if (this.Leader != null)
				{
					GameObject gameObject = this.Leader.transform.parent.gameObject;
					bool flag3;
					if (!PhotonConnectionFactory.Instance.IsPredatorFishOn && !isRig && largestTemplate != RodTemplate.OffsetJig)
					{
						flag3 = rodEquipment.All((InventoryItem x) => x.ItemType == ItemTypes.Line || x.ItemType == ItemTypes.Reel);
					}
					else
					{
						flag3 = true;
					}
					gameObject.SetActive(flag3);
				}
				if (this.PVASinker != null)
				{
					this.PVASinker.transform.parent.gameObject.SetActive(false);
				}
				if (this.SpinningSinker != null)
				{
					this.SpinningSinker.transform.parent.gameObject.SetActive(isRig);
				}
				if (this.SpodChumAdditional != null)
				{
					this.SpodChumAdditional.transform.parent.gameObject.SetActive(false);
				}
			}
		}
		else
		{
			if (this.LureHook != null)
			{
				this.LureHook.transform.parent.gameObject.SetActive(true);
			}
			if (this.Tackle != null)
			{
				this.Tackle.transform.parent.gameObject.SetActive(true);
			}
			if (this.Feeder != null)
			{
				this.Feeder.transform.parent.gameObject.SetActive(false);
			}
			if (this.SpodFeeder != null)
			{
				this.SpodFeeder.transform.parent.gameObject.SetActive(false);
			}
			if (this.Chum != null)
			{
				this.Chum.transform.parent.gameObject.SetActive(false);
			}
			if (this.Bell != null)
			{
				this.Bell.transform.parent.gameObject.SetActive(false);
			}
			if (this.Leader != null)
			{
				this.Leader.transform.parent.gameObject.SetActive(PhotonConnectionFactory.Instance.IsPredatorFishOn);
			}
			if (this.PVASinker != null)
			{
				this.PVASinker.transform.parent.gameObject.SetActive(false);
			}
			if (this.SpinningSinker != null)
			{
				this.SpinningSinker.transform.parent.gameObject.SetActive(false);
			}
			if (this.SpodChumAdditional != null)
			{
				this.SpodChumAdditional.transform.parent.gameObject.SetActive(false);
			}
		}
		if (rod != null)
		{
			this.Reel.Set(inventory.FirstOrDefault(delegate(InventoryItem x)
			{
				Guid? parentItemInstanceId3 = x.ParentItemInstanceId;
				bool flag11 = parentItemInstanceId3 != null;
				Guid? instanceId3 = rod.InstanceId;
				return flag11 == (instanceId3 != null) && (parentItemInstanceId3 == null || parentItemInstanceId3.GetValueOrDefault() == instanceId3.GetValueOrDefault()) && x.ItemType == ItemTypes.Reel;
			}), true);
			if (this.Line != null)
			{
				this.Line.Set(inventory.FirstOrDefault(delegate(InventoryItem x)
				{
					Guid? parentItemInstanceId4 = x.ParentItemInstanceId;
					bool flag12 = parentItemInstanceId4 != null;
					Guid? instanceId4 = rod.InstanceId;
					return flag12 == (instanceId4 != null) && (parentItemInstanceId4 == null || parentItemInstanceId4.GetValueOrDefault() == instanceId4.GetValueOrDefault()) && x.ItemType == ItemTypes.Line;
				}), true);
			}
			if (this.LureHook != null)
			{
				this.LureHook.Set(inventory.FirstOrDefault(delegate(InventoryItem x)
				{
					Guid? parentItemInstanceId5 = x.ParentItemInstanceId;
					bool flag13 = parentItemInstanceId5 != null;
					Guid? instanceId5 = rod.InstanceId;
					return flag13 == (instanceId5 != null) && (parentItemInstanceId5 == null || parentItemInstanceId5.GetValueOrDefault() == instanceId5.GetValueOrDefault()) && (x.ItemType == ItemTypes.Hook || x.ItemType == ItemTypes.Lure || x.ItemType == ItemTypes.JigHead);
				}), true);
			}
			bool flag4 = this.LureHook != null && this.LureHook.InventoryItem != null;
			bool flag5 = flag4 && this.LureHook.InventoryItem.ItemSubType == ItemSubTypes.OffsetHook;
			bool flag6 = flag4 && (this.LureHook.InventoryItem.ItemType == ItemTypes.JigHead || flag5 || this.LureHook.InventoryItem.ItemSubType == ItemSubTypes.BassJig || this.LureHook.InventoryItem.ItemSubType == ItemSubTypes.Spinner || this.LureHook.InventoryItem.ItemSubType == ItemSubTypes.BarblessSpinners || this.LureHook.InventoryItem.ItemSubType == ItemSubTypes.Spinnerbait || this.LureHook.InventoryItem.ItemSubType == ItemSubTypes.BuzzBait);
			if (this.Bait != null)
			{
				bool flag7 = (rod.ItemSubType != ItemSubTypes.SpodRod && ((rod.ItemSubType != ItemSubTypes.SpinningRod && rod.ItemSubType != ItemSubTypes.CastingRod) || flag6 || isRig || (largestTemplate == RodTemplate.Lure && !flag4))) || largestTemplate == RodTemplate.Jig;
				this.Bait.transform.parent.gameObject.SetActive(flag7);
				this.Bait.Set(inventory.FirstOrDefault(delegate(InventoryItem x)
				{
					Guid? parentItemInstanceId6 = x.ParentItemInstanceId;
					bool flag14 = parentItemInstanceId6 != null;
					Guid? instanceId6 = rod.InstanceId;
					return flag14 == (instanceId6 != null) && (parentItemInstanceId6 == null || parentItemInstanceId6.GetValueOrDefault() == instanceId6.GetValueOrDefault()) && (x.ItemType == ItemTypes.Bait || x.ItemType == ItemTypes.JigBait);
				}), true);
			}
			if (this.Tackle != null)
			{
				this.Tackle.Set(inventory.FirstOrDefault(delegate(InventoryItem x)
				{
					Guid? parentItemInstanceId7 = x.ParentItemInstanceId;
					bool flag15 = parentItemInstanceId7 != null;
					Guid? instanceId7 = rod.InstanceId;
					return flag15 == (instanceId7 != null) && (parentItemInstanceId7 == null || parentItemInstanceId7.GetValueOrDefault() == instanceId7.GetValueOrDefault()) && (x.ItemType == ItemTypes.Bobber || x.ItemType == ItemTypes.TerminalTackle);
				}), true);
			}
			if (this.Bell != null)
			{
				this.Bell.Set(inventory.FirstOrDefault(delegate(InventoryItem x)
				{
					Guid? parentItemInstanceId8 = x.ParentItemInstanceId;
					bool flag16 = parentItemInstanceId8 != null;
					Guid? instanceId8 = rod.InstanceId;
					return flag16 == (instanceId8 != null) && (parentItemInstanceId8 == null || parentItemInstanceId8.GetValueOrDefault() == instanceId8.GetValueOrDefault()) && x.ItemType == ItemTypes.Bell;
				}), true);
			}
			bool pvaFeederAvailable = inventory.FirstOrDefault(delegate(InventoryItem x)
			{
				Guid? parentItemInstanceId9 = x.ParentItemInstanceId;
				bool flag17 = parentItemInstanceId9 != null;
				Guid? instanceId9 = rod.InstanceId;
				return flag17 == (instanceId9 != null) && (parentItemInstanceId9 == null || parentItemInstanceId9.GetValueOrDefault() == instanceId9.GetValueOrDefault()) && x.ItemSubType == ItemSubTypes.PvaFeeder;
			}) != null;
			if (this.Feeder != null)
			{
				this.Feeder.Set(inventory.FirstOrDefault(delegate(InventoryItem x)
				{
					Guid? parentItemInstanceId10 = x.ParentItemInstanceId;
					bool flag18 = parentItemInstanceId10 != null;
					Guid? instanceId10 = rod.InstanceId;
					return flag18 == (instanceId10 != null) && (parentItemInstanceId10 == null || parentItemInstanceId10.GetValueOrDefault() == instanceId10.GetValueOrDefault()) && ((rod.ItemSubType == ItemSubTypes.CarpRod && (x.ItemSubType == ItemSubTypes.PvaFeeder || x.ItemSubType == ItemSubTypes.FlatFeeder || (x.ItemSubType == ItemSubTypes.Sinker && !pvaFeederAvailable))) || (rod.ItemSubType != ItemSubTypes.SpodRod && !isRig && rod.ItemSubType != ItemSubTypes.CarpRod && (x.ItemType == ItemTypes.Feeder || x.ItemType == ItemTypes.Sinker)));
				}), true);
			}
			if (this.SpinningSinker != null)
			{
				this.SpinningSinker.Set(inventory.FirstOrDefault(delegate(InventoryItem x)
				{
					Guid? parentItemInstanceId11 = x.ParentItemInstanceId;
					bool flag19 = parentItemInstanceId11 != null;
					Guid? instanceId11 = rod.InstanceId;
					return flag19 == (instanceId11 != null) && (parentItemInstanceId11 == null || parentItemInstanceId11.GetValueOrDefault() == instanceId11.GetValueOrDefault()) && (x.ItemSubType == ItemSubTypes.SpinningSinker || x.ItemSubType == ItemSubTypes.DropSinker);
				}), true);
			}
			if (this.SpodFeeder != null)
			{
				this.SpodFeeder.Set(inventory.FirstOrDefault(delegate(InventoryItem x)
				{
					Guid? parentItemInstanceId12 = x.ParentItemInstanceId;
					bool flag20 = parentItemInstanceId12 != null;
					Guid? instanceId12 = rod.InstanceId;
					return flag20 == (instanceId12 != null) && (parentItemInstanceId12 == null || parentItemInstanceId12.GetValueOrDefault() == instanceId12.GetValueOrDefault()) && x.ItemSubType == ItemSubTypes.SpodFeeder;
				}), true);
			}
			if (this.PVASinker != null && pvaFeederAvailable)
			{
				this.PVASinker.Set(inventory.FirstOrDefault(delegate(InventoryItem x)
				{
					Guid? parentItemInstanceId13 = x.ParentItemInstanceId;
					bool flag21 = parentItemInstanceId13 != null;
					Guid? instanceId13 = rod.InstanceId;
					return flag21 == (instanceId13 != null) && (parentItemInstanceId13 == null || parentItemInstanceId13.GetValueOrDefault() == instanceId13.GetValueOrDefault()) && rod.ItemSubType == ItemSubTypes.CarpRod && x.ItemType == ItemTypes.Sinker;
				}), true);
			}
			Guid firstChum = Guid.Empty;
			if (this.Chum != null)
			{
				this.Chum.Set(inventory.FirstOrDefault(delegate(InventoryItem x)
				{
					Guid? parentItemInstanceId14 = x.ParentItemInstanceId;
					bool flag22 = parentItemInstanceId14 != null;
					Guid? instanceId14 = rod.InstanceId;
					return flag22 == (instanceId14 != null) && (parentItemInstanceId14 == null || parentItemInstanceId14.GetValueOrDefault() == instanceId14.GetValueOrDefault()) && x.ItemType == ItemTypes.Chum;
				}), true);
				firstChum = ((this.Chum.InventoryItem == null || this.Chum.InventoryItem.InstanceId == null) ? Guid.Empty : this.Chum.InventoryItem.InstanceId.Value);
			}
			if (this.SpodChumAdditional != null)
			{
				this.SpodChumAdditional.Set(inventory.FirstOrDefault(delegate(InventoryItem x)
				{
					if (x.InstanceId != null && x.InstanceId.Value != firstChum)
					{
						Guid? parentItemInstanceId15 = x.ParentItemInstanceId;
						bool flag23 = parentItemInstanceId15 != null;
						Guid? instanceId15 = rod.InstanceId;
						if (flag23 == (instanceId15 != null) && (parentItemInstanceId15 == null || parentItemInstanceId15.GetValueOrDefault() == instanceId15.GetValueOrDefault()))
						{
							return x.ItemType == ItemTypes.Chum;
						}
					}
					return false;
				}), true);
			}
			if (this.Leader != null)
			{
				bool activeInHierarchy = this.Leader.transform.parent.gameObject.activeInHierarchy;
				bool flag8 = RodTemplates.GetTypesForTemplate(largestTemplate).ToList<ItemTypes>().Contains(ItemTypes.Leader);
				if (activeInHierarchy && !flag5 && !flag8)
				{
					if (!rodEquipment.All((InventoryItem x) => x.ItemType == ItemTypes.Line || x.ItemType == ItemTypes.Reel))
					{
						this.Leader.transform.parent.gameObject.SetActive(false);
					}
				}
				this.Leader.Set(inventory.FirstOrDefault(delegate(InventoryItem x)
				{
					Guid? parentItemInstanceId16 = x.ParentItemInstanceId;
					bool flag24 = parentItemInstanceId16 != null;
					Guid? instanceId16 = rod.InstanceId;
					return flag24 == (instanceId16 != null) && (parentItemInstanceId16 == null || parentItemInstanceId16.GetValueOrDefault() == instanceId16.GetValueOrDefault()) && x.ItemType == ItemTypes.Leader;
				}), true);
			}
			if (this.Rod != null && this.Rod.ChangeHandler != null)
			{
				this.Rod.ChangeHandler.OnChange();
			}
		}
		else
		{
			if (this.Reel != null)
			{
				this.Reel.Set(null, true);
			}
			if (this.Line != null)
			{
				this.Line.Set(null, true);
			}
			if (this.LureHook != null)
			{
				this.LureHook.Set(null, true);
			}
			if (this.Bait != null)
			{
				this.Bait.Set(null, true);
			}
			if (this.Tackle != null)
			{
				this.Tackle.Set(null, true);
			}
			if (this.Bell != null)
			{
				this.Bell.Set(null, true);
			}
			if (this.Feeder != null)
			{
				this.Feeder.Set(null, true);
			}
			if (this.SpodFeeder != null)
			{
				this.SpodFeeder.Set(null, true);
			}
			if (this.PVASinker != null)
			{
				this.PVASinker.Set(null, true);
			}
			if (this.SpinningSinker != null)
			{
				this.SpinningSinker.Set(null, true);
			}
			if (this.Chum != null)
			{
				this.Chum.Set(null, true);
			}
			if (this.SpodChumAdditional != null)
			{
				this.SpodChumAdditional.Set(null, true);
			}
			if (this.Leader != null)
			{
				this.Leader.Set(null, true);
			}
		}
		this.RebuildLayout();
	}

	public ActiveStorage activeStorage;

	public int SlotId;

	public VerticalLayoutGroup layout;

	public InventoryItemDollComponent Rod;

	public InventoryItemDollComponent Reel;

	public InventoryItemDollComponent Line;

	public InventoryItemDollComponent Tackle;

	public InventoryItemDollComponent LureHook;

	public InventoryItemDollComponent Bait;

	public InventoryItemDollComponent Bell;

	public InventoryItemDollComponent Feeder;

	public InventoryItemDollComponent SpodFeeder;

	public InventoryItemDollComponent PVASinker;

	public InventoryItemDollComponent SpinningSinker;

	public InventoryItemDollComponent Chum;

	public InventoryItemDollComponent SpodChumAdditional;

	public InventoryItemDollComponent Leader;

	private ContentPreferredSizeSetter[] sizeSetters;
}
