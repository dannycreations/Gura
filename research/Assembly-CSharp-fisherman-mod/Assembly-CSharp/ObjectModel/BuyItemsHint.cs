using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	[JsonObject(1)]
	public class BuyItemsHint : BaseHint, IBuyItemsStorageResource
	{
		public bool BuyAnyway { get; set; }

		public bool ShouldAssembleRod { get; set; }

		[JsonProperty]
		public string RodResourceKey { get; set; }

		[JsonProperty]
		public int[][] ItemIds { get; set; }

		[JsonProperty]
		public bool OnlyGlobalShop { get; set; }

		[JsonProperty]
		[JsonConverter(typeof(StringEnumConverter))]
		public StoragePlaces? Storage { get; set; }

		[JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
		public StoragePlaces[] Storages { get; set; }

		[JsonProperty]
		public float? MinLoad { get; set; }

		[JsonProperty]
		public float? MaxLoad { get; set; }

		[JsonProperty]
		public int? LineLength { get; set; }

		[JsonProperty]
		public float? LineMinLoad { get; set; }

		[JsonProperty]
		public float? LineMaxLoad { get; set; }

		[JsonProperty]
		public float? LineMinThickness { get; set; }

		[JsonProperty]
		public float? LineMaxThickness { get; set; }

		[JsonProperty]
		public int? LeaderLength { get; set; }

		[JsonProperty]
		public float? LeaderMinLoad { get; set; }

		[JsonProperty]
		public float? LeaderMaxLoad { get; set; }

		[JsonProperty]
		public float? LeaderMinThickness { get; set; }

		[JsonProperty]
		public float? LeaderMaxThickness { get; set; }

		[JsonProperty]
		public float? ChumWeight { get; set; }

		internal IEnumerable<string> GetTranslationsOnClient()
		{
			return new string[]
			{
				"$MoveItemToEquipment", "$MoveItemToDoll", "$MoveItemTypeToEquipment", "$MoveItemTypeToDoll", "$BuyGlobalItem", "$BuyLocalItem", "$GotoHomeItemInStorage", "$GotoHomeBuyGlobalItem", "$BuyGlobalItemType", "$BuyLocalItemType",
				"$GotoHomeItemTypeInStorage", "$GotoHomeBuyGlobalItemType", "$CantBuyGlobalItem", "$CantBuyGlobalItemType", "$NoPlaceToBuyNewRodOnPond", "$NoPlaceToBuyNewReelOnPond", "$NoPlaceToBuyNewLineOnPond", "$NoPlaceToBuyNewTackleOnPond", "$NoPlaceToBuyNewChumOnPond", "$NoPlaceToBuyNewItemStorageOverloaded",
				"$NoPlaceToBuyNewRodOnGlobal", "$NoPlaceToBuyNewReelOnGlobal", "$NoPlaceToBuyNewLineOnGlobal", "$NoPlaceToBuyNewTackleOnGlobal", "$NoPlaceToBuyNewChumOnGlobal"
			};
		}

		private MissionInventoryFilter GetFilter(int index)
		{
			if (this.rodIndexes.Contains(index) || this.reelIndexes.Contains(index))
			{
				return new MissionInventoryFilter
				{
					MinLoad = this.MinLoad,
					MaxLoad = this.MaxLoad
				};
			}
			if (this.lineIndexes.Contains(index))
			{
				MissionInventoryFilter missionInventoryFilter = new MissionInventoryFilter();
				missionInventoryFilter.MinLoad = this.LineMinLoad;
				missionInventoryFilter.MaxLoad = this.LineMaxLoad;
				MissionInventoryFilter missionInventoryFilter2 = missionInventoryFilter;
				int? lineLength = this.LineLength;
				missionInventoryFilter2.LineLength = new int?((lineLength == null) ? 10 : lineLength.Value);
				missionInventoryFilter.LineMinThickness = this.LineMinThickness;
				missionInventoryFilter.LineMaxThickness = this.LineMaxThickness;
				return missionInventoryFilter;
			}
			if (this.leaderIndexes.Contains(index))
			{
				MissionInventoryFilter missionInventoryFilter = new MissionInventoryFilter();
				missionInventoryFilter.MinLoad = this.LeaderMinLoad;
				missionInventoryFilter.MaxLoad = this.LeaderMaxLoad;
				MissionInventoryFilter missionInventoryFilter3 = missionInventoryFilter;
				int? leaderLength = this.LeaderLength;
				missionInventoryFilter3.LeaderLength = new int?((leaderLength == null) ? 0 : leaderLength.Value);
				missionInventoryFilter.LeaderMinThickness = this.LeaderMinThickness;
				missionInventoryFilter.LeaderMaxThickness = this.LeaderMaxThickness;
				return missionInventoryFilter;
			}
			if (this.chumIndexes.Contains(index))
			{
				return new MissionInventoryFilter
				{
					ChumWeight = this.ChumWeight
				};
			}
			return null;
		}

		private Action<HintMessage> GetInitMessage(int index)
		{
			if (this.rodIndexes.Contains(index) || this.reelIndexes.Contains(index))
			{
				return delegate(HintMessage message)
				{
					if (this.Storage != null)
					{
						message.Storage = this.Storage.Value;
					}
					if (this.MinLoad != null)
					{
						message.MinLoad = this.MinLoad.Value;
					}
					if (this.MaxLoad != null)
					{
						message.MaxLoad = this.MaxLoad.Value;
					}
				};
			}
			if (this.lineIndexes.Contains(index))
			{
				return delegate(HintMessage message)
				{
					if (this.Storage != null)
					{
						message.Storage = this.Storage.Value;
					}
					if (this.LineLength != null)
					{
						message.Length = (float)this.LineLength.Value;
					}
					if (this.LineMinLoad != null)
					{
						message.MinLoad = this.LineMinLoad.Value;
					}
					if (this.LineMaxLoad != null)
					{
						message.MaxLoad = this.LineMaxLoad.Value;
					}
					if (this.LineMinThickness != null)
					{
						message.MinThickness = this.LineMinThickness.Value;
					}
					if (this.LineMaxThickness != null)
					{
						message.MaxThickness = this.LineMaxThickness.Value;
					}
				};
			}
			if (this.leaderIndexes.Contains(index))
			{
				return delegate(HintMessage message)
				{
					if (this.Storage != null)
					{
						message.Storage = this.Storage.Value;
					}
					if (this.LeaderLength != null)
					{
						message.Length = (float)this.LeaderLength.Value;
					}
					if (this.LeaderMinLoad != null)
					{
						message.MinLoad = this.LeaderMinLoad.Value;
					}
					if (this.LeaderMaxLoad != null)
					{
						message.MaxLoad = this.LeaderMaxLoad.Value;
					}
					if (this.LeaderMinThickness != null)
					{
						message.MinThickness = this.LeaderMinThickness.Value;
					}
					if (this.LeaderMaxThickness != null)
					{
						message.MaxThickness = this.LeaderMaxThickness.Value;
					}
				};
			}
			if (this.chumIndexes.Contains(index))
			{
				return delegate(HintMessage message)
				{
					if (this.Storage != null)
					{
						message.Storage = this.Storage.Value;
					}
					if (this.ChumWeight != null)
					{
						message.Weight = this.ChumWeight.Value;
					}
				};
			}
			return null;
		}

		public override IEnumerable<HintMessage> Check(MissionsContext context)
		{
			this.EnsureConfiguration(context);
			base.LastCheckResult = false;
			List<int> list = null;
			int index = 0;
			List<KeyValuePair<int, InventoryItem>> list2 = null;
			if (this.BuyAnyway)
			{
				list = this.ItemIds.Take(1).SelectMany((int[] a) => a).ToList<int>();
			}
			else
			{
				int assemblingSlot = this.RodResource.GetAssemblingSlot(context);
				for (int j = 0; j < this.ItemIds.Length; j++)
				{
					int[] array = this.ItemIds[j];
					if (array.Length != 0)
					{
						MissionInventoryFilter filter2 = this.GetFilter(j);
						var list3 = array.Select(delegate(int itemId)
						{
							MissionInventoryContext inventory = context.Inventory;
							BuyItemsHint $this = this;
							MissionInventoryFilter filter3 = filter2;
							int assemblingSlot2 = assemblingSlot;
							return new
							{
								itemId = itemId,
								existingItem = inventory.HasItemAtCurrentLocation(itemId, $this, filter3, false, assemblingSlot2),
								canMoveFromStorageItem = ((!context.GetProfile().Inventory.IsStorageAvailable() || (!(this.Storage == StoragePlaces.Equipment) && !(this.Storage == StoragePlaces.Doll))) ? null : context.Inventory.HasItemAtCurrentLocation(itemId, null, filter2, false, 0))
							};
						}).ToList();
						if (!list3.Any(e => e.existingItem != null))
						{
							list = (from e in list3
								where e.existingItem == null
								select e.itemId).ToList<int>();
							list2 = (from e in list3
								where e.existingItem == null && e.canMoveFromStorageItem != null
								select new KeyValuePair<int, InventoryItem>(e.itemId, e.canMoveFromStorageItem)).ToList<KeyValuePair<int, InventoryItem>>();
							index = j;
							break;
						}
					}
				}
			}
			MissionInventoryFilter filter = this.GetFilter(index);
			Action<HintMessage> initMessage = this.GetInitMessage(index);
			index *= 10;
			if (list == null || !list.Any<int>())
			{
				return new HintMessage[0];
			}
			base.LastCheckResult = true;
			if (list2 != null && list2.Any<KeyValuePair<int, InventoryItem>>())
			{
				string templateItem = ((!(this.Storage == StoragePlaces.Equipment)) ? "$MoveItemToDoll" : "$MoveItemToEquipment");
				string templateType = ((!(this.Storage == StoragePlaces.Equipment)) ? "$MoveItemTypeToDoll" : "$MoveItemTypeToEquipment");
				return list2.SelectMany(delegate(KeyValuePair<int, InventoryItem> i)
				{
					IEnumerable<HintMessage> enumerable;
					if (i.Key < 0)
					{
						BaseHint $this2 = this;
						MissionsContext missionsContext = context;
						string text = templateType;
						int num = -i.Key;
						int num2 = ++index;
						Action<HintMessage> action = initMessage;
						string text2 = i.Value.InstanceId.ToString();
						enumerable = $this2.ApplyTemplateForItemType(missionsContext, text, num, text2, null, HintItemClass.InventoryCategory, action, num2, null, null);
					}
					else
					{
						BaseHint $this3 = this;
						MissionsContext missionsContext = context;
						string text2 = templateItem;
						int num2 = i.Key;
						int num = ++index;
						Action<HintMessage> action = initMessage;
						string text = i.Value.InstanceId.ToString();
						enumerable = $this3.ApplyTemplateForItem(missionsContext, text2, num2, text, null, HintItemClass.InventoryItem, action, num, null, null);
					}
					return enumerable;
				});
			}
			if (context.PondId != 0)
			{
				if (!this.OnlyGlobalShop)
				{
					List<int> list4 = list.Where((int itemId) => context.GetGameCacheAdapter().IsInLocalShop(itemId, filter)).ToList<int>();
					if (list4.Any<int>())
					{
						List<int> list5 = ((context.AvailableRodSlots > 0) ? new List<int>() : list4.Where((int itemId) => context.Inventory.IsRod(itemId)).ToList<int>());
						if (list5.Any<int>())
						{
							return list5.SelectMany(delegate(int itemId)
							{
								IEnumerable<HintMessage> enumerable2;
								if (itemId < 0)
								{
									BaseHint $this4 = this;
									MissionsContext missionsContext2 = context;
									string text3 = "$NoPlaceToBuyNewRodOnPond";
									int num3 = -itemId;
									int num4 = ++index;
									Action<HintMessage> action2 = initMessage;
									enumerable2 = $this4.ApplyTemplateForItemType(missionsContext2, text3, num3, null, null, HintItemClass.InventoryCategory, action2, num4, null, null);
								}
								else
								{
									BaseHint $this5 = this;
									MissionsContext missionsContext2 = context;
									string text3 = "$NoPlaceToBuyNewRodOnPond";
									int num3 = ++index;
									Action<HintMessage> action2 = initMessage;
									enumerable2 = $this5.ApplyTemplateForItem(missionsContext2, text3, itemId, null, null, HintItemClass.InventoryItem, action2, num3, null, null);
								}
								return enumerable2;
							});
						}
						List<int> list6 = ((context.AvailableReelSlots > 0) ? new List<int>() : list4.Where((int itemId) => context.Inventory.IsReel(itemId)).ToList<int>());
						if (list6.Any<int>())
						{
							return list6.SelectMany(delegate(int itemId)
							{
								IEnumerable<HintMessage> enumerable3;
								if (itemId < 0)
								{
									BaseHint $this6 = this;
									MissionsContext missionsContext3 = context;
									string text4 = "$NoPlaceToBuyNewReelOnPond";
									int num5 = -itemId;
									int num6 = ++index;
									Action<HintMessage> action3 = initMessage;
									enumerable3 = $this6.ApplyTemplateForItemType(missionsContext3, text4, num5, null, null, HintItemClass.InventoryCategory, action3, num6, null, null);
								}
								else
								{
									BaseHint $this7 = this;
									MissionsContext missionsContext3 = context;
									string text4 = "$NoPlaceToBuyNewReelOnPond";
									int num5 = ++index;
									Action<HintMessage> action3 = initMessage;
									enumerable3 = $this7.ApplyTemplateForItem(missionsContext3, text4, itemId, null, null, HintItemClass.InventoryItem, action3, num5, null, null);
								}
								return enumerable3;
							});
						}
						List<int> list7 = ((context.AvailableLineSlots > 0) ? new List<int>() : list4.Where((int itemId) => context.Inventory.IsLine(itemId)).ToList<int>());
						if (list7.Any<int>())
						{
							return list7.SelectMany(delegate(int itemId)
							{
								IEnumerable<HintMessage> enumerable4;
								if (itemId < 0)
								{
									BaseHint $this8 = this;
									MissionsContext missionsContext4 = context;
									string text5 = "$NoPlaceToBuyNewLineOnPond";
									int num7 = -itemId;
									int num8 = ++index;
									Action<HintMessage> action4 = initMessage;
									enumerable4 = $this8.ApplyTemplateForItemType(missionsContext4, text5, num7, null, null, HintItemClass.InventoryCategory, action4, num8, null, null);
								}
								else
								{
									BaseHint $this9 = this;
									MissionsContext missionsContext4 = context;
									string text5 = "$NoPlaceToBuyNewLineOnPond";
									int num7 = ++index;
									Action<HintMessage> action4 = initMessage;
									enumerable4 = $this9.ApplyTemplateForItem(missionsContext4, text5, itemId, null, null, HintItemClass.InventoryItem, action4, num7, null, null);
								}
								return enumerable4;
							});
						}
						List<int> list8 = ((context.AvailableTackleSlots > 0) ? new List<int>() : list4.Where((int itemId) => context.Inventory.IsTackle(itemId)).ToList<int>());
						if (list8.Any<int>())
						{
							return list8.SelectMany(delegate(int itemId)
							{
								IEnumerable<HintMessage> enumerable5;
								if (itemId < 0)
								{
									BaseHint $this10 = this;
									MissionsContext missionsContext5 = context;
									string text6 = "$NoPlaceToBuyNewTackleOnPond";
									int num9 = -itemId;
									int num10 = ++index;
									Action<HintMessage> action5 = initMessage;
									enumerable5 = $this10.ApplyTemplateForItemType(missionsContext5, text6, num9, null, null, HintItemClass.InventoryCategory, action5, num10, null, null);
								}
								else
								{
									BaseHint $this11 = this;
									MissionsContext missionsContext5 = context;
									string text6 = "$NoPlaceToBuyNewTackleOnPond";
									int num9 = ++index;
									Action<HintMessage> action5 = initMessage;
									enumerable5 = $this11.ApplyTemplateForItem(missionsContext5, text6, itemId, null, null, HintItemClass.InventoryItem, action5, num9, null, null);
								}
								return enumerable5;
							});
						}
						List<int> list9 = ((context.AvailableChumSlots > 0) ? new List<int>() : list4.Where((int itemId) => context.Inventory.IsChum(itemId)).ToList<int>());
						if (list9.Any<int>())
						{
							return list9.SelectMany(delegate(int itemId)
							{
								IEnumerable<HintMessage> enumerable6;
								if (itemId < 0)
								{
									BaseHint $this12 = this;
									MissionsContext missionsContext6 = context;
									string text7 = "$NoPlaceToBuyNewChumOnPond";
									int num11 = -itemId;
									int num12 = ++index;
									Action<HintMessage> action6 = initMessage;
									enumerable6 = $this12.ApplyTemplateForItemType(missionsContext6, text7, num11, null, null, HintItemClass.InventoryCategory, action6, num12, null, null);
								}
								else
								{
									BaseHint $this13 = this;
									MissionsContext missionsContext6 = context;
									string text7 = "$NoPlaceToBuyNewChumOnPond";
									int num11 = ++index;
									Action<HintMessage> action6 = initMessage;
									enumerable6 = $this13.ApplyTemplateForItem(missionsContext6, text7, itemId, null, null, HintItemClass.InventoryItem, action6, num11, null, null);
								}
								return enumerable6;
							});
						}
						return list4.SelectMany(delegate(int itemId)
						{
							IEnumerable<HintMessage> enumerable7;
							if (itemId < 0)
							{
								BaseHint $this14 = this;
								MissionsContext missionsContext7 = context;
								string text8 = "$BuyLocalItemType";
								int num13 = -itemId;
								int num14 = ++index;
								Action<HintMessage> action7 = initMessage;
								enumerable7 = $this14.ApplyTemplateForItemType(missionsContext7, text8, num13, null, null, HintItemClass.InventoryCategory, action7, num14, null, null);
							}
							else
							{
								BaseHint $this15 = this;
								MissionsContext missionsContext7 = context;
								string text8 = "$BuyLocalItem";
								int num13 = ++index;
								Action<HintMessage> action7 = initMessage;
								enumerable7 = $this15.ApplyTemplateForItem(missionsContext7, text8, itemId, null, null, HintItemClass.InventoryItem, action7, num13, null, null);
							}
							return enumerable7;
						});
					}
				}
				if (!this.BuyAnyway)
				{
					var list10 = (from id in list
						select new
						{
							id = id,
							instance = context.Inventory.HasItemAtHome(id, filter)
						} into i
						where i.instance != null
						select i).ToList();
					if (list10.Any())
					{
						return list10.SelectMany(delegate(i)
						{
							IEnumerable<HintMessage> enumerable8;
							if (i.id < 0)
							{
								BaseHint $this16 = this;
								MissionsContext missionsContext8 = context;
								string text9 = "$GotoHomeItemTypeInStorage";
								int num15 = -i.id;
								int num16 = ++index;
								Action<HintMessage> action8 = initMessage;
								string text10 = i.instance.InstanceId.ToString();
								enumerable8 = $this16.ApplyTemplateForItemType(missionsContext8, text9, num15, text10, null, HintItemClass.InventoryCategory, action8, num16, null, null);
							}
							else
							{
								BaseHint $this17 = this;
								MissionsContext missionsContext8 = context;
								string text10 = "$GotoHomeItemInStorage";
								int num16 = i.id;
								int num15 = ++index;
								Action<HintMessage> action8 = initMessage;
								string text9 = i.instance.InstanceId.ToString();
								enumerable8 = $this17.ApplyTemplateForItem(missionsContext8, text10, num16, text9, null, HintItemClass.InventoryItem, action8, num15, null, null);
							}
							return enumerable8;
						});
					}
				}
			}
			List<int> list11 = list.Where((int itemId) => context.GetGameCacheAdapter().IsInGlobalShop(itemId, filter)).ToList<int>();
			if (!list11.Any<int>())
			{
				return list.SelectMany(delegate(int itemId)
				{
					IEnumerable<HintMessage> enumerable9;
					if (itemId < 0)
					{
						BaseHint $this18 = this;
						MissionsContext missionsContext9 = context;
						string text11 = "$CantBuyGlobalItemType";
						int num17 = -itemId;
						int num18 = ++index;
						Action<HintMessage> action9 = initMessage;
						enumerable9 = $this18.ApplyTemplateForItemType(missionsContext9, text11, num17, null, null, HintItemClass.InventoryCategory, action9, num18, null, null);
					}
					else
					{
						BaseHint $this19 = this;
						MissionsContext missionsContext9 = context;
						string text11 = "$CantBuyGlobalItem";
						int num17 = ++index;
						Action<HintMessage> action9 = initMessage;
						enumerable9 = $this19.ApplyTemplateForItem(missionsContext9, text11, itemId, null, null, HintItemClass.InventoryItem, action9, num17, null, null);
					}
					return enumerable9;
				});
			}
			if (context.GetProfile().Inventory.IsStorageOverloaded)
			{
				return list11.SelectMany(delegate(int itemId)
				{
					IEnumerable<HintMessage> enumerable10;
					if (itemId < 0)
					{
						BaseHint $this20 = this;
						MissionsContext missionsContext10 = context;
						string text12 = "$NoPlaceToBuyNewItemStorageOverloaded";
						int num19 = -itemId;
						int num20 = ++index;
						Action<HintMessage> action10 = initMessage;
						enumerable10 = $this20.ApplyTemplateForItemType(missionsContext10, text12, num19, null, null, HintItemClass.InventoryCategory, action10, num20, null, null);
					}
					else
					{
						BaseHint $this21 = this;
						MissionsContext missionsContext10 = context;
						string text12 = "$NoPlaceToBuyNewItemStorageOverloaded";
						int num19 = ++index;
						Action<HintMessage> action10 = initMessage;
						enumerable10 = $this21.ApplyTemplateForItem(missionsContext10, text12, itemId, null, null, HintItemClass.InventoryItem, action10, num19, null, null);
					}
					return enumerable10;
				});
			}
			return list11.SelectMany(delegate(int itemId)
			{
				IEnumerable<HintMessage> enumerable11;
				if (itemId < 0)
				{
					BaseHint $this22 = this;
					MissionsContext missionsContext11 = context;
					string text13 = ((context.PondId != 0) ? "$GotoHomeBuyGlobalItemType" : "$BuyGlobalItemType");
					int num21 = -itemId;
					int num22 = ++index;
					Action<HintMessage> action11 = initMessage;
					enumerable11 = $this22.ApplyTemplateForItemType(missionsContext11, text13, num21, null, null, HintItemClass.InventoryCategory, action11, num22, null, null);
				}
				else
				{
					BaseHint $this23 = this;
					MissionsContext missionsContext11 = context;
					string text13 = ((context.PondId != 0) ? "$GotoHomeBuyGlobalItem" : "$BuyGlobalItem");
					int num21 = ++index;
					Action<HintMessage> action11 = initMessage;
					enumerable11 = $this23.ApplyTemplateForItem(missionsContext11, text13, itemId, null, null, HintItemClass.InventoryItem, action11, num21, null, null);
				}
				return enumerable11;
			});
		}

		private void EnsureConfiguration(MissionsContext context)
		{
			if (this.configurationInitialized)
			{
				return;
			}
			if (this.RodResource != null && this.ItemIds == null)
			{
				this.ItemIds = this.RodResource.Where((int[] a) => a != null && a.Length > 0).ToArray<int[]>();
				this.rodIndexes.Clear();
				this.reelIndexes.Clear();
				this.lineIndexes.Clear();
				this.leaderIndexes.Clear();
				this.chumIndexes.Clear();
				for (int i = 0; i < this.ItemIds.Length; i++)
				{
					if (this.ItemIds[i].Any((int id) => context.Inventory.IsRod(id)))
					{
						this.rodIndexes.Add(i);
					}
					if (this.ItemIds[i].Any((int id) => context.Inventory.IsReel(id)))
					{
						this.reelIndexes.Add(i);
					}
					if (this.ItemIds[i].Any((int id) => context.Inventory.IsLine(id)))
					{
						this.lineIndexes.Add(i);
					}
					if (this.ItemIds[i].Any((int id) => context.Inventory.IsLeader(id)))
					{
						this.leaderIndexes.Add(i);
					}
					if (this.ItemIds[i].Any((int id) => context.Inventory.IsChum(id)))
					{
						this.chumIndexes.Add(i);
					}
				}
				this.OnlyGlobalShop = this.RodResource.OnlyGlobalShop;
				this.MinLoad = this.RodResource.MinLoad;
				this.MaxLoad = this.RodResource.MaxLoad;
				this.LineLength = this.RodResource.LineLength;
				this.LineMinLoad = this.RodResource.LineMinLoad;
				this.LineMaxLoad = this.RodResource.LineMaxLoad;
				this.LineMinThickness = this.RodResource.LineMinThickness;
				this.LineMaxThickness = this.RodResource.LineMaxThickness;
				this.LeaderLength = this.RodResource.LeaderLength;
				this.LeaderMinLoad = this.RodResource.LeaderMinLoad;
				this.LeaderMaxLoad = this.RodResource.LeaderMaxLoad;
				this.LeaderMinThickness = this.RodResource.LeaderMinThickness;
				this.LeaderMaxThickness = this.RodResource.LeaderMaxThickness;
				this.ChumWeight = this.RodResource.ChumWeight;
				IBuyItemsStorageResource buyItemsStorageResource = this.RodResource as IBuyItemsStorageResource;
				if (buyItemsStorageResource != null)
				{
					this.Storage = buyItemsStorageResource.Storage;
					this.Storages = buyItemsStorageResource.Storages;
				}
			}
			this.configurationInitialized = true;
		}

		public override object Clone()
		{
			BuyItemsHint buyItemsHint = (BuyItemsHint)base.Clone();
			buyItemsHint.RodResource = null;
			return buyItemsHint;
		}

		internal IBuyItemsResource RodResource;

		private HashSet<int> rodIndexes = new HashSet<int>();

		private HashSet<int> reelIndexes = new HashSet<int>();

		private HashSet<int> lineIndexes = new HashSet<int>();

		private HashSet<int> leaderIndexes = new HashSet<int>();

		private HashSet<int> chumIndexes = new HashSet<int>();

		public const string MoveItemToEquipment = "$MoveItemToEquipment";

		public const string MoveItemToDoll = "$MoveItemToDoll";

		public const string MoveItemTypeToEquipment = "$MoveItemTypeToEquipment";

		public const string MoveItemTypeToDoll = "$MoveItemTypeToDoll";

		public const string BuyGlobalItem = "$BuyGlobalItem";

		public const string BuyLocalItem = "$BuyLocalItem";

		public const string GotoHomeItemInStorage = "$GotoHomeItemInStorage";

		public const string GotoHomeBuyGlobalItem = "$GotoHomeBuyGlobalItem";

		public const string BuyGlobalItemType = "$BuyGlobalItemType";

		public const string BuyLocalItemType = "$BuyLocalItemType";

		public const string GotoHomeItemTypeInStorage = "$GotoHomeItemTypeInStorage";

		public const string GotoHomeBuyGlobalItemType = "$GotoHomeBuyGlobalItemType";

		public const string CantBuyGlobalItem = "$CantBuyGlobalItem";

		public const string CantBuyGlobalItemType = "$CantBuyGlobalItemType";

		public const string NoPlaceToBuyNewRodOnPond = "$NoPlaceToBuyNewRodOnPond";

		public const string NoPlaceToBuyNewReelOnPond = "$NoPlaceToBuyNewReelOnPond";

		public const string NoPlaceToBuyNewLineOnPond = "$NoPlaceToBuyNewLineOnPond";

		public const string NoPlaceToBuyNewTackleOnPond = "$NoPlaceToBuyNewTackleOnPond";

		public const string NoPlaceToBuyNewChumOnPond = "$NoPlaceToBuyNewChumOnPond";

		public const string NoPlaceToBuyNewItemStorageOverloaded = "$NoPlaceToBuyNewItemStorageOverloaded";

		public const string NoPlaceToBuyNewRodOnGlobal = "$NoPlaceToBuyNewRodOnGlobal";

		public const string NoPlaceToBuyNewReelOnGlobal = "$NoPlaceToBuyNewReelOnGlobal";

		public const string NoPlaceToBuyNewLineOnGlobal = "$NoPlaceToBuyNewLineOnGlobal";

		public const string NoPlaceToBuyNewTackleOnGlobal = "$NoPlaceToBuyNewTackleOnGlobal";

		public const string NoPlaceToBuyNewChumOnGlobal = "$NoPlaceToBuyNewChumOnGlobal";

		private bool configurationInitialized;
	}
}
