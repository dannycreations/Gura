using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(1)]
	public abstract class BaseHint : IHint
	{
		[JsonProperty]
		public int OrderIndex { get; set; }

		public bool LastCheckResult { get; set; }

		public abstract IEnumerable<HintMessage> Check(MissionsContext context);

		public virtual object Clone()
		{
			return base.MemberwiseClone();
		}

		public IEnumerable<HintMessage> ApplyTemplate(MissionsContext context, string code, Action<HintMessage> init = null, int orderOffset = 0, string translationCode = null, string codeHash = null)
		{
			yield return new HintMessage
			{
				Code = code
			}.SetAutomatic().SetMissionAndTask(this.mission, this.task).SetMessageIdAsCode(codeHash)
				.ApplyInit(init)
				.SetOrderIndex(this.OrderIndex + orderOffset)
				.Translate(translationCode);
			yield break;
		}

		public IEnumerable<HintMessage> ApplyTemplateForItem(MissionsContext context, string code, int itemId, string instanceId = null, StoragePlaces? sourceStorage = null, HintItemClass itemClass = HintItemClass.InventoryItem, Action<HintMessage> init = null, int orderOffset = 0, string translationCode = null, string codeHash = null)
		{
			yield return new HintMessage
			{
				Code = code
			}.SetAutomatic().SetMissionAndTask(this.mission, this.task).SetItemId(context, itemId, itemClass, instanceId, sourceStorage)
				.SetMessageIdAsCodeItemId(codeHash, null)
				.ApplyInit(init)
				.SetOrderIndex(this.OrderIndex + orderOffset)
				.Translate(translationCode);
			yield break;
		}

		public IEnumerable<HintMessage> ApplyTemplateForItemType(MissionsContext context, string code, int itemId, string instanceId = null, StoragePlaces? sourceStorage = null, HintItemClass itemClass = HintItemClass.InventoryCategory, Action<HintMessage> init = null, int orderOffset = 0, string translationCode = null, string codeHash = null)
		{
			yield return new HintMessage
			{
				Code = code
			}.SetAutomatic().SetMissionAndTask(this.mission, this.task).SetItemId(context, itemId, itemClass, instanceId, sourceStorage)
				.SetMessageIdAsCodeItemId(codeHash, null)
				.ApplyInit(init)
				.SetOrderIndex(this.OrderIndex + orderOffset)
				.Translate(translationCode);
			yield break;
		}

		public MissionOnClient mission;

		public MissionTaskTrackedOnClient task;
	}
}
