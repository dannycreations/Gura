using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	[JsonObject(1)]
	public class AssembleRodCondition : BaseCondition, IMissionRodResource, IMissionClientCondition, IBuyItemsResource, IEnumerable<int[]>, IEnumerable
	{
		public AssembleRodCondition()
		{
			this.buyItemsHint = new BuyItemsHint
			{
				RodResource = this,
				ShouldAssembleRod = true
			};
			this.assembleRodHint = new AssembleRodHint
			{
				RodResource = this
			};
		}

		[JsonProperty]
		public bool OnlyGlobalShop { get; set; }

		[JsonProperty]
		public bool AutomaticHints { get; set; }

		[JsonProperty]
		public bool AssembleRodHintsOnly { get; set; }

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
		public float? ChumWeight { get; set; }

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

		[JsonProperty]
		public bool RodInHands { get; set; }

		[JsonProperty]
		[JsonConverter(typeof(StringEnumConverter))]
		public RodTemplate? RodTemplate { get; set; }

		[JsonProperty]
		public bool AllowIncomplete { get; set; }

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

		public void EnsureConfiguration(MissionsContext context)
		{
			if (this.configurationInitialized)
			{
				return;
			}
			this.InitConditionLevelConfiguration(context);
			this.configurationInitialized = true;
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
			GameCacheAdapter cacheAdapter = context.GetGameCacheAdapter();
			List<ItemSubTypes> list = (from itemId in AssembleRodCondition.JoinedArray(this.RodId, this.RodIds)
				select cacheAdapter.ResolveItemGloablly(itemId).ItemSubType).Union(from categoryId in AssembleRodCondition.JoinedArray(this.RodCategoryId, this.RodCategoryIds)
				select (ItemSubTypes)categoryId).ToList<ItemSubTypes>();
			bool flag;
			if (list.Any<ItemSubTypes>())
			{
				flag = list.All(new Func<ItemSubTypes, bool>(RodTemplatesExtensions.IsRodWithBobber));
			}
			else
			{
				flag = false;
			}
			this.IsRodWithBobber = flag;
			bool flag2;
			if (list.Any<ItemSubTypes>())
			{
				flag2 = list.All(new Func<ItemSubTypes, bool>(RodTemplatesExtensions.IsRodWithHook));
			}
			else
			{
				flag2 = false;
			}
			this.IsRodWithHook = flag2;
			bool flag3;
			if (list.Any<ItemSubTypes>())
			{
				flag3 = list.All(new Func<ItemSubTypes, bool>(RodTemplatesExtensions.IsRodWithLure));
			}
			else
			{
				flag3 = false;
			}
			this.IsRodWithLure = flag3;
			bool flag4;
			if (list.Any<ItemSubTypes>())
			{
				flag4 = list.All(new Func<ItemSubTypes, bool>(RodTemplatesExtensions.IsRodWithLeader));
			}
			else
			{
				flag4 = false;
			}
			this.IsRodWithLeader = flag4;
			bool flag5;
			if (list.Any<ItemSubTypes>())
			{
				flag5 = list.All(new Func<ItemSubTypes, bool>(RodTemplatesExtensions.IsRodWithFeeder));
			}
			else
			{
				flag5 = false;
			}
			this.IsRodWithFeeder = flag5;
			bool flag6;
			if (list.Any<ItemSubTypes>())
			{
				flag6 = list.All((ItemSubTypes subType) => subType == ItemSubTypes.CastingRod);
			}
			else
			{
				flag6 = false;
			}
			this.IsCastingRod = flag6;
			bool flag7;
			if (list.Any<ItemSubTypes>())
			{
				flag7 = list.All((ItemSubTypes subType) => subType == ItemSubTypes.FeederRod);
			}
			else
			{
				flag7 = false;
			}
			this.IsFeederRod = flag7;
			bool flag8;
			if (list.Any<ItemSubTypes>())
			{
				flag8 = list.All((ItemSubTypes subType) => subType == ItemSubTypes.BottomRod);
			}
			else
			{
				flag8 = false;
			}
			this.IsBottomRod = flag8;
			bool flag9;
			if (list.Any<ItemSubTypes>())
			{
				flag9 = list.All((ItemSubTypes subType) => subType == ItemSubTypes.SpodRod);
			}
			else
			{
				flag9 = false;
			}
			this.IsSpodRod = flag9;
			this.shouldCompleteRod = (this.RodTemplate != null && this.RodTemplate != ObjectModel.RodTemplate.UnEquiped) || !this.AllowIncomplete;
		}

		public int GetAssemblingSlot(MissionsContext context)
		{
			MissionRodContext missionRodContext = (from r in context.Inventory.RodsOnDoll
				where (this.RodId != 0 && r.Rod.ItemId == this.RodId) || (this.RodIds != null && this.RodIds.Length > 0 && this.RodIds.Contains(r.Rod.ItemId)) || (this.RodCondition != null && this.RodCondition.Match(r.Rod, false)) || (this.RodCategoryId != 0 && (int)r.Rod.ItemSubType == this.RodCategoryId) || (this.RodCategoryIds != null && this.RodCategoryIds.Length > 0 && this.RodCategoryIds.Contains((int)r.Rod.ItemSubType))
				where r.Rod.Durability > 0
				select r).OrderBy(delegate(MissionRodContext r)
			{
				if (this.MinLoad != null)
				{
					float? minLoad = this.MinLoad;
					if (!(r.Rod.MaxLoad >= minLoad))
					{
						return 100;
					}
				}
				if (this.MaxLoad != null)
				{
					float? maxLoad = this.MaxLoad;
					if (!(r.Rod.MaxLoad <= maxLoad))
					{
						return 100;
					}
				}
				return 1;
			}).ThenBy(delegate(MissionRodContext r)
			{
				if (this.MinLoad != null)
				{
					float? minLoad2 = this.MinLoad;
					if (!(r.Reel.MaxLoad >= minLoad2))
					{
						return 100;
					}
				}
				if (this.MaxLoad != null)
				{
					float? maxLoad2 = this.MaxLoad;
					if (!(r.Reel.MaxLoad <= maxLoad2))
					{
						return 100;
					}
				}
				if (this.ReelId != 0 && r.Reel.ItemId == this.ReelId)
				{
					return 1;
				}
				if (this.ReelIds != null && this.ReelIds.Contains(r.Reel.ItemId))
				{
					return 2;
				}
				if (this.ReelCondition != null && this.ReelCondition.Match(r.Reel, false))
				{
					return 3;
				}
				if (this.ReelCategoryId != 0 && (int)r.Reel.ItemSubType == this.ReelCategoryId)
				{
					return 4;
				}
				if (this.ReelCategoryIds != null && this.ReelCategoryIds.Contains((int)r.Reel.ItemSubType))
				{
					return 5;
				}
				return 100;
			}).FirstOrDefault<MissionRodContext>();
			return (missionRodContext == null) ? 0 : missionRodContext.Rod.Slot;
		}

		public IEnumerator<int[]> GetEnumerator()
		{
			return this.GetItemIds().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetItemIds().GetEnumerator();
		}

		private IEnumerable<int[]> GetItemIds()
		{
			yield return (from i in AssembleRodCondition.JoinedArray(this.RodId, this.RodIds, this.RodCategoryId, this.RodCategoryIds, 0)
				where i != 0
				select i).ToArray<int>();
			yield return (from i in AssembleRodCondition.JoinedArray(this.ReelId, this.ReelIds, this.ReelCategoryId, this.ReelCategoryIds, (this.levelCondition < 1 && !this.shouldCompleteRod) ? 0 : ((!this.IsCastingRod) ? 30 : 32))
				where i != 0
				select i).ToArray<int>();
			yield return (from i in AssembleRodCondition.JoinedArray(this.LineId, this.LineIds, this.LineCategoryId, this.LineCategoryIds, (this.levelCondition < 2 && !this.shouldCompleteRod) ? 0 : 5)
				where i != 0
				select i).ToArray<int>();
			yield return (from i in AssembleRodCondition.JoinedArray(this.BellId, this.BellIds, this.BellCategoryId, this.BellCategoryIds, 0)
				where i != 0
				select i).ToArray<int>();
			yield return (from i in AssembleRodCondition.JoinedArray(this.BobberId, this.BobberIds, this.BobberCategoryId, this.BobberCategoryIds, ((this.levelCondition < 3 && !this.shouldCompleteRod) || !this.IsRodWithBobber) ? 0 : 7)
				where i != 0
				select i).ToArray<int>();
			yield return (from i in AssembleRodCondition.JoinedArray(this.FeederId, this.FeederIds, this.FeederCategoryId, this.FeederCategoryIds, ((this.levelCondition < 4 && !this.shouldCompleteRod) || !this.IsRodWithFeeder || (!this.HasAnyChumCondition && (!this.IsFeederRod || this.HasAnySinkerCondition))) ? 0 : 9)
				where i != 0
				select i).ToArray<int>();
			yield return (from i in AssembleRodCondition.JoinedArray(this.ChumId, this.ChumIds, this.ChumCategoryId, this.ChumCategoryIds, 0)
				where i != 0
				select i).ToArray<int>();
			yield return (from i in AssembleRodCondition.JoinedArray(this.SinkerId, this.SinkerIds, this.SinkerCategoryId, this.SinkerCategoryIds, ((this.levelCondition < 6 && !this.shouldCompleteRod) || !this.IsBottomRod || this.HasAnyFeederCondition || this.HasAnyChumCondition) ? 0 : 8)
				where i != 0
				select i).ToArray<int>();
			yield return (from i in AssembleRodCondition.JoinedArray(this.LeaderId, this.LeaderIds, this.LeaderCategoryId, this.LeaderCategoryIds, ((this.levelCondition < 7 && !this.shouldCompleteRod) || !this.IsRodWithLeader) ? 0 : 66)
				where i != 0
				select i).ToArray<int>();
			yield return (from i in AssembleRodCondition.JoinedArray(this.HookId, this.HookIds, this.HookCategoryId, this.HookCategoryIds, ((this.levelCondition < 8 && !this.shouldCompleteRod) || !this.IsRodWithHook) ? 0 : 6)
				where i != 0
				select i).ToArray<int>();
			yield return (from i in AssembleRodCondition.JoinedArray(this.JigHeadId, this.JigHeadIds, this.JigHeadCategoryId, this.JigHeadCategoryIds, ((this.levelCondition < 8 && !this.shouldCompleteRod) || !this.IsRodWithLure || !this.HasAnyJigBaitCondition) ? 0 : 35)
				where i != 0
				select i).ToArray<int>();
			yield return (from i in AssembleRodCondition.JoinedArray(this.LureId, this.LureIds, this.LureCategoryId, this.LureCategoryIds, ((this.levelCondition < 8 && !this.shouldCompleteRod) || !this.IsRodWithLure || this.HasAnyJigHeadCondition || this.HasAnyJigBaitCondition) ? 0 : 11)
				where i != 0
				select i).ToArray<int>();
			yield return (from i in AssembleRodCondition.JoinedArray(this.BaitId, this.BaitIds, this.BaitCategoryId, this.BaitCategoryIds, ((this.levelCondition < 9 && !this.shouldCompleteRod) || !this.IsRodWithHook) ? 0 : 10)
				where i != 0
				select i).ToArray<int>();
			yield return (from i in AssembleRodCondition.JoinedArray(this.JigBaitId, this.JigBaitIds, this.JigBaitCategoryId, this.JigBaitCategoryIds, ((this.levelCondition < 9 && !this.shouldCompleteRod) || !this.IsRodWithLure || !this.HasAnyJigHeadCondition) ? 0 : 12)
				where i != 0
				select i).ToArray<int>();
			yield return (from i in AssembleRodCondition.JoinedArray(0, this.TackleIds)
				where i != 0
				select i).ToArray<int>();
			yield break;
		}

		private static IEnumerable<int> JoinedArray(int itemId, int[] itemIds)
		{
			if (itemId != 0)
			{
				yield return itemId;
			}
			if (itemIds != null)
			{
				foreach (int id in itemIds)
				{
					yield return id;
				}
			}
			yield break;
		}

		private static IEnumerable<int> JoinedArray(int itemId, int[] itemIds, int categoryId, int[] categoryIds, int parentCategoryId = 0)
		{
			if (itemId != 0)
			{
				yield return itemId;
			}
			if (itemIds != null)
			{
				foreach (int id in itemIds)
				{
					yield return id;
				}
			}
			if (categoryId != 0)
			{
				yield return -categoryId;
			}
			if (categoryIds != null)
			{
				foreach (int id2 in categoryIds)
				{
					yield return -id2;
				}
			}
			if (itemId == 0 && (itemIds == null || itemIds.Length <= 0) && categoryId == 0 && (categoryIds == null || categoryIds.Length <= 0) && parentCategoryId != 0)
			{
				yield return -parentCategoryId;
			}
			yield break;
		}

		public List<string> GetTranslationsOnClient()
		{
			List<string> list = new List<string>();
			List<IHint> list2 = this.GenerateHints();
			if (list2.Contains(this.buyItemsHint))
			{
				list.AddRange(this.buyItemsHint.GetTranslationsOnClient());
			}
			if (list2.Contains(this.assembleRodHint))
			{
				list.AddRange(this.assembleRodHint.GetTranslationsOnClient());
			}
			return list;
		}

		protected override List<IHint> GenerateHints()
		{
			if (!this.AutomaticHints)
			{
				return new List<IHint>();
			}
			if (this.AssembleRodHintsOnly)
			{
				this.assembleRodHint.ShouldGenerateHints = null;
				return new List<IHint> { this.assembleRodHint };
			}
			this.assembleRodHint.ShouldGenerateHints = () => !this.buyItemsHint.LastCheckResult;
			return new List<IHint> { this.buyItemsHint, this.assembleRodHint };
		}

		protected override string[] MonitoringDependencies
		{
			get
			{
				List<string> list = new List<string> { "MoveInventory", "PondId", "AdditionalInventoryCapacity", "GlobalShopInventory", "LocalShopInventory" };
				if (this.RodInHands)
				{
					list.Add("RodMoveInventory");
				}
				return list.ToArray();
			}
		}

		public MissionRodContext Rod { get; private set; }

		public int Slot
		{
			get
			{
				return (this.Rod == null || this.Rod.IsEmpty) ? (-1) : this.Rod.Rod.Slot;
			}
		}

		private static bool CheckItem(InventoryItem item, int itemId, int[] itemIds, InventoryCondition condition, int categoryId, int[] categoryIds, bool checkDurability = true)
		{
			return (((itemId != 0 && item.ItemId == itemId) || (itemIds != null && itemIds.Length > 0 && itemIds.Contains(item.ItemId)) || (condition != null && condition.Match(item, false)) || (categoryId != 0 && ((int)item.ItemSubType == categoryId || (int)item.ItemType == categoryId)) || (categoryIds != null && (categoryIds.Contains((int)item.ItemSubType) || categoryIds.Contains((int)item.ItemType)))) && (!checkDurability || item.Durability > 0)) || (itemId == 0 && (itemIds == null || itemIds.Length == 0) && condition == null && categoryId == 0 && (categoryIds == null || categoryIds.Length == 0));
		}

		public override bool Check(MissionsContext context)
		{
			AssembleRodCondition.<Check>c__AnonStorey5 <Check>c__AnonStorey = new AssembleRodCondition.<Check>c__AnonStorey5();
			<Check>c__AnonStorey.context = context;
			<Check>c__AnonStorey.$this = this;
			this.EnsureConfiguration(<Check>c__AnonStorey.context);
			int slot = this.Slot;
			MissionRodContext[] array;
			if (this.RodInHands)
			{
				(array = new MissionRodContext[1])[0] = <Check>c__AnonStorey.context.Inventory.RodInHands;
			}
			else
			{
				array = <Check>c__AnonStorey.context.Inventory.RodsOnDoll;
			}
			IEnumerable<MissionRodContext> enumerable = array;
			this.Rod = enumerable.FirstOrDefault(delegate(MissionRodContext rod)
			{
				if (!AssembleRodCondition.CheckItem(rod.Rod, <Check>c__AnonStorey.$this.RodId, <Check>c__AnonStorey.$this.RodIds, <Check>c__AnonStorey.$this.RodCondition, <Check>c__AnonStorey.$this.RodCategoryId, <Check>c__AnonStorey.$this.RodCategoryIds, true))
				{
					return false;
				}
				if (!AssembleRodCondition.CheckItem(rod.Reel, <Check>c__AnonStorey.$this.ReelId, <Check>c__AnonStorey.$this.ReelIds, <Check>c__AnonStorey.$this.ReelCondition, <Check>c__AnonStorey.$this.ReelCategoryId, <Check>c__AnonStorey.$this.ReelCategoryIds, true))
				{
					return false;
				}
				if (!AssembleRodCondition.CheckItem(rod.Line, <Check>c__AnonStorey.$this.LineId, <Check>c__AnonStorey.$this.LineIds, <Check>c__AnonStorey.$this.LineCondition, <Check>c__AnonStorey.$this.LineCategoryId, <Check>c__AnonStorey.$this.LineCategoryIds, true))
				{
					return false;
				}
				if (<Check>c__AnonStorey.$this.LineLength != null)
				{
					double? length = rod.Line.Length;
					bool flag = length != null;
					int? lineLength = <Check>c__AnonStorey.$this.LineLength;
					double? num = ((lineLength == null) ? null : new double?((double)lineLength.Value));
					if ((flag & (num != null)) && length.GetValueOrDefault() >= num.GetValueOrDefault())
					{
						goto IL_1AD;
					}
				}
				if (<Check>c__AnonStorey.$this.LineLength != null)
				{
					return false;
				}
				IL_1AD:
				if (<Check>c__AnonStorey.$this.LineMinThickness != null)
				{
					float? lineMinThickness = <Check>c__AnonStorey.$this.LineMinThickness;
					if (rod.Line.Thickness >= lineMinThickness)
					{
						goto IL_221;
					}
				}
				if (<Check>c__AnonStorey.$this.LineMinThickness != null)
				{
					return false;
				}
				IL_221:
				if (<Check>c__AnonStorey.$this.LineMaxThickness != null)
				{
					float? lineMaxThickness = <Check>c__AnonStorey.$this.LineMaxThickness;
					if (rod.Line.Thickness <= lineMaxThickness)
					{
						goto IL_295;
					}
				}
				if (<Check>c__AnonStorey.$this.LineMaxThickness != null)
				{
					return false;
				}
				IL_295:
				if (!AssembleRodCondition.CheckItem(rod.Bell, <Check>c__AnonStorey.$this.BellId, <Check>c__AnonStorey.$this.BellIds, <Check>c__AnonStorey.$this.BellCondition, <Check>c__AnonStorey.$this.BellCategoryId, <Check>c__AnonStorey.$this.BellCategoryIds, true))
				{
					return false;
				}
				if (!AssembleRodCondition.CheckItem(rod.Bobber, <Check>c__AnonStorey.$this.BobberId, <Check>c__AnonStorey.$this.BobberIds, <Check>c__AnonStorey.$this.BobberCondition, <Check>c__AnonStorey.$this.BobberCategoryId, <Check>c__AnonStorey.$this.BobberCategoryIds, true))
				{
					return false;
				}
				if (!AssembleRodCondition.CheckItem(rod.Hook, <Check>c__AnonStorey.$this.HookId, <Check>c__AnonStorey.$this.HookIds, <Check>c__AnonStorey.$this.HookCondition, <Check>c__AnonStorey.$this.HookCategoryId, <Check>c__AnonStorey.$this.HookCategoryIds, true))
				{
					return false;
				}
				if (!AssembleRodCondition.CheckItem(rod.JigHead, <Check>c__AnonStorey.$this.JigHeadId, <Check>c__AnonStorey.$this.JigHeadIds, <Check>c__AnonStorey.$this.JigHeadCondition, <Check>c__AnonStorey.$this.JigHeadCategoryId, <Check>c__AnonStorey.$this.JigHeadCategoryIds, true))
				{
					return false;
				}
				if (!AssembleRodCondition.CheckItem(rod.Lure, <Check>c__AnonStorey.$this.LureId, <Check>c__AnonStorey.$this.LureIds, <Check>c__AnonStorey.$this.LureCondition, <Check>c__AnonStorey.$this.LureCategoryId, <Check>c__AnonStorey.$this.LureCategoryIds, true))
				{
					return false;
				}
				if (!AssembleRodCondition.CheckItem(rod.Bait, <Check>c__AnonStorey.$this.BaitId, <Check>c__AnonStorey.$this.BaitIds, <Check>c__AnonStorey.$this.BaitCondition, <Check>c__AnonStorey.$this.BaitCategoryId, <Check>c__AnonStorey.$this.BaitCategoryIds, true))
				{
					return false;
				}
				if (!AssembleRodCondition.CheckItem(rod.JigBait, <Check>c__AnonStorey.$this.JigBaitId, <Check>c__AnonStorey.$this.JigBaitIds, <Check>c__AnonStorey.$this.JigBaitCondition, <Check>c__AnonStorey.$this.JigBaitCategoryId, <Check>c__AnonStorey.$this.JigBaitCategoryIds, true))
				{
					return false;
				}
				if (!AssembleRodCondition.CheckItem(rod.Feeder, <Check>c__AnonStorey.$this.FeederId, <Check>c__AnonStorey.$this.FeederIds, <Check>c__AnonStorey.$this.FeederCondition, <Check>c__AnonStorey.$this.FeederCategoryId, <Check>c__AnonStorey.$this.FeederCategoryIds, true))
				{
					return false;
				}
				if (!AssembleRodCondition.CheckItem(rod.Sinker, <Check>c__AnonStorey.$this.SinkerId, <Check>c__AnonStorey.$this.SinkerIds, <Check>c__AnonStorey.$this.SinkerCondition, <Check>c__AnonStorey.$this.SinkerCategoryId, <Check>c__AnonStorey.$this.SinkerCategoryIds, true))
				{
					return false;
				}
				if (!AssembleRodCondition.CheckItem(rod.Leader, <Check>c__AnonStorey.$this.LeaderId, <Check>c__AnonStorey.$this.LeaderIds, <Check>c__AnonStorey.$this.LeaderCondition, <Check>c__AnonStorey.$this.LeaderCategoryId, <Check>c__AnonStorey.$this.LeaderCategoryIds, true))
				{
					return false;
				}
				if ((!<Check>c__AnonStorey.$this.HasAnyChumCondition || !rod.Chum.ChumBase.Any((ChumBase i) => AssembleRodCondition.CheckItem(i, <Check>c__AnonStorey.ChumId, <Check>c__AnonStorey.ChumIds, <Check>c__AnonStorey.ChumCondition, <Check>c__AnonStorey.ChumCategoryId, <Check>c__AnonStorey.ChumCategoryIds, false))) && <Check>c__AnonStorey.$this.HasAnyChumCondition)
				{
					return false;
				}
				if ((<Check>c__AnonStorey.$this.TackleIds == null || !<Check>c__AnonStorey.$this.TackleIds.All((int t) => rod.TackleIds.Contains(t))) && <Check>c__AnonStorey.$this.TackleIds != null && <Check>c__AnonStorey.$this.TackleIds.Length != 0)
				{
					return false;
				}
				if (<Check>c__AnonStorey.$this.MinLoad != null)
				{
					float? minLoad = <Check>c__AnonStorey.$this.MinLoad;
					if (rod.Rod.MaxLoad >= minLoad)
					{
						if (<Check>c__AnonStorey.$this.AllowIncomplete && <Check>c__AnonStorey.$this.levelCondition < 1)
						{
							goto IL_71A;
						}
						float? minLoad2 = <Check>c__AnonStorey.$this.MinLoad;
						if (rod.Reel.MaxLoad >= minLoad2)
						{
							goto IL_71A;
						}
					}
				}
				if (<Check>c__AnonStorey.$this.MinLoad != null)
				{
					return false;
				}
				IL_71A:
				if (<Check>c__AnonStorey.$this.MaxLoad != null)
				{
					float? maxLoad = <Check>c__AnonStorey.$this.MaxLoad;
					if (rod.Rod.MaxLoad <= maxLoad)
					{
						if (<Check>c__AnonStorey.$this.AllowIncomplete && <Check>c__AnonStorey.$this.levelCondition < 1)
						{
							goto IL_7EC;
						}
						float? maxLoad2 = <Check>c__AnonStorey.$this.MaxLoad;
						if (rod.Reel.MaxLoad <= maxLoad2)
						{
							goto IL_7EC;
						}
					}
				}
				if (<Check>c__AnonStorey.$this.MaxLoad != null)
				{
					return false;
				}
				IL_7EC:
				if (<Check>c__AnonStorey.$this.LineMinLoad != null)
				{
					if (<Check>c__AnonStorey.$this.AllowIncomplete && <Check>c__AnonStorey.$this.levelCondition < 2)
					{
						goto IL_881;
					}
					float? lineMinLoad = <Check>c__AnonStorey.$this.LineMinLoad;
					if (rod.Line.MaxLoad >= lineMinLoad)
					{
						goto IL_881;
					}
				}
				if (<Check>c__AnonStorey.$this.LineMinLoad != null)
				{
					return false;
				}
				IL_881:
				if (<Check>c__AnonStorey.$this.LineMaxLoad != null)
				{
					if (<Check>c__AnonStorey.$this.AllowIncomplete && <Check>c__AnonStorey.$this.levelCondition < 2)
					{
						goto IL_916;
					}
					float? lineMaxLoad = <Check>c__AnonStorey.$this.LineMaxLoad;
					if (rod.Line.MaxLoad <= lineMaxLoad)
					{
						goto IL_916;
					}
				}
				if (<Check>c__AnonStorey.$this.LineMaxLoad != null)
				{
					return false;
				}
				IL_916:
				if (<Check>c__AnonStorey.$this.LeaderMinLoad != null)
				{
					if ((<Check>c__AnonStorey.$this.AllowIncomplete && <Check>c__AnonStorey.$this.levelCondition < 7) || (!<Check>c__AnonStorey.$this.IsRodWithLeader && !<Check>c__AnonStorey.$this.HasAnyLeaderCondition))
					{
						goto IL_9CB;
					}
					float? leaderMinLoad = <Check>c__AnonStorey.$this.LeaderMinLoad;
					if (rod.Leader.MaxLoad >= leaderMinLoad)
					{
						goto IL_9CB;
					}
				}
				if (<Check>c__AnonStorey.$this.LeaderMinLoad != null)
				{
					return false;
				}
				IL_9CB:
				if (<Check>c__AnonStorey.$this.LeaderMaxLoad != null)
				{
					if ((<Check>c__AnonStorey.$this.AllowIncomplete && <Check>c__AnonStorey.$this.levelCondition < 7) || (!<Check>c__AnonStorey.$this.IsRodWithLeader && !<Check>c__AnonStorey.$this.HasAnyLeaderCondition))
					{
						goto IL_A80;
					}
					float? leaderMaxLoad = <Check>c__AnonStorey.$this.LeaderMaxLoad;
					if (rod.Leader.MaxLoad <= leaderMaxLoad)
					{
						goto IL_A80;
					}
				}
				if (<Check>c__AnonStorey.$this.LeaderMaxLoad != null)
				{
					return false;
				}
				IL_A80:
				return (<Check>c__AnonStorey.$this.RodTemplate != null && <Check>c__AnonStorey.context.GetProfile().Inventory.RodTemplate.MatchedTemplate(rod.Rod) == <Check>c__AnonStorey.$this.RodTemplate) || (<Check>c__AnonStorey.$this.RodTemplate == null && <Check>c__AnonStorey.context.GetProfile().Inventory.RodTemplate.MatchedTemplate(rod.Rod) != ObjectModel.RodTemplate.UnEquiped) || <Check>c__AnonStorey.$this.AllowIncomplete;
			});
			return this.Rod != null;
		}

		public override object Clone()
		{
			AssembleRodCondition assembleRodCondition = (AssembleRodCondition)base.Clone();
			assembleRodCondition.buyItemsHint = (BuyItemsHint)this.buyItemsHint.Clone();
			assembleRodCondition.buyItemsHint.RodResource = assembleRodCondition;
			assembleRodCondition.buyItemsHint.ShouldAssembleRod = true;
			assembleRodCondition.assembleRodHint = (AssembleRodHint)this.assembleRodHint.Clone();
			assembleRodCondition.assembleRodHint.RodResource = assembleRodCondition;
			if (this.RodCondition != null)
			{
				assembleRodCondition.RodCondition = (InventoryCondition)this.RodCondition.Clone();
			}
			if (this.ReelCondition != null)
			{
				assembleRodCondition.ReelCondition = (InventoryCondition)this.ReelCondition.Clone();
			}
			if (this.LineCondition != null)
			{
				assembleRodCondition.LineCondition = (InventoryCondition)this.LineCondition.Clone();
			}
			if (this.BobberCondition != null)
			{
				assembleRodCondition.BobberCondition = (InventoryCondition)this.BobberCondition.Clone();
			}
			if (this.HookCondition != null)
			{
				assembleRodCondition.HookCondition = (InventoryCondition)this.HookCondition.Clone();
			}
			if (this.JigHeadCondition != null)
			{
				assembleRodCondition.JigHeadCondition = (InventoryCondition)this.JigHeadCondition.Clone();
			}
			if (this.LureCondition != null)
			{
				assembleRodCondition.LureCondition = (InventoryCondition)this.LureCondition.Clone();
			}
			if (this.BaitCondition != null)
			{
				assembleRodCondition.BaitCondition = (InventoryCondition)this.BaitCondition.Clone();
			}
			if (this.JigBaitCondition != null)
			{
				assembleRodCondition.JigBaitCondition = (InventoryCondition)this.JigBaitCondition.Clone();
			}
			if (this.FeederCondition != null)
			{
				assembleRodCondition.FeederCondition = (InventoryCondition)this.FeederCondition.Clone();
			}
			if (this.SinkerCondition != null)
			{
				assembleRodCondition.SinkerCondition = (InventoryCondition)this.SinkerCondition.Clone();
			}
			if (this.LeaderCondition != null)
			{
				assembleRodCondition.LeaderCondition = (InventoryCondition)this.LeaderCondition.Clone();
			}
			if (this.ChumCondition != null)
			{
				assembleRodCondition.ChumCondition = (InventoryCondition)this.ChumCondition.Clone();
			}
			if (this.BellCondition != null)
			{
				assembleRodCondition.BellCondition = (InventoryCondition)this.BellCondition.Clone();
			}
			return assembleRodCondition;
		}

		public MissionTaskTrackedOnClient Task { get; set; }

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event MissionClientConditionHintsChangedHandler HintsChanged;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event MissionClientConditionCompletedEventHandler Completed;

		private bool AllCachePropertiesLoaded(MissionsContext context)
		{
			return this.itemsAllLoaded && context.GetGameCacheAdapter().ShopCachePropertiesLoaded;
		}

		private void EnsureCaches(MissionsContext context)
		{
			if (!this.itemsAllRequested)
			{
				this.itemsAllRequested = true;
				int[] array = (from id in this.GetItemIds().SelectMany((int[] a) => a)
					where id > 0
					select id).ToArray<int>();
				CacheLibrary.ItemsCache.GetItems(array, 500000 + base.oid);
			}
			context.GetGameCacheAdapter().EnsureCaches();
		}

		private void ItemsCache_OnGotItems(List<InventoryItem> items, int sunscriberId)
		{
			this.itemsAllLoaded = true;
			this.shouldProcess = true;
		}

		public bool CheckMonitoredDependencies(string dependency)
		{
			bool flag = this.monitoringDependencies.Contains(dependency);
			this.shouldProcess = this.shouldProcess || flag;
			return flag;
		}

		public void Start()
		{
			MissionOnClient missionOnClient = new MissionOnClient
			{
				MissionId = this.Task.MissionId,
				Code = this.Task.MissionCode
			};
			this.monitoringDependencies = this.MonitoringDependencies;
			this.buyItemsHint.mission = missionOnClient;
			this.buyItemsHint.task = this.Task;
			this.buyItemsHint.OrderIndex = this.Task.HintsOrderIndex;
			this.assembleRodHint.mission = missionOnClient;
			this.assembleRodHint.task = this.Task;
			this.assembleRodHint.OrderIndex = this.Task.HintsOrderIndex + 100;
			CacheLibrary.ItemsCache.OnGotItems += this.ItemsCache_OnGotItems;
			this.shouldProcess = true;
		}

		public void Update(MissionsContext context)
		{
			this.EnsureCaches(context);
			if (this.shouldProcess && this.AllCachePropertiesLoaded(context))
			{
				bool flag = this.Check(context);
				if (!flag)
				{
					this.ForceGenerateHints(context);
				}
				if (this.Task.IsClientCompleted != flag && this.Completed != null)
				{
					this.Completed(this, flag, null);
				}
				this.shouldProcess = false;
			}
		}

		public void ForceGenerateHints(MissionsContext context)
		{
			if (this.AllCachePropertiesLoaded(context))
			{
				List<HintMessage> list = this.GenerateHints().SelectMany((IHint h) => h.Check(context)).ToList<HintMessage>();
				if (this.HintsChanged != null)
				{
					this.HintsChanged(this, list);
				}
			}
		}

		public void Destroy()
		{
			CacheLibrary.ItemsCache.OnGotItems -= this.ItemsCache_OnGotItems;
		}

		private int levelCondition;

		private bool shouldCompleteRod;

		private bool IsRodWithBobber;

		private bool IsRodWithHook;

		private bool IsCastingRod;

		private bool IsRodWithLure;

		private bool IsRodWithLeader;

		private bool IsRodWithFeeder;

		private bool IsFeederRod;

		private bool IsBottomRod;

		private bool IsSpodRod;

		private bool configurationInitialized;

		private BuyItemsHint buyItemsHint;

		private AssembleRodHint assembleRodHint;

		private bool shouldProcess;

		private string[] monitoringDependencies;

		private bool itemsAllRequested;

		private bool itemsAllLoaded;
	}
}
