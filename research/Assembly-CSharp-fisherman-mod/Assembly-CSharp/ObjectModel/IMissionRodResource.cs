using System;
using System.Collections;
using System.Collections.Generic;

namespace ObjectModel
{
	public interface IMissionRodResource : IBuyItemsResource, IEnumerable<int[]>, IEnumerable
	{
		int RodId { get; set; }

		int[] RodIds { get; set; }

		int RodCategoryId { get; set; }

		int[] RodCategoryIds { get; set; }

		InventoryCondition RodCondition { get; set; }

		int ReelId { get; set; }

		int[] ReelIds { get; set; }

		int ReelCategoryId { get; set; }

		int[] ReelCategoryIds { get; set; }

		InventoryCondition ReelCondition { get; set; }

		int LineId { get; set; }

		int[] LineIds { get; set; }

		int LineCategoryId { get; set; }

		int[] LineCategoryIds { get; set; }

		InventoryCondition LineCondition { get; set; }

		int BobberId { get; set; }

		int[] BobberIds { get; set; }

		int BobberCategoryId { get; set; }

		int[] BobberCategoryIds { get; set; }

		InventoryCondition BobberCondition { get; set; }

		int HookId { get; set; }

		int[] HookIds { get; set; }

		int HookCategoryId { get; set; }

		int[] HookCategoryIds { get; set; }

		InventoryCondition HookCondition { get; set; }

		int JigHeadId { get; set; }

		int[] JigHeadIds { get; set; }

		int JigHeadCategoryId { get; set; }

		int[] JigHeadCategoryIds { get; set; }

		InventoryCondition JigHeadCondition { get; set; }

		int LureId { get; set; }

		int[] LureIds { get; set; }

		int LureCategoryId { get; set; }

		int[] LureCategoryIds { get; set; }

		InventoryCondition LureCondition { get; set; }

		int BaitId { get; set; }

		int[] BaitIds { get; set; }

		int BaitCategoryId { get; set; }

		int[] BaitCategoryIds { get; set; }

		InventoryCondition BaitCondition { get; set; }

		int JigBaitId { get; set; }

		int[] JigBaitIds { get; set; }

		int JigBaitCategoryId { get; set; }

		int[] JigBaitCategoryIds { get; set; }

		InventoryCondition JigBaitCondition { get; set; }

		int FeederId { get; set; }

		int[] FeederIds { get; set; }

		int FeederCategoryId { get; set; }

		int[] FeederCategoryIds { get; set; }

		InventoryCondition FeederCondition { get; set; }

		int SinkerId { get; set; }

		int[] SinkerIds { get; set; }

		int SinkerCategoryId { get; set; }

		int[] SinkerCategoryIds { get; set; }

		InventoryCondition SinkerCondition { get; set; }

		int LeaderId { get; set; }

		int[] LeaderIds { get; set; }

		int LeaderCategoryId { get; set; }

		int[] LeaderCategoryIds { get; set; }

		InventoryCondition LeaderCondition { get; set; }

		int ChumId { get; set; }

		int[] ChumIds { get; set; }

		int ChumCategoryId { get; set; }

		int[] ChumCategoryIds { get; set; }

		InventoryCondition ChumCondition { get; set; }

		int BellId { get; set; }

		int[] BellIds { get; set; }

		int BellCategoryId { get; set; }

		int[] BellCategoryIds { get; set; }

		InventoryCondition BellCondition { get; set; }

		int[] TackleIds { get; set; }

		RodTemplate? RodTemplate { get; set; }

		bool AllowIncomplete { get; set; }
	}
}
