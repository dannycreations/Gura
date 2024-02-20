using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectModel
{
	public class MissionInventoryContext
	{
		public MissionsContext Context
		{
			set
			{
				this.context = value;
			}
		}

		public MissionDollContext ItemsOnDoll
		{
			get
			{
				MissionDollContext missionDollContext = new MissionDollContext
				{
					Context = this.context
				};
				missionDollContext.AddRange(this.context.GetProfile().Inventory.Where((InventoryItem i) => i.Storage == StoragePlaces.Doll));
				return missionDollContext;
			}
		}

		public IEnumerable<MissionRodContext> RodsOnDoll
		{
			get
			{
				return from r in this.context.GetProfile().Inventory.OfType<Rod>()
					where r.IsRodOnDoll
					select r into rod
					select new MissionRodContext(this.context, rod);
			}
		}

		public MissionRodContext RodInHands
		{
			get
			{
				Rod rod = this.context.GetProfile().Inventory.OfType<Rod>().FirstOrDefault((Rod r) => r.Storage == StoragePlaces.Hands);
				return new MissionRodContext(this.context, rod);
			}
		}

		public InventoryItem HasItemAtCurrentLocation(int itemId, IBuyItemsStorageResource storageCondition = null, MissionInventoryFilter filter = null, bool checkDoll = false, int slot = 0)
		{
			if (itemId < 0)
			{
				return this.HasItemOfTypeAtCurrentLocation(-itemId, storageCondition, filter, checkDoll, slot);
			}
			Func<InventoryItem, bool> predicateStorage = MissionInventoryContext.GetPredicateStorage(storageCondition);
			return (from i in this.GetHasItemAtCurrentLocationQuery((IEnumerable<InventoryItem> q) => MissionInventoryContext.ApplyInventoryItemFilter(q, filter, false), checkDoll, slot)
				where i.Durability > 0
				where i.ItemId == itemId || (i is Chum && ((Chum)i).Ingredients.Any((ChumIngredient ii) => ii.ItemId == itemId))
				select i).Where(predicateStorage).FirstOrDefault<InventoryItem>();
		}

		public InventoryItem HasItemOfTypeAtCurrentLocation(int typeId, IBuyItemsStorageResource storageCondition = null, MissionInventoryFilter filter = null, bool checkDoll = false, int slot = 0)
		{
			Func<InventoryItem, bool> predicateStorage = MissionInventoryContext.GetPredicateStorage(storageCondition);
			return (from i in this.GetHasItemAtCurrentLocationQuery((IEnumerable<InventoryItem> q) => MissionInventoryContext.ApplyInventoryItemFilter(q, filter, false), checkDoll, slot)
				where i.Durability > 0
				where (int)i.ItemSubType == typeId || (int)i.ItemType == typeId || (i is Chum && ((Chum)i).Ingredients.Any((ChumIngredient ii) => (int)ii.ItemSubType == typeId || (int)ii.ItemType == typeId))
				select i).Where(predicateStorage).FirstOrDefault<InventoryItem>();
		}

		public InventoryItem HasItemAtHome(int itemId, MissionInventoryFilter filter = null)
		{
			if (itemId < 0)
			{
				return this.HasItemOfTypeAtHome(-itemId, filter);
			}
			return (from i in this.GetHasItemAtHomeQuery((IEnumerable<InventoryItem> q) => MissionInventoryContext.ApplyInventoryItemFilter(q, filter, false))
				where i.Durability > 0
				where i.ItemId == itemId
				select i).FirstOrDefault<InventoryItem>();
		}

		public InventoryItem HasItemOfTypeAtHome(int typeId, MissionInventoryFilter filter = null)
		{
			return (from i in this.GetHasItemAtHomeQuery((IEnumerable<InventoryItem> q) => MissionInventoryContext.ApplyInventoryItemFilter(q, filter, false))
				where i.Durability > 0
				where (int)i.ItemSubType == typeId || (int)i.ItemType == typeId
				select i).FirstOrDefault<InventoryItem>();
		}

		private IEnumerable<InventoryItem> GetHasItemAtCurrentLocationQuery(Func<IEnumerable<InventoryItem>, IEnumerable<InventoryItem>> filter, bool checkDoll, int slot)
		{
			if (filter == null)
			{
				filter = (IEnumerable<InventoryItem> q) => q;
			}
			Inventory inventory = this.context.GetProfile().Inventory;
			IEnumerable<InventoryItem> enumerable = filter(inventory.Where((InventoryItem i) => i.Storage == StoragePlaces.Equipment));
			if (checkDoll)
			{
				enumerable = enumerable.Union(filter(inventory.Where((InventoryItem i) => i.Storage == StoragePlaces.Doll || i.Storage == StoragePlaces.Hands || i.Storage == StoragePlaces.Shore || i.Storage == StoragePlaces.ParentItem)));
			}
			else
			{
				enumerable = enumerable.Union(filter((from i in inventory.OfType<Rod>()
					where i.Storage == StoragePlaces.Doll || i.Storage == StoragePlaces.Hands || i.Storage == StoragePlaces.Shore
					select i).Cast<InventoryItem>()));
				if (slot > 0)
				{
					enumerable = enumerable.Union(filter(inventory.Where((InventoryItem i) => i.Storage == StoragePlaces.ParentItem && i.ParentItem != null && i.ParentItem.Slot == slot)));
				}
			}
			if (inventory.IsStorageAvailable())
			{
				enumerable = enumerable.Union(filter(inventory.Where((InventoryItem i) => i.Storage == StoragePlaces.Storage)));
			}
			return enumerable;
		}

		private IEnumerable<InventoryItem> GetHasItemAtHomeQuery(Func<IEnumerable<InventoryItem>, IEnumerable<InventoryItem>> filter)
		{
			if (filter == null)
			{
				filter = (IEnumerable<InventoryItem> q) => q;
			}
			Inventory inventory = this.context.GetProfile().Inventory;
			return filter(inventory.Where((InventoryItem i) => i.Storage == StoragePlaces.Storage));
		}

		private static Func<InventoryItem, bool> GetPredicateStorage(IBuyItemsStorageResource storageCondition)
		{
			Func<InventoryItem, bool> func = (InventoryItem i) => true;
			if (storageCondition != null && (storageCondition.Storage != null || (storageCondition.Storages != null && storageCondition.Storages.Length > 0)))
			{
				func = (InventoryItem i) => (storageCondition.Storage != null && i.Storage == storageCondition.Storage) || (storageCondition.Storages != null && storageCondition.Storages.Contains(i.Storage));
			}
			return func;
		}

		private static IEnumerable<InventoryItem> ApplyInventoryItemFilter(IEnumerable<InventoryItem> query, MissionInventoryFilter filter, bool shop = false)
		{
			if (filter != null)
			{
				if (filter.LineMinThickness != null)
				{
					query = query.OfType<Line>().Where(delegate(Line i)
					{
						float? lineMinThickness = filter.LineMinThickness;
						return i.Thickness >= lineMinThickness;
					}).Cast<InventoryItem>();
				}
				if (filter.LineMaxThickness != null)
				{
					query = query.OfType<Line>().Where(delegate(Line i)
					{
						float? lineMaxThickness = filter.LineMaxThickness;
						return i.Thickness <= lineMaxThickness;
					}).Cast<InventoryItem>();
				}
				if (shop && filter.LineLength != null)
				{
					query = (from i in query.OfType<Line>().Where(delegate(Line i)
						{
							int? lineLength = filter.LineLength;
							return i.Count >= lineLength;
						})
						orderby i.Count descending
						select i).Cast<InventoryItem>();
				}
				if (!shop && filter.LineLength != null)
				{
					query = (from i in query.OfType<Line>().Where(delegate(Line i)
						{
							double? length = i.Length;
							bool flag = length != null;
							int? lineLength2 = filter.LineLength;
							double? num = ((lineLength2 == null) ? null : new double?((double)lineLength2.Value));
							return (flag & (num != null)) && length.GetValueOrDefault() >= num.GetValueOrDefault();
						})
						orderby i.Length descending
						select i).Cast<InventoryItem>();
				}
				if (filter.LeaderMinThickness != null)
				{
					query = query.OfType<Leader>().Where(delegate(Leader i)
					{
						float? leaderMinThickness = filter.LeaderMinThickness;
						return i.Thickness >= leaderMinThickness;
					}).Cast<InventoryItem>();
				}
				if (filter.LeaderMaxThickness != null)
				{
					query = query.OfType<Leader>().Where(delegate(Leader i)
					{
						float? leaderMaxThickness = filter.LeaderMaxThickness;
						return i.Thickness <= leaderMaxThickness;
					}).Cast<InventoryItem>();
				}
				if (filter.LeaderLength != null)
				{
					query = (from i in query.OfType<Leader>().Where(delegate(Leader i)
						{
							int? leaderLength = filter.LeaderLength;
							float? num2 = ((leaderLength == null) ? null : new float?((float)leaderLength.Value));
							return i.LeaderLength >= num2;
						})
						orderby i.LeaderLength descending
						select i).Cast<InventoryItem>();
				}
				if (filter.ItemType != ItemTypes.All)
				{
					query = query.Where(delegate(InventoryItem i)
					{
						ItemTypes itemType = filter.ItemType;
						if (itemType != ItemTypes.Chum)
						{
							return i.ItemType == filter.ItemType;
						}
						return i.ItemType == ItemTypes.Chum || i.ItemType == ItemTypes.ChumBase || i.ItemType == ItemTypes.ChumAroma || i.ItemType == ItemTypes.ChumParticle;
					});
				}
				if (filter.ChumWeight != null)
				{
					query = query.Where(delegate(InventoryItem i)
					{
						bool flag2;
						if (i is Chum || i is ChumIngredient)
						{
							float? chumWeight = filter.ChumWeight;
							flag2 = i.Amount >= chumWeight;
						}
						else
						{
							flag2 = false;
						}
						return flag2;
					});
				}
				if (filter.MinLoad != null)
				{
					query = query.Where(delegate(InventoryItem i)
					{
						bool flag3;
						if (i is Rod || i is Reel || i is Line || i is Leader)
						{
							float? minLoad = filter.MinLoad;
							flag3 = MissionInventoryContext.GetItemMaxLoad(i) >= minLoad;
						}
						else
						{
							flag3 = false;
						}
						return flag3;
					});
				}
				if (filter.MaxLoad != null)
				{
					query = query.Where(delegate(InventoryItem i)
					{
						bool flag4;
						if (i is Rod || i is Reel || i is Line || i is Leader)
						{
							float? maxLoad = filter.MaxLoad;
							flag4 = MissionInventoryContext.GetItemMaxLoad(i) <= maxLoad;
						}
						else
						{
							flag4 = false;
						}
						return flag4;
					});
				}
			}
			return query;
		}

		public static bool CheckInventoryFilter(InventoryItem item, MissionInventoryFilter filter, bool shop = false)
		{
			if (filter == null)
			{
				return true;
			}
			if (filter.ItemType != ItemTypes.All && item.ItemType != filter.ItemType)
			{
				return false;
			}
			if (filter.LineLength != null || filter.LineMinThickness != null || filter.LineMaxThickness != null)
			{
				Line line = item as Line;
				if (line == null)
				{
					return false;
				}
				if (shop && filter.LineLength != null)
				{
					int? lineLength = filter.LineLength;
					if (line.Count < lineLength)
					{
						return false;
					}
				}
				if (!shop && filter.LineLength != null)
				{
					double? length = line.Length;
					bool flag = length != null;
					int? lineLength2 = filter.LineLength;
					double? num = ((lineLength2 == null) ? null : new double?((double)lineLength2.Value));
					if ((flag & (num != null)) && length.GetValueOrDefault() < num.GetValueOrDefault())
					{
						return false;
					}
				}
				if (filter.LineMinThickness != null)
				{
					float? lineMinThickness = filter.LineMinThickness;
					if (line.Thickness < lineMinThickness)
					{
						return false;
					}
				}
				if (filter.LineMaxThickness != null)
				{
					float? lineMaxThickness = filter.LineMaxThickness;
					if (line.Thickness > lineMaxThickness)
					{
						return false;
					}
				}
			}
			if (filter.LeaderLength != null || filter.LeaderMinThickness != null || filter.LeaderMaxThickness != null)
			{
				Leader leader = item as Leader;
				if (leader == null)
				{
					return false;
				}
				if (filter.LeaderLength != null)
				{
					int? leaderLength = filter.LeaderLength;
					float? num2 = ((leaderLength == null) ? null : new float?((float)leaderLength.Value));
					if (leader.LeaderLength < num2)
					{
						return false;
					}
				}
				if (filter.LeaderMinThickness != null)
				{
					float? leaderMinThickness = filter.LeaderMinThickness;
					if (leader.Thickness < leaderMinThickness)
					{
						return false;
					}
				}
				if (filter.LeaderMaxThickness != null)
				{
					float? leaderMaxThickness = filter.LeaderMaxThickness;
					if (leader.Thickness > leaderMaxThickness)
					{
						return false;
					}
				}
			}
			if (filter.MinLoad != null || filter.MaxLoad != null)
			{
				if (item.ItemType != ItemTypes.Rod && item.ItemType != ItemTypes.Reel && item.ItemType != ItemTypes.Line && item.ItemType != ItemTypes.Leader)
				{
					return false;
				}
				float itemMaxLoad = MissionInventoryContext.GetItemMaxLoad(item);
				if (filter.MinLoad != null && itemMaxLoad < filter.MinLoad)
				{
					return false;
				}
				if (filter.MaxLoad != null && itemMaxLoad > filter.MaxLoad)
				{
					return false;
				}
			}
			return true;
		}

		public bool IsRod(int itemId)
		{
			ItemSubTypes itemSubTypes = ((itemId >= 0) ? this.context.GetGameCacheAdapter().ResolveItemGloablly(itemId).ItemSubType : ((ItemSubTypes)(-(ItemSubTypes)itemId)));
			return itemSubTypes == ItemSubTypes.Rod || itemSubTypes == ItemSubTypes.TelescopicRod || itemSubTypes == ItemSubTypes.MatchRod || itemSubTypes == ItemSubTypes.SpinningRod || itemSubTypes == ItemSubTypes.CastingRod || itemSubTypes == ItemSubTypes.FeederRod || itemSubTypes == ItemSubTypes.BottomRod || itemSubTypes == ItemSubTypes.CarpRod || itemSubTypes == ItemSubTypes.SpodRod || itemSubTypes == ItemSubTypes.FlyRod;
		}

		public bool IsReel(int itemId)
		{
			ItemSubTypes itemSubTypes = ((itemId >= 0) ? this.context.GetGameCacheAdapter().ResolveItemGloablly(itemId).ItemSubType : ((ItemSubTypes)(-(ItemSubTypes)itemId)));
			return itemSubTypes == ItemSubTypes.Reel || itemSubTypes == ItemSubTypes.SpinReel || itemSubTypes == ItemSubTypes.CastReel || itemSubTypes == ItemSubTypes.LineRunningReel || itemSubTypes == ItemSubTypes.FlyReel;
		}

		public bool IsLine(int itemId)
		{
			ItemSubTypes itemSubTypes = ((itemId >= 0) ? this.context.GetGameCacheAdapter().ResolveItemGloablly(itemId).ItemSubType : ((ItemSubTypes)(-(ItemSubTypes)itemId)));
			return itemSubTypes == ItemSubTypes.Line || itemSubTypes == ItemSubTypes.MonoLine || itemSubTypes == ItemSubTypes.BraidLine || itemSubTypes == ItemSubTypes.FlurLine;
		}

		public bool IsLeader(int itemId)
		{
			ItemSubTypes itemSubTypes = ((itemId >= 0) ? this.context.GetGameCacheAdapter().ResolveItemGloablly(itemId).ItemSubType : ((ItemSubTypes)(-(ItemSubTypes)itemId)));
			return itemSubTypes == ItemSubTypes.Leader || itemSubTypes == ItemSubTypes.MonoLeader || itemSubTypes == ItemSubTypes.FlurLeader || itemSubTypes == ItemSubTypes.BraidLeader || itemSubTypes == ItemSubTypes.SteelLeader || itemSubTypes == ItemSubTypes.CarpLeader;
		}

		public bool IsTackle(int itemId)
		{
			ItemTypes itemTypes;
			if (itemId < 0)
			{
				InventoryCategory inventoryCategory = this.context.GetGameCacheAdapter().ResolveInventoryCategory(-itemId);
				itemTypes = ((inventoryCategory.ParentCategoryId != null) ? ((ItemTypes)inventoryCategory.ParentCategoryId.Value) : ((ItemTypes)inventoryCategory.CategoryId));
			}
			else
			{
				itemTypes = this.context.GetGameCacheAdapter().ResolveItemGloablly(itemId).ItemType;
			}
			return itemTypes == ItemTypes.Bobber || itemTypes == ItemTypes.Hook || itemTypes == ItemTypes.Bait || itemTypes == ItemTypes.Lure || itemTypes == ItemTypes.JigHead || itemTypes == ItemTypes.JigBait || itemTypes == ItemTypes.Feeder || itemTypes == ItemTypes.Sinker || itemTypes == ItemTypes.Leader || itemTypes == ItemTypes.Bell;
		}

		public bool IsChum(int itemId)
		{
			ItemTypes itemTypes;
			if (itemId < 0)
			{
				InventoryCategory inventoryCategory = this.context.GetGameCacheAdapter().ResolveInventoryCategory(-itemId);
				itemTypes = ((inventoryCategory.ParentCategoryId != null) ? ((ItemTypes)inventoryCategory.ParentCategoryId.Value) : ((ItemTypes)inventoryCategory.CategoryId));
			}
			else
			{
				itemTypes = this.context.GetGameCacheAdapter().ResolveItemGloablly(itemId).ItemType;
			}
			return itemTypes == ItemTypes.ChumBase || itemTypes == ItemTypes.ChumAroma || itemTypes == ItemTypes.ChumParticle;
		}

		public static float GetItemMaxLoad(InventoryItem item)
		{
			Rod rod = item as Rod;
			if (rod != null)
			{
				return rod.MaxLoad;
			}
			Reel reel = item as Reel;
			if (reel != null)
			{
				return reel.MaxLoad;
			}
			Line line = item as Line;
			if (line != null)
			{
				return line.MaxLoad;
			}
			Leader leader = item as Leader;
			if (leader != null)
			{
				return leader.MaxLoad;
			}
			return 0f;
		}

		public InventoryItem GetItem(int itemId, int languageId)
		{
			return MissionInventoryContext.ItemFactoryFunc(itemId, languageId);
		}

		private MissionsContext context;

		public static Func<int, int, InventoryItem> ItemFactoryFunc;
	}
}
