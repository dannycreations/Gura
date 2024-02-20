using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(1)]
	public class InventoryMovementMapInfo
	{
		public InventoryMovementMapInfo()
		{
			this.ItemMap = new Dictionary<int, Guid>();
			this.SplitMap = new Dictionary<int, Guid>();
			this.CombineMapStorageNew = new Dictionary<Guid, Guid>();
			this.CombineMapEquipmentNew = new Dictionary<Guid, Guid>();
			this.RelatedInfo = new Dictionary<Guid, InventoryMovementMapInfo>();
		}

		[JsonIgnore]
		public bool HasData
		{
			get
			{
				return this.InstanceId != null || this.OperationDateTime != null || this.OperationTime != null || this.ItemMap.Any<KeyValuePair<int, Guid>>() || this.SplitMap.Any<KeyValuePair<int, Guid>>() || this.CombineMapStorageNew.Any<KeyValuePair<Guid, Guid>>() || this.CombineMapEquipmentNew.Any<KeyValuePair<Guid, Guid>>() || this.RelatedInfo.Any<KeyValuePair<Guid, InventoryMovementMapInfo>>();
			}
		}

		[JsonProperty]
		public Guid? InstanceId { get; set; }

		[JsonProperty]
		public DateTime? OperationDateTime { get; set; }

		[JsonProperty]
		public TimeSpan? OperationTime { get; set; }

		[JsonProperty]
		public Dictionary<int, Guid> ItemMap { get; set; }

		[JsonProperty]
		public Dictionary<int, Guid> SplitMap { get; set; }

		[JsonProperty]
		public Dictionary<Guid, Guid> CombineMapStorageNew { get; set; }

		[JsonProperty]
		public Dictionary<Guid, Guid> CombineMapEquipmentNew { get; set; }

		public Dictionary<Guid, Guid> GetCombineMapNew(StoragePlaces storage)
		{
			if (storage == StoragePlaces.Storage)
			{
				return this.CombineMapStorageNew;
			}
			if (storage == StoragePlaces.Equipment)
			{
				return this.CombineMapEquipmentNew;
			}
			return null;
		}

		[JsonProperty]
		public Dictionary<Guid, InventoryMovementMapInfo> RelatedInfo { get; set; }

		public InventoryMovementMapInfo GetRelatedInfoOrNew(Guid instanceId)
		{
			InventoryMovementMapInfo inventoryMovementMapInfo;
			if (this.RelatedInfo.TryGetValue(instanceId, out inventoryMovementMapInfo))
			{
				return inventoryMovementMapInfo;
			}
			return new InventoryMovementMapInfo();
		}

		public void SetRelatedInfoIfHasData(Guid instanceId, InventoryMovementMapInfo relatedInfo)
		{
			if (relatedInfo.HasData)
			{
				this.RelatedInfo[instanceId] = relatedInfo;
			}
		}
	}
}
