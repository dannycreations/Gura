using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class MissionTaskTrackedOnClient
	{
		[JsonProperty]
		public int MissionId { get; set; }

		[JsonProperty]
		public string MissionCode { get; set; }

		[JsonProperty]
		public int TaskId { get; set; }

		[JsonProperty]
		public string Code { get; set; }

		[JsonProperty]
		public bool IsDynamic { get; set; }

		[JsonProperty]
		public bool IsCompleted { get; set; }

		[JsonProperty]
		public bool IsHidden { get; set; }

		[JsonProperty]
		public string ResourceKey { get; set; }

		[JsonProperty]
		public string Asset { get; set; }

		[JsonProperty]
		public int HintsOrderIndex { get; set; }

		[JsonProperty]
		public int HintsCount { get; set; }

		[JsonProperty]
		public string ConfigJson { get; set; }

		[JsonProperty]
		public List<HintMessageTranslationOnClient> Translations { get; set; }

		public override string ToString()
		{
			return string.Format("MissionTaskTrackedOnClient: MissionId: {0}, MissionCode: {1}, TaskId: {2}, Code: {3}, ResourceKey: {4}, Asset: {5}", new object[] { this.MissionId, this.MissionCode, this.TaskId, this.Code, this.ResourceKey, this.Asset });
		}

		public IMissionClientCondition Condition { get; set; }

		public Exception Exception { get; set; }

		public bool IsClientCompleted { get; set; }

		public string ClientProgress { get; set; }

		public void TranslateMessage(HintMessage message, string translationCode = null)
		{
			string code = translationCode ?? message.Code;
			if (string.IsNullOrEmpty(code))
			{
				return;
			}
			int taskId = message.TaskId;
			HintMessageTranslationOnClient hintMessageTranslationOnClient = ((this.Translations != null) ? this.Translations.FirstOrDefault((HintMessageTranslationOnClient t) => t.Code == code) : null);
			if (hintMessageTranslationOnClient == null)
			{
				return;
			}
			if ((message.IsAutomatic || !string.IsNullOrEmpty(message.Title)) && !string.IsNullOrEmpty(hintMessageTranslationOnClient.Title))
			{
				message.OriginalTitle = message.Title;
				message.Title = hintMessageTranslationOnClient.Title;
			}
			if ((message.IsAutomatic || !string.IsNullOrEmpty(message.Description)) && !string.IsNullOrEmpty(hintMessageTranslationOnClient.Description))
			{
				message.OriginalDescription = message.Description;
				message.Description = hintMessageTranslationOnClient.Description;
			}
			if ((message.IsAutomatic || !string.IsNullOrEmpty(message.Tooltip)) && !string.IsNullOrEmpty(hintMessageTranslationOnClient.Tooltip))
			{
				message.OriginalTooltip = message.Tooltip;
				message.Tooltip = hintMessageTranslationOnClient.Tooltip;
			}
			message.ImageId = hintMessageTranslationOnClient.ImageId;
		}
	}
}
