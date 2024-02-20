using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using ObjectModel;

namespace Assets.Scripts.UI._2D.Inventory
{
	public class InventoryHelper
	{
		public static bool IsBlocked2Equip(InventoryItem rod, InventoryItem ii, bool isDoll = false)
		{
			if (InitRods.Instance != null)
			{
				if (ii is ChumIngredient)
				{
					if (ChumMixing.Instance != null && ChumMixing.Instance.IsBlockedBySnowballs(ii))
					{
						return true;
					}
					if (ii is ChumBase)
					{
						return (!InitRods.Instance.IsChumMixing && InventoryHelper.IsBlocked2EquipChumBase(rod, ii as ChumBase, isDoll)) || (ChumMixing.Instance != null && InitRods.Instance.IsChumMixing && ChumMixing.Instance.IsDifferentBases(ii));
					}
					return !InitRods.Instance.IsChumMixing || (ChumMixing.Instance != null && ChumMixing.Instance.IsParticleBlocked(ii));
				}
				else if (InitRods.Instance.IsChumMixing)
				{
					return true;
				}
			}
			bool flag = rod != null;
			bool flag2 = ii != null;
			bool flag3 = ii is Rod;
			bool flag4 = !isDoll && flag && RodHelper.IsInventorySlotOccupiedByRodStand(rod.Slot);
			bool flag5 = flag && flag2 && (rod.ItemSubType != ItemSubTypes.CarpRod || (rod.ItemSubType == ItemSubTypes.CarpRod && (ii.ItemSubType != ItemSubTypes.CommonBait || (ii.ItemSubType == ItemSubTypes.CommonBait && Inventory.IsCarpBait(ii)))));
			bool flag6 = !isDoll && flag && flag2 && (!ListOfCompatibility.GetCompatibilityEquipment(rod.ItemSubType).Contains(ii.ItemSubType) || !flag5);
			bool flag7 = !isDoll && flag && !flag3 && flag2 && !InventoryHelper.CanAggregate(rod as Rod, ii);
			bool flag8 = !isDoll && flag && ii is Chum && InventoryHelper.IsBlocked2EquipChum(rod, (Chum)ii);
			bool flag9 = isDoll && flag2 && (!InventoryHelper.DollItemTypes.Contains(ii.ItemType) || (ii is Chum && InventoryHelper.IsBlocked2EquipChumHands(ii as Chum)));
			return flag4 || flag6 || flag8 || flag9 || flag7;
		}

		public static bool HasFeeder(InventoryItem rod)
		{
			Profile profile = PhotonConnectionFactory.Instance.Profile;
			return profile != null && rod != null && profile.Inventory.Items.ToList<InventoryItem>().Any(delegate(InventoryItem p)
			{
				Guid? parentItemInstanceId = p.ParentItemInstanceId;
				bool flag = parentItemInstanceId != null;
				Guid? instanceId = rod.InstanceId;
				return flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault()) && p.ItemType == ItemTypes.Feeder;
			});
		}

		public static Feeder GetFeeder(InventoryItem rod)
		{
			Profile profile = PhotonConnectionFactory.Instance.Profile;
			if (profile == null || rod == null)
			{
				return null;
			}
			return (Feeder)profile.Inventory.FirstOrDefault(delegate(InventoryItem x)
			{
				Guid? parentItemInstanceId = x.ParentItemInstanceId;
				bool flag = parentItemInstanceId != null;
				Guid? instanceId = rod.InstanceId;
				return flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault()) && x.ItemType == ItemTypes.Feeder;
			});
		}

		public static string ItemCountStr(InventoryItem ii)
		{
			int num = InventoryHelper.ItemCount(ii);
			return (num <= 1) ? string.Empty : num.ToString(CultureInfo.InvariantCulture);
		}

		public static int ItemCount(InventoryItem ii)
		{
			if (ii.ItemType == ItemTypes.Line)
			{
				return (int)MeasuringSystemManager.LineLength((ii.Length == null) ? 0f : ((float)ii.Length.Value));
			}
			return ii.Count;
		}

		public static InventoryItem GetFirstItemByItemId(int itemId)
		{
			if (PhotonConnectionFactory.Instance.Profile == null)
			{
				return null;
			}
			return PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemId == itemId);
		}

		public static bool IsBlocked2EquipChumBase(InventoryItem rod, ChumBase chum, bool isDoll = false)
		{
			Feeder feeder = InventoryHelper.GetFeeder(rod);
			if (feeder == null && !isDoll)
			{
				return true;
			}
			ItemSubTypes itemSubType = chum.ItemSubType;
			if (itemSubType == ItemSubTypes.ChumGroundbaits)
			{
				if (!isDoll && feeder.ItemSubType == ItemSubTypes.FlatFeeder)
				{
					return true;
				}
			}
			else if (itemSubType == ItemSubTypes.ChumCarpbaits)
			{
				if (isDoll || feeder.ItemSubType == ItemSubTypes.CageFeeder || feeder.ItemSubType == ItemSubTypes.FlatFeeder)
				{
					return true;
				}
			}
			else if (itemSubType == ItemSubTypes.ChumMethodMix && !isDoll && feeder.ItemSubType == ItemSubTypes.CageFeeder)
			{
				return true;
			}
			return false;
		}

		public static bool IsBlocked2EquipChum(InventoryItem rod, Chum chum)
		{
			Feeder feeder = InventoryHelper.GetFeeder(rod);
			if (feeder == null)
			{
				return true;
			}
			ItemSubTypes chumType = InventoryHelper.GetChumType(chum);
			if (chumType == ItemSubTypes.ChumGroundbaits)
			{
				if (feeder.ItemSubType == ItemSubTypes.FlatFeeder)
				{
					return true;
				}
			}
			else if (chumType == ItemSubTypes.ChumCarpbaits)
			{
				if (feeder.ItemSubType == ItemSubTypes.CageFeeder || feeder.ItemSubType == ItemSubTypes.FlatFeeder)
				{
					return true;
				}
			}
			else if (chumType == ItemSubTypes.ChumMethodMix && feeder.ItemSubType == ItemSubTypes.CageFeeder)
			{
				return true;
			}
			return false;
		}

		public static bool IsBlocked2EquipChumHands(Chum chum)
		{
			ItemSubTypes chumType = InventoryHelper.GetChumType(chum);
			return chumType == ItemSubTypes.ChumCarpbaits;
		}

		public static ItemSubTypes GetChumType(Chum chum)
		{
			List<ChumBase> chumBase = chum.ChumBase;
			return (chumBase.Count <= 0) ? ItemSubTypes.All : chumBase[0].ItemSubType;
		}

		public static RodTemplate MatchedTemplate(Rod rod)
		{
			return PhotonConnectionFactory.Instance.Profile.Inventory.RodTemplate.MatchedTemplate(rod);
		}

		public static RodTemplate MatchedTemplateWith(Rod rod, InventoryItem item)
		{
			return PhotonConnectionFactory.Instance.Profile.Inventory.RodTemplate.MatchedTemplateWith(rod, item);
		}

		public static RodTemplate MatchedTemplatePartialWith(Rod rod, InventoryItem item)
		{
			return PhotonConnectionFactory.Instance.Profile.Inventory.RodTemplate.MatchedTemplatePartialWith(rod, item);
		}

		public static RodTemplate GetRodTemplatePartial(Rod rod)
		{
			return PhotonConnectionFactory.Instance.Profile.Inventory.RodTemplate.MatchedTemplatePartial(rod);
		}

		public static RodTemplate GetLargestRodTemplatePartial(Rod rod)
		{
			return PhotonConnectionFactory.Instance.Profile.Inventory.RodTemplate.MatchedLargestTemplatePartial(rod);
		}

		public static RodTemplate GetClosestTemplate(Rod rod)
		{
			RodTemplate rodTemplate = InventoryHelper.MatchedTemplate(rod);
			if (rodTemplate == RodTemplate.UnEquiped)
			{
				rodTemplate = InventoryHelper.GetRodTemplatePartial(rod);
			}
			return rodTemplate;
		}

		public static RodTemplate GetLargestTemplate(Rod rod)
		{
			RodTemplate rodTemplate = InventoryHelper.MatchedTemplate(rod);
			RodTemplate largestRodTemplatePartial = InventoryHelper.GetLargestRodTemplatePartial(rod);
			return (RodTemplates.GetTypesForTemplate(largestRodTemplatePartial).Length <= RodTemplates.GetTypesForTemplate(rodTemplate).Length) ? rodTemplate : largestRodTemplatePartial;
		}

		public static RodTemplate GetClosestTemplateWith(Rod rod, InventoryItem item)
		{
			RodTemplate rodTemplate = InventoryHelper.MatchedTemplateWith(rod, item);
			if (rodTemplate == RodTemplate.UnEquiped)
			{
				rodTemplate = InventoryHelper.MatchedTemplatePartialWith(rod, item);
			}
			return rodTemplate;
		}

		public static DropMeDoll GetCurrentRodSlotForSubType(ItemSubTypes subType)
		{
			List<DropMeDoll> list = InitRods.DropMeComponents.Where((DropMeDoll x) => x.InventoryItemView != null && x.InventoryItemView.Storage != StoragePlaces.Equipment && x.InventoryItemView.Storage != StoragePlaces.Storage && Array.Exists<int>(x.typeId, (int y) => y == (int)subType)).ToList<DropMeDoll>();
			DropMeDoll dropMeDoll;
			if ((dropMeDoll = list.FirstOrDefault((DropMeDoll x) => x.InventoryItemView.InventoryItem == null)) == null)
			{
				dropMeDoll = list.FirstOrDefault((DropMeDoll x) => x.InventoryItemView.InventoryItem != null && x.InventoryItemView.InventoryItem.ItemSubType == subType) ?? list.FirstOrDefault<DropMeDoll>();
			}
			return dropMeDoll;
		}

		public static bool CanAggregate(Rod rod, InventoryItem item)
		{
			List<InventoryItem> list = PhotonConnectionFactory.Instance.Profile.Inventory.Items.Where(delegate(InventoryItem x)
			{
				Guid? parentItemInstanceId = x.ParentItemInstanceId;
				bool flag = parentItemInstanceId != null;
				Guid? instanceId = rod.InstanceId;
				return flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault());
			}).ToList<InventoryItem>();
			return rod.CanAggregate(item, list);
		}

		public static readonly IList<ItemTypes> Blocked2EquipItemTypes = new ReadOnlyCollection<ItemTypes>(new List<ItemTypes>
		{
			ItemTypes.Outfit,
			ItemTypes.Boat,
			ItemTypes.Tool,
			ItemTypes.UnderwaterItem
		});

		public static readonly IList<ItemTypes> DollItemTypes = new ReadOnlyCollection<ItemTypes>(new List<ItemTypes>
		{
			ItemTypes.Outfit,
			ItemTypes.Boat,
			ItemTypes.Tool,
			ItemTypes.Chum
		});
	}
}
