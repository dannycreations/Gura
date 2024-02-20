using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	[JsonObject(1)]
	public class AssembleRodHint : BaseHint
	{
		internal IEnumerable<string> GetTranslationsOnClient()
		{
			return new string[]
			{
				"$MoveItemToEquipment", "$MoveItemToDoll", "$MoveItemTypeToEquipment", "$MoveItemTypeToDoll", "$MoveRodOnDoll", "$MoveRodTypeOnDoll", "$MoveItemOnRod", "$MoveItemTypeOnRod", "$MoveItemOnRodLength", "$MoveItemTypeOnRodLength",
				"$MinLoadWrong", "$MaxLoadWrong", "$NoPlaceToEquipRodOnGlobal", "$NoPlaceToEquipReelOnGlobal", "$NoPlaceToEquipLineOnGlobal", "$NoPlaceToEquipTackleOnGlobal", "$NoPlaceToEquipChumOnGlobal"
			};
		}

		[JsonProperty]
		public string RodResourceKey { get; set; }

		[JsonProperty]
		public int RodId { get; set; }

		[JsonProperty]
		public int[] RodIds { get; set; }

		[JsonProperty]
		public int RodCategoryId { get; set; }

		[JsonProperty]
		public int[] RodCategoryIds { get; set; }

		[JsonProperty]
		public InventoryCondition RodCondition { get; set; }

		[JsonProperty]
		public int ReelId { get; set; }

		[JsonProperty]
		public int[] ReelIds { get; set; }

		[JsonProperty]
		public int ReelCategoryId { get; set; }

		[JsonProperty]
		public int[] ReelCategoryIds { get; set; }

		[JsonProperty]
		public InventoryCondition ReelCondition { get; set; }

		[JsonProperty]
		public int LineId { get; set; }

		[JsonProperty]
		public int[] LineIds { get; set; }

		[JsonProperty]
		public int LineCategoryId { get; set; }

		[JsonProperty]
		public int[] LineCategoryIds { get; set; }

		[JsonProperty]
		public InventoryCondition LineCondition { get; set; }

		[JsonProperty]
		public int BobberId { get; set; }

		[JsonProperty]
		public int[] BobberIds { get; set; }

		[JsonProperty]
		public int BobberCategoryId { get; set; }

		[JsonProperty]
		public int[] BobberCategoryIds { get; set; }

		[JsonProperty]
		public InventoryCondition BobberCondition { get; set; }

		[JsonProperty]
		public int HookId { get; set; }

		[JsonProperty]
		public int[] HookIds { get; set; }

		[JsonProperty]
		public int HookCategoryId { get; set; }

		[JsonProperty]
		public int[] HookCategoryIds { get; set; }

		[JsonProperty]
		public InventoryCondition HookCondition { get; set; }

		[JsonProperty]
		public int JigHeadId { get; set; }

		[JsonProperty]
		public int[] JigHeadIds { get; set; }

		[JsonProperty]
		public int JigHeadCategoryId { get; set; }

		[JsonProperty]
		public int[] JigHeadCategoryIds { get; set; }

		[JsonProperty]
		public InventoryCondition JigHeadCondition { get; set; }

		[JsonProperty]
		public int LureId { get; set; }

		[JsonProperty]
		public int[] LureIds { get; set; }

		[JsonProperty]
		public int LureCategoryId { get; set; }

		[JsonProperty]
		public int[] LureCategoryIds { get; set; }

		[JsonProperty]
		public InventoryCondition LureCondition { get; set; }

		[JsonProperty]
		public int BaitId { get; set; }

		[JsonProperty]
		public int[] BaitIds { get; set; }

		[JsonProperty]
		public int BaitCategoryId { get; set; }

		[JsonProperty]
		public int[] BaitCategoryIds { get; set; }

		[JsonProperty]
		public InventoryCondition BaitCondition { get; set; }

		[JsonProperty]
		public int JigBaitId { get; set; }

		[JsonProperty]
		public int[] JigBaitIds { get; set; }

		[JsonProperty]
		public int JigBaitCategoryId { get; set; }

		[JsonProperty]
		public int[] JigBaitCategoryIds { get; set; }

		[JsonProperty]
		public InventoryCondition JigBaitCondition { get; set; }

		[JsonProperty]
		public int FeederId { get; set; }

		[JsonProperty]
		public int[] FeederIds { get; set; }

		[JsonProperty]
		public int FeederCategoryId { get; set; }

		[JsonProperty]
		public int[] FeederCategoryIds { get; set; }

		[JsonProperty]
		public InventoryCondition FeederCondition { get; set; }

		[JsonProperty]
		public int SinkerId { get; set; }

		[JsonProperty]
		public int[] SinkerIds { get; set; }

		[JsonProperty]
		public int SinkerCategoryId { get; set; }

		[JsonProperty]
		public int[] SinkerCategoryIds { get; set; }

		[JsonProperty]
		public InventoryCondition SinkerCondition { get; set; }

		[JsonProperty]
		public int LeaderId { get; set; }

		[JsonProperty]
		public int[] LeaderIds { get; set; }

		[JsonProperty]
		public int LeaderCategoryId { get; set; }

		[JsonProperty]
		public int[] LeaderCategoryIds { get; set; }

		[JsonProperty]
		public InventoryCondition LeaderCondition { get; set; }

		[JsonProperty]
		public int ChumId { get; set; }

		[JsonProperty]
		public int[] ChumIds { get; set; }

		[JsonProperty]
		public int ChumCategoryId { get; set; }

		[JsonProperty]
		public int[] ChumCategoryIds { get; set; }

		[JsonProperty]
		public InventoryCondition ChumCondition { get; set; }

		[JsonProperty]
		public int BellId { get; set; }

		[JsonProperty]
		public int[] BellIds { get; set; }

		[JsonProperty]
		public int BellCategoryId { get; set; }

		[JsonProperty]
		public int[] BellCategoryIds { get; set; }

		[JsonProperty]
		public InventoryCondition BellCondition { get; set; }

		[JsonProperty]
		public int[] TackleIds { get; set; }

		[JsonConfig]
		public float? MinLoad { get; set; }

		[JsonConfig]
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

		[JsonProperty]
		[JsonConverter(typeof(StringEnumConverter))]
		public RodTemplate? RodTemplate { get; set; }

		[JsonProperty]
		public bool AllowIncomplete { get; set; }

		public Func<bool> ShouldGenerateHints { get; set; }

		private IEnumerable<HintMessage> CheckRodComponent(MissionsContext context, InventoryItem item, int itemId, int[] itemIds, InventoryCondition condition, int categoryId, int[] categoryIds, int parentCategoryId, Func<InventoryItem, bool> predicate, Action<HintMessage> initMessage, ref int orderOffset, MissionInventoryFilter filter = null, string itemTemplate = null, string itemTypeTemplate = null)
		{
			if (itemTemplate == null)
			{
				itemTemplate = "$MoveItemOnRod";
			}
			if (itemTypeTemplate == null)
			{
				itemTypeTemplate = "$MoveItemTypeOnRod";
			}
			int offset = orderOffset;
			InventoryItem inventoryItem = null;
			bool flag = itemId != 0 && (item.ItemId != itemId || (predicate != null && !predicate(item))) && (inventoryItem = context.Inventory.HasItemAtCurrentLocation(itemId, null, filter, false, 0)) != null;
			bool flag2 = itemIds != null && (itemIds.All((int id) => item.ItemId != id2) || (predicate != null && !predicate(item))) && itemIds.Any((int id) => (item.ItemId != id2 || (predicate != null && !predicate(item))) && context.Inventory.HasItemAtCurrentLocation(id2, null, filter, false, 0) != null);
			bool flag3 = condition != null && !condition.Match(item, false);
			InventoryItem inventoryItem2 = null;
			bool flag4 = categoryId != 0 && (((int)item.ItemSubType != categoryId && (int)item.ItemType != categoryId) || (predicate != null && !predicate(item))) && (inventoryItem2 = context.Inventory.HasItemOfTypeAtCurrentLocation(categoryId, null, filter, false, 0)) != null;
			bool flag5 = categoryIds != null && ((!categoryIds.Contains((int)item.ItemSubType) && !categoryIds.Contains((int)item.ItemType)) || (predicate != null && !predicate(item))) && categoryIds.Any((int id) => context.Inventory.HasItemOfTypeAtCurrentLocation(id2, null, filter, false, 0) != null);
			bool flag6 = parentCategoryId != 0 && (item.ItemId == 0 || (predicate != null && !predicate(item))) && itemId == 0 && (itemIds == null || itemIds.Length <= 0) && condition == null && categoryId == 0 && (categoryIds == null || categoryIds.Length <= 0);
			InventoryItem inventoryItem3 = ((!flag6) ? null : context.Inventory.HasItemOfTypeAtCurrentLocation(parentCategoryId, null, filter, false, 0));
			if ((flag || flag2 || flag3 || flag4 || flag5 || flag6) && (itemId == 0 || flag) && (itemIds == null || flag2) && (condition == null || flag3) && (categoryId == 0 || flag4) && (categoryIds == null || flag5))
			{
				IEnumerable<HintMessage> enumerable;
				if (flag)
				{
					MissionsContext missionsContext = context;
					string text = itemTemplate;
					Action<HintMessage> action = initMessage;
					string text2 = inventoryItem.InstanceId.ToString();
					enumerable = base.ApplyTemplateForItem(missionsContext, text, itemId, text2, new StoragePlaces?(inventoryItem.Storage), HintItemClass.InventoryItem, action, 10 * ++offset, null, null);
				}
				else
				{
					enumerable = new List<HintMessage>();
				}
				IEnumerable<HintMessage> enumerable2;
				if (flag2)
				{
					enumerable2 = (from id in itemIds
						select new
						{
							id = id2,
							instance = context.Inventory.HasItemAtCurrentLocation(id2, null, filter, false, 0)
						} into i
						where i.instance != null
						select i).SelectMany(delegate(i)
					{
						BaseHint $this = this;
						MissionsContext context2 = context;
						string itemTemplate2 = itemTemplate;
						int id2 = i.id;
						Action<HintMessage> initMessage2 = initMessage;
						string text3 = i.instance.InstanceId.ToString();
						return $this.ApplyTemplateForItem(context2, itemTemplate2, id2, text3, new StoragePlaces?(i.instance.Storage), HintItemClass.InventoryItem, initMessage2, 10 * ++offset, null, null);
					});
				}
				else
				{
					enumerable2 = new List<HintMessage>();
				}
				IEnumerable<HintMessage> enumerable3 = enumerable.Union(enumerable2);
				IEnumerable<HintMessage> enumerable4;
				if (flag4)
				{
					MissionsContext missionsContext = context;
					string text2 = itemTypeTemplate;
					Action<HintMessage> action = initMessage;
					string text = inventoryItem2.InstanceId.ToString();
					enumerable4 = base.ApplyTemplateForItemType(missionsContext, text2, categoryId, text, new StoragePlaces?(inventoryItem2.Storage), HintItemClass.InventoryCategory, action, 10 * ++offset, null, null);
				}
				else
				{
					enumerable4 = new List<HintMessage>();
				}
				IEnumerable<HintMessage> enumerable5;
				if (flag5)
				{
					enumerable5 = (from id in categoryIds
						select new
						{
							id = id,
							instance = context.Inventory.HasItemOfTypeAtCurrentLocation(id, null, filter, false, 0)
						} into i
						where i.instance != null
						select i).SelectMany(delegate(i)
					{
						BaseHint $this2 = this;
						MissionsContext context3 = context;
						string itemTypeTemplate2 = itemTypeTemplate;
						int id3 = i.id;
						Action<HintMessage> initMessage3 = initMessage;
						string text4 = i.instance.InstanceId.ToString();
						return $this2.ApplyTemplateForItemType(context3, itemTypeTemplate2, id3, text4, new StoragePlaces?(i.instance.Storage), HintItemClass.InventoryCategory, initMessage3, 10 * ++offset, null, null);
					});
				}
				else
				{
					enumerable5 = new List<HintMessage>();
				}
				IEnumerable<HintMessage> enumerable6 = enumerable4.Union(enumerable5);
				IEnumerable<HintMessage> enumerable7;
				if (flag6 && inventoryItem3 != null)
				{
					MissionsContext missionsContext = context;
					string text = itemTypeTemplate;
					Action<HintMessage> action = initMessage;
					string text2 = inventoryItem3.InstanceId.ToString();
					enumerable7 = base.ApplyTemplateForItemType(missionsContext, text, parentCategoryId, text2, new StoragePlaces?(inventoryItem3.Storage), HintItemClass.InventoryCategory, action, 10 * ++offset, null, null);
				}
				else
				{
					enumerable7 = new List<HintMessage>();
				}
				return enumerable3.Union(enumerable6.Union(enumerable7));
			}
			return null;
		}

		public override IEnumerable<HintMessage> Check(MissionsContext context)
		{
			this.EnsureConfiguration(context);
			if (this.ShouldGenerateHints != null && !this.ShouldGenerateHints())
			{
				return new HintMessage[0];
			}
			if (this.RodId == 0 && (this.RodIds == null || this.RodIds.Length <= 0) && this.RodCondition == null && this.RodCategoryId == 0 && (this.RodCategoryIds == null || this.RodCategoryIds.Length <= 0))
			{
				return new HintMessage[0];
			}
			int slot = this.RodResource.GetAssemblingSlot(context);
			MissionRodContext rod = context.Inventory.RodsOnDoll.Where((MissionRodContext r) => r.Rod.Slot == slot).FirstOrDefault<MissionRodContext>();
			Action<HintMessage> initRodAndSlot = delegate(HintMessage message)
			{
				message.RodId = rod.Rod.ItemId;
				message.Slot = rod.Rod.Slot;
			};
			Action<HintMessage> action = delegate(HintMessage message)
			{
				if (this.MinLoad != null)
				{
					message.MinLoad = this.MinLoad.Value;
				}
				if (this.MaxLoad != null)
				{
					message.MaxLoad = this.MaxLoad.Value;
				}
			};
			Action<HintMessage> action2 = delegate(HintMessage message)
			{
				initRodAndSlot(message);
				if (this.MinLoad != null)
				{
					message.MinLoad = this.MinLoad.Value;
				}
				if (this.MaxLoad != null)
				{
					message.MaxLoad = this.MaxLoad.Value;
				}
			};
			Action<HintMessage> action3 = delegate(HintMessage message)
			{
				initRodAndSlot(message);
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
			Action<HintMessage> action4 = delegate(HintMessage message)
			{
				initRodAndSlot(message);
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
			Action<HintMessage> action5 = delegate(HintMessage message)
			{
				initRodAndSlot(message);
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
				if (this.ChumWeight != null)
				{
					message.Weight = this.ChumWeight.Value;
				}
			};
			int offset = 0;
			if (rod == null)
			{
				MissionInventoryFilter missionInventoryFilter = new MissionInventoryFilter
				{
					ItemType = ItemTypes.Rod,
					MinLoad = this.MinLoad,
					MaxLoad = this.MaxLoad
				};
				IEnumerable<HintMessage> enumerable = this.CheckRodComponent(context, new Rod(), this.RodId, this.RodIds, this.RodCondition, this.RodCategoryId, this.RodCategoryIds, 0, null, action, ref offset, missionInventoryFilter, "$MoveRodOnDoll", "$MoveRodTypeOnDoll");
				if (enumerable != null)
				{
					return enumerable.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableRodSlots > 0) ? m : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipRodOnGlobal").Translate(null));
				}
			}
			else
			{
				if (rod.Rod.ItemId != 0 && this.MinLoad != null)
				{
					float? minLoad = this.MinLoad;
					if (!(rod.Rod.MaxLoad >= minLoad))
					{
						MissionsContext missionsContext = context;
						string text = "$MinLoadWrong";
						int num = rod.RodId;
						int num2 = 10 * ++offset;
						Action<HintMessage> action6 = action2;
						return base.ApplyTemplateForItem(missionsContext, text, num, null, null, HintItemClass.InventoryItem, action6, num2, null, null);
					}
				}
				if (rod.Rod.ItemId != 0 && this.MaxLoad != null)
				{
					float? maxLoad = this.MaxLoad;
					if (!(rod.Rod.MaxLoad <= maxLoad))
					{
						MissionsContext missionsContext = context;
						string text = "$MaxLoadWrong";
						int num2 = rod.RodId;
						int num = 10 * ++offset;
						Action<HintMessage> action6 = action2;
						return base.ApplyTemplateForItem(missionsContext, text, num2, null, null, HintItemClass.InventoryItem, action6, num, null, null);
					}
				}
				bool flag = rod.Rod.ItemSubType.IsRodWithBobber();
				bool flag2 = rod.Rod.ItemSubType.IsRodWithHook();
				bool flag3 = rod.Rod.ItemSubType.IsRodWithLure();
				bool flag4 = rod.Rod.ItemSubType.IsRodWithLeader();
				bool flag5 = rod.Rod.ItemSubType.IsRodWithFeeder();
				bool flag6 = rod.Rod.ItemSubType == ItemSubTypes.CastingRod;
				bool flag7 = rod.Rod.ItemSubType == ItemSubTypes.FeederRod;
				bool flag8 = rod.Rod.ItemSubType == ItemSubTypes.BottomRod;
				bool flag9 = rod.Rod.ItemSubType == ItemSubTypes.SpodRod;
				bool flag10 = rod.Hook.ItemId == 0;
				bool flag11 = rod.JigHead.ItemId != 0;
				bool flag12 = rod.Feeder.ItemId != 0;
				MissionInventoryFilter missionInventoryFilter2 = new MissionInventoryFilter
				{
					ItemType = ItemTypes.Reel,
					MinLoad = this.MinLoad,
					MaxLoad = this.MaxLoad
				};
				IEnumerable<HintMessage> enumerable2 = this.CheckRodComponent(context, rod.Reel, this.ReelId, this.ReelIds, this.ReelCondition, this.ReelCategoryId, this.ReelCategoryIds, (this.levelCondition < 1 && !this.shouldCompleteRod) ? 0 : ((!flag6) ? 30 : 32), null, action2, ref offset, missionInventoryFilter2, null, null);
				if (enumerable2 != null)
				{
					return enumerable2.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableReelSlots > 0 || rod.ReelId != 0) ? m : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipReelOnGlobal").Translate(null));
				}
				if (rod.Reel.ItemId != 0 && this.MinLoad != null)
				{
					float? minLoad2 = this.MinLoad;
					if (!(rod.Reel.MaxLoad >= minLoad2))
					{
						MissionsContext missionsContext = context;
						string text = "$MinLoadWrong";
						int num = rod.ReelId;
						int num2 = 10 * ++offset;
						Action<HintMessage> action6 = action2;
						return base.ApplyTemplateForItem(missionsContext, text, num, null, null, HintItemClass.InventoryItem, action6, num2, null, null);
					}
				}
				if (rod.Reel.ItemId != 0 && this.MaxLoad != null)
				{
					float? maxLoad2 = this.MaxLoad;
					if (!(rod.Reel.MaxLoad <= maxLoad2))
					{
						MissionsContext missionsContext = context;
						string text = "$MaxLoadWrong";
						int num2 = rod.ReelId;
						int num = 10 * ++offset;
						Action<HintMessage> action6 = action2;
						return base.ApplyTemplateForItem(missionsContext, text, num2, null, null, HintItemClass.InventoryItem, action6, num, null, null);
					}
				}
				Func<InventoryItem, bool> func = delegate(InventoryItem item)
				{
					if (this.LineLength != null && item is Line)
					{
						double? length = ((Line)item).Length;
						bool flag13 = length != null;
						int? lineLength2 = this.LineLength;
						double? num3 = ((lineLength2 == null) ? null : new double?((double)lineLength2.Value));
						if ((flag13 & (num3 != null)) && length.GetValueOrDefault() < num3.GetValueOrDefault())
						{
							return false;
						}
					}
					return true;
				};
				MissionInventoryFilter missionInventoryFilter3 = new MissionInventoryFilter();
				missionInventoryFilter3.ItemType = ItemTypes.Line;
				missionInventoryFilter3.MinLoad = this.LineMinLoad;
				missionInventoryFilter3.MaxLoad = this.LineMaxLoad;
				MissionInventoryFilter missionInventoryFilter4 = missionInventoryFilter3;
				int? lineLength = this.LineLength;
				missionInventoryFilter4.LineLength = new int?((lineLength == null) ? 10 : lineLength.Value);
				missionInventoryFilter3.LineMinThickness = this.LineMinThickness;
				missionInventoryFilter3.LineMaxThickness = this.LineMaxThickness;
				MissionInventoryFilter missionInventoryFilter5 = missionInventoryFilter3;
				IEnumerable<HintMessage> enumerable3 = this.CheckRodComponent(context, rod.Line, this.LineId, this.LineIds, this.LineCondition, this.LineCategoryId, this.LineCategoryIds, (this.levelCondition < 2 && !this.shouldCompleteRod) ? 0 : 5, func, action3, ref offset, missionInventoryFilter5, (this.LineLength == null) ? "$MoveItemOnRod" : "$MoveItemOnRodLength", (this.LineLength == null) ? "$MoveItemTypeOnRod" : "$MoveItemTypeOnRodLength");
				if (enumerable3 != null)
				{
					return enumerable3.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableLineSlots > 0 || rod.LineId != 0) ? m : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipLineOnGlobal").Translate(null));
				}
				if (rod.Line.ItemId != 0 && this.LineMinLoad != null)
				{
					float? lineMinLoad = this.LineMinLoad;
					if (!(rod.Line.MaxLoad >= lineMinLoad))
					{
						MissionsContext missionsContext = context;
						string text = "$MinLoadWrong";
						int num = rod.LineId;
						int num2 = 10 * ++offset;
						Action<HintMessage> action6 = action3;
						return base.ApplyTemplateForItem(missionsContext, text, num, null, null, HintItemClass.InventoryItem, action6, num2, null, null);
					}
				}
				if (rod.Line.ItemId != 0 && this.LineMaxLoad != null)
				{
					float? lineMaxLoad = this.LineMaxLoad;
					if (!(rod.Line.MaxLoad <= lineMaxLoad))
					{
						MissionsContext missionsContext = context;
						string text = "$MaxLoadWrong";
						int num2 = rod.LineId;
						int num = 10 * ++offset;
						Action<HintMessage> action6 = action3;
						return base.ApplyTemplateForItem(missionsContext, text, num2, null, null, HintItemClass.InventoryItem, action6, num, null, null);
					}
				}
				IEnumerable<HintMessage> enumerable4 = this.CheckRodComponent(context, rod.Bell, this.BellId, this.BellIds, this.BellCondition, this.BellCategoryId, this.BellCategoryIds, 0, null, initRodAndSlot, ref offset, null, null, null);
				if (enumerable4 != null)
				{
					return enumerable4.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableTackleSlots > 0 || rod.BellId != 0) ? m : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipTackleOnGlobal").Translate(null));
				}
				IEnumerable<HintMessage> enumerable5 = this.CheckRodComponent(context, rod.Bobber, this.BobberId, this.BobberIds, this.BobberCondition, this.BobberCategoryId, this.BobberCategoryIds, ((this.levelCondition < 3 && !this.shouldCompleteRod) || !flag) ? 0 : 7, null, initRodAndSlot, ref offset, null, null, null);
				if (enumerable5 != null)
				{
					return enumerable5.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableTackleSlots > 0 || rod.BobberId != 0) ? m : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipTackleOnGlobal").Translate(null));
				}
				IEnumerable<HintMessage> enumerable6 = this.CheckRodComponent(context, rod.Feeder, this.FeederId, this.FeederIds, this.FeederCondition, this.FeederCategoryId, this.FeederCategoryIds, ((this.levelCondition < 4 && !this.shouldCompleteRod) || !flag5 || (!this.HasAnyChumCondition && (!flag7 || this.HasAnySinkerCondition))) ? 0 : 9, null, initRodAndSlot, ref offset, null, null, null);
				if (enumerable6 != null)
				{
					return enumerable6.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableTackleSlots > 0 || rod.FeederId != 0) ? m : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipTackleOnGlobal").Translate(null));
				}
				IEnumerable<HintMessage> enumerable7 = this.CheckRodComponent(context, rod.Sinker, this.SinkerId, this.SinkerIds, this.SinkerCondition, this.SinkerCategoryId, this.SinkerCategoryIds, ((this.levelCondition < 6 && !this.shouldCompleteRod) || !flag8 || this.HasAnyFeederCondition || this.HasAnyChumCondition) ? 0 : 8, null, initRodAndSlot, ref offset, null, null, null);
				if (enumerable7 != null)
				{
					return enumerable7.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableTackleSlots > 0 || rod.SinkerId != 0) ? m : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipTackleOnGlobal").Translate(null));
				}
				Func<InventoryItem, bool> func2 = delegate(InventoryItem item)
				{
					if (this.LeaderLength != null && item is Leader)
					{
						int? leaderLength2 = this.LeaderLength;
						float? num4 = ((leaderLength2 == null) ? null : new float?((float)leaderLength2.Value));
						if (((Leader)item).LeaderLength < num4)
						{
							return false;
						}
					}
					return true;
				};
				missionInventoryFilter3 = new MissionInventoryFilter();
				missionInventoryFilter3.ItemType = ItemTypes.Leader;
				missionInventoryFilter3.MinLoad = this.LeaderMinLoad;
				missionInventoryFilter3.MaxLoad = this.LeaderMaxLoad;
				MissionInventoryFilter missionInventoryFilter6 = missionInventoryFilter3;
				int? leaderLength = this.LeaderLength;
				missionInventoryFilter6.LeaderLength = new int?((leaderLength == null) ? 0 : leaderLength.Value);
				missionInventoryFilter3.LeaderMinThickness = this.LeaderMinThickness;
				missionInventoryFilter3.LeaderMaxThickness = this.LeaderMaxThickness;
				MissionInventoryFilter missionInventoryFilter7 = missionInventoryFilter3;
				IEnumerable<HintMessage> enumerable8 = this.CheckRodComponent(context, rod.Leader, this.LeaderId, this.LeaderIds, this.LeaderCondition, this.LeaderCategoryId, this.LeaderCategoryIds, ((this.levelCondition < 7 && !this.shouldCompleteRod) || !flag4) ? 0 : 66, func2, action4, ref offset, missionInventoryFilter7, (this.LeaderLength == null) ? "$MoveItemOnRod" : "$MoveItemOnRodLength", (this.LeaderLength == null) ? "$MoveItemTypeOnRod" : "$MoveItemTypeOnRodLength");
				if (enumerable8 != null)
				{
					return enumerable8.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableTackleSlots > 0 || rod.LeaderId != 0) ? m.MoveToEquipmentMessage() : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipTackleOnGlobal").Translate(null)).EquipMessagesFirst();
				}
				if (rod.Leader.ItemId != 0 && this.LeaderMinLoad != null)
				{
					float? leaderMinLoad = this.LeaderMinLoad;
					if (!(rod.Leader.MaxLoad >= leaderMinLoad))
					{
						MissionsContext missionsContext = context;
						string text = "$MinLoadWrong";
						int num = rod.LeaderId;
						int num2 = 10 * ++offset;
						Action<HintMessage> action6 = action4;
						return base.ApplyTemplateForItem(missionsContext, text, num, null, null, HintItemClass.InventoryItem, action6, num2, null, null);
					}
				}
				if (rod.Leader.ItemId != 0 && this.LeaderMaxLoad != null)
				{
					float? leaderMaxLoad = this.LeaderMaxLoad;
					if (!(rod.Leader.MaxLoad <= leaderMaxLoad))
					{
						MissionsContext missionsContext = context;
						string text = "$MaxLoadWrong";
						int num2 = rod.LeaderId;
						int num = 10 * ++offset;
						Action<HintMessage> action6 = action4;
						return base.ApplyTemplateForItem(missionsContext, text, num2, null, null, HintItemClass.InventoryItem, action6, num, null, null);
					}
				}
				IEnumerable<HintMessage> enumerable9 = this.CheckRodComponent(context, rod.Hook, this.HookId, this.HookIds, this.HookCondition, this.HookCategoryId, this.HookCategoryIds, ((this.levelCondition < 8 && !this.shouldCompleteRod) || !flag2) ? 0 : 6, null, initRodAndSlot, ref offset, null, null, null);
				if (enumerable9 != null)
				{
					return enumerable9.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableTackleSlots > 0 || rod.HookId != 0) ? m.MoveToEquipmentMessage() : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipTackleOnGlobal").Translate(null)).EquipMessagesFirst();
				}
				if ((this.levelCondition >= 8 || this.shouldCompleteRod) && flag3 && flag10 && !this.HasAnyLureCondition && !this.HasAnyJigHeadCondition && !this.HasAnyJigBaitCondition)
				{
					MissionsContext missionsContext = context;
					string text = "$MoveItemTypeOnRod";
					int num = 35;
					Action<HintMessage> action6 = initRodAndSlot;
					int num2 = 10 * ++offset;
					IEnumerable<HintMessage> enumerable10 = base.ApplyTemplateForItemType(missionsContext, text, num, null, null, HintItemClass.InventoryCategory, action6, num2, null, null);
					missionsContext = context;
					text = "$MoveItemTypeOnRod";
					num2 = 11;
					action6 = initRodAndSlot;
					num = 10 * ++offset;
					IEnumerable<HintMessage> enumerable11 = base.ApplyTemplateForItemType(missionsContext, text, num2, null, null, HintItemClass.InventoryCategory, action6, num, null, null);
					return enumerable10.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableTackleSlots > 0 || rod.JigHeadId != 0) ? m.MoveToEquipmentMessage() : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipTackleOnGlobal").Translate(null)).Union(enumerable11.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableTackleSlots > 0 || rod.LureId != 0) ? m.MoveToEquipmentMessage() : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipTackleOnGlobal").Translate(null))).EquipMessagesFirst();
				}
				IEnumerable<HintMessage> enumerable12 = this.CheckRodComponent(context, rod.JigHead, this.JigHeadId, this.JigHeadIds, this.JigHeadCondition, this.JigHeadCategoryId, this.JigHeadCategoryIds, ((this.levelCondition < 8 && !this.shouldCompleteRod) || !flag3 || (!this.HasAnyJigBaitCondition && (!flag10 || this.HasAnyLureCondition))) ? 0 : 35, null, initRodAndSlot, ref offset, null, null, null);
				if (enumerable12 != null)
				{
					return enumerable12.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableTackleSlots > 0 || rod.JigHeadId != 0) ? m.MoveToEquipmentMessage() : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipTackleOnGlobal").Translate(null)).EquipMessagesFirst();
				}
				IEnumerable<HintMessage> enumerable13 = this.CheckRodComponent(context, rod.Lure, this.LureId, this.LureIds, this.LureCondition, this.LureCategoryId, this.LureCategoryIds, ((this.levelCondition < 8 && !this.shouldCompleteRod) || !flag3 || !flag10 || this.HasAnyJigHeadCondition || this.HasAnyJigBaitCondition) ? 0 : 11, null, initRodAndSlot, ref offset, null, null, null);
				if (enumerable13 != null)
				{
					return enumerable13.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableTackleSlots > 0 || rod.LureId != 0) ? m.MoveToEquipmentMessage() : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipTackleOnGlobal").Translate(null)).EquipMessagesFirst();
				}
				IEnumerable<HintMessage> enumerable14 = this.CheckRodComponent(context, rod.Bait, this.BaitId, this.BaitIds, this.BaitCondition, this.BaitCategoryId, this.BaitCategoryIds, ((this.levelCondition < 9 && !this.shouldCompleteRod) || !flag2) ? 0 : 10, null, initRodAndSlot, ref offset, null, null, null);
				if (enumerable14 != null)
				{
					return enumerable14.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableTackleSlots > 0 || rod.BaitId != 0) ? m.MoveToEquipmentMessage() : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipTackleOnGlobal").Translate(null)).EquipMessagesFirst();
				}
				IEnumerable<HintMessage> enumerable15 = this.CheckRodComponent(context, rod.JigBait, this.JigBaitId, this.JigBaitIds, this.JigBaitCondition, this.JigBaitCategoryId, this.JigBaitCategoryIds, ((this.levelCondition < 9 && !this.shouldCompleteRod) || !flag3 || !flag11) ? 0 : 12, null, initRodAndSlot, ref offset, null, null, null);
				if (enumerable15 != null)
				{
					return enumerable15.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableTackleSlots > 0 || rod.JigBaitId != 0) ? m.MoveToEquipmentMessage() : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipTackleOnGlobal").Translate(null)).EquipMessagesFirst();
				}
				Func<InventoryItem, bool> func3 = delegate(InventoryItem item)
				{
					if (this.ChumWeight != null && (item is Chum || item is ChumIngredient))
					{
						float? chumWeight = this.ChumWeight;
						if (item.Amount < chumWeight)
						{
							return false;
						}
					}
					return true;
				};
				MissionInventoryFilter missionInventoryFilter8 = new MissionInventoryFilter
				{
					ItemType = ItemTypes.Chum,
					ChumWeight = this.ChumWeight
				};
				IEnumerable<HintMessage> enumerable16 = this.CheckRodComponent(context, rod.Chum, this.ChumId, this.ChumIds, this.ChumCondition, this.ChumCategoryId, this.ChumCategoryIds, 0, func3, action5, ref offset, missionInventoryFilter8, null, null);
				if (enumerable16 != null)
				{
					return enumerable16.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableChumSlots > 0 || rod.ChumId != 0) ? m.MoveToEquipmentMessage() : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipChumOnGlobal").Translate(null)).EquipMessagesFirst();
				}
				if (this.TackleIds != null && this.TackleIds.Length > 0)
				{
					IEnumerable<HintMessage> enumerable17 = (from id in this.TackleIds
						select new
						{
							id = id,
							instance = context.Inventory.HasItemAtCurrentLocation(id, null, null, false, 0)
						} into i
						where i.instance != null
						select i).SelectMany(delegate(i)
					{
						BaseHint $this = this;
						MissionsContext context2 = context;
						string text2 = "$MoveItemOnRod";
						int id = i.id;
						Action<HintMessage> initRodAndSlot2 = initRodAndSlot;
						int num5 = 10 * ++offset;
						string text3 = i.instance.InstanceId.ToString();
						return $this.ApplyTemplateForItem(context2, text2, id, text3, null, HintItemClass.InventoryItem, initRodAndSlot2, num5, null, null);
					});
					if (enumerable17 != null)
					{
						return enumerable17.Select((HintMessage m) => (m.SourceStorage != StoragePlaces.Storage || context.PondId != 0 || context.AvailableTackleSlots > 0) ? m.MoveToEquipmentMessage() : m.SetMessageIdAsCodeItemId(null, "$NoPlaceToEquipTackleOnGlobal").Translate(null)).EquipMessagesFirst();
					}
				}
			}
			return new HintMessage[0];
		}

		private bool HasAnyReelCondition
		{
			get
			{
				return this.ReelId != 0 || (this.ReelIds != null && this.ReelIds.Length > 0) || this.ReelCondition != null || this.ReelCategoryId != 0 || (this.ReelCategoryIds != null && this.ReelCategoryIds.Length > 0);
			}
		}

		private bool HasAnyLineCondition
		{
			get
			{
				return this.LineId != 0 || (this.LineIds != null && this.LineIds.Length > 0) || this.LineCondition != null || this.LineCategoryId != 0 || (this.LineCategoryIds != null && this.LineCategoryIds.Length > 0) || this.LineMinLoad != null || this.LineMaxLoad != null || this.LineLength != null || this.LineMinThickness != null || this.LineMaxThickness != null;
			}
		}

		private bool HasAnyBobberCondition
		{
			get
			{
				return this.BobberId != 0 || (this.BobberIds != null && this.BobberIds.Length > 0) || this.BobberCondition != null || this.BobberCategoryId != 0 || (this.BobberCategoryIds != null && this.BobberCategoryIds.Length > 0);
			}
		}

		private bool HasAnyHookCondition
		{
			get
			{
				return this.HookId != 0 || (this.HookIds != null && this.HookIds.Length > 0) || this.HookCondition != null || this.HookCategoryId != 0 || (this.HookCategoryIds != null && this.HookCategoryIds.Length > 0);
			}
		}

		private bool HasAnyLureCondition
		{
			get
			{
				return this.LureId != 0 || (this.LureIds != null && this.LureIds.Length > 0) || this.LureCondition != null || this.LureCategoryId != 0 || (this.LureCategoryIds != null && this.LureCategoryIds.Length > 0);
			}
		}

		private bool HasAnyJigHeadCondition
		{
			get
			{
				return this.JigHeadId != 0 || (this.JigHeadIds != null && this.JigHeadIds.Length > 0) || this.JigHeadCondition != null || this.JigHeadCategoryId != 0 || (this.JigHeadCategoryIds != null && this.JigHeadCategoryIds.Length > 0);
			}
		}

		private bool HasAnyBaitCondition
		{
			get
			{
				return this.BaitId != 0 || (this.BaitIds != null && this.BaitIds.Length > 0) || this.BaitCondition != null || this.BaitCategoryId != 0 || (this.BaitCategoryIds != null && this.BaitCategoryIds.Length > 0);
			}
		}

		private bool HasAnyJigBaitCondition
		{
			get
			{
				return this.JigBaitId != 0 || (this.JigBaitIds != null && this.JigBaitIds.Length > 0) || this.JigBaitCondition != null || this.JigBaitCategoryId != 0 || (this.JigBaitCategoryIds != null && this.JigBaitCategoryIds.Length > 0);
			}
		}

		private bool HasAnyFeederCondition
		{
			get
			{
				return this.FeederId != 0 || (this.FeederIds != null && this.FeederIds.Length > 0) || this.FeederCondition != null || this.FeederCategoryId != 0 || (this.FeederCategoryIds != null && this.FeederCategoryIds.Length > 0);
			}
		}

		private bool HasAnySinkerCondition
		{
			get
			{
				return this.SinkerId != 0 || (this.SinkerIds != null && this.SinkerIds.Length > 0) || this.SinkerCondition != null || this.SinkerCategoryId != 0 || (this.SinkerCategoryIds != null && this.SinkerCategoryIds.Length > 0);
			}
		}

		private bool HasAnyLeaderCondition
		{
			get
			{
				return this.LeaderId != 0 || (this.LeaderIds != null && this.LeaderIds.Length > 0) || this.LeaderCondition != null || this.LeaderCategoryId != 0 || (this.LeaderCategoryIds != null && this.LeaderCategoryIds.Length > 0) || this.LeaderMinLoad != null || this.LeaderMaxLoad != null || this.LeaderLength != null || this.LeaderMinThickness != null || this.LeaderMaxThickness != null;
			}
		}

		private bool HasAnyChumCondition
		{
			get
			{
				return this.ChumId != 0 || (this.ChumIds != null && this.ChumIds.Length > 0) || this.ChumCondition != null || this.ChumCategoryId != 0 || (this.ChumCategoryIds != null && this.ChumCategoryIds.Length > 0) || this.ChumWeight != null;
			}
		}

		private bool HasAnyBellCondition
		{
			get
			{
				return this.BellId != 0 || (this.BellIds != null && this.BellIds.Length > 0) || this.BellCondition != null || this.BellCategoryId != 0 || (this.BellCategoryIds != null && this.BellCategoryIds.Length > 0);
			}
		}

		private void InitConditionLevelConfiguration(MissionsContext context)
		{
			this.levelCondition = 0;
			if (this.HasAnyReelCondition)
			{
				this.levelCondition = 1;
			}
			if (this.HasAnyLineCondition)
			{
				this.levelCondition = 2;
			}
			if (this.HasAnyBobberCondition)
			{
				this.levelCondition = 3;
			}
			if (this.HasAnyFeederCondition)
			{
				this.levelCondition = 4;
			}
			if (this.HasAnyChumCondition)
			{
				this.levelCondition = 5;
			}
			if (this.HasAnySinkerCondition)
			{
				this.levelCondition = 6;
			}
			if (this.HasAnyLeaderCondition)
			{
				this.levelCondition = 7;
			}
			if (this.HasAnyHookCondition || this.HasAnyLureCondition || this.HasAnyJigHeadCondition)
			{
				this.levelCondition = 8;
			}
			if (this.HasAnyBaitCondition || this.HasAnyJigBaitCondition)
			{
				this.levelCondition = 9;
			}
			this.shouldCompleteRod = (this.RodTemplate != null && this.RodTemplate != ObjectModel.RodTemplate.UnEquiped) || !this.AllowIncomplete;
		}

		private void EnsureConfiguration(MissionsContext context)
		{
			if (this.configurationInitialized)
			{
				return;
			}
			if (this.RodResource != null && this.RodId == 0)
			{
				this.RodId = this.RodResource.RodId;
				this.RodIds = this.RodResource.RodIds;
				this.RodCategoryId = this.RodResource.RodCategoryId;
				this.RodCategoryIds = this.RodResource.RodCategoryIds;
				this.RodCondition = this.RodResource.RodCondition;
				this.ReelId = this.RodResource.ReelId;
				this.ReelIds = this.RodResource.ReelIds;
				this.ReelCategoryId = this.RodResource.ReelCategoryId;
				this.ReelCategoryIds = this.RodResource.ReelCategoryIds;
				this.ReelCondition = this.RodResource.ReelCondition;
				this.LineId = this.RodResource.LineId;
				this.LineIds = this.RodResource.LineIds;
				this.LineCategoryId = this.RodResource.LineCategoryId;
				this.LineCategoryIds = this.RodResource.LineCategoryIds;
				this.LineCondition = this.RodResource.LineCondition;
				this.BobberId = this.RodResource.BobberId;
				this.BobberIds = this.RodResource.BobberIds;
				this.BobberCategoryId = this.RodResource.BobberCategoryId;
				this.BobberCategoryIds = this.RodResource.BobberCategoryIds;
				this.BobberCondition = this.RodResource.BobberCondition;
				this.HookId = this.RodResource.HookId;
				this.HookIds = this.RodResource.HookIds;
				this.HookCategoryId = this.RodResource.HookCategoryId;
				this.HookCategoryIds = this.RodResource.HookCategoryIds;
				this.HookCondition = this.RodResource.HookCondition;
				this.JigHeadId = this.RodResource.JigHeadId;
				this.JigHeadIds = this.RodResource.JigHeadIds;
				this.JigHeadCategoryId = this.RodResource.JigHeadCategoryId;
				this.JigHeadCategoryIds = this.RodResource.JigHeadCategoryIds;
				this.JigHeadCondition = this.RodResource.JigHeadCondition;
				this.LureId = this.RodResource.LureId;
				this.LureIds = this.RodResource.LureIds;
				this.LureCategoryId = this.RodResource.LureCategoryId;
				this.LureCategoryIds = this.RodResource.LureCategoryIds;
				this.LureCondition = this.RodResource.LureCondition;
				this.BaitId = this.RodResource.BaitId;
				this.BaitIds = this.RodResource.BaitIds;
				this.BaitCategoryId = this.RodResource.BaitCategoryId;
				this.BaitCategoryIds = this.RodResource.BaitCategoryIds;
				this.BaitCondition = this.RodResource.BaitCondition;
				this.JigBaitId = this.RodResource.JigBaitId;
				this.JigBaitIds = this.RodResource.JigBaitIds;
				this.JigBaitCategoryId = this.RodResource.JigBaitCategoryId;
				this.JigBaitCategoryIds = this.RodResource.JigBaitCategoryIds;
				this.JigBaitCondition = this.RodResource.JigBaitCondition;
				this.FeederId = this.RodResource.FeederId;
				this.FeederIds = this.RodResource.FeederIds;
				this.FeederCategoryId = this.RodResource.FeederCategoryId;
				this.FeederCategoryIds = this.RodResource.FeederCategoryIds;
				this.FeederCondition = this.RodResource.FeederCondition;
				this.SinkerId = this.RodResource.SinkerId;
				this.SinkerIds = this.RodResource.SinkerIds;
				this.SinkerCategoryId = this.RodResource.SinkerCategoryId;
				this.SinkerCategoryIds = this.RodResource.SinkerCategoryIds;
				this.SinkerCondition = this.RodResource.SinkerCondition;
				this.LeaderId = this.RodResource.LeaderId;
				this.LeaderIds = this.RodResource.LeaderIds;
				this.LeaderCategoryId = this.RodResource.LeaderCategoryId;
				this.LeaderCategoryIds = this.RodResource.LeaderCategoryIds;
				this.LeaderCondition = this.RodResource.LeaderCondition;
				this.ChumId = this.RodResource.ChumId;
				this.ChumIds = this.RodResource.ChumIds;
				this.ChumCategoryId = this.RodResource.ChumCategoryId;
				this.ChumCategoryIds = this.RodResource.ChumCategoryIds;
				this.ChumCondition = this.RodResource.ChumCondition;
				this.BellId = this.RodResource.BellId;
				this.BellIds = this.RodResource.BellIds;
				this.BellCategoryId = this.RodResource.BellCategoryId;
				this.BellCategoryIds = this.RodResource.BellCategoryIds;
				this.BellCondition = this.RodResource.BellCondition;
				this.TackleIds = this.RodResource.TackleIds;
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
				this.RodTemplate = this.RodResource.RodTemplate;
				this.AllowIncomplete = this.RodResource.AllowIncomplete;
			}
			this.InitConditionLevelConfiguration(context);
			this.configurationInitialized = true;
		}

		public override object Clone()
		{
			AssembleRodHint assembleRodHint = (AssembleRodHint)base.Clone();
			assembleRodHint.RodResource = null;
			return assembleRodHint;
		}

		public const string MoveItemToEquipment = "$MoveItemToEquipment";

		public const string MoveItemToDoll = "$MoveItemToDoll";

		public const string MoveItemTypeToEquipment = "$MoveItemTypeToEquipment";

		public const string MoveItemTypeToDoll = "$MoveItemTypeToDoll";

		public const string MoveRodOnDoll = "$MoveRodOnDoll";

		public const string MoveRodTypeOnDoll = "$MoveRodTypeOnDoll";

		public const string MoveItemOnRod = "$MoveItemOnRod";

		public const string MoveItemTypeOnRod = "$MoveItemTypeOnRod";

		public const string MoveItemOnRodLength = "$MoveItemOnRodLength";

		public const string MoveItemTypeOnRodLength = "$MoveItemTypeOnRodLength";

		public const string MinLoadWrong = "$MinLoadWrong";

		public const string MaxLoadWrong = "$MaxLoadWrong";

		public const string NoPlaceToEquipRodOnGlobal = "$NoPlaceToEquipRodOnGlobal";

		public const string NoPlaceToEquipReelOnGlobal = "$NoPlaceToEquipReelOnGlobal";

		public const string NoPlaceToEquipLineOnGlobal = "$NoPlaceToEquipLineOnGlobal";

		public const string NoPlaceToEquipTackleOnGlobal = "$NoPlaceToEquipTackleOnGlobal";

		public const string NoPlaceToEquipChumOnGlobal = "$NoPlaceToEquipChumOnGlobal";

		internal IMissionRodResource RodResource;

		private int levelCondition;

		private bool shouldCompleteRod;

		private bool configurationInitialized;
	}
}
