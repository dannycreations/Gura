using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(1)]
	public class Inventory : List<InventoryItem>
	{
		public static Func<float> ExchangeRate { get; set; }

		[JsonIgnore]
		public Profile Profile
		{
			set
			{
				this.profile = value;
			}
		}

		[JsonIgnore]
		public RodTemplates RodTemplate
		{
			get
			{
				RodTemplates rodTemplates;
				if ((rodTemplates = this.rodTemplate) == null)
				{
					rodTemplates = (this.rodTemplate = new RodTemplates(this));
				}
				return rodTemplates;
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event OnInventoryChange OnInventoryChange;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event OnItemLostBecauseBroken OnItemLostBecauseBroken;

		[JsonIgnore]
		public static int DefaultInventoryCapacity { get; set; }

		[JsonIgnore]
		public static int MaxInventoryCapacity { get; set; }

		[JsonIgnore]
		public int CurrentInventoryCapacity
		{
			get
			{
				int defaultInventoryCapacity = Inventory.DefaultInventoryCapacity;
				int? additionalInventoryCapacity = this.profile.AdditionalInventoryCapacity;
				return defaultInventoryCapacity + ((additionalInventoryCapacity == null) ? 0 : additionalInventoryCapacity.Value);
			}
		}

		[JsonIgnore]
		public int AvailableInventoryCapacity
		{
			get
			{
				return this.CurrentInventoryCapacity - this.Count((InventoryItem i) => i.Storage == StoragePlaces.Storage && i.IsOccupyInventorySpace);
			}
		}

		[JsonIgnore]
		public bool IsStorageOverloaded
		{
			get
			{
				return (from item in this
					where item.Storage == StoragePlaces.Storage
					where item.IsOccupyInventorySpace
					select item).Skip(this.CurrentInventoryCapacity).Any<InventoryItem>();
			}
		}

		[JsonIgnore]
		public bool IsStorageFull
		{
			get
			{
				return (from item in this
					where item.Storage == StoragePlaces.Storage
					where item.IsOccupyInventorySpace
					select item).Take(this.CurrentInventoryCapacity).Count<InventoryItem>() == this.CurrentInventoryCapacity;
			}
		}

		[JsonIgnore]
		public List<InventoryItem> StorageInventory
		{
			get
			{
				return (from item in this
					where item.Storage == StoragePlaces.Storage
					where item.IsOccupyInventorySpace
					select item).Take(this.CurrentInventoryCapacity).ToList<InventoryItem>();
			}
		}

		[JsonIgnore]
		public List<InventoryItem> StorageExceededInventory
		{
			get
			{
				return (from item in this
					where item.Storage == StoragePlaces.Storage
					where item.IsOccupyInventorySpace
					select item).Skip(this.CurrentInventoryCapacity).ToList<InventoryItem>();
			}
		}

		[JsonIgnore]
		public static int DefaultRodSetupCapacity { get; set; }

		[JsonIgnore]
		public static int MaxRodSetupCapacity { get; set; }

		[JsonIgnore]
		public int CurrentRodSetupCapacity
		{
			get
			{
				int defaultRodSetupCapacity = Inventory.DefaultRodSetupCapacity;
				int? additionalRodSetupCapacity = this.profile.AdditionalRodSetupCapacity;
				return defaultRodSetupCapacity + ((additionalRodSetupCapacity == null) ? 0 : additionalRodSetupCapacity.Value);
			}
		}

		[JsonIgnore]
		public int RodSetupsCount
		{
			get
			{
				return this.profile.InventoryRodSetups.Count<RodSetup>();
			}
		}

		[JsonIgnore]
		public static int DefaultBuoyCapacity { get; set; }

		[JsonIgnore]
		public static int MaxBuoyCapacity { get; set; }

		[JsonIgnore]
		public int CurrentBuoyCapacity
		{
			get
			{
				int defaultBuoyCapacity = Inventory.DefaultBuoyCapacity;
				int? additionalBuoyCapacity = this.profile.AdditionalBuoyCapacity;
				return defaultBuoyCapacity + ((additionalBuoyCapacity == null) ? 0 : additionalBuoyCapacity.Value);
			}
		}

		[JsonIgnore]
		public int AvailableBuoyCapacity
		{
			get
			{
				return this.CurrentBuoyCapacity - ((this.profile.Buoys == null) ? 0 : this.profile.Buoys.Count);
			}
		}

		[JsonIgnore]
		public static int DefaultChumRecipesCapacity { get; set; }

		[JsonIgnore]
		public static int MaxChumRecipesCapacity { get; set; }

		[JsonIgnore]
		public int CurrentChumRecipesCapacity
		{
			get
			{
				int defaultChumRecipesCapacity = Inventory.DefaultChumRecipesCapacity;
				int? additionalChumRecipesCapacity = this.profile.AdditionalChumRecipesCapacity;
				return defaultChumRecipesCapacity + ((additionalChumRecipesCapacity == null) ? 0 : additionalChumRecipesCapacity.Value);
			}
		}

		[JsonIgnore]
		public int ChumRecipesCount
		{
			get
			{
				return this.profile.ChumRecipes.Count<Chum>();
			}
		}

		[JsonIgnore]
		public int AvailableChumRecipesCapacity
		{
			get
			{
				return this.CurrentChumRecipesCapacity - this.ChumRecipesCount;
			}
		}

		[JsonIgnore]
		public static bool IsRetail { get; set; }

		[JsonIgnore]
		public static float ChumGroundbaitsMixTime { get; set; }

		[JsonIgnore]
		public static float ChumCarpbaitsMixTime { get; set; }

		[JsonIgnore]
		public static float ChumMethodMixMixTime { get; set; }

		[JsonIgnore]
		public static float ChumGroundbaitsMixTimePremium { get; set; }

		[JsonIgnore]
		public static float ChumCarpbaitsMixTimePremium { get; set; }

		[JsonIgnore]
		public static float ChumMethodMixMixTimePremium { get; set; }

		[JsonIgnore]
		public static float ChumHandCapacity { get; set; }

		[JsonIgnore]
		public static float ChumSplitTime { get; set; } = 1.5f;

		[JsonIgnore]
		public static float ChumSplitTimePremium { get; set; } = 1.5f;

		[JsonIgnore]
		public static float ChumSplitTimeHands { get; set; } = 22f;

		[JsonIgnore]
		public static float ChumSplitTimeHandsPremium { get; set; } = 8f;

		[JsonIgnore]
		public static float MixBasePossiblePercentageMin { get; set; } = 75f;

		[JsonIgnore]
		public static float MixAromaPossiblePercentage { get; set; } = 20f;

		[JsonIgnore]
		public static float MixParticlePossiblePercentage { get; set; } = 20f;

		[JsonIgnore]
		public static float ChumHandMaxCastLength { get; set; }

		[JsonIgnore]
		public static float SilverRepairRate { get; set; } = 0.5f;

		[JsonIgnore]
		public static float GoldRepairRate { get; set; } = 0.5f;

		[JsonProperty(ItemTypeNameHandling = 3)]
		public InventoryItem[] Items
		{
			get
			{
				return base.ToArray();
			}
			set
			{
				base.Clear();
				base.AddRange(value);
				this.ResolveParents();
			}
		}

		public void ResolveParents()
		{
			this.ResolveParents(this);
		}

		public void ResolveParents(IEnumerable<InventoryItem> items)
		{
			using (IEnumerator<InventoryItem> enumerator = items.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					InventoryItem item = enumerator.Current;
					if (item.ParentItemInstanceId != null && item.ParentItem == null)
					{
						InventoryItem inventoryItem = base.Find(delegate(InventoryItem i)
						{
							Guid? instanceId = i.InstanceId;
							bool flag = instanceId != null;
							Guid? parentItemInstanceId = item.ParentItemInstanceId;
							return flag == (parentItemInstanceId != null) && (instanceId == null || instanceId.GetValueOrDefault() == parentItemInstanceId.GetValueOrDefault());
						});
						if (inventoryItem != null)
						{
							item.ParentItem = inventoryItem;
						}
					}
				}
			}
		}

		public List<MissionItem> MissionItems
		{
			get
			{
				return this.OfType<MissionItem>().ToList<MissionItem>();
			}
		}

		public string LastVerificationError { get; set; }

		public bool CanSwapRods(Rod currentRodInHands, Rod rodToHands)
		{
			this.LastVerificationError = null;
			if (currentRodInHands == null && rodToHands == null)
			{
				this.LastVerificationError = "Can't swap rods - no rod specified";
				return false;
			}
			if (currentRodInHands != null && currentRodInHands.Storage != StoragePlaces.Hands && currentRodInHands.Storage != StoragePlaces.Doll)
			{
				this.LastVerificationError = "Can't swap rods - current rod should be in hands";
				return false;
			}
			if (rodToHands != null && rodToHands.Storage != StoragePlaces.Doll && rodToHands.Storage != StoragePlaces.Hands)
			{
				this.LastVerificationError = "Can't swap rods - new rod should be on doll";
				return false;
			}
			if (rodToHands != null && this.GetRodTemplate(rodToHands) == ObjectModel.RodTemplate.UnEquiped)
			{
				this.LastVerificationError = "Can't swap rods - new rod is unequipped";
				return false;
			}
			return true;
		}

		public bool CanMove(InventoryItem item, InventoryItem parent, StoragePlaces storage, bool checkCapacity = true)
		{
			this.LastVerificationError = null;
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			if (this.profile.IsTravelingByCar != null && this.profile.IsTravelingByCar.Value && !this.HasCar())
			{
				this.LastVerificationError = "Player does not have car, but traveling on car";
				return false;
			}
			if (item.Rent != null)
			{
				this.LastVerificationError = "Can't modify rented item";
				return false;
			}
			if (item == parent)
			{
				this.LastVerificationError = "Can't make self parented item";
				return false;
			}
			if (!this.CheckDurability(item, parent, storage))
			{
				return false;
			}
			if (!this.CheckItemAvailability(item, storage))
			{
				return false;
			}
			if (parent != null && !this.CheckItemAvailability(parent, storage))
			{
				return false;
			}
			if (!this.CheckStorageCapacity(item, new StoragePlaces?(storage), null))
			{
				return false;
			}
			if (item is Rod && parent == null && ((item.Storage == StoragePlaces.Hands && storage == StoragePlaces.Doll) || (item.Storage == StoragePlaces.Doll && storage == StoragePlaces.Hands)))
			{
				return true;
			}
			if (storage == StoragePlaces.Equipment && item.Storage != StoragePlaces.ParentItem && this.IsBreakingEquipmentConstraints(item, null))
			{
				return false;
			}
			if (storage == StoragePlaces.ParentItem && item.Storage != StoragePlaces.Equipment && this.IsBreakingEquipmentConstraints(item, null))
			{
				return false;
			}
			if (storage == StoragePlaces.ParentItem && parent == null)
			{
				this.LastVerificationError = "Can't parent to NULL";
				return false;
			}
			if (storage == StoragePlaces.ParentItem && parent.Storage != StoragePlaces.Doll && parent.Storage != StoragePlaces.Hands && parent.Storage != StoragePlaces.ParentItem)
			{
				this.LastVerificationError = "Can't parent to unequipped rod";
				return false;
			}
			return (storage != StoragePlaces.Doll || !this.IsBreakingDollConstraints(item)) && (storage != StoragePlaces.Hands || !this.IsBreakingConstraints(new Dictionary<ItemSubTypes, InventoryConstraint>
			{
				{
					ItemSubTypes.Rod,
					new InventoryConstraint(1, true, null, null)
				},
				{
					ItemSubTypes.Chum,
					new InventoryConstraint(1, true, null, null)
				}
			}, StoragePlaces.Hands, item, null, null)) && (storage != StoragePlaces.Shore || !this.IsBreakingShoreConstraints(item)) && (parent != null || (storage != StoragePlaces.Doll && storage != StoragePlaces.Hands) || this.CheckItemAggregationDoll(item)) && this.CheckItemAggregation(item, parent, checkCapacity) && this.CheckParent(parent) && (item.Storage != StoragePlaces.ParentItem || this.CheckParent(item.ParentItem)) && this.CanUseChumExpired(item as Chum);
		}

		public bool CanCombine(InventoryItem item, InventoryItem targetItem)
		{
			this.LastVerificationError = null;
			return this.CheckItemAvailability(item, item.Storage) && this.CheckItemAvailability(targetItem, targetItem.Storage) && this.CheckStorageCapacity(item, null, targetItem) && (item.Storage != StoragePlaces.ParentItem || this.CheckParent(item.ParentItem)) && item != targetItem && item.CanCombineWith(targetItem);
		}

		public bool CanSplit(InventoryItem item, int count)
		{
			this.LastVerificationError = null;
			if (item.Rent != null)
			{
				this.LastVerificationError = "Can't modify rented item";
				return false;
			}
			if (!this.CheckItemAvailability(item, item.Storage))
			{
				return false;
			}
			if (!this.CheckStorageCapacity(item, null, null))
			{
				return false;
			}
			if (item.Count < count)
			{
				this.LastVerificationError = "Can't split item - not enough";
				return false;
			}
			return true;
		}

		public bool CanSplit(InventoryItem item, float amount)
		{
			this.LastVerificationError = null;
			if (item.Rent != null)
			{
				this.LastVerificationError = "Can't modify rented item";
				return false;
			}
			if (!this.CheckItemAvailability(item, item.Storage))
			{
				return false;
			}
			if (!this.CheckStorageCapacity(item, null, null))
			{
				return false;
			}
			if (item.Amount < amount)
			{
				this.LastVerificationError = "Can't split item - not enough";
				return false;
			}
			return true;
		}

		public bool CanCut(InventoryItem item, int count)
		{
			this.LastVerificationError = null;
			if (item.Rent != null)
			{
				this.LastVerificationError = "Can't modify rented item";
				return false;
			}
			return item.Count >= count;
		}

		public bool CanSubordinate(InventoryItem currentParent, InventoryItem newParent)
		{
			this.LastVerificationError = null;
			if (newParent != null && !this.CheckDurability(newParent, null, currentParent.Storage))
			{
				return false;
			}
			if (!this.CheckItemAvailability(currentParent, currentParent.Storage))
			{
				return false;
			}
			if (newParent != null && !this.CheckItemAvailability(newParent, newParent.Storage))
			{
				return false;
			}
			if (newParent != null && !this.CheckStorageCapacity(newParent, null, null))
			{
				return false;
			}
			if (!this.CheckParent(currentParent) || !this.CheckParent(newParent))
			{
				return false;
			}
			if (currentParent.Rent != null)
			{
				this.LastVerificationError = "Can't modify rented item";
				return false;
			}
			if (newParent == null)
			{
				return true;
			}
			if ((currentParent.ItemType != newParent.ItemType || currentParent.ItemSubType != newParent.ItemSubType) && !this.AreInterchangable(currentParent.ItemType, newParent.ItemType) && !this.AreInterchangable(currentParent.ItemSubType, newParent.ItemSubType))
			{
				this.LastVerificationError = "Item can be replaced only with item of the same type";
				return false;
			}
			return true;
		}

		private bool AreInterchangable(ItemTypes type1, ItemTypes type2)
		{
			return type1 == ItemTypes.Boat && type2 == ItemTypes.Boat;
		}

		private bool AreInterchangable(ItemSubTypes type1, ItemSubTypes type2)
		{
			return (type1 == ItemSubTypes.Keepnet && type2 == ItemSubTypes.Stringer) || (type1 == ItemSubTypes.Stringer && type2 == ItemSubTypes.Keepnet);
		}

		public bool CanReplace(InventoryItem item, InventoryItem replacementItem)
		{
			this.LastVerificationError = null;
			if (item == null || replacementItem == null)
			{
				this.LastVerificationError = "Items for replacement can't be null";
				return false;
			}
			if (item.Rent != null)
			{
				this.LastVerificationError = "Can't modify rented item";
				return false;
			}
			if (!this.CheckItemAvailability(item, item.Storage))
			{
				return false;
			}
			if (!this.CheckItemAvailability(replacementItem, replacementItem.Storage))
			{
				return false;
			}
			if (!this.CheckDurability(replacementItem, null, item.Storage))
			{
				return false;
			}
			if (!this.CheckStorageCapacity(replacementItem, null, null))
			{
				return false;
			}
			if (item.Storage == StoragePlaces.ParentItem && !this.CheckParent(item.ParentItem))
			{
				return false;
			}
			if (item == replacementItem)
			{
				this.LastVerificationError = "Can't replace item with itself";
				return false;
			}
			if (item.ParentItem == null && (item.Storage == StoragePlaces.Doll || item.Storage == StoragePlaces.Hands) && !this.CheckItemAggregationDoll(replacementItem))
			{
				return false;
			}
			if (!this.CheckItemAggregation(replacementItem, item.ParentItem, false))
			{
				return false;
			}
			if (item.ItemType != replacementItem.ItemType && (!item.ItemType.IsSwappableLuresType() || !replacementItem.ItemType.IsSwappableLuresType()) && (item.ItemSubType != ItemSubTypes.OffsetHook || !replacementItem.ItemType.IsSwappableLuresType()) && (!item.ItemType.IsSwappableLuresType() || replacementItem.ItemSubType != ItemSubTypes.OffsetHook) && (item.ItemType != ItemTypes.Sinker || replacementItem.ItemType != ItemTypes.Feeder) && (item.ItemType != ItemTypes.Feeder || replacementItem.ItemType != ItemTypes.Sinker))
			{
				this.LastVerificationError = "Item can be replaced only with item of the same type";
				return false;
			}
			if (item.ItemType == ItemTypes.Rod || item.ItemType == ItemTypes.Reel || item.ItemType == ItemTypes.Outfit)
			{
				this.LastVerificationError = "Item can be replaced only with item of the same type";
				return item.ItemSubType == replacementItem.ItemSubType;
			}
			return this.CanUseChumExpired(replacementItem as Chum);
		}

		public bool CanDestroyItem(InventoryItem item)
		{
			this.LastVerificationError = null;
			return !(item is MissionItem);
		}

		public bool CheckItemAvailability(InventoryItem item, StoragePlaces storage)
		{
			if (!this.IsStorageAvailable() && (item.Storage == StoragePlaces.Storage || storage == StoragePlaces.Storage))
			{
				this.LastVerificationError = "Storage is not available at pond";
				return false;
			}
			if ((item.Storage == StoragePlaces.CarEquipment || storage == StoragePlaces.CarEquipment) && !this.IsCarAvailable())
			{
				this.LastVerificationError = "No car - no car equipment";
				return false;
			}
			if ((item.Storage == StoragePlaces.LodgeEquipment || storage == StoragePlaces.LodgeEquipment) && !this.IsLodgeAvailable())
			{
				this.LastVerificationError = "No lodge - no lodge equipment";
				return false;
			}
			return true;
		}

		private bool CheckStorageCapacity(InventoryItem item, StoragePlaces? targetStorage, InventoryItem targetItem = null)
		{
			if (item.IsOccupyInventorySpace && this.IsStorageFull)
			{
				if (targetStorage == StoragePlaces.Storage)
				{
					this.LastVerificationError = "Can't move items into overloaded storage";
					return false;
				}
				if (item.Storage == StoragePlaces.Storage && this.StorageExceededInventory.Any(delegate(InventoryItem exceeded)
				{
					Guid? instanceId = exceeded.InstanceId;
					bool flag = instanceId != null;
					Guid? instanceId2 = item.InstanceId;
					return flag == (instanceId2 != null) && (instanceId == null || instanceId.GetValueOrDefault() == instanceId2.GetValueOrDefault());
				}))
				{
					this.LastVerificationError = "Can't move items from exceeded storage";
					return false;
				}
				if (targetItem != null && targetItem.Storage == StoragePlaces.Storage && this.StorageExceededInventory.Any(delegate(InventoryItem exceeded)
				{
					Guid? instanceId3 = exceeded.InstanceId;
					bool flag2 = instanceId3 != null;
					Guid? instanceId4 = targetItem.InstanceId;
					return flag2 == (instanceId4 != null) && (instanceId3 == null || instanceId3.GetValueOrDefault() == instanceId4.GetValueOrDefault());
				}))
				{
					this.LastVerificationError = "Can't move items into overloaded storage";
					return false;
				}
			}
			return true;
		}

		private void MoveRelatedItems(InventoryItem item, InventoryItem replacementItem, Guid? parentItemInstanceId, StoragePlaces oldStorage, StoragePlaces newStorage, InventoryMovementMapInfo info, bool moveToEquipment = true)
		{
			if (item is LuresBox && oldStorage == StoragePlaces.Doll && newStorage != StoragePlaces.Doll)
			{
				foreach (InventoryItem inventoryItem in this.TackleOutOfStorage)
				{
					InventoryMovementMapInfo relatedInfoOrNew = info.GetRelatedInfoOrNew(inventoryItem.InstanceId.Value);
					this.MoveItem(inventoryItem, null, StoragePlaces.Storage, relatedInfoOrNew, true, true);
					info.SetRelatedInfoIfHasData(inventoryItem.InstanceId.Value, relatedInfoOrNew);
				}
				foreach (InventoryItem inventoryItem2 in this.ChumOutOfStorage)
				{
					InventoryMovementMapInfo relatedInfoOrNew2 = info.GetRelatedInfoOrNew(inventoryItem2.InstanceId.Value);
					this.MoveItem(inventoryItem2, null, StoragePlaces.Storage, relatedInfoOrNew2, true, true);
					info.SetRelatedInfoIfHasData(inventoryItem2.InstanceId.Value, relatedInfoOrNew2);
				}
			}
			if (item is Hat && oldStorage == StoragePlaces.Doll && newStorage != StoragePlaces.Doll)
			{
				foreach (InventoryItem inventoryItem3 in this.TackleOutOfStorage)
				{
					InventoryMovementMapInfo relatedInfoOrNew3 = info.GetRelatedInfoOrNew(inventoryItem3.InstanceId.Value);
					this.MoveItem(inventoryItem3, null, StoragePlaces.Storage, relatedInfoOrNew3, true, true);
					info.SetRelatedInfoIfHasData(inventoryItem3.InstanceId.Value, relatedInfoOrNew3);
				}
			}
			if (item is RodCase && oldStorage == StoragePlaces.Doll && newStorage != StoragePlaces.Doll)
			{
				foreach (InventoryItem inventoryItem4 in this.RodsAndReelsOutOfStorage)
				{
					InventoryMovementMapInfo relatedInfoOrNew4 = info.GetRelatedInfoOrNew(inventoryItem4.InstanceId.Value);
					this.MoveItem(inventoryItem4, null, StoragePlaces.Storage, relatedInfoOrNew4, true, true);
					info.SetRelatedInfoIfHasData(inventoryItem4.InstanceId.Value, relatedInfoOrNew4);
				}
				foreach (InventoryItem inventoryItem5 in this.TackleOutOfStorage)
				{
					InventoryMovementMapInfo relatedInfoOrNew5 = info.GetRelatedInfoOrNew(inventoryItem5.InstanceId.Value);
					this.MoveItem(inventoryItem5, null, StoragePlaces.Storage, relatedInfoOrNew5, true, true);
					info.SetRelatedInfoIfHasData(inventoryItem5.InstanceId.Value, relatedInfoOrNew5);
				}
				this.RenumberSlots();
			}
			List<InventoryItem> list = new List<InventoryItem>();
			bool flag = false;
			if (item is Rod && (oldStorage == StoragePlaces.Doll || oldStorage == StoragePlaces.Hands || oldStorage == StoragePlaces.Shore) && newStorage != StoragePlaces.Doll && newStorage != StoragePlaces.Hands && newStorage != StoragePlaces.Shore)
			{
				flag = true;
				List<InventoryItem> list2 = this.Where(delegate(InventoryItem i)
				{
					Guid? parentItemInstanceId2 = i.ParentItemInstanceId;
					bool flag2 = parentItemInstanceId2 != null;
					Guid? instanceId = item.InstanceId;
					return flag2 == (instanceId != null) && (parentItemInstanceId2 == null || parentItemInstanceId2.GetValueOrDefault() == instanceId.GetValueOrDefault());
				}).ToList<InventoryItem>();
				list.AddRange(list2);
			}
			if (item is Reel && oldStorage == StoragePlaces.ParentItem && newStorage != StoragePlaces.ParentItem && parentItemInstanceId != null && (replacementItem == null || replacementItem.ItemType != ItemTypes.Reel))
			{
				List<InventoryItem> list3 = (from i in this.Where(delegate(InventoryItem i)
					{
						Guid? parentItemInstanceId3 = i.ParentItemInstanceId;
						return parentItemInstanceId3 != null == (parentItemInstanceId != null) && (parentItemInstanceId3 == null || parentItemInstanceId3.GetValueOrDefault() == parentItemInstanceId.GetValueOrDefault());
					})
					where !(i is Bell) && !(i is Reel)
					select i).ToList<InventoryItem>();
				list.AddRange(list3);
			}
			if (item is Line && oldStorage == StoragePlaces.ParentItem && newStorage != StoragePlaces.ParentItem && parentItemInstanceId != null && (replacementItem == null || replacementItem.ItemType != ItemTypes.Line))
			{
				List<InventoryItem> list4 = (from i in this.Where(delegate(InventoryItem i)
					{
						Guid? parentItemInstanceId4 = i.ParentItemInstanceId;
						return parentItemInstanceId4 != null == (parentItemInstanceId != null) && (parentItemInstanceId4 == null || parentItemInstanceId4.GetValueOrDefault() == parentItemInstanceId.GetValueOrDefault());
					})
					where !(i is Bell) && !(i is Reel) && !(i is Line)
					select i).ToList<InventoryItem>();
				list.AddRange(list4);
			}
			if (item is Bobber && oldStorage == StoragePlaces.ParentItem && newStorage != StoragePlaces.ParentItem && parentItemInstanceId != null && (replacementItem == null || replacementItem.ItemType != ItemTypes.Bobber))
			{
				List<InventoryItem> list5 = (from i in this.Where(delegate(InventoryItem i)
					{
						Guid? parentItemInstanceId5 = i.ParentItemInstanceId;
						return parentItemInstanceId5 != null == (parentItemInstanceId != null) && (parentItemInstanceId5 == null || parentItemInstanceId5.GetValueOrDefault() == parentItemInstanceId.GetValueOrDefault());
					})
					where !(i is Bell) && !(i is Reel) && !(i is Line) && !(i is Bobber)
					select i).ToList<InventoryItem>();
				list.AddRange(list5);
			}
			if (item.ItemType == ItemTypes.Feeder && oldStorage == StoragePlaces.ParentItem && newStorage != StoragePlaces.ParentItem && parentItemInstanceId != null)
			{
				List<InventoryItem> list6 = (from i in this.Where(delegate(InventoryItem i)
					{
						Guid? parentItemInstanceId6 = i.ParentItemInstanceId;
						return parentItemInstanceId6 != null == (parentItemInstanceId != null) && (parentItemInstanceId6 == null || parentItemInstanceId6.GetValueOrDefault() == parentItemInstanceId.GetValueOrDefault());
					})
					where i.ItemType == ItemTypes.Chum
					select i).ToList<InventoryItem>();
				list.AddRange(list6);
				if (item.ItemSubType == ItemSubTypes.PvaFeeder && replacementItem != null && (replacementItem.ItemType != ItemTypes.Feeder || replacementItem.ItemSubType != ItemSubTypes.PvaFeeder))
				{
					List<InventoryItem> list7 = (from i in this.Where(delegate(InventoryItem i)
						{
							Guid? parentItemInstanceId7 = i.ParentItemInstanceId;
							return parentItemInstanceId7 != null == (parentItemInstanceId != null) && (parentItemInstanceId7 == null || parentItemInstanceId7.GetValueOrDefault() == parentItemInstanceId.GetValueOrDefault());
						})
						where i.ItemType == ItemTypes.Sinker
						select i).Where(delegate(InventoryItem i)
					{
						Guid? instanceId2 = i.InstanceId;
						bool flag3 = instanceId2 != null;
						Guid? instanceId3 = replacementItem.InstanceId;
						return flag3 != (instanceId3 != null) || (instanceId2 != null && instanceId2.GetValueOrDefault() != instanceId3.GetValueOrDefault());
					}).ToList<InventoryItem>();
					list.AddRange(list7);
				}
			}
			if (item is Leader && oldStorage == StoragePlaces.ParentItem && newStorage != StoragePlaces.ParentItem && parentItemInstanceId != null && (replacementItem == null || replacementItem.ItemSubType != item.ItemSubType))
			{
				List<InventoryItem> list8 = (from i in this.Where(delegate(InventoryItem i)
					{
						Guid? parentItemInstanceId8 = i.ParentItemInstanceId;
						return parentItemInstanceId8 != null == (parentItemInstanceId != null) && (parentItemInstanceId8 == null || parentItemInstanceId8.GetValueOrDefault() == parentItemInstanceId.GetValueOrDefault());
					})
					where i.ItemType == ItemTypes.Hook || i.ItemType == ItemTypes.Lure || i.ItemType == ItemTypes.Bait || i.ItemType == ItemTypes.Sinker || i.ItemType == ItemTypes.Feeder || i.ItemType == ItemTypes.JigHead || i.ItemType == ItemTypes.JigBait
					select i).ToList<InventoryItem>();
				list.AddRange(list8);
			}
			if (item is Hook && oldStorage == StoragePlaces.ParentItem && newStorage != StoragePlaces.ParentItem && parentItemInstanceId != null && (replacementItem == null || replacementItem.ItemSubType != item.ItemSubType))
			{
				List<InventoryItem> list9 = (from i in this.Where(delegate(InventoryItem i)
					{
						Guid? parentItemInstanceId9 = i.ParentItemInstanceId;
						return parentItemInstanceId9 != null == (parentItemInstanceId != null) && (parentItemInstanceId9 == null || parentItemInstanceId9.GetValueOrDefault() == parentItemInstanceId.GetValueOrDefault());
					})
					where i.ItemType == ItemTypes.Bait || i.ItemType == ItemTypes.JigBait
					select i).ToList<InventoryItem>();
				list.AddRange(list9);
			}
			if (item.ItemType == ItemTypes.JigHead && oldStorage == StoragePlaces.ParentItem && newStorage != StoragePlaces.ParentItem && parentItemInstanceId != null && (replacementItem == null || replacementItem.ItemType != ItemTypes.JigHead))
			{
				List<InventoryItem> list10 = (from i in this.Where(delegate(InventoryItem i)
					{
						Guid? parentItemInstanceId10 = i.ParentItemInstanceId;
						return parentItemInstanceId10 != null == (parentItemInstanceId != null) && (parentItemInstanceId10 == null || parentItemInstanceId10.GetValueOrDefault() == parentItemInstanceId.GetValueOrDefault());
					})
					where i.ItemType == ItemTypes.JigBait
					select i).ToList<InventoryItem>();
				list.AddRange(list10);
			}
			if (list.Any<InventoryItem>())
			{
				foreach (InventoryItem inventoryItem6 in list.Distinct<InventoryItem>())
				{
					if (flag)
					{
						InventoryItem inventoryItem7 = this.MoveOrCombineItem(inventoryItem6, null, StoragePlaces.Storage, info, false);
						if (moveToEquipment && !this.ShouldRemoveItem(inventoryItem7) && this.CanMove(inventoryItem7, null, StoragePlaces.Equipment, true))
						{
							this.MoveOrCombineItem(inventoryItem7, null, StoragePlaces.Equipment, info, false);
						}
					}
					else
					{
						this.MoveOrCombineItem(inventoryItem6, null, StoragePlaces.Equipment, info, false);
					}
				}
			}
		}

		public int GetEmptySlotNumber()
		{
			for (int i = 1; i <= this.RodSlotsCount; i++)
			{
				if (this.GetRodInSlot(i) == null)
				{
					return i;
				}
			}
			return 0;
		}

		public void RenumberSlots()
		{
			int num = 1;
			foreach (InventoryItem inventoryItem in from i in this.Items
				where i is Rod && (i.Storage == StoragePlaces.Doll || i.Storage == StoragePlaces.Hands)
				orderby i.Slot
				select i)
			{
				inventoryItem.Slot = num;
				num++;
			}
			foreach (InventoryItem inventoryItem2 in this.Items.Where((InventoryItem i) => i is Rod && i.Storage != StoragePlaces.Doll && i.Storage != StoragePlaces.Hands))
			{
				inventoryItem2.Slot = 0;
			}
		}

		public bool CanMoveOrCombineItem(InventoryItem item, InventoryItem parent, StoragePlaces storage)
		{
			return this.CanMoveOrCombineItem(item, parent, storage, new InventoryMovementMapInfo());
		}

		public bool CanMoveOrCombineItem(InventoryItem item, InventoryItem parent, StoragePlaces storage, InventoryMovementMapInfo info)
		{
			this.LastVerificationError = null;
			InventoryItem inventoryItem = this.CheckMoveIsCombine(item, parent, storage, info);
			if (inventoryItem != null)
			{
				return this.CanCombine(item, inventoryItem);
			}
			return this.CanMove(item, parent, storage, true);
		}

		public InventoryItem MoveOrCombineItem(InventoryItem item, InventoryItem parent, StoragePlaces storage, InventoryMovementMapInfo info, bool moveRelatedItems = true)
		{
			InventoryItem inventoryItem = this.CheckMoveIsCombine(item, parent, storage, info);
			if (inventoryItem != null)
			{
				if (item.IsStockableByAmount)
				{
					InventoryItem inventoryItem2 = inventoryItem;
					float amount = item.Amount;
					this.CombineItem(item, inventoryItem2, amount, info, null, moveRelatedItems);
				}
				else
				{
					InventoryItem inventoryItem3 = inventoryItem;
					int count = item.Count;
					this.CombineItem(item, inventoryItem3, count, info, null, moveRelatedItems);
				}
				return inventoryItem;
			}
			this.MoveItem(item, parent, storage, info, moveRelatedItems, true);
			return item;
		}

		private bool CheckDurability(InventoryItem item, InventoryItem parent, StoragePlaces storage)
		{
			if (parent != null && parent.Durability == 0)
			{
				this.LastVerificationError = "Can't equip item on broken item";
				return false;
			}
			if (item.Durability > 0)
			{
				this.LastVerificationError = null;
				return true;
			}
			if (storage == StoragePlaces.Doll && item is Rod)
			{
				return true;
			}
			if (storage == StoragePlaces.Equipment || storage == StoragePlaces.Storage || storage == StoragePlaces.CarEquipment || storage == StoragePlaces.LodgeEquipment)
			{
				return true;
			}
			this.LastVerificationError = "Can't equip broken item";
			return false;
		}

		private bool IsBreakingEquipmentConstraints(InventoryItem item, List<InventoryItem> itemsToReplace = null)
		{
			if (!item.IsOccupyInventorySpace)
			{
				return false;
			}
			if (item is Reel)
			{
				return this.IsBreakingConstraints(this.EquipConstraintsCache(), StoragePlaces.Equipment, item, new StoragePlaces?(StoragePlaces.ParentItem), itemsToReplace);
			}
			IDictionary<ItemSubTypes, InventoryConstraint> dictionary = this.EquipConstraintsCache();
			StoragePlaces storagePlaces = StoragePlaces.Equipment;
			return this.IsBreakingConstraints(dictionary, storagePlaces, item, null, itemsToReplace);
		}

		private bool IsBreakingDollConstraints(InventoryItem item)
		{
			return this.IsBreakingConstraints(this.DollConstraintsCache(), StoragePlaces.Doll, item, null, null);
		}

		private bool IsBreakingShoreConstraints(InventoryItem item)
		{
			return this.IsBreakingConstraints(this.ShoreConstraintsCache(), StoragePlaces.Shore, item, null, null);
		}

		private bool IsBreakingConstraints(IDictionary<ItemSubTypes, InventoryConstraint> constraints, StoragePlaces storage, InventoryItem item, StoragePlaces? altStorage = null, List<InventoryItem> itemsToReplace = null)
		{
			if (!item.IsOccupyInventorySpace)
			{
				return false;
			}
			InventoryConstraint inventoryConstraint = null;
			if (constraints.ContainsKey(item.ItemSubType))
			{
				inventoryConstraint = constraints[item.ItemSubType];
			}
			else
			{
				ItemSubTypes itemSubTypes = Inventory.ToSubtype(item.ItemType);
				if (constraints.ContainsKey(itemSubTypes))
				{
					inventoryConstraint = constraints[itemSubTypes];
				}
			}
			if (inventoryConstraint == null)
			{
				this.LastVerificationError = "No constraint - item of this type can't be equiped at all";
				return true;
			}
			int num = this.GetCurrentCount(storage, inventoryConstraint, item, altStorage);
			Guid? currentItem = this.GetCurrentItemInstanceId(storage, inventoryConstraint, item, altStorage);
			if (itemsToReplace != null)
			{
				num -= Inventory.GetCurrentCount(itemsToReplace, storage, inventoryConstraint, item, (itemsToReplace == null || !itemsToReplace.Any<InventoryItem>()) ? null : new StoragePlaces?(StoragePlaces.ParentItem));
			}
			if (storage == StoragePlaces.Doll)
			{
				num += this.GetCurrentCount(StoragePlaces.Hands, inventoryConstraint, item, altStorage);
				if (currentItem == null)
				{
					currentItem = this.GetCurrentItemInstanceId(StoragePlaces.Hands, inventoryConstraint, item, altStorage);
				}
			}
			if (num >= inventoryConstraint.Count)
			{
				InventoryItem inventoryItem = ((currentItem == null) ? null : this.FirstOrDefault(delegate(InventoryItem i)
				{
					Guid? instanceId = i.InstanceId;
					return instanceId != null == (currentItem != null) && (instanceId == null || instanceId.GetValueOrDefault() == currentItem.GetValueOrDefault());
				}));
				this.LastVerificationError = "Can't equip any more items" + string.Format(".[[ There are {0} item(s) already equiped. Moving from {1} to {2} item {3}. Existing item: {4}]]", new object[]
				{
					num,
					Enum.GetName(typeof(StoragePlaces), item.Storage),
					Enum.GetName(typeof(StoragePlaces), storage),
					item,
					(inventoryItem == null) ? "(none)" : inventoryItem.ToString()
				});
			}
			return num >= inventoryConstraint.Count;
		}

		public int GetConstraintAvailableCount(IDictionary<ItemSubTypes, InventoryConstraint> constraints, StoragePlaces storage, InventoryItem item, StoragePlaces? altStorage = null)
		{
			InventoryConstraint inventoryConstraint = null;
			if (constraints.ContainsKey(item.ItemSubType))
			{
				inventoryConstraint = constraints[item.ItemSubType];
			}
			else
			{
				ItemSubTypes itemSubTypes = Inventory.ToSubtype(item.ItemType);
				if (constraints.ContainsKey(itemSubTypes))
				{
					inventoryConstraint = constraints[itemSubTypes];
				}
			}
			if (inventoryConstraint == null)
			{
				return 0;
			}
			int num = this.GetCurrentCount(storage, inventoryConstraint, item, altStorage);
			if (storage == StoragePlaces.Doll)
			{
				num += this.GetCurrentCount(StoragePlaces.Hands, inventoryConstraint, item, altStorage);
			}
			return inventoryConstraint.Count - num;
		}

		public int GetConstraintAvailableCountDoll(InventoryItem item)
		{
			return this.GetConstraintAvailableCount(this.DollConstraintsCache(), StoragePlaces.Doll, item, null);
		}

		public int GetConstraintAvailableCountEquipment(InventoryItem item)
		{
			if (item is Reel)
			{
				return this.GetConstraintAvailableCount(this.EquipConstraintsCache(), StoragePlaces.Equipment, item, new StoragePlaces?(StoragePlaces.ParentItem));
			}
			return this.GetConstraintAvailableCount(this.EquipConstraintsCache(), StoragePlaces.Equipment, item, null);
		}

		public int GetConstraintAvailableCountShore(InventoryItem item)
		{
			return this.GetConstraintAvailableCount(this.ShoreConstraintsCache(), StoragePlaces.Shore, item, null);
		}

		public int GetCurrentItemsCount(ItemTypes itemType)
		{
			StoragePlaces storagePlaces = StoragePlaces.Equipment;
			if (itemType == ItemTypes.Rod || itemType == ItemTypes.Outfit)
			{
				storagePlaces = StoragePlaces.Doll;
			}
			IDictionary<ItemSubTypes, InventoryConstraint> dictionary = null;
			if (storagePlaces == StoragePlaces.Doll)
			{
				dictionary = this.DollConstraintsCache();
			}
			else if (storagePlaces == StoragePlaces.Equipment)
			{
				dictionary = this.EquipConstraintsCache();
			}
			ItemSubTypes itemSubTypes = Inventory.ToSubtype(itemType);
			InventoryConstraint inventoryConstraint = null;
			if (dictionary.ContainsKey(itemSubTypes))
			{
				inventoryConstraint = dictionary[itemSubTypes];
			}
			StoragePlaces? storagePlaces2 = null;
			if (storagePlaces == StoragePlaces.Equipment && itemType == ItemTypes.Reel)
			{
				storagePlaces2 = new StoragePlaces?(StoragePlaces.ParentItem);
			}
			return this.GetCurrentCount(storagePlaces, inventoryConstraint, new InventoryItem
			{
				ItemType = itemType
			}, storagePlaces2);
		}

		private static IEnumerable<InventoryItem> GetConstraintItemsQuery(List<InventoryItem> list, StoragePlaces storage, InventoryConstraint constraint, InventoryItem item, StoragePlaces? altStorage = null, bool returnFirstOnly = false)
		{
			if (constraint == null)
			{
				foreach (InventoryItem k in list.Where((InventoryItem it) => it.IsOccupyInventorySpace || storage == StoragePlaces.Doll || storage == StoragePlaces.Hands))
				{
					if (k != item && (k.Storage == storage || (altStorage != null && k.Storage == altStorage)) && (item.ItemType == k.ItemType || item.ItemSubType == k.ItemSubType))
					{
						yield return k;
						if (returnFirstOnly)
						{
							yield break;
						}
					}
				}
			}
			else if (constraint.AltTypes != null && constraint.AltTypes.Length > 0)
			{
				using (IEnumerator<InventoryItem> enumerator2 = list.Where((InventoryItem it) => it.IsOccupyInventorySpace || storage == StoragePlaces.Doll || storage == StoragePlaces.Hands).GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						InventoryItem l = enumerator2.Current;
						if (l != item && (l.Storage == storage || (altStorage != null && l.Storage == altStorage)) && constraint.AltTypes.Any((ItemTypes subType) => subType == l.ItemType))
						{
							yield return l;
							if (returnFirstOnly)
							{
								yield break;
							}
						}
					}
				}
			}
			else if (constraint.AltSubTypes != null && constraint.AltSubTypes.Length > 0)
			{
				using (IEnumerator<InventoryItem> enumerator3 = list.Where((InventoryItem it) => it.IsOccupyInventorySpace || storage == StoragePlaces.Doll || storage == StoragePlaces.Hands).GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						InventoryItem i = enumerator3.Current;
						if (i != item && (i.Storage == storage || (altStorage != null && i.Storage == altStorage)) && constraint.AltSubTypes.Any((ItemSubTypes subType) => subType == i.ItemSubType))
						{
							yield return i;
							if (returnFirstOnly)
							{
								yield break;
							}
						}
					}
				}
			}
			else
			{
				foreach (InventoryItem j in list.Where((InventoryItem it) => it.IsOccupyInventorySpace || storage == StoragePlaces.Doll || storage == StoragePlaces.Hands))
				{
					if (j != item && (j.Storage == storage || (altStorage != null && j.Storage == altStorage)) && ((constraint.PerType && item.ItemType == j.ItemType) || (!constraint.PerType && item.ItemSubType == j.ItemSubType)))
					{
						yield return j;
						if (returnFirstOnly)
						{
							yield break;
						}
					}
				}
			}
			yield break;
		}

		private static int GetCurrentCount(List<InventoryItem> list, StoragePlaces storage, InventoryConstraint constraint, InventoryItem item, StoragePlaces? altStorage = null)
		{
			List<InventoryItem> list2 = Inventory.GetConstraintItemsQuery(list, storage, constraint, item, altStorage, false).ToList<InventoryItem>();
			return list2.Count<InventoryItem>();
		}

		private int GetCurrentCount(StoragePlaces storage, InventoryConstraint constraint, InventoryItem item, StoragePlaces? altStorage = null)
		{
			return Inventory.GetCurrentCount(this, storage, constraint, item, altStorage);
		}

		private Guid? GetCurrentItemInstanceId(StoragePlaces storage, InventoryConstraint constraint, InventoryItem item, StoragePlaces? altStorage = null)
		{
			List<InventoryItem> list = Inventory.GetConstraintItemsQuery(this, storage, constraint, item, altStorage, true).ToList<InventoryItem>();
			using (List<InventoryItem>.Enumerator enumerator = list.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					InventoryItem inventoryItem = enumerator.Current;
					return inventoryItem.InstanceId;
				}
			}
			return null;
		}

		private bool CheckParent(InventoryItem parent)
		{
			Rod rod = parent as Rod;
			if (rod != null && rod.IsCasted)
			{
				this.LastVerificationError = "Rod can not be modified when casted";
				return false;
			}
			return true;
		}

		public bool HasCar()
		{
			foreach (InventoryItem inventoryItem in this)
			{
				if (inventoryItem is Car)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasLodge(int pondId)
		{
			foreach (InventoryItem inventoryItem in this)
			{
				if (inventoryItem is Lodge && ((Lodge)inventoryItem).PondId == pondId)
				{
					return true;
				}
			}
			return false;
		}

		private static ItemSubTypes ToSubtype(ItemTypes type)
		{
			return (ItemSubTypes)type;
		}

		public IDictionary<ItemSubTypes, InventoryConstraint> EquipConstraintsCache()
		{
			if (this.equipConstraintsCache == null)
			{
				this.equipConstraintsCache = this.InitEquipConstraintCache();
			}
			return this.equipConstraintsCache;
		}

		public IDictionary<ItemSubTypes, InventoryConstraint> DollConstraintsCache()
		{
			if (this.dollConstraintsCache == null)
			{
				this.dollConstraintsCache = this.InitDollConstraintCache();
			}
			return this.dollConstraintsCache;
		}

		public IDictionary<ItemSubTypes, InventoryConstraint> ShoreConstraintsCache()
		{
			if (this.shoreConstraintsCache == null)
			{
				this.shoreConstraintsCache = this.InitShoreConstraintCache();
			}
			return this.shoreConstraintsCache;
		}

		public void InvalidateConstraints(InventoryItem item)
		{
			if (item is RodCase)
			{
				this.dollConstraintsCache = null;
			}
			if (item is RodCase || item is LuresBox || item is Hat)
			{
				this.equipConstraintsCache = null;
			}
			if (item is RodStand)
			{
				this.shoreConstraintsCache = null;
			}
		}

		private IDictionary<ItemSubTypes, InventoryConstraint> InitEquipConstraintCache()
		{
			IDictionary<ItemSubTypes, InventoryConstraint> dictionary = this.CloneConstraints(Inventory.BasicEquipmentConstraints);
			foreach (InventoryItem inventoryItem in this)
			{
				if (inventoryItem.Storage == StoragePlaces.Doll)
				{
					if (inventoryItem is RodCase)
					{
						dictionary[ItemSubTypes.Reel].Increment(((RodCase)inventoryItem).RodWithReelCount + ((RodCase)inventoryItem).ReelCount);
						dictionary[ItemSubTypes.Line].Increment(((RodCase)inventoryItem).RodWithReelCount);
					}
					else if (inventoryItem is LuresBox)
					{
						LuresBox luresBox = (LuresBox)inventoryItem;
						dictionary[ItemSubTypes.Reel].Increment(luresBox.ReelCount);
						dictionary[ItemSubTypes.Line].Increment(luresBox.LineCount);
						dictionary[ItemSubTypes.Bait].Increment(luresBox.TackleKitCount);
						dictionary[ItemSubTypes.Lure].Increment(luresBox.TackleKitCount);
						dictionary[ItemSubTypes.Hook].Increment(luresBox.TackleKitCount);
						dictionary[ItemSubTypes.Bobber].Increment(luresBox.TackleKitCount);
						dictionary[ItemSubTypes.JigHead].Increment(luresBox.TackleKitCount);
						dictionary[ItemSubTypes.JigBait].Increment(luresBox.TackleKitCount);
						dictionary[ItemSubTypes.Sinker].Increment(luresBox.TackleKitCount);
						dictionary[ItemSubTypes.Feeder].Increment(luresBox.TackleKitCount);
						dictionary[ItemSubTypes.Bell].Increment(luresBox.TackleKitCount);
						dictionary[ItemSubTypes.Leader].Increment(luresBox.TackleKitCount);
						dictionary[ItemSubTypes.UnderwaterItem].Increment(luresBox.TackleKitCount);
						dictionary[ItemSubTypes.MissionItem].Increment(luresBox.TackleKitCount);
						dictionary[ItemSubTypes.Chum].Increment(luresBox.ChumCount);
						dictionary[ItemSubTypes.ChumBase].Increment(luresBox.ChumCount);
						dictionary[ItemSubTypes.ChumParticle].Increment(luresBox.ChumCount);
						dictionary[ItemSubTypes.ChumAroma].Increment(luresBox.ChumCount);
					}
					else if (inventoryItem is Hat)
					{
						Hat hat = (Hat)inventoryItem;
						dictionary[ItemSubTypes.Bait].Increment(hat.TackleKitCount);
						dictionary[ItemSubTypes.Lure].Increment(hat.TackleKitCount);
						dictionary[ItemSubTypes.Hook].Increment(hat.TackleKitCount);
						dictionary[ItemSubTypes.Bobber].Increment(hat.TackleKitCount);
						dictionary[ItemSubTypes.JigHead].Increment(hat.TackleKitCount);
						dictionary[ItemSubTypes.JigBait].Increment(hat.TackleKitCount);
						dictionary[ItemSubTypes.Sinker].Increment(hat.TackleKitCount);
						dictionary[ItemSubTypes.Feeder].Increment(hat.TackleKitCount);
						dictionary[ItemSubTypes.Bell].Increment(hat.TackleKitCount);
						dictionary[ItemSubTypes.Leader].Increment(hat.TackleKitCount);
						dictionary[ItemSubTypes.UnderwaterItem].Increment(hat.TackleKitCount);
						dictionary[ItemSubTypes.MissionItem].Increment(hat.TackleKitCount);
					}
				}
			}
			return dictionary;
		}

		private IDictionary<ItemSubTypes, InventoryConstraint> CloneConstraints(IDictionary<ItemSubTypes, InventoryConstraint> constraintsCollection)
		{
			Dictionary<ItemSubTypes, InventoryConstraint> dictionary = new Dictionary<ItemSubTypes, InventoryConstraint>();
			foreach (ItemSubTypes itemSubTypes in constraintsCollection.Keys)
			{
				dictionary[itemSubTypes] = constraintsCollection[itemSubTypes].Clone();
			}
			return dictionary;
		}

		private IDictionary<ItemSubTypes, InventoryConstraint> InitDollConstraintCache()
		{
			IDictionary<ItemSubTypes, InventoryConstraint> dictionary = this.CloneConstraints(Inventory.BasicDollConstraints);
			foreach (InventoryItem inventoryItem in this)
			{
				if (inventoryItem.Storage == StoragePlaces.Doll && inventoryItem is RodCase)
				{
					dictionary[ItemSubTypes.Rod].Increment(((RodCase)inventoryItem).RodWithReelCount);
				}
			}
			return dictionary;
		}

		private IDictionary<ItemSubTypes, InventoryConstraint> InitShoreConstraintCache()
		{
			IDictionary<ItemSubTypes, InventoryConstraint> dictionary = this.CloneConstraints(Inventory.BasicShoreConstraints);
			foreach (InventoryItem inventoryItem in this)
			{
				if (inventoryItem.Storage == StoragePlaces.Doll && inventoryItem is RodStand)
				{
					dictionary[ItemSubTypes.Rod] = new InventoryConstraint(((RodStand)inventoryItem).RodCount, true, null, null);
				}
			}
			return dictionary;
		}

		private bool CheckItemAggregationDoll(InventoryItem item)
		{
			Chum chum = item as Chum;
			return chum == null || Chum.ChumSubtype(chum) != ItemSubTypes.ChumCarpbaits;
		}

		private bool CheckItemAggregation(InventoryItem item, InventoryItem parent, bool checkCapacity = true)
		{
			if (parent == null)
			{
				return true;
			}
			if (parent.AllowedEquipment == null)
			{
				return false;
			}
			if (parent.CanAggregate(item, this.Items.Where(delegate(InventoryItem i)
			{
				Guid? parentItemInstanceId = i.ParentItemInstanceId;
				bool flag = parentItemInstanceId != null;
				Guid? instanceId = parent.InstanceId;
				return flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault());
			}).ToList<InventoryItem>()))
			{
				if (!checkCapacity)
				{
					return true;
				}
				int maxAggregatesCount = parent.GetMaxAggregatesCount(item, this);
				if (maxAggregatesCount > 0)
				{
					int itemsCountOfTypeOnParent = this.GetItemsCountOfTypeOnParent(parent, item.ItemType, item.ItemSubType);
					if (itemsCountOfTypeOnParent < maxAggregatesCount)
					{
						return true;
					}
					Guid? itemOfTypeOnParent = this.GetItemOfTypeOnParent(parent, item.ItemType, item.ItemSubType);
					this.LastVerificationError = "Can't equip any more items" + string.Format(".[[ Item of type {1} can't be equiped with item of type {0}, there is already {2} item equiped. Existing item: {3}]]", new object[]
					{
						Enum.GetName(typeof(ItemSubTypes), item.ItemSubType),
						Enum.GetName(typeof(ItemSubTypes), parent.ItemSubType),
						itemsCountOfTypeOnParent,
						itemOfTypeOnParent
					});
				}
				else
				{
					this.LastVerificationError = "No constraint - item of this type can't be equiped at all";
				}
			}
			else
			{
				this.LastVerificationError = "Can't equip item" + string.Format(".[[ Item of type {1} can't be equiped with item of type {0}]]", Enum.GetName(typeof(ItemSubTypes), item.ItemSubType), Enum.GetName(typeof(ItemSubTypes), parent.ItemSubType));
			}
			return false;
		}

		private IEnumerable<InventoryItem> GetItemsOfTypeOnParentQuery(InventoryItem parent, ItemTypes type, ItemSubTypes subType)
		{
			foreach (InventoryItem item in this)
			{
				if (item.ParentItemInstanceId != null)
				{
					Guid? parentItemInstanceId = item.ParentItemInstanceId;
					bool flag = parentItemInstanceId != null;
					Guid? instanceId = parent.InstanceId;
					if (flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault()) && (item.ItemType == type || item.ItemSubType == subType))
					{
						yield return item;
					}
				}
			}
			yield break;
		}

		private int GetItemsCountOfTypeOnParent(InventoryItem parent, ItemTypes type, ItemSubTypes subType)
		{
			List<InventoryItem> list = this.GetItemsOfTypeOnParentQuery(parent, type, subType).ToList<InventoryItem>();
			return list.Count;
		}

		private Guid? GetItemOfTypeOnParent(InventoryItem parent, ItemTypes type, ItemSubTypes subType)
		{
			List<InventoryItem> list = this.GetItemsOfTypeOnParentQuery(parent, type, subType).ToList<InventoryItem>();
			using (List<InventoryItem>.Enumerator enumerator = list.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					InventoryItem inventoryItem = enumerator.Current;
					return inventoryItem.InstanceId;
				}
			}
			return null;
		}

		public bool IsStorageAvailable()
		{
			return this.profile.PondId == null;
		}

		public bool IsCarAvailable()
		{
			return this.HasCar() && (this.profile.PondId == null || (this.profile.IsTravelingByCar != null && this.profile.IsTravelingByCar.Value));
		}

		public bool IsLodgeAvailable()
		{
			return this.profile.PondId != null && this.HasLodge(this.profile.PondId.Value);
		}

		public StoragePlaces? GetDefaultStorage(InventoryItem item)
		{
			if (this.CanMove(item, null, StoragePlaces.Doll, true))
			{
				return new StoragePlaces?(StoragePlaces.Doll);
			}
			if (this.CanMove(item, null, StoragePlaces.Equipment, true))
			{
				return new StoragePlaces?(StoragePlaces.Equipment);
			}
			if (this.IsStorageAvailable() && this.CanMove(item, null, StoragePlaces.Storage, true))
			{
				return new StoragePlaces?(StoragePlaces.Storage);
			}
			if (this.IsCarAvailable() && this.CanMove(item, null, StoragePlaces.CarEquipment, true))
			{
				return new StoragePlaces?(StoragePlaces.CarEquipment);
			}
			if (this.IsLodgeAvailable() && this.CanMove(item, null, StoragePlaces.LodgeEquipment, true))
			{
				return new StoragePlaces?(StoragePlaces.LodgeEquipment);
			}
			return null;
		}

		public int RodSlotsCount
		{
			get
			{
				return this.DollConstraintsCache()[ItemSubTypes.Rod].Count;
			}
		}

		public void EnsureInventoryItemAfterStorageWasManuallySet(InventoryItem item)
		{
			if (item is Rod && item.Storage == StoragePlaces.Doll && item.Slot == 0)
			{
				item.Slot = this.GetEmptySlotNumber();
			}
			this.SetRodActiveQuiver(item as FeederRod);
		}

		public IEnumerable<InventoryItem> RodsAndReelsOutOfStorage
		{
			get
			{
				List<InventoryItem> list = new List<InventoryItem>();
				this.CheckExcessForItemType(StoragePlaces.Doll, ItemTypes.Rod, list, null);
				this.CheckExcessForItemType(StoragePlaces.Equipment, ItemTypes.Reel, list, null);
				return list;
			}
		}

		public IEnumerable<InventoryItem> TackleOutOfStorage
		{
			get
			{
				List<InventoryItem> list = new List<InventoryItem>();
				this.CheckExcessForItemType(StoragePlaces.Equipment, ItemTypes.Reel, list, null);
				this.CheckExcessForItemType(StoragePlaces.Equipment, ItemTypes.Line, list, null);
				this.CheckExcessForItemType(StoragePlaces.Equipment, ItemTypes.Lure, list, null);
				return list;
			}
		}

		public IEnumerable<InventoryItem> ChumOutOfStorage
		{
			get
			{
				List<InventoryItem> list = new List<InventoryItem>();
				this.CheckExcessForItemType(StoragePlaces.Equipment, ItemTypes.Chum, list, null);
				return list;
			}
		}

		public string GetConstraintInfo
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder("D:");
				Inventory.DumpConstraint(this.DollConstraintsCache(), stringBuilder);
				stringBuilder.AppendLine();
				stringBuilder.Append("E:");
				Inventory.DumpConstraint(this.EquipConstraintsCache(), stringBuilder);
				return stringBuilder.ToString();
			}
		}

		private static void DumpConstraint(IDictionary<ItemSubTypes, InventoryConstraint> dollConstraints, StringBuilder info)
		{
			foreach (ItemSubTypes itemSubTypes in dollConstraints.Keys)
			{
				InventoryConstraint inventoryConstraint = dollConstraints[itemSubTypes];
				info.AppendFormat("{0}-{1}-{2}", Enum.GetName(typeof(ItemSubTypes), itemSubTypes), inventoryConstraint.Count, (!inventoryConstraint.PerType) ? "N" : "Y");
				if (inventoryConstraint.AltTypes != null)
				{
					foreach (ItemTypes itemTypes in inventoryConstraint.AltTypes)
					{
						info.AppendFormat("-{0}", Enum.GetName(typeof(ItemTypes), itemTypes));
					}
				}
				info.Append(";");
			}
		}

		public IEnumerable<InventoryItem> GetAllTackleOutOfStorage(InventoryItem[] preserveList)
		{
			List<InventoryItem> list = new List<InventoryItem>();
			this.CheckExcessForItemType(StoragePlaces.Equipment, ItemTypes.Reel, list, preserveList);
			this.CheckExcessForItemType(StoragePlaces.Equipment, ItemTypes.Line, list, preserveList);
			this.CheckExcessForItemType(StoragePlaces.Equipment, ItemTypes.Lure, list, preserveList);
			return list;
		}

		public IEnumerable<InventoryItem> GetAllChumOutOfStorage(InventoryItem[] preserveList)
		{
			List<InventoryItem> list = new List<InventoryItem>();
			this.CheckExcessForItemType(StoragePlaces.Equipment, ItemTypes.Chum, list, preserveList);
			return list;
		}

		private void CheckExcessForItemType(StoragePlaces storage, ItemTypes itemType, IList<InventoryItem> target, IEnumerable<InventoryItem> preserveList = null)
		{
			IDictionary<ItemSubTypes, InventoryConstraint> dictionary;
			if (storage == StoragePlaces.Doll)
			{
				dictionary = this.DollConstraintsCache();
			}
			else
			{
				if (storage != StoragePlaces.Equipment)
				{
					throw new NotImplementedException(string.Format("Can't check excess for storage {0}", Enum.GetName(typeof(StoragePlaces), storage)));
				}
				dictionary = this.EquipConstraintsCache();
			}
			StoragePlaces? storagePlaces = null;
			if (storage == StoragePlaces.Equipment && itemType == ItemTypes.Reel)
			{
				storagePlaces = new StoragePlaces?(StoragePlaces.ParentItem);
			}
			int itemExcessCount = this.GetItemExcessCount(dictionary, storage, itemType, storagePlaces);
			if (itemExcessCount > 0)
			{
				InventoryItem[] array = (from r in this
					where r.Storage == storage
					where r.IsOccupyInventorySpace
					where this.MatchType(itemType, r.ItemType)
					where preserveList == null || !preserveList.Contains(r)
					select r into i
					orderby (!(i is Rod)) ? i.DateReceived.Ticks : ((long)i.Slot)
					select i).ToArray<InventoryItem>();
				this.AppendTailToList(array, itemExcessCount, target);
			}
		}

		private bool MatchType(ItemTypes givenType, ItemTypes itemType)
		{
			return (this.terminalTackleTypes.Contains(givenType) && this.terminalTackleTypes.Contains(itemType)) || (this.chumTypes.Contains(givenType) && this.chumTypes.Contains(itemType)) || givenType == itemType;
		}

		private void AppendTailToList(InventoryItem[] items, int length, IList<InventoryItem> target)
		{
			int num = 0;
			for (int i = items.Length - 1; i >= 0; i--)
			{
				if (num >= length)
				{
					break;
				}
				target.Add(items[i]);
				num++;
			}
		}

		private int GetItemExcessCount(IDictionary<ItemSubTypes, InventoryConstraint> constraints, StoragePlaces storage, ItemTypes itemType, StoragePlaces? altStorage = null)
		{
			ItemSubTypes itemSubTypes = Inventory.ToSubtype(itemType);
			InventoryConstraint inventoryConstraint = null;
			if (constraints.ContainsKey(itemSubTypes))
			{
				inventoryConstraint = constraints[itemSubTypes];
			}
			int num = ((inventoryConstraint != null) ? inventoryConstraint.Count : 0);
			int num2 = this.GetCurrentCount(storage, inventoryConstraint, new InventoryItem
			{
				ItemType = itemType
			}, altStorage);
			if (storage == StoragePlaces.Doll)
			{
				num2 += this.GetCurrentCount(StoragePlaces.Hands, inventoryConstraint, new InventoryItem
				{
					ItemType = itemType
				}, altStorage);
			}
			int num3 = num2 - num;
			return (num3 <= 0) ? 0 : num3;
		}

		public bool CanRemoveSetup(RodSetup setup)
		{
			this.LastVerificationError = null;
			if (!this.profile.InventoryRodSetups.Contains(setup))
			{
				this.LastVerificationError = "Can't clear setup - non existing setup";
				return false;
			}
			return true;
		}

		public void RemoveSetup(RodSetup setup)
		{
			setup.Items = null;
			setup.Profile = null;
			setup.Name = null;
			this.profile.InventoryRodSetups.Remove(setup);
		}

		public string GetNewSetupName(int sourceSlot)
		{
			if (sourceSlot < 0 || sourceSlot > this.RodSlotsCount)
			{
				return null;
			}
			Rod rodInSlot = this.GetRodInSlot(sourceSlot);
			if (rodInSlot == null)
			{
				return null;
			}
			string rodSubtypeName = Inventory.GetRodSubtypeName(rodInSlot);
			int nextIndexForRodSetup = this.GetNextIndexForRodSetup(rodSubtypeName);
			return string.Format("{0} preset #{1}", rodSubtypeName, nextIndexForRodSetup);
		}

		public bool CanAddSetup(RodSetup setup)
		{
			this.LastVerificationError = null;
			if (this.RodSetupsCount >= this.CurrentRodSetupCapacity)
			{
				this.LastVerificationError = "Can't save setup - no slots available";
				return false;
			}
			return true;
		}

		public bool CanSaveNewSetup(int sourceSlot, string name)
		{
			this.LastVerificationError = null;
			if (name == null || string.IsNullOrEmpty(name.Trim()))
			{
				this.LastVerificationError = "Can't save setup - empty name";
				return false;
			}
			Rod rodInSlot = this.GetRodInSlot(sourceSlot);
			if (rodInSlot == null)
			{
				this.LastVerificationError = "Can't save setup - empty Rod in slot";
				return false;
			}
			RodTemplate rodTemplate = this.RodTemplate.MatchedTemplate(rodInSlot);
			RodTemplate rodTemplate2 = this.RodTemplate.MatchedTemplatePartial(rodInSlot);
			if (rodTemplate == ObjectModel.RodTemplate.UnEquiped && rodTemplate2 == ObjectModel.RodTemplate.UnEquiped)
			{
				this.LastVerificationError = "Can't save setup - Rod is UnEquiped";
				return false;
			}
			if (this.RodSetupsCount >= this.CurrentRodSetupCapacity)
			{
				this.LastVerificationError = "Can't save setup - no slots available";
				return false;
			}
			return true;
		}

		public RodSetup SaveNewSetup(int sourceSlot, string name)
		{
			Rod rodInSlot = this.GetRodInSlot(sourceSlot);
			List<InventoryItem> list = (from i in this.GetRodEquipment(rodInSlot)
				where !(i is Chum)
				select i).ToList<InventoryItem>();
			RodSetup rodSetup = new RodSetup();
			rodSetup.InstanceId = Guid.NewGuid();
			rodSetup.Name = name;
			rodSetup.Items = new Rod[] { rodInSlot }.Union(list).ToList<InventoryItem>();
			rodSetup.LineLength = list.OfType<Line>().Select(delegate(Line l)
			{
				double? length = l.Length;
				return (length == null) ? 0.0 : length.Value;
			}).FirstOrDefault<double>();
			RodSetup rodSetup2 = rodSetup;
			this.AddSetup(rodSetup2);
			return rodSetup2;
		}

		public void AddSetup(RodSetup setup)
		{
			setup.Profile = this.profile;
			this.profile.InventoryRodSetups.Add(setup);
			setup.LastActivityTime = new DateTime?(DateTime.UtcNow);
		}

		public bool CanRenameSetup(RodSetup setup, string name)
		{
			this.LastVerificationError = null;
			if (!this.profile.InventoryRodSetups.Contains(setup))
			{
				this.LastVerificationError = "Can't rename setup - non existing setup";
				return false;
			}
			if (setup.Name == name)
			{
				return true;
			}
			if (name == null || string.IsNullOrEmpty(name.Trim()))
			{
				this.LastVerificationError = "Can't save setup - empty name";
				return false;
			}
			return true;
		}

		public void RenameSetup(RodSetup setup, string name)
		{
			setup.Name = name;
			setup.LastActivityTime = new DateTime?(DateTime.UtcNow);
		}

		public bool CanEquipSetup(RodSetup sourceSetup, int targetSlot)
		{
			this.LastVerificationError = null;
			if (sourceSetup == null)
			{
				throw new ArgumentNullException("sourceSetup");
			}
			if (sourceSetup.IsEmpty)
			{
				this.LastVerificationError = "Can't equip empty setup";
				return false;
			}
			if (targetSlot < 0 || targetSlot > this.RodSlotsCount)
			{
				this.LastVerificationError = "Can't equip setup - wrong slot";
				return false;
			}
			Rod rodInSlot = this.GetRodInSlot(targetSlot);
			if (!this.IsStorageAvailable())
			{
				if (rodInSlot == null)
				{
					this.LastVerificationError = "Can't equip setup - no Rod in slot";
					return false;
				}
				Rod rod = sourceSetup.Rod;
				if (rodInSlot.ItemSubType != rod.ItemSubType)
				{
					this.LastVerificationError = "Can't equip setup - Rod in slot is of wrong type";
					return false;
				}
			}
			List<InventoryItem> list = ((rodInSlot == null) ? new List<InventoryItem>() : this.GetRodEquipment(rodInSlot));
			List<InventoryItem> currentSlotItems = ((rodInSlot == null) ? new List<InventoryItem>() : new Rod[] { rodInSlot }.Union(list).ToList<InventoryItem>());
			currentSlotItems = (from i in currentSlotItems
				where !(i is Chum)
				where !i.IsBroken()
				select i).ToList<InventoryItem>();
			Dictionary<InventoryItem, InventoryItem> newItemsMap = sourceSetup.ItemsToEquip.ToDictionary((InventoryItem i) => i, (InventoryItem i) => this.GetAvailableItemForSetup(sourceSetup, i, currentSlotItems));
			List<InventoryItem> list2 = currentSlotItems.Where((InventoryItem i) => !newItemsMap.Values.Contains(i)).ToList<InventoryItem>();
			foreach (KeyValuePair<InventoryItem, InventoryItem> keyValuePair in newItemsMap)
			{
				InventoryItem value = keyValuePair.Value;
				if (value == null)
				{
					this.LastVerificationError = "Can't equip setup - some items missing in Inventory";
					return false;
				}
				if (!this.CheckStorageCapacity(value, null, null))
				{
					return false;
				}
				if (value is Reel)
				{
					if (!currentSlotItems.Any((InventoryItem i) => i is Reel) && value.Storage != StoragePlaces.Equipment && value.Storage != StoragePlaces.ParentItem && this.IsBreakingEquipmentConstraints(value, list2))
					{
						return false;
					}
				}
			}
			return true;
		}

		public void EquipSetup(RodSetup sourceSetup, int targetSlot, InventoryMovementMapInfo info)
		{
			if (sourceSetup.IsEmpty)
			{
				return;
			}
			sourceSetup.LastActivityTime = new DateTime?(DateTime.UtcNow);
			Rod rodInSlot = this.GetRodInSlot(targetSlot);
			List<InventoryItem> list = ((rodInSlot == null) ? new List<InventoryItem>() : this.GetRodEquipment(rodInSlot));
			List<InventoryItem> currentSlotItems = ((rodInSlot == null) ? new List<InventoryItem>() : new Rod[] { rodInSlot }.Union(list).ToList<InventoryItem>());
			currentSlotItems = (from i in currentSlotItems
				where !(i is Chum)
				where !i.IsBroken()
				select i).ToList<InventoryItem>();
			StoragePlaces targetStorage = ((!this.IsStorageAvailable()) ? StoragePlaces.Equipment : StoragePlaces.Storage);
			if (rodInSlot != null)
			{
				if (this.IsStorageAvailable())
				{
					this.MoveItem(rodInSlot, null, targetStorage, info, true, false);
				}
				else
				{
					foreach (InventoryItem inventoryItem in list)
					{
						this.MoveOrCombineItem(inventoryItem, null, targetStorage, info, false);
					}
				}
			}
			currentSlotItems = (from i in currentSlotItems
				select this.FirstOrDefault(delegate(InventoryItem ii)
				{
					if (ii.ItemId == i.ItemId)
					{
						Guid? instanceId = ii.InstanceId;
						bool flag = instanceId != null;
						Guid? instanceId2 = i.InstanceId;
						if (flag == (instanceId2 != null) && (instanceId == null || instanceId.GetValueOrDefault() == instanceId2.GetValueOrDefault()))
						{
							return ii.Storage == targetStorage;
						}
					}
					return false;
				}) into i
				where i != null
				select i).ToList<InventoryItem>();
			Dictionary<InventoryItem, int> itemsToSplit = sourceSetup.ItemsToSplit;
			IEnumerable<InventoryItem> itemsPrefferableInEquipmentStorage = sourceSetup.ItemsPrefferableInEquipmentStorage;
			Dictionary<InventoryItem, InventoryItem> dictionary = sourceSetup.ItemsToEquip.ToDictionary((InventoryItem i) => i, delegate(InventoryItem i)
			{
				Guid instanceId;
				if (info.ItemMap.TryGetValue(i.ItemId, out instanceId))
				{
					return this.First((InventoryItem item) => item.InstanceId == instanceId);
				}
				InventoryItem availableItemForSetup = this.GetAvailableItemForSetup(sourceSetup, i, currentSlotItems);
				info.ItemMap[i.ItemId] = availableItemForSetup.InstanceId.Value;
				return availableItemForSetup;
			});
			Rod rod2;
			if (this.IsStorageAvailable())
			{
				Rod rod = dictionary.Keys.OfType<Rod>().First<Rod>();
				rod2 = (Rod)dictionary[rod];
				dictionary.Remove(rod);
				rod2.Slot = targetSlot;
				this.MoveItem(rod2, null, StoragePlaces.Doll, info, true, true);
			}
			else
			{
				rod2 = rodInSlot;
			}
			foreach (InventoryItem inventoryItem2 in dictionary.Keys)
			{
				InventoryItem inventoryItem3 = dictionary[inventoryItem2];
				int num = 1;
				if (itemsToSplit.ContainsKey(inventoryItem2))
				{
					num = itemsToSplit[inventoryItem2];
				}
				if (inventoryItem3.IsStockableByAmount)
				{
					if (inventoryItem3.Amount > (float)num)
					{
						InventoryItem inventoryItem4 = this.SplitItem(inventoryItem3, (float)num, info);
						this.MoveItem(inventoryItem4, rod2, StoragePlaces.ParentItem, info, true, true);
						if (inventoryItem3 is Line && ((Line)inventoryItem3).Length < 10.0)
						{
							this.RemoveItem(inventoryItem3, false);
						}
						if (this.IsStorageAvailable() && itemsPrefferableInEquipmentStorage.Contains(inventoryItem2) && (double)Math.Abs(inventoryItem3.Amount) > 1E-05 && inventoryItem3.Storage != StoragePlaces.Equipment && this.CanMoveOrCombineItem(inventoryItem3, null, StoragePlaces.Equipment, info))
						{
							this.MoveOrCombineItem(inventoryItem3, null, StoragePlaces.Equipment, info, true);
						}
					}
					else
					{
						this.MoveItem(inventoryItem3, rod2, StoragePlaces.ParentItem, info, true, true);
					}
				}
				else if (inventoryItem3.Count > num)
				{
					InventoryItem inventoryItem5 = this.SplitItem(inventoryItem3, num, info);
					this.MoveItem(inventoryItem5, rod2, StoragePlaces.ParentItem, info, true, true);
					if (inventoryItem3 is Line && ((Line)inventoryItem3).Length < 10.0)
					{
						this.RemoveItem(inventoryItem3, false);
					}
					if (this.IsStorageAvailable() && itemsPrefferableInEquipmentStorage.Contains(inventoryItem2) && inventoryItem3.Count > 0 && inventoryItem3.Storage != StoragePlaces.Equipment && this.CanMoveOrCombineItem(inventoryItem3, null, StoragePlaces.Equipment, info))
					{
						this.MoveOrCombineItem(inventoryItem3, null, StoragePlaces.Equipment, info, true);
					}
				}
				else
				{
					this.MoveItem(inventoryItem3, rod2, StoragePlaces.ParentItem, info, true, true);
				}
			}
		}

		public Rod GetRodInSlot(int slot)
		{
			return (from i in this.OfType<Rod>()
				where i.IsRodOnDoll
				where i.Slot == slot
				select i).FirstOrDefault<Rod>();
		}

		public List<InventoryItem> GetRodEquipment(Rod rod)
		{
			return this.Where(delegate(InventoryItem i)
			{
				Guid? parentItemInstanceId = i.ParentItemInstanceId;
				bool flag = parentItemInstanceId != null;
				Guid? instanceId = rod.InstanceId;
				return flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault());
			}).ToList<InventoryItem>();
		}

		public int GetNextIndexForRodSetup(string prefix)
		{
			int? num = this.profile.InventoryRodSetups.Where((RodSetup p) => p.Name.StartsWith(prefix + " ")).Select(delegate(RodSetup p)
			{
				int num3 = p.Name.LastIndexOf('#');
				if (num3 < 0)
				{
					return null;
				}
				string text = p.Name.Substring(num3 + 1);
				int num4;
				if (!int.TryParse(text, out num4))
				{
					return null;
				}
				return new int?(num4);
			}).Max();
			int num2 = ((num == null) ? 0 : num.Value);
			return num2 + 1;
		}

		public Dictionary<InventoryItem, InventoryItem> GetItemsToEquipSetup(RodSetup sourceSetup, int targetSlot)
		{
			Rod rodInSlot = this.GetRodInSlot(targetSlot);
			List<InventoryItem> list = ((rodInSlot == null) ? new List<InventoryItem>() : this.GetRodEquipment(rodInSlot));
			List<InventoryItem> currentSlotItems = ((rodInSlot == null) ? new List<InventoryItem>() : new Rod[] { rodInSlot }.Union(list).ToList<InventoryItem>());
			currentSlotItems = (from i in currentSlotItems
				where !(i is Chum)
				where !i.IsBroken()
				select i).ToList<InventoryItem>();
			return sourceSetup.ItemsToEquip.ToDictionary((InventoryItem i) => i, (InventoryItem i) => this.GetAvailableItemForSetup(sourceSetup, i, currentSlotItems));
		}

		private InventoryItem GetAvailableItemForSetup(RodSetup setup, InventoryItem setupItem, List<InventoryItem> currentSlotItems)
		{
			Line line = setupItem as Line;
			Func<InventoryItem, bool> func = (InventoryItem i) => i.ItemId == setupItem.ItemId;
			Func<InventoryItem, bool> func2 = (InventoryItem i) => i.Durability > 0;
			if (line != null)
			{
				func = delegate(InventoryItem i)
				{
					Line line3 = i as Line;
					return line3 != null && (line3.Thickness == line.Thickness && line3.ItemSubType == line.ItemSubType) && line3.Length > 10.0;
				};
			}
			IEnumerable<InventoryItem> enumerable;
			if (this.IsStorageAvailable())
			{
				enumerable = new InventoryItem[0].Union(currentSlotItems.Where(func2).Where(func)).Union(this.Where((InventoryItem i) => i.Storage == StoragePlaces.Equipment).Where(func2).Where(func)).Union(this.StorageInventory.Where(func2).Where(func))
					.Union(this.StorageExceededInventory.Where(func2).Where(func));
			}
			else
			{
				enumerable = new InventoryItem[0].Union(currentSlotItems.Where(func2).Where(func)).Union(this.Where((InventoryItem i) => i.Storage == StoragePlaces.Equipment).Where(func2).Where(func));
			}
			if (line == null)
			{
				return enumerable.FirstOrDefault<InventoryItem>();
			}
			int desirableLineLength = setup.DesirableLineLength;
			Func<IEnumerable<InventoryItem>, InventoryItem> func3 = delegate(IEnumerable<InventoryItem> items)
			{
				Line line2;
				if ((line2 = (from l in items.OfType<Line>()
					where l.Length >= (double)desirableLineLength
					orderby l.Length
					select l).FirstOrDefault<Line>()) == null)
				{
					line2 = (from l in items.OfType<Line>()
						where l.Length < (double)desirableLineLength
						orderby l.Length descending
						select l).FirstOrDefault<Line>();
				}
				return line2;
			};
			if (this.IsStorageAvailable())
			{
				return func3(new InventoryItem[0].Union(currentSlotItems.Where(func2).Where(func)).Union(this.Where((InventoryItem i) => i.Storage == StoragePlaces.Equipment).Where(func2).Where(func)).Union(this.StorageInventory.Where(func2).Where(func))) ?? func3(this.StorageExceededInventory.Where(func2).Where(func));
			}
			return func3(enumerable);
		}

		public static string GetRodSubtypeName(InventoryItem rod)
		{
			string text = rod.ItemSubType.ToString().Replace("Rod", string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				text = "Rod";
			}
			return text;
		}

		public static int GetChumSlotUnlockLevelForBase(int level)
		{
			int num;
			int num2;
			int num3;
			Inventory.GetChumSlotsForLevel(level, out num, out num2, out num3);
			int num4 = num;
			for (int i = level + 1; i <= Profile.LevelCap; i++)
			{
				Inventory.GetChumSlotsForLevel(i, out num, out num2, out num3);
				int num5 = num;
				if (num5 > num4)
				{
					return i;
				}
			}
			return -1;
		}

		public static int GetChumSlotUnlockLevelForAroma(int level)
		{
			int num;
			int num2;
			int num3;
			Inventory.GetChumSlotsForLevel(level, out num, out num2, out num3);
			int num4 = num2;
			for (int i = level + 1; i <= Profile.LevelCap; i++)
			{
				Inventory.GetChumSlotsForLevel(i, out num, out num2, out num3);
				int num5 = num2;
				if (num5 > num4)
				{
					return i;
				}
			}
			return -1;
		}

		public static int GetChumSlotUnlockLevelForParticle(int level)
		{
			int num;
			int num2;
			int num3;
			Inventory.GetChumSlotsForLevel(level, out num, out num2, out num3);
			int num4 = num3;
			for (int i = level + 1; i <= Profile.LevelCap; i++)
			{
				Inventory.GetChumSlotsForLevel(i, out num, out num2, out num3);
				int num5 = num3;
				if (num5 > num4)
				{
					return i;
				}
			}
			return -1;
		}

		public static int GetChumSlotsForLevel(int level, out int chumBaseSlots, out int chumAromaSlots, out int chumParticleSlots)
		{
			chumBaseSlots = 0;
			chumAromaSlots = 0;
			chumParticleSlots = 0;
			int num = 2;
			if (level >= num)
			{
				chumBaseSlots++;
				num = 3;
			}
			if (level >= num)
			{
				chumAromaSlots++;
				num = 5;
			}
			if (level >= num)
			{
				chumParticleSlots++;
				num = 12;
			}
			if (level >= num)
			{
				chumBaseSlots++;
				num = 20;
			}
			if (level >= num)
			{
				chumAromaSlots++;
				num = 30;
			}
			if (level >= num)
			{
				chumBaseSlots++;
				num = 36;
			}
			if (level >= num)
			{
				chumAromaSlots++;
				num = 42;
			}
			if (level >= num)
			{
				chumParticleSlots++;
				num = 46;
			}
			if (level >= num)
			{
				chumParticleSlots++;
				num = 0;
			}
			return num;
		}

		public bool CanAddChumIngredient(Chum chum, ChumIngredient ingredient)
		{
			this.LastVerificationError = null;
			ItemSubTypes itemSubTypes = Chum.ChumSubtype(chum);
			if (ingredient is ChumBase && itemSubTypes != ItemSubTypes.All && itemSubTypes != ingredient.ItemSubType)
			{
				this.LastVerificationError = "Can't add ingredient - wrong base type";
				return false;
			}
			ChumBase chumBase = chum.ChumBase.First<ChumBase>();
			if (chumBase.SpecialItem == InventorySpecialItem.Snow && ingredient.SpecialItem != InventorySpecialItem.Snow)
			{
				this.LastVerificationError = "Can't add ingredient - wrong base type";
				return false;
			}
			if (ingredient is ChumParticle && itemSubTypes != ItemSubTypes.ChumGroundbaits)
			{
				this.LastVerificationError = "Can't add particles - only groundbaits allow particles";
				return false;
			}
			return true;
		}

		public static void AddChumIngredient(Chum chum, ChumIngredient ingredient, double weight)
		{
			if (chum.Ingredients == null)
			{
				chum.Ingredients = new List<ChumIngredient>();
			}
			if (weight <= 0.0)
			{
				return;
			}
			ChumIngredient chumIngredient = chum.Ingredients.FirstOrDefault((ChumIngredient i) => i.ItemId == ingredient.ItemId);
			if (chumIngredient != null)
			{
				chumIngredient.Weight = new double?(weight);
			}
			else
			{
				ChumIngredient chumIngredient2 = (ChumIngredient)Inventory.CloneItem(ingredient);
				chumIngredient2.Weight = new double?(weight);
				chumIngredient2.InstanceId = ingredient.InstanceId;
				chum.Ingredients.Add(chumIngredient2);
			}
			chum.UpdateIngredients(true);
		}

		public static void RemoveChumIngredient(Chum chum, ChumIngredient ingredient)
		{
			if (chum.Ingredients != null)
			{
				ChumIngredient chumIngredient = chum.Ingredients.FirstOrDefault((ChumIngredient i) => i.ItemId == ingredient.ItemId);
				if (chumIngredient != null)
				{
					chum.Ingredients.Remove(chumIngredient);
				}
			}
			chum.UpdateIngredients(true);
		}

		public static void ClearChumIngredients(Chum chum)
		{
			if (chum.Ingredients != null)
			{
				chum.Ingredients.Clear();
				chum.UpdateIngredients(true);
			}
		}

		[JsonIgnore]
		public Chum MixingChum { get; set; }

		public bool CanMixChum(Chum chum)
		{
			this.LastVerificationError = null;
			if (!this.CanSaveRecipe(chum))
			{
				return false;
			}
			if (this.profile.PondId == null)
			{
				this.LastVerificationError = "Can't mix chum - should be on pond";
				return false;
			}
			using (IEnumerator<ChumIngredient> enumerator = chum.Ingredients.Where((ChumIngredient i) => (double)Math.Abs(i.Amount) > 1E-05).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ChumIngredient ingredient = enumerator.Current;
					IEnumerable<ChumIngredient> enumerable = (from i in this.profile.Inventory.OfType<ChumIngredient>()
						where i.ItemId == ingredient.ItemId && i.ItemSubType == ingredient.ItemSubType && i.Storage == StoragePlaces.Equipment
						select i).Where(delegate(ChumIngredient i)
					{
						Guid? instanceId = i.InstanceId;
						bool flag = instanceId != null;
						Guid? instanceId2 = ingredient.InstanceId;
						return flag == (instanceId2 != null) && (instanceId == null || instanceId.GetValueOrDefault() == instanceId2.GetValueOrDefault());
					});
					if (!enumerable.Any<ChumIngredient>())
					{
						this.LastVerificationError = "Can't mix chum - ingredient not found";
						return false;
					}
					ChumIngredient chumIngredient = enumerable.Where((ChumIngredient i) => i.Amount >= ingredient.Amount).FirstOrDefault<ChumIngredient>();
					if (chumIngredient == null)
					{
						this.LastVerificationError = "Can't mix chum - ingredient not enough";
						return false;
					}
					if ((double)Math.Abs(chumIngredient.Amount) <= 1E-05)
					{
						this.LastVerificationError = "Can't mix chum - ingredient without weight configured";
						return false;
					}
				}
			}
			return true;
		}

		public static string GetChumNameMixed(string chumName)
		{
			return string.Format("{0}{1}", "Mix ", chumName);
		}

		public Chum MixChum(Chum chum, InventoryMovementMapInfo mapInfo)
		{
			ChumBase heaviestChumBase = chum.HeaviestChumBase;
			Chum chum2 = new Chum
			{
				Ingredients = new List<ChumIngredient>(),
				Name = (string.IsNullOrEmpty(chum.Name) ? Inventory.GetChumNameMixed(heaviestChumBase.Name) : chum.Name),
				ThumbnailBID = heaviestChumBase.DollThumbnailBID,
				DollThumbnailBID = heaviestChumBase.DollThumbnailBID,
				ImageData = chum.ImageData,
				Storage = StoragePlaces.Equipment
			};
			chum2.InitBasicChumProperties();
			if (mapInfo.OperationTime != null)
			{
				chum2.MixTime = mapInfo.OperationTime.Value;
			}
			else
			{
				Chum chum3 = chum2;
				TimeSpan? pondTimeSpent = this.profile.PondTimeSpent;
				chum3.MixTime = ((pondTimeSpent == null) ? TimeSpan.Zero : pondTimeSpent.Value);
				mapInfo.OperationTime = new TimeSpan?(chum2.MixTime);
			}
			if (mapInfo.InstanceId != null)
			{
				chum2.InstanceId = mapInfo.InstanceId;
			}
			else
			{
				chum2.InstanceId = new Guid?(Guid.NewGuid());
				mapInfo.InstanceId = chum2.InstanceId;
			}
			chum2.SplitFromInstanceId = chum2.InstanceId;
			using (IEnumerator<ChumIngredient> enumerator = chum.Ingredients.Where((ChumIngredient i) => (double)Math.Abs(i.Amount) > 1E-05).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Inventory.<MixChum>c__AnonStorey1A <MixChum>c__AnonStorey1A = new Inventory.<MixChum>c__AnonStorey1A();
					<MixChum>c__AnonStorey1A.ingredient = enumerator.Current;
					ChumIngredient chumIngredient;
					Guid instanceId;
					if (mapInfo.ItemMap.TryGetValue(<MixChum>c__AnonStorey1A.ingredient.ItemId, out instanceId))
					{
						chumIngredient = (ChumIngredient)this.First((InventoryItem item) => item.InstanceId == instanceId);
					}
					else
					{
						chumIngredient = (from i in (from i in this.profile.Inventory.OfType<ChumIngredient>()
								where i.ItemId == <MixChum>c__AnonStorey1A.ingredient.ItemId && i.ItemSubType == <MixChum>c__AnonStorey1A.ingredient.ItemSubType && i.Storage == StoragePlaces.Equipment
								select i).Where(delegate(ChumIngredient i)
							{
								Guid? instanceId3 = i.InstanceId;
								bool flag = instanceId3 != null;
								Guid? instanceId2 = <MixChum>c__AnonStorey1A.ingredient.InstanceId;
								return flag == (instanceId2 != null) && (instanceId3 == null || instanceId3.GetValueOrDefault() == instanceId2.GetValueOrDefault());
							})
							where i.Amount >= <MixChum>c__AnonStorey1A.ingredient.Amount
							select i).FirstOrDefault<ChumIngredient>();
						mapInfo.ItemMap[<MixChum>c__AnonStorey1A.ingredient.ItemId] = chumIngredient.InstanceId.Value;
					}
					if ((double)Math.Abs(chumIngredient.Amount - <MixChum>c__AnonStorey1A.ingredient.Amount) <= 1E-05)
					{
						this.RemoveItem(chumIngredient, false);
					}
					else
					{
						this.CutItemWeight(chumIngredient, (double)<MixChum>c__AnonStorey1A.ingredient.Amount);
					}
					chum2.Ingredients.Add(<MixChum>c__AnonStorey1A.ingredient);
				}
			}
			chum2.UpdateIngredients(true);
			this.AddItem(chum2);
			Chum chum4 = chum2;
			TimeSpan? pondTimeSpent2 = this.profile.PondTimeSpent;
			chum4.SetPondTimeSpent((pondTimeSpent2 == null) ? TimeSpan.Zero : pondTimeSpent2.Value);
			return chum2;
		}

		public bool CanRenameChum(Chum chum, string name)
		{
			this.LastVerificationError = null;
			if (!this.profile.Inventory.Contains(chum))
			{
				this.LastVerificationError = "Can't rename chum - non existing chum";
				return false;
			}
			if (chum.Name == name)
			{
				return true;
			}
			if (name == null || string.IsNullOrEmpty(name.Trim()))
			{
				this.LastVerificationError = "Can't save chum - empty name";
				return false;
			}
			return true;
		}

		public void RenameChum(Chum chum, string name)
		{
			chum.Name = name;
		}

		public List<InventoryItem> OutdateChum()
		{
			List<InventoryItem> list = new List<InventoryItem>();
			foreach (Chum chum in (from c in this.OfType<Chum>()
				where c.Storage != StoragePlaces.Storage
				select c).ToArray<Chum>())
			{
				this.RemoveItem(chum, false);
				list.Add(chum);
			}
			return list;
		}

		public float GetChumCapacityToLoad(Chum chum)
		{
			StoragePlaces storage = chum.Storage;
			if (storage != StoragePlaces.ParentItem)
			{
				return Inventory.ChumHandCapacity;
			}
			Feeder feeder = this.OfType<Feeder>().FirstOrDefault(delegate(Feeder f)
			{
				bool flag2;
				if (f.Storage == StoragePlaces.ParentItem)
				{
					Guid? parentItemInstanceId = f.ParentItemInstanceId;
					bool flag = parentItemInstanceId != null;
					Guid? parentItemInstanceId2 = chum.ParentItemInstanceId;
					flag2 = flag == (parentItemInstanceId2 != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == parentItemInstanceId2.GetValueOrDefault());
				}
				else
				{
					flag2 = false;
				}
				return flag2;
			});
			if (feeder == null)
			{
				return 0f;
			}
			return feeder.ChumCapacity;
		}

		public float GetChumSplitTimeInMilliseconds(Chum chum)
		{
			StoragePlaces storage = chum.Storage;
			if (storage != StoragePlaces.ParentItem)
			{
				return (this.profile.SubscriptionId == null) ? (Inventory.ChumSplitTimeHands * 1000f) : (Inventory.ChumSplitTimeHandsPremium * 1000f);
			}
			Feeder feeder = this.OfType<Feeder>().First(delegate(Feeder f)
			{
				bool flag2;
				if (f.Storage == StoragePlaces.ParentItem)
				{
					Guid? parentItemInstanceId = f.ParentItemInstanceId;
					bool flag = parentItemInstanceId != null;
					Guid? parentItemInstanceId2 = chum.ParentItemInstanceId;
					flag2 = flag == (parentItemInstanceId2 != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == parentItemInstanceId2.GetValueOrDefault());
				}
				else
				{
					flag2 = false;
				}
				return flag2;
			});
			return (this.profile.SubscriptionId == null) ? (Inventory.ChumSplitTime * 1000f) : (Inventory.ChumSplitTimePremium * 1000f);
		}

		public Chum FindChumMix(Chum chum)
		{
			return this.OfType<Chum>().FirstOrDefault(delegate(Chum c)
			{
				Guid? instanceId = c.InstanceId;
				bool flag = instanceId != null;
				Guid? splitFromInstanceId = chum.SplitFromInstanceId;
				return flag == (splitFromInstanceId != null) && (instanceId == null || instanceId.GetValueOrDefault() == splitFromInstanceId.GetValueOrDefault()) && c.IsHidden != true && c.Storage == StoragePlaces.Equipment;
			});
		}

		public bool CanBeginFillChum(Chum chum)
		{
			this.LastVerificationError = null;
			if (chum.Storage != StoragePlaces.ParentItem && chum.Storage != StoragePlaces.Hands)
			{
				this.LastVerificationError = "Can't fill chum - not equipped on rod";
				return false;
			}
			if (chum.WasFilled && (double)Math.Abs(chum.Amount) > 1E-05)
			{
				this.LastVerificationError = "Can't fill chum - chum should be empty";
				return false;
			}
			Chum chum2 = this.FindChumMix(chum);
			float chumCapacityToLoad = this.GetChumCapacityToLoad(chum);
			if ((double)Math.Abs(chumCapacityToLoad) <= 1E-05)
			{
				return false;
			}
			if ((double)Math.Abs(chum.Amount) <= 1E-05)
			{
				if (chum2 == null)
				{
					this.LastVerificationError = "Can't fill chum - unavailable mix";
					return false;
				}
				if (chum2.Amount < chumCapacityToLoad)
				{
					this.LastVerificationError = "Can't fill chum - not enough mix amount";
					return false;
				}
			}
			return true;
		}

		public void BeginFillChum(Chum chum, InventoryMovementMapInfo info)
		{
			chum.BeginSplitTime = new DateTime?(DateTime.UtcNow);
		}

		public bool CanFinishFillChum(Chum chum)
		{
			this.LastVerificationError = null;
			if (chum.Storage != StoragePlaces.ParentItem && chum.Storage != StoragePlaces.Hands)
			{
				this.LastVerificationError = "Can't fill chum - not equipped on rod";
				return false;
			}
			if (chum.SplitFromInstanceId == null)
			{
				this.LastVerificationError = "Can't fill chum - unknown mix";
				return false;
			}
			Chum chum2 = this.FindChumMix(chum);
			float chumCapacityToLoad = this.GetChumCapacityToLoad(chum);
			if ((double)Math.Abs(chumCapacityToLoad) <= 1E-05)
			{
				return false;
			}
			if ((double)Math.Abs(chum.Amount) <= 1E-05)
			{
				if (chum2 == null)
				{
					this.LastVerificationError = "Can't fill chum - unavailable mix";
					return false;
				}
				if (chum2.Amount < chumCapacityToLoad)
				{
					this.LastVerificationError = "Can't fill chum - not enough mix amount";
					return false;
				}
			}
			float chumSplitTimeInMilliseconds = this.GetChumSplitTimeInMilliseconds(chum);
			if (chum.BeginSplitTime == null || DateTime.UtcNow.Subtract(chum.BeginSplitTime.Value).TotalMilliseconds < (double)chumSplitTimeInMilliseconds)
			{
				this.LastVerificationError = "Can't fill chum in such short time";
				return false;
			}
			return true;
		}

		public Chum FinishFillChum(Chum chum, InventoryMovementMapInfo info)
		{
			chum.BeginSplitTime = null;
			float chumCapacityToLoad = this.GetChumCapacityToLoad(chum);
			if ((double)Math.Abs(chum.Amount - chumCapacityToLoad) <= 1E-05)
			{
				Chum chum2 = (Chum)this.SplitItem(chum, chumCapacityToLoad, info);
				chum2.Storage = chum.Storage;
				chum2.ParentItem = chum.ParentItem;
				chum2.ParentItemInstanceId = chum.ParentItemInstanceId;
				chum2.WasFilled = true;
				return chum2;
			}
			Chum chum3 = this.FindChumMix(chum);
			if (chum3 == null)
			{
				throw new ArgumentNullException("Chum mix not found");
			}
			Chum chum4 = (Chum)this.SplitItemAndReplace(chum, chum3, chumCapacityToLoad, info);
			chum4.WasFilled = true;
			return chum4;
		}

		public bool CanCancelFillChum(Chum chum)
		{
			this.LastVerificationError = null;
			if (chum.Storage != StoragePlaces.ParentItem && chum.Storage != StoragePlaces.Hands && chum.Storage != StoragePlaces.Doll)
			{
				this.LastVerificationError = "Can't unlink chum - chum should be empty";
				return false;
			}
			return true;
		}

		public void CancelFillChum(Chum chum)
		{
			chum.BeginSplitTime = null;
		}

		public bool CanUseChumExpired(Chum chum)
		{
			this.LastVerificationError = null;
			if (chum == null)
			{
				return true;
			}
			if (chum.IsExpired)
			{
				this.LastVerificationError = "Can't use chum - Usage time expired";
				return false;
			}
			return true;
		}

		public bool CanUseChumOnRod(Chum chum, InventoryItem rod, float amount)
		{
			this.LastVerificationError = null;
			if (chum == null)
			{
				return true;
			}
			if (amount != 0f)
			{
				if (rod == null)
				{
					if (amount != Inventory.ChumHandCapacity)
					{
						this.LastVerificationError = "Can't use chum - amount differs from Feeder capacity";
						return false;
					}
				}
				else
				{
					Feeder feeder = this.OfType<Feeder>().FirstOrDefault(delegate(Feeder f)
					{
						Guid? parentItemInstanceId = f.ParentItemInstanceId;
						bool flag = parentItemInstanceId != null;
						Guid? instanceId = rod.InstanceId;
						return flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault());
					});
					if (feeder.ChumCapacity != amount)
					{
						this.LastVerificationError = "Can't use chum - amount differs from Feeder capacity";
						return false;
					}
				}
			}
			return true;
		}

		public bool CanThrowChum(Chum chum)
		{
			if (chum.Storage != StoragePlaces.ParentItem && chum.Storage != StoragePlaces.Hands && chum.Storage != StoragePlaces.Doll)
			{
				this.LastVerificationError = "Can't unlink chum - chum should be empty";
				return false;
			}
			return true;
		}

		public Chum ThrowChum(Chum chum, InventoryMovementMapInfo mapInfo)
		{
			Chum chum2 = (Chum)Inventory.CloneItemFull(chum);
			if (mapInfo.InstanceId != null)
			{
				chum2.InstanceId = mapInfo.InstanceId;
			}
			else
			{
				chum2.InstanceId = new Guid?(Guid.NewGuid());
				mapInfo.InstanceId = chum2.InstanceId;
			}
			chum2.IsHidden = new bool?(true);
			chum2.Storage = StoragePlaces.Equipment;
			chum2.ParentItem = null;
			chum2.ParentItemInstanceId = null;
			this.AddItem(chum2);
			if (chum.Storage == StoragePlaces.Hands)
			{
				Chum chum3 = this.FindChumMix(chum);
				if (chum3 != null && chum3.Amount >= Inventory.ChumHandCapacity)
				{
					if ((double)Math.Abs(chum3.Amount - Inventory.ChumHandCapacity) <= 1E-05)
					{
						chum.WasFilled = false;
						this.RemoveItem(chum3, false);
					}
					else
					{
						chum.WasFilled = false;
						this.CutItemWeight(chum3, (double)Inventory.ChumHandCapacity);
					}
				}
				else
				{
					this.RemoveItem(chum, false);
				}
			}
			return chum2;
		}

		public bool CanSaveRecipe(Chum chum)
		{
			this.LastVerificationError = null;
			if (chum.Ingredients == null)
			{
				chum.Ingredients = new List<ChumIngredient>();
			}
			int num;
			int num2;
			int num3;
			Inventory.GetChumSlotsForLevel(this.profile.Level, out num, out num2, out num3);
			if (chum.ChumBase.Count > num)
			{
				this.LastVerificationError = "Can't mix chum - base count exceeded";
				return false;
			}
			if (chum.ChumAroma.Count > num2)
			{
				this.LastVerificationError = "Can't mix chum - aroma count exceeded";
				return false;
			}
			if (chum.ChumParticle.Count > num3)
			{
				this.LastVerificationError = "Can't mix chum - particle count exceeded";
				return false;
			}
			if (chum.PercentageBase < Inventory.MixBasePossiblePercentageMin)
			{
				this.LastVerificationError = "Can't mix chum - base percentage is too low";
				return false;
			}
			if (chum.PercentageAroma > Inventory.MixAromaPossiblePercentage)
			{
				this.LastVerificationError = "Can't mix chum - aroma percentage is too much";
				return false;
			}
			if (chum.PercentageParticle > Inventory.MixParticlePossiblePercentage)
			{
				this.LastVerificationError = "Can't mix chum - particle percentage is too much";
				return false;
			}
			if (!chum.Ingredients.Any((ChumIngredient i) => (double)Math.Abs(i.Amount) > 1E-05))
			{
				this.LastVerificationError = "Can't mix chum - empty mix";
				return false;
			}
			return true;
		}

		public bool CanSaveNewChumRecipe(Chum chum)
		{
			this.LastVerificationError = null;
			if (!this.CanSaveRecipe(chum))
			{
				return false;
			}
			if (chum.Name == null || string.IsNullOrEmpty(chum.Name.Trim()))
			{
				this.LastVerificationError = "Can't save ChumRecipe - empty name";
				return false;
			}
			if ((chum.InstanceId == null || chum.InstanceId == Guid.Empty) && this.ChumRecipesCount >= this.CurrentChumRecipesCapacity)
			{
				this.LastVerificationError = "Can't save ChumRecipe - no slots available";
				return false;
			}
			return true;
		}

		public void SaveNewChumRecipe(Chum recipe, InventoryMovementMapInfo mapInfo)
		{
			ChumBase heaviestChumBase = recipe.HeaviestChumBase;
			recipe.ThumbnailBID = heaviestChumBase.DollThumbnailBID;
			recipe.DollThumbnailBID = heaviestChumBase.DollThumbnailBID;
			recipe.InitBasicChumProperties();
			recipe.BeginSplitTime = null;
			recipe.MixTime = TimeSpan.Zero;
			recipe.SetPondTimeSpent(TimeSpan.Zero);
			if (recipe.InstanceId != null && recipe.InstanceId.Value != Guid.Empty)
			{
				Chum chum = this.profile.ChumRecipes.First(delegate(Chum r)
				{
					Guid? instanceId = r.InstanceId;
					bool flag = instanceId != null;
					Guid? instanceId2 = recipe.InstanceId;
					return flag == (instanceId2 != null) && (instanceId == null || instanceId.GetValueOrDefault() == instanceId2.GetValueOrDefault());
				});
				int num = this.profile.ChumRecipes.IndexOf(chum);
				this.profile.ChumRecipes[num] = recipe;
			}
			else
			{
				if (mapInfo.InstanceId != null)
				{
					recipe.InstanceId = mapInfo.InstanceId;
				}
				else
				{
					recipe.InstanceId = new Guid?(Guid.NewGuid());
					mapInfo.InstanceId = recipe.InstanceId;
				}
				this.profile.ChumRecipes.Add(recipe);
			}
		}

		public bool CanRenameChumRecipe(Chum recipe, string name)
		{
			this.LastVerificationError = null;
			if (!this.profile.ChumRecipes.Contains(recipe))
			{
				this.LastVerificationError = "Can't rename ChumRecipe - non existing recipe";
				return false;
			}
			if (recipe.Name == name)
			{
				return true;
			}
			if (name == null || string.IsNullOrEmpty(name.Trim()))
			{
				this.LastVerificationError = "Can't save ChumRecipe - empty name";
				return false;
			}
			return true;
		}

		public void RenameChumRecipe(Chum recipe, string name)
		{
			recipe.Name = name;
		}

		public bool CanRemoveChumRecipe(Chum recipe)
		{
			this.LastVerificationError = null;
			if (!this.profile.ChumRecipes.Contains(recipe))
			{
				this.LastVerificationError = "Can't remove ChumRecipe - non existing recipe";
				return false;
			}
			return true;
		}

		public void RemoveChumRecipe(Chum recipe)
		{
			this.profile.ChumRecipes.Remove(recipe);
		}

		public bool CanSetQuiverIndex(FeederRod rod, int index)
		{
			this.LastVerificationError = null;
			if (index > 0)
			{
				if (rod.QuiverTips == null)
				{
					this.LastVerificationError = "Can't set quiver index - no quiver with such ItemId defined in FeederRod";
					return false;
				}
				if (rod.QuiverTips.FirstOrDefault((QuiverTip q) => q.ItemId == index) == null)
				{
					this.LastVerificationError = "Can't set quiver index - no quiver with such ItemId defined in FeederRod";
					return false;
				}
			}
			return true;
		}

		public void SetQuiverIndex(FeederRod rod, int index)
		{
			rod.QuiverId = new int?(index);
		}

		private void SetRodActiveQuiver(FeederRod feederRod)
		{
			if (feederRod == null)
			{
				return;
			}
			if ((feederRod.Storage == StoragePlaces.Doll || feederRod.Storage == StoragePlaces.Hands) && (feederRod.QuiverId == null || feederRod.QuiverId == 0) && feederRod.QuiverTips != null)
			{
				QuiverTip quiverTip = feederRod.QuiverTips.FirstOrDefault((QuiverTip q) => !q.IsBroken);
				feederRod.QuiverId = ((quiverTip == null) ? null : new int?(quiverTip.ItemId));
			}
			if (feederRod.Storage == StoragePlaces.Storage)
			{
				feederRod.QuiverId = null;
			}
		}

		public static bool IsCarpBait(InventoryItem bait)
		{
			return bait.ItemSubType == ItemSubTypes.BoilBait || bait.ItemId == 469 || bait.ItemId == 699 || bait.ItemId == 5600 || bait.ItemId == 5610;
		}

		public RodTemplate GetRodTemplate(Rod rod)
		{
			RodTemplate rodTemplate = this.RodTemplate.MatchedTemplate(rod);
			if (rodTemplate != ObjectModel.RodTemplate.UnEquiped && rod is FeederRod)
			{
				FeederRod feederRod = (FeederRod)rod;
				if (feederRod.QuiverId == null || feederRod.QuiverId == 0)
				{
					return ObjectModel.RodTemplate.UnEquiped;
				}
				if (feederRod.QuiverTips == null)
				{
					return ObjectModel.RodTemplate.UnEquiped;
				}
				QuiverTip quiverTip = feederRod.QuiverTips.FirstOrDefault((QuiverTip q) => q.ItemId == feederRod.QuiverId);
				if (quiverTip == null)
				{
					return ObjectModel.RodTemplate.UnEquiped;
				}
				if (quiverTip.IsBroken)
				{
					return ObjectModel.RodTemplate.UnEquiped;
				}
			}
			return rodTemplate;
		}

		public RodTemplate GetRodTemplateWith(Rod rod, InventoryItem itemToEquip)
		{
			RodTemplate rodTemplate = this.RodTemplate.MatchedTemplateWith(rod, itemToEquip);
			if (rodTemplate != ObjectModel.RodTemplate.UnEquiped && rod is FeederRod)
			{
				FeederRod feederRod = (FeederRod)rod;
				if (feederRod.QuiverId == null || feederRod.QuiverId == 0)
				{
					return ObjectModel.RodTemplate.UnEquiped;
				}
				if (feederRod.QuiverTips == null)
				{
					return ObjectModel.RodTemplate.UnEquiped;
				}
				QuiverTip quiverTip = feederRod.QuiverTips.FirstOrDefault((QuiverTip q) => q.ItemId == feederRod.QuiverId);
				if (quiverTip == null)
				{
					return ObjectModel.RodTemplate.UnEquiped;
				}
				if (quiverTip.IsBroken)
				{
					return ObjectModel.RodTemplate.UnEquiped;
				}
			}
			return rodTemplate;
		}

		public bool IsEquipped(InventoryItem item)
		{
			EchoSounder echoSounder = item as EchoSounder;
			if (echoSounder != null)
			{
				int num = this.Count(delegate(InventoryItem i)
				{
					if (i is SounderBattery && i.ItemSubType == ItemSubTypes.SounderBattery)
					{
						Guid? parentItemInstanceId = i.ParentItemInstanceId;
						bool flag = parentItemInstanceId != null;
						Guid? instanceId = echoSounder.InstanceId;
						if (flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault()))
						{
							return ((SounderBattery)i).Charge > 0f;
						}
					}
					return false;
				});
				return num == echoSounder.BatteryCapacity;
			}
			MotorBoat motorBoat = item as MotorBoat;
			if (motorBoat != null)
			{
				return this.Any(delegate(InventoryItem i)
				{
					Guid? parentItemInstanceId2 = i.ParentItemInstanceId;
					bool flag2 = parentItemInstanceId2 != null;
					Guid? instanceId2 = motorBoat.InstanceId;
					return flag2 == (instanceId2 != null) && (parentItemInstanceId2 == null || parentItemInstanceId2.GetValueOrDefault() == instanceId2.GetValueOrDefault()) && i is BoatFuel && i.ItemSubType == ItemSubTypes.BoatFuel && i.Weight > 0.0;
				});
			}
			return item is Kayak;
		}

		public bool CanSetLeaderLength(Rod rod, float leaderLength)
		{
			this.LastVerificationError = null;
			Leader leader = this.OfType<Leader>().FirstOrDefault(delegate(Leader i)
			{
				Guid? parentItemInstanceId = i.ParentItemInstanceId;
				bool flag = parentItemInstanceId != null;
				Guid? instanceId = rod.InstanceId;
				return flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault()) && i.Storage == StoragePlaces.ParentItem;
			});
			if (leader != null)
			{
				if (leaderLength > leader.LeaderLength)
				{
					this.LastVerificationError = "Can't set leader length longer than have";
					return false;
				}
				if (leader.Length != null)
				{
					double? length = leader.Length;
					if ((double)leaderLength > length)
					{
						this.LastVerificationError = "Can't set leader length longer than previous value";
						return false;
					}
				}
			}
			return true;
		}

		public void SetLeaderLength(Rod rod, float leaderLength)
		{
			if (rod.ItemSubType.IsRodWithBobber())
			{
				rod.LeaderLength = leaderLength;
			}
			else
			{
				Leader leader = this.OfType<Leader>().FirstOrDefault(delegate(Leader i)
				{
					Guid? parentItemInstanceId = i.ParentItemInstanceId;
					bool flag = parentItemInstanceId != null;
					Guid? instanceId = rod.InstanceId;
					return flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault()) && i.Storage == StoragePlaces.ParentItem;
				});
				if (leader != null)
				{
					if (leader.ItemSubType.IsCuttableLeader())
					{
						if (leaderLength < leader.LeaderLength)
						{
							leader.WasCut = new bool?(true);
						}
						leader.Length = new double?((double)leaderLength);
					}
					rod.LeaderLength = leaderLength;
				}
			}
		}

		private void SetRodLeaderLength(Leader leader)
		{
			if (leader == null)
			{
				return;
			}
			if (leader.Storage == StoragePlaces.ParentItem)
			{
				Rod rod = this.OfType<Rod>().First(delegate(Rod i)
				{
					Guid? instanceId = i.InstanceId;
					bool flag = instanceId != null;
					Guid? parentItemInstanceId = leader.ParentItemInstanceId;
					return flag == (parentItemInstanceId != null) && (instanceId == null || instanceId.GetValueOrDefault() == parentItemInstanceId.GetValueOrDefault());
				});
				if (!rod.ItemSubType.IsRodWithBobber())
				{
					double? length = leader.Length;
					float num = (float)((length == null) ? ((double)leader.LeaderLength) : length.Value);
					rod.LeaderLength = num;
				}
			}
		}

		private bool ShouldRemoveItem(InventoryItem item)
		{
			return (item.IsStockableByAmount && (double)Math.Abs(item.Amount) <= 1E-05) || item.IsBroken();
		}

		private bool RemoveItemOnReturnToStorage(InventoryItem item)
		{
			if ((item.Storage == StoragePlaces.Storage || item.Storage == StoragePlaces.Equipment) && this.ShouldRemoveItem(item))
			{
				this.RemoveItem(item, true);
				return true;
			}
			return false;
		}

		public void SetItemCount(InventoryItem item, int count)
		{
			int count2 = item.Count;
			item.Count = count;
			string text = ((count <= count2) ? ((count >= count2) ? "SetItemCount" : "RemoveItemCount") : "AddItemCount");
			this.RaiseInventoryChange(new InventoryChange
			{
				Item = item,
				Updated = true,
				Op = text
			});
			this.InvalidateConstraints(item);
		}

		public void AddItemCount(InventoryItem item, int addCount)
		{
			item.Count += addCount;
			this.RaiseInventoryChange(new InventoryChange
			{
				Item = item,
				Updated = true,
				Op = "AddItemCount"
			});
			this.InvalidateConstraints(item);
		}

		public void AddItem(InventoryItem item)
		{
			base.Add(item);
			this.RaiseInventoryChange(new InventoryChange
			{
				Item = item,
				New = true,
				Op = "New"
			});
			this.InvalidateConstraints(item);
		}

		public InventoryItem JoinItem(InventoryItem item, StoragePlaces? targetStorage = null)
		{
			if (targetStorage != null)
			{
				item.Storage = targetStorage.Value;
			}
			if (item is Waistcoat && this.profile.PondId != null)
			{
				item.Storage = StoragePlaces.Storage;
			}
			if (item is Firework)
			{
				item.Storage = StoragePlaces.Equipment;
			}
			if (item.NestedItems != null && item.NestedItems.Length > 0)
			{
				foreach (InventoryItem inventoryItem in this)
				{
					Guid? parentItemInstanceId = inventoryItem.ParentItemInstanceId;
					bool flag = parentItemInstanceId != null;
					Guid? instanceId = item.InstanceId;
					if (flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault()) && inventoryItem.ParentItem == null)
					{
						inventoryItem.ParentItem = item;
					}
				}
			}
			if (item.IsUnstockable)
			{
				this.AddItem(item);
				this.InvalidateConstraints(item);
				this.EnsureInventoryItemAfterStorageWasManuallySet(item);
				return null;
			}
			InventoryItem inventoryItem2 = null;
			StoragePlaces storage = item.Storage;
			if (storage == StoragePlaces.Equipment || storage == StoragePlaces.Storage)
			{
				for (int i = 0; i < base.Count; i++)
				{
					InventoryItem inventoryItem3 = base[i];
					if (item.CanCombineWith(inventoryItem3) && inventoryItem3.ParentItemInstanceId == null && inventoryItem3.Storage == StoragePlaces.Equipment)
					{
						inventoryItem2 = inventoryItem3;
						break;
					}
				}
				if (inventoryItem2 == null)
				{
					for (int j = 0; j < base.Count; j++)
					{
						InventoryItem inventoryItem4 = base[j];
						if (item.CanCombineWith(inventoryItem4) && inventoryItem4.ParentItemInstanceId == null && inventoryItem4.Storage != (StoragePlaces)0 && (inventoryItem4.Storage == storage || inventoryItem4.Storage == StoragePlaces.Equipment))
						{
							inventoryItem2 = inventoryItem4;
							break;
						}
					}
				}
			}
			if (inventoryItem2 == null)
			{
				this.AddItem(item);
				this.InvalidateConstraints(item);
				this.EnsureInventoryItemAfterStorageWasManuallySet(item);
				return null;
			}
			if (item.IsStockableByAmount)
			{
				this.CombineItem(item, inventoryItem2, item.Amount, new InventoryMovementMapInfo(), null, false);
			}
			else
			{
				this.CombineItem(item, inventoryItem2, item.Count, new InventoryMovementMapInfo(), null, false);
			}
			if (storage == StoragePlaces.Equipment)
			{
				inventoryItem2.Storage = StoragePlaces.Equipment;
			}
			item.Storage = inventoryItem2.Storage;
			this.InvalidateConstraints(item);
			return inventoryItem2;
		}

		public void RemoveItem(InventoryItem item, bool isBroken = false)
		{
			foreach (InventoryItem inventoryItem in this.GetNestedItems(item, true).ToArray<InventoryItem>())
			{
				this.RemoveItem(inventoryItem, false);
			}
			foreach (InventoryItem inventoryItem2 in this)
			{
				if (inventoryItem2.ParentItemInstanceId != null)
				{
					Guid? parentItemInstanceId = inventoryItem2.ParentItemInstanceId;
					bool flag = parentItemInstanceId != null;
					Guid? instanceId = item.InstanceId;
					if (flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault()))
					{
						this.RaiseInventoryChange(new InventoryChange
						{
							Item = inventoryItem2,
							From = inventoryItem2.Storage,
							FromItem = inventoryItem2.ParentItem,
							To = StoragePlaces.Equipment,
							Op = "MoveRelated"
						});
						inventoryItem2.ParentItemInstanceId = null;
						inventoryItem2.ParentItem = null;
						inventoryItem2.Storage = StoragePlaces.Equipment;
					}
				}
			}
			if (item is Car)
			{
				foreach (InventoryItem inventoryItem3 in this)
				{
					if (inventoryItem3.Storage == StoragePlaces.CarEquipment)
					{
						this.RaiseInventoryChange(new InventoryChange
						{
							Item = inventoryItem3,
							From = StoragePlaces.CarEquipment,
							To = StoragePlaces.Storage,
							Op = "MoveRelated"
						});
						inventoryItem3.Storage = StoragePlaces.Storage;
					}
				}
			}
			if (item is Lodge)
			{
				foreach (InventoryItem inventoryItem4 in this)
				{
					if (inventoryItem4.Storage == StoragePlaces.LodgeEquipment)
					{
						this.RaiseInventoryChange(new InventoryChange
						{
							Item = inventoryItem4,
							From = StoragePlaces.LodgeEquipment,
							To = StoragePlaces.Storage,
							Op = "MoveRelated"
						});
						inventoryItem4.Storage = StoragePlaces.Storage;
					}
				}
			}
			base.Remove(item);
			this.RaiseInventoryChange(new InventoryChange
			{
				Item = item,
				Destroyed = true,
				Op = "Destroy",
				Broken = isBroken
			});
			this.InvalidateConstraints(item);
		}

		public IEnumerable<InventoryItem> GetNestedItems(InventoryItem parent, bool wholeOnly = false)
		{
			if (parent.NestedItems != null)
			{
				NestedItem[] nestedItems = parent.NestedItems;
				for (int j = 0; j < nestedItems.Length; j++)
				{
					NestedItem nestedItemRef = nestedItems[j];
					if (!wholeOnly || !nestedItemRef.CanBeDisassembled)
					{
						InventoryItem nestedItem = this.FirstOrDefault(delegate(InventoryItem i)
						{
							Guid? parentItemInstanceId = i.ParentItemInstanceId;
							bool flag = parentItemInstanceId != null;
							Guid? instanceId = parent.InstanceId;
							return flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault()) && i.Storage == StoragePlaces.ParentItem && i.ItemId == nestedItemRef.ItemId;
						});
						if (nestedItem != null)
						{
							yield return nestedItem;
						}
					}
				}
			}
			yield break;
		}

		public void SwapRods(Rod currentRodInHands, Rod rodToHands)
		{
			if (currentRodInHands != null)
			{
				this.MoveItem(currentRodInHands, null, StoragePlaces.Doll, true, true);
			}
			if (rodToHands != null)
			{
				this.MoveItem(rodToHands, null, StoragePlaces.Hands, true, true);
			}
		}

		public InventoryMovementMapInfo MoveItem(InventoryItem item, InventoryItem parent, StoragePlaces storage, bool moveRelatedItems = true, bool moveRelatedItemsToEquipment = true)
		{
			InventoryMovementMapInfo inventoryMovementMapInfo = new InventoryMovementMapInfo();
			this.MoveItem(item, parent, storage, inventoryMovementMapInfo, moveRelatedItems, moveRelatedItemsToEquipment);
			return inventoryMovementMapInfo;
		}

		public void MoveItem(InventoryItem item, InventoryItem parent, StoragePlaces storage, InventoryMovementMapInfo info, bool moveRelatedItems = true, bool moveRelatedItemsToEquipment = true)
		{
			StoragePlaces storage2 = item.Storage;
			InventoryItem parentItem = item.ParentItem;
			Guid? parentItemInstanceId = item.ParentItemInstanceId;
			if (parent == null)
			{
				if (storage != StoragePlaces.ParentItem)
				{
					item.ParentItemInstanceId = null;
					item.ParentItem = null;
				}
			}
			else
			{
				storage = StoragePlaces.ParentItem;
				item.ParentItemInstanceId = parent.InstanceId;
				item.ParentItem = parent;
			}
			StoragePlaces storage3 = item.Storage;
			item.Storage = storage;
			if (storage != StoragePlaces.Doll && storage != StoragePlaces.Hands && item.Slot != 0)
			{
				item.Slot = 0;
			}
			this.InvalidateConstraints(item);
			this.SetRodActiveQuiver(item as FeederRod);
			this.SetRodLeaderLength(item as Leader);
			if (!this.RemoveItemOnReturnToStorage(item))
			{
				this.RaiseInventoryChange(new InventoryChange
				{
					Item = item,
					From = storage2,
					FromItem = parentItem,
					To = storage,
					ToItem = parent,
					Op = "Move"
				});
			}
			if (moveRelatedItems)
			{
				this.MoveRelatedItems(item, null, parentItemInstanceId, storage3, storage, info, moveRelatedItemsToEquipment);
			}
		}

		public InventoryItem CheckMoveIsCombine(InventoryItem item, InventoryItem parent, StoragePlaces storage, InventoryMovementMapInfo info)
		{
			if (parent != null || storage == StoragePlaces.ParentItem)
			{
				return null;
			}
			Dictionary<Guid, Guid> combineMapNew = info.GetCombineMapNew(storage);
			Guid combineItemId;
			if (combineMapNew != null && combineMapNew.TryGetValue(item.InstanceId.Value, out combineItemId))
			{
				InventoryItem inventoryItem = this.FirstOrDefault((InventoryItem i) => item.CanCombineWith(i) && i.Storage == storage && i.InstanceId == combineItemId);
				if (inventoryItem != null)
				{
					return inventoryItem;
				}
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't find item to combine! ({0}) in {1}", combineItemId, storage));
			}
			else
			{
				InventoryItem inventoryItem2 = this.FirstOrDefault((InventoryItem i) => item.CanCombineWith(i) && i.Storage == storage);
				if (inventoryItem2 != null)
				{
					return inventoryItem2;
				}
				return null;
			}
		}

		public InventoryItem SplitItem(InventoryItem item, int count, InventoryMovementMapInfo info)
		{
			InventoryItem inventoryItem = Inventory.CloneItem(item);
			Guid guid;
			inventoryItem.InstanceId = new Guid?((!info.SplitMap.TryGetValue(item.ItemId, out guid)) ? Guid.NewGuid() : guid);
			info.SplitMap[item.ItemId] = inventoryItem.InstanceId.Value;
			item.SplitTo(inventoryItem, count);
			if (item.Count > 0)
			{
				this.RaiseInventoryChange(new InventoryChange
				{
					Item = item,
					Updated = true,
					Op = "Split"
				});
			}
			else
			{
				this.RemoveItem(item, false);
			}
			base.Add(inventoryItem);
			this.RaiseInventoryChange(new InventoryChange
			{
				Item = inventoryItem,
				FromItem = item,
				New = true,
				Op = "Split"
			});
			return inventoryItem;
		}

		public InventoryItem SplitItem(InventoryItem item, float amount, InventoryMovementMapInfo info)
		{
			InventoryItem inventoryItem = Inventory.CloneItem(item);
			Guid guid;
			inventoryItem.InstanceId = new Guid?((!info.SplitMap.TryGetValue(item.ItemId, out guid)) ? Guid.NewGuid() : guid);
			info.SplitMap[item.ItemId] = inventoryItem.InstanceId.Value;
			item.SplitTo(inventoryItem, amount);
			if ((double)Math.Abs(item.Amount) > 1E-05)
			{
				this.RaiseInventoryChange(new InventoryChange
				{
					Item = item,
					Updated = true,
					Op = "Split"
				});
			}
			else
			{
				this.RemoveItem(item, false);
			}
			base.Add(inventoryItem);
			this.RaiseInventoryChange(new InventoryChange
			{
				Item = inventoryItem,
				FromItem = item,
				New = true,
				Op = "Split"
			});
			return inventoryItem;
		}

		public void CutItem(InventoryItem item, int count)
		{
			item.Count -= count;
			this.RaiseInventoryChange(new InventoryChange
			{
				Item = item,
				Updated = true,
				Op = "Cut"
			});
		}

		public void CutItemWeight(InventoryItem item, double weight)
		{
			double? weight2 = item.Weight;
			item.Weight = ((weight2 == null) ? null : new double?(weight2.GetValueOrDefault() - weight));
			this.RaiseInventoryChange(new InventoryChange
			{
				Item = item,
				Updated = true,
				Op = "Cut"
			});
		}

		public bool CombineItem(InventoryItem item, InventoryItem targetItem, int count, InventoryMovementMapInfo info, InventoryItem replacementItem = null, bool moveRelatedItems = true)
		{
			Guid? parentItemInstanceId = item.ParentItemInstanceId;
			StoragePlaces storage = item.Storage;
			StoragePlaces storage2 = targetItem.Storage;
			bool flag = false;
			if (item.IsBroken())
			{
				this.RemoveItem(item, true);
				flag = true;
			}
			else
			{
				if (targetItem.CombineWith(item, count))
				{
					item.Storage = targetItem.Storage;
					item.ParentItem = null;
					this.RemoveItem(item, false);
					flag = true;
				}
				else
				{
					this.RaiseInventoryChange(new InventoryChange
					{
						Item = item,
						Updated = true,
						Op = "Combine"
					});
				}
				this.RaiseInventoryChange(new InventoryChange
				{
					Item = targetItem,
					Updated = true,
					Op = "Combine"
				});
				info.GetCombineMapNew(targetItem.Storage)[item.InstanceId.Value] = targetItem.InstanceId.Value;
			}
			if (moveRelatedItems)
			{
				this.MoveRelatedItems(item, replacementItem, parentItemInstanceId, storage, storage2, info, true);
			}
			return flag;
		}

		public bool CombineItem(InventoryItem item, InventoryItem targetItem, float amount, InventoryMovementMapInfo info, InventoryItem replacementItem = null, bool moveRelatedItems = true)
		{
			Guid? parentItemInstanceId = item.ParentItemInstanceId;
			StoragePlaces storage = item.Storage;
			StoragePlaces storage2 = targetItem.Storage;
			bool flag = false;
			if (item.IsBroken())
			{
				this.RemoveItem(item, true);
				flag = true;
			}
			else
			{
				if (targetItem.CombineWith(item, amount))
				{
					item.Storage = targetItem.Storage;
					item.ParentItem = null;
					this.RemoveItem(item, false);
					flag = true;
				}
				else
				{
					this.RaiseInventoryChange(new InventoryChange
					{
						Item = item,
						Updated = true,
						Op = "Combine"
					});
				}
				this.RaiseInventoryChange(new InventoryChange
				{
					Item = targetItem,
					Updated = true,
					Op = "Combine"
				});
				info.GetCombineMapNew(targetItem.Storage)[item.InstanceId.Value] = targetItem.InstanceId.Value;
			}
			if (moveRelatedItems)
			{
				this.MoveRelatedItems(item, replacementItem, parentItemInstanceId, storage, storage2, info, true);
			}
			return flag;
		}

		public void SubordinateItem(InventoryItem currentParent, InventoryItem newParent)
		{
			StoragePlaces storage = currentParent.Storage;
			int num = 0;
			if (currentParent is Rod)
			{
				num = currentParent.Slot;
			}
			StoragePlaces storage2 = newParent.Storage;
			currentParent.Storage = storage2;
			currentParent.Slot = 0;
			foreach (InventoryItem inventoryItem in this)
			{
				Guid? parentItemInstanceId = inventoryItem.ParentItemInstanceId;
				bool flag = parentItemInstanceId != null;
				Guid? instanceId = currentParent.InstanceId;
				if (flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault()))
				{
					inventoryItem.ParentItemInstanceId = newParent.InstanceId;
					inventoryItem.ParentItem = newParent;
					this.RaiseInventoryChange(new InventoryChange
					{
						Item = inventoryItem,
						FromItem = currentParent,
						ToItem = newParent,
						From = StoragePlaces.ParentItem,
						To = StoragePlaces.ParentItem,
						Op = "MoveRelated"
					});
				}
			}
			newParent.Storage = storage;
			newParent.Slot = num;
			this.SetRodActiveQuiver(currentParent as FeederRod);
			this.SetRodActiveQuiver(newParent as FeederRod);
			this.RaiseInventoryChange(new InventoryChange
			{
				Item = currentParent,
				From = newParent.Storage,
				To = currentParent.Storage,
				Op = "Subordinate"
			});
			this.RaiseInventoryChange(new InventoryChange
			{
				Item = newParent,
				To = newParent.Storage,
				From = currentParent.Storage,
				Op = "Subordinate"
			});
		}

		public void ReplaceItem(InventoryItem item, InventoryItem replacementItem, InventoryMovementMapInfo info, bool moveRelatedItems = true)
		{
			StoragePlaces storage = item.Storage;
			InventoryItem parentItem = item.ParentItem;
			Guid? parentItemInstanceId = item.ParentItemInstanceId;
			Guid? parentItemInstanceId2 = item.ParentItemInstanceId;
			InventoryItem inventoryItem = this.CheckMoveIsCombine(item, null, replacementItem.Storage, info);
			if (inventoryItem != null)
			{
				if (item.IsStockableByAmount)
				{
					this.CombineItem(item, inventoryItem, item.Amount, info, replacementItem, true);
				}
				else
				{
					this.CombineItem(item, inventoryItem, item.Count, info, replacementItem, true);
				}
				replacementItem.Storage = storage;
				replacementItem.ParentItem = parentItem;
				replacementItem.ParentItemInstanceId = parentItemInstanceId2;
			}
			else
			{
				item.Storage = replacementItem.Storage;
				item.ParentItem = replacementItem.ParentItem;
				item.ParentItemInstanceId = replacementItem.ParentItemInstanceId;
				replacementItem.Storage = storage;
				replacementItem.ParentItem = parentItem;
				replacementItem.ParentItemInstanceId = parentItemInstanceId;
				replacementItem.Slot = item.Slot;
				item.Slot = 0;
				if (!this.RemoveItemOnReturnToStorage(item))
				{
					this.RaiseInventoryChange(new InventoryChange
					{
						Item = item,
						From = replacementItem.Storage,
						FromItem = replacementItem.ParentItem,
						To = item.Storage,
						ToItem = item.ParentItem,
						Op = "Replace"
					});
				}
			}
			this.InvalidateConstraints(item);
			this.SetRodLeaderLength(replacementItem as Leader);
			this.RaiseInventoryChange(new InventoryChange
			{
				Item = replacementItem,
				From = (inventoryItem ?? item).Storage,
				FromItem = (inventoryItem ?? item).ParentItem,
				To = replacementItem.Storage,
				ToItem = replacementItem.ParentItem,
				Op = "Replace"
			});
			if (moveRelatedItems)
			{
				this.MoveRelatedItems(item, replacementItem, parentItemInstanceId2, storage, item.Storage, info, true);
			}
		}

		public InventoryItem SplitItemAndReplace(InventoryItem item, InventoryItem replacementItem, int count, InventoryMovementMapInfo info)
		{
			InventoryItem inventoryItem = ((replacementItem.Count != count) ? this.SplitItem(replacementItem, count, info) : replacementItem);
			this.ReplaceItem(item, inventoryItem, info, true);
			return inventoryItem;
		}

		public InventoryItem SplitItemAndReplace(InventoryItem item, InventoryItem replacementItem, float amount, InventoryMovementMapInfo info)
		{
			InventoryItem inventoryItem = ((replacementItem.Amount != amount || replacementItem is Chum) ? this.SplitItem(replacementItem, amount, info) : replacementItem);
			this.ReplaceItem(item, inventoryItem, info, true);
			return inventoryItem;
		}

		public static void Multiply(InventoryItem item, int itemCount)
		{
			if (item.IsStockableByAmount)
			{
				item.Amount *= (float)itemCount;
			}
			else
			{
				item.Count *= itemCount;
			}
			item.Init();
		}

		public static InventoryItem InitItem(InventoryItem item)
		{
			return item;
		}

		public static InventoryItem CloneItem(InventoryItem item)
		{
			Type type = item.GetType();
			InventoryItem inventoryItem = (InventoryItem)Activator.CreateInstance(type);
			inventoryItem.MakeCloneOfItem(item, false);
			inventoryItem.Durability = item.Durability;
			inventoryItem.MaxDurability = item.MaxDurability;
			inventoryItem.MinDurability = item.MinDurability;
			inventoryItem.Storage = item.Storage;
			return inventoryItem;
		}

		public static InventoryItem CloneItemFull(InventoryItem item)
		{
			Type type = item.GetType();
			InventoryItem inventoryItem = (InventoryItem)Activator.CreateInstance(type);
			inventoryItem.MakeCloneOfItem(item, true);
			return inventoryItem;
		}

		public static InventoryItem DisassembleItem(InventoryItem item)
		{
			if (item == null)
			{
				return null;
			}
			item.InstanceId = new Guid?(Guid.Empty);
			item.ParentItemInstanceId = new Guid?(Guid.Empty);
			item.Slot = 0;
			item.Storage = StoragePlaces.Storage;
			return item;
		}

		public InventoryItem GetItem(Guid instanceId)
		{
			foreach (InventoryItem inventoryItem in this)
			{
				if (inventoryItem.InstanceId == instanceId)
				{
					return inventoryItem;
				}
			}
			return null;
		}

		private void RaiseInventoryChange(InventoryChange inventoryChange)
		{
			if (this.OnInventoryChange != null)
			{
				this.OnInventoryChange(inventoryChange);
			}
			if (inventoryChange.Broken && this.OnItemLostBecauseBroken != null)
			{
				this.OnItemLostBecauseBroken(inventoryChange.Item);
			}
		}

		public void PreviewRepair(InventoryItem item, out int damageRestored, out float cost, out string currency)
		{
			if (item.MaxDurability == null)
			{
				throw new ArgumentException("Can't repair item: MaxDurability is null, item# " + item.InstanceId);
			}
			damageRestored = 0;
			cost = 0f;
			currency = null;
			if (!item.IsRepairable)
			{
				return;
			}
			if (item.Durability == item.MaxDurability)
			{
				return;
			}
			if (item.Durability == 0 && item.RaretyId < 2)
			{
				return;
			}
			int value = item.MaxDurability.Value;
			damageRestored = value - item.Durability;
			if (damageRestored > 0)
			{
				if (item.PriceSilver != null)
				{
					currency = "SC";
					cost = (float)item.PriceSilver.Value * Inventory.SilverRepairRate * (float)damageRestored / (float)item.MaxDurability.Value;
				}
				else if (item.PriceGold != null)
				{
					if (Inventory.IsRetail)
					{
						currency = "GC";
						cost = (float)((int)Math.Ceiling(item.PriceGold.Value * (double)Inventory.GoldRepairRate * (double)damageRestored / (double)item.MaxDurability.Value));
						if (cost == 0f)
						{
							damageRestored = 0;
						}
					}
					else
					{
						currency = "SC";
						cost = (float)item.PriceGold.Value * Inventory.GoldRepairRate * (float)damageRestored / (float)item.MaxDurability.Value * Inventory.ExchangeRate();
					}
				}
				cost = (float)((int)cost);
				if (cost < 1f)
				{
					cost = 1f;
				}
			}
		}

		public void Repair(InventoryItem item, int damageRestored)
		{
			item.RepairItem(damageRestored);
			this.RaiseInventoryChange(new InventoryChange
			{
				Item = item,
				Repaired = true,
				Op = "Repaired"
			});
		}

		public void ApplyWear(InventoryItem item, int damage)
		{
			item.ApplyWear(damage);
			this.RaiseInventoryChange(new InventoryChange
			{
				Item = item,
				Weared = true,
				Op = "Weared"
			});
		}

		public int ApplyWearBreakQuiverOnCast(FeederRod rod, QuiverTip quiver)
		{
			int num = rod.RodDurabilityLossForQuiver(quiver);
			rod.ApplyWear(num);
			quiver.IsBroken = true;
			this.RaiseInventoryChange(new InventoryChange
			{
				Item = rod,
				Weared = true,
				Op = "Weared"
			});
			return num;
		}

		public string GetInventoryAsString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (InventoryItem inventoryItem in from i in this.ToList<InventoryItem>()
				orderby i.InstanceId.ToString()
				select i)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append("\n");
				}
				stringBuilder.AppendFormat("{0} {1} {2} {3} {4} {5} {6} {7} {8:#0.000} {9:#0.000} {10:#0.000}", new object[]
				{
					inventoryItem.InstanceId,
					inventoryItem.ItemId,
					inventoryItem.GetType().Name,
					Enum.GetName(typeof(StoragePlaces), inventoryItem.Storage),
					inventoryItem.ParentItemInstanceId,
					(inventoryItem.ParentItem == null) ? string.Empty : inventoryItem.ParentItem.InstanceId.ToString(),
					inventoryItem.Slot,
					inventoryItem.Count,
					inventoryItem.Amount,
					inventoryItem.Length,
					inventoryItem.Weight
				});
			}
			return stringBuilder.ToString();
		}

		public const string PlayerHasNoCar = "Player does not have car, but traveling on car";

		public const string CantParentToSelf = "Can't make self parented item";

		public const string ReplacementCantBeNull = "Items for replacement can't be null";

		public const string CantReplaceWithSelf = "Can't replace item with itself";

		public const string ReplacementIsOfTheWrongType = "Item can be replaced only with item of the same type";

		public const string StorageIsNotAvailable = "Storage is not available at pond";

		public const string NoCar = "No car - no car equipment";

		public const string NoLodge = "No lodge - no lodge equipment";

		public const string ParentIsBroken = "Can't equip item on broken item";

		public const string ItemIsBroken = "Can't equip broken item";

		public const string ItemCantBeEquiped = "No constraint - item of this type can't be equiped at all";

		public const string NoMorePlaceToEquip = "Can't equip any more items";

		public const string RodCantBeModifiedWhenCasted = "Rod can not be modified when casted";

		public const string EquipmentRulesBreached = "Can't equip item";

		public const string CantParentToNull = "Can't parent to NULL";

		public const string CantParentToUnequipped = "Can't parent to unequipped rod";

		public const string CantMoveIntoOverloadedStorage = "Can't move items into overloaded storage";

		public const string CantMoveFromExceededStorage = "Can't move items from exceeded storage";

		public const string CantModifyRentedItem = "Can't modify rented item";

		public const string CantSwapRodsNoRodSpecified = "Can't swap rods - no rod specified";

		public const string CantSwapRodsCurrentRodShouldBeInHands = "Can't swap rods - current rod should be in hands";

		public const string CantSwapRodsNewRodShouldBeOnDoll = "Can't swap rods - new rod should be on doll";

		public const string CantSwapRodsNewRodUnequipped = "Can't swap rods - new rod is unequipped";

		public const string CantSplitItemNotEnough = "Can't split item - not enough";

		public const string CantSaveSetupEmptyRod = "Can't save setup - empty Rod in slot";

		public const string CantSaveSetupRodUnEquiped = "Can't save setup - Rod is UnEquiped";

		public const string CantSaveSetupNoSlotsAvailable = "Can't save setup - no slots available";

		public const string CantSaveSetupEmptyName = "Can't save setup - empty name";

		public const string CantEquipSetupStorageNotAvailable = "Can't equip setup - Storage is not available";

		public const string CantEquipSetupEmptySetup = "Can't equip empty setup";

		public const string CantEquipSetupSlotIsWrong = "Can't equip setup - wrong slot";

		public const string CantEquipSetupRodIsInHands = "Can't equip setup - Rod is in Hands";

		public const string CantEquipSetupNoRodInSlot = "Can't equip setup - no Rod in slot";

		public const string CantEquipSetupRodInSlotWrongType = "Can't equip setup - Rod in slot is of wrong type";

		public const string CantEquipSetupMissingItem = "Can't equip setup - some items missing in Inventory";

		public const string CantEquipSetupNotEnoughCapacityToUnequip = "Can't equip setup - not enough capacity to unequip current Rod";

		public const string CantClearSetupNotExistingSetup = "Can't clear setup - non existing setup";

		public const string CantRenameSetupNotExistingSetup = "Can't rename setup - non existing setup";

		public const string CantAddChumIngredientDifferentBaseType = "Can't add ingredient - wrong base type";

		public const string CantAddParticlesOnlyGroundbaitsCanContainParticles = "Can't add particles - only groundbaits allow particles";

		public const string CantMixChumNotOnPond = "Can't mix chum - should be on pond";

		public const string CantMixChumBaseNotEnough = "Can't mix chum - base percentage is too low";

		public const string CantMixChumAromaExceeded = "Can't mix chum - aroma percentage is too much";

		public const string CantMixChumParticleExceeded = "Can't mix chum - particle percentage is too much";

		public const string CantMixChumEmptyMix = "Can't mix chum - empty mix";

		public const string CantMixChumIngredientNoWeight = "Can't mix chum - ingredient without weight configured";

		public const string CantMixChumIngredientNotFound = "Can't mix chum - ingredient not found";

		public const string CantMixChumIngredientNotEnough = "Can't mix chum - ingredient not enough";

		public const string CantMixChumBaseCountExceeded = "Can't mix chum - base count exceeded";

		public const string CantMixChumAromaCountExceeded = "Can't mix chum - aroma count exceeded";

		public const string CantMixChumParticleCountExceeded = "Can't mix chum - particle count exceeded";

		public const string CantRenameChumNotExistingChum = "Can't rename chum - non existing chum";

		public const string CantSaveChumEmptyName = "Can't save chum - empty name";

		public const string CantCancelFillChumNotEquippedOnRod = "Can't unlink chum - chum should be empty";

		public const string CantFillChumNotEmpty = "Can't fill chum - chum should be empty";

		public const string CantFillChumNotEquippedOnRod = "Can't fill chum - not equipped on rod";

		public const string CantFillChumUnknownMix = "Can't fill chum - unknown mix";

		public const string CantFillChumUnavailableMix = "Can't fill chum - unavailable mix";

		public const string CantFillChumTooShortTime = "Can't fill chum in such short time";

		public const string CantFillChumNotEnoughMix = "Can't fill chum - not enough mix amount";

		public const string CantUseChumUsageTimeExpired = "Can't use chum - Usage time expired";

		public const string CantUseChumWrongAmount = "Can't use chum - amount differs from Feeder capacity";

		public const string CantSaveChumRecipeEmptyName = "Can't save ChumRecipe - empty name";

		public const string CantSaveChumRecipeNoSlotsAvailable = "Can't save ChumRecipe - no slots available";

		public const string CantRenameChumRecipeNotExistingRecipe = "Can't rename ChumRecipe - non existing recipe";

		public const string CantRemoveChumRecipeNotExistingRecipe = "Can't remove ChumRecipe - non existing recipe";

		public const string CantSetLeaderLengthLongerThanHave = "Can't set leader length longer than have";

		public const string CantSetLeaderLengthLongerThanPrev = "Can't set leader length longer than previous value";

		public const string CantSetQuiverIndexNoQuiverSpecified = "Can't set quiver index - no quiver with such ItemId defined in FeederRod";

		public const string SC = "SC";

		public const string GC = "GC";

		public const string BC = "GC";

		public const string USD = "USD";

		public const int MinLineLength = 10;

		public const int MinLeaderLength = 0;

		public const int UnexistingItemId = 2147483647;

		public const string ChumNamePrefix = "Mix ";

		[JsonIgnore]
		private Profile profile;

		[JsonIgnore]
		private RodTemplates rodTemplate;

		[JsonIgnore]
		private static readonly InventoryConstraint BasicTeminalTackleConstraint = new InventoryConstraint(10, false, new ItemTypes[]
		{
			ItemTypes.Bait,
			ItemTypes.Lure,
			ItemTypes.Hook,
			ItemTypes.Bobber,
			ItemTypes.JigHead,
			ItemTypes.JigBait,
			ItemTypes.Sinker,
			ItemTypes.Feeder,
			ItemTypes.Bell,
			ItemTypes.Leader,
			ItemTypes.UnderwaterItem,
			ItemTypes.MissionItem
		}, null);

		[JsonIgnore]
		private static readonly InventoryConstraint BasicChumConstraint = new InventoryConstraint(10, false, new ItemTypes[]
		{
			ItemTypes.Chum,
			ItemTypes.ChumBase,
			ItemTypes.ChumParticle,
			ItemTypes.ChumAroma
		}, null);

		[JsonIgnore]
		private static readonly IDictionary<ItemSubTypes, InventoryConstraint> BasicEquipmentConstraints = new Dictionary<ItemSubTypes, InventoryConstraint>
		{
			{
				ItemSubTypes.Reel,
				new InventoryConstraint(1, true, null, null)
			},
			{
				ItemSubTypes.Line,
				new InventoryConstraint(1, true, null, null)
			},
			{
				ItemSubTypes.Bait,
				Inventory.BasicTeminalTackleConstraint
			},
			{
				ItemSubTypes.Lure,
				Inventory.BasicTeminalTackleConstraint
			},
			{
				ItemSubTypes.Hook,
				Inventory.BasicTeminalTackleConstraint
			},
			{
				ItemSubTypes.Bobber,
				Inventory.BasicTeminalTackleConstraint
			},
			{
				ItemSubTypes.JigHead,
				Inventory.BasicTeminalTackleConstraint
			},
			{
				ItemSubTypes.JigBait,
				Inventory.BasicTeminalTackleConstraint
			},
			{
				ItemSubTypes.Sinker,
				Inventory.BasicTeminalTackleConstraint
			},
			{
				ItemSubTypes.Feeder,
				Inventory.BasicTeminalTackleConstraint
			},
			{
				ItemSubTypes.Bell,
				Inventory.BasicTeminalTackleConstraint
			},
			{
				ItemSubTypes.Leader,
				Inventory.BasicTeminalTackleConstraint
			},
			{
				ItemSubTypes.UnderwaterItem,
				Inventory.BasicTeminalTackleConstraint
			},
			{
				ItemSubTypes.MissionItem,
				Inventory.BasicTeminalTackleConstraint
			},
			{
				ItemSubTypes.Chum,
				Inventory.BasicChumConstraint
			},
			{
				ItemSubTypes.ChumBase,
				Inventory.BasicChumConstraint
			},
			{
				ItemSubTypes.ChumParticle,
				Inventory.BasicChumConstraint
			},
			{
				ItemSubTypes.ChumAroma,
				Inventory.BasicChumConstraint
			},
			{
				ItemSubTypes.Misc,
				new InventoryConstraint(10, false, null, null)
			},
			{
				ItemSubTypes.RepairKit,
				new InventoryConstraint(10, true, null, null)
			}
		};

		[JsonIgnore]
		private static readonly IDictionary<ItemSubTypes, InventoryConstraint> BasicDollConstraints = new Dictionary<ItemSubTypes, InventoryConstraint>
		{
			{
				ItemSubTypes.Rod,
				new InventoryConstraint(1, true, null, null)
			},
			{
				ItemSubTypes.RodCase,
				new InventoryConstraint(1, false, null, null)
			},
			{
				ItemSubTypes.LuresBox,
				new InventoryConstraint(1, false, null, null)
			},
			{
				ItemSubTypes.Keepnet,
				new InventoryConstraint(1, false, null, new ItemSubTypes[]
				{
					ItemSubTypes.Keepnet,
					ItemSubTypes.Stringer
				})
			},
			{
				ItemSubTypes.Stringer,
				new InventoryConstraint(1, false, null, new ItemSubTypes[]
				{
					ItemSubTypes.Keepnet,
					ItemSubTypes.Stringer
				})
			},
			{
				ItemSubTypes.Boat,
				new InventoryConstraint(1, true, null, null)
			},
			{
				ItemSubTypes.RodStand,
				new InventoryConstraint(1, false, null, null)
			},
			{
				ItemSubTypes.Tool,
				new InventoryConstraint(1, false, null, null)
			},
			{
				ItemSubTypes.Outfit,
				new InventoryConstraint(1, false, null, null)
			},
			{
				ItemSubTypes.Chum,
				new InventoryConstraint(1, false, null, null)
			}
		};

		[JsonIgnore]
		private static readonly IDictionary<ItemSubTypes, InventoryConstraint> BasicShoreConstraints = new Dictionary<ItemSubTypes, InventoryConstraint>();

		[JsonIgnore]
		private IDictionary<ItemSubTypes, InventoryConstraint> equipConstraintsCache;

		[JsonIgnore]
		private IDictionary<ItemSubTypes, InventoryConstraint> dollConstraintsCache;

		[JsonIgnore]
		private IDictionary<ItemSubTypes, InventoryConstraint> shoreConstraintsCache;

		private readonly ItemTypes[] terminalTackleTypes = new ItemTypes[]
		{
			ItemTypes.TerminalTackle,
			ItemTypes.Hook,
			ItemTypes.Bobber,
			ItemTypes.Sinker,
			ItemTypes.Feeder,
			ItemTypes.Bait,
			ItemTypes.Lure,
			ItemTypes.JigHead,
			ItemTypes.JigBait,
			ItemTypes.Bell,
			ItemTypes.Leader,
			ItemTypes.UnderwaterItem
		};

		private readonly ItemTypes[] chumTypes = new ItemTypes[]
		{
			ItemTypes.Chum,
			ItemTypes.ChumBase,
			ItemTypes.ChumAroma,
			ItemTypes.ChumParticle
		};
	}
}
