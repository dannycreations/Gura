using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectModel
{
	public static class HintMessageUtilities
	{
		public static HintMessage MoveToEquipmentMessage(this HintMessage message)
		{
			if (message.SourceStorage == StoragePlaces.Storage)
			{
				message.Storage = StoragePlaces.Equipment;
				string text = ((message.ItemClass != HintItemClass.InventoryCategory) ? "$MoveItemToEquipment" : "$MoveItemTypeToEquipment");
				message.SetMessageIdAsCodeItemId(null, text);
				message.Translate(null);
			}
			else
			{
				message.DisplayStorage = StoragePlaces.Equipment;
			}
			return message;
		}

		public static IEnumerable<HintMessage> EquipMessagesFirst(this IEnumerable<HintMessage> messages)
		{
			List<HintMessage> list = messages.Where((HintMessage m) => m.Code != "$MoveItemTypeToEquipment" && m.Code != "$MoveItemToEquipment" && m.Code != "$NoPlaceToEquipTackleOnGlobal").ToList<HintMessage>();
			if (list.Count > 0)
			{
				return list;
			}
			return messages;
		}
	}
}
