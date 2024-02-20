using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class WearTackleController : MonoBehaviour
{
	internal void OnEnable()
	{
		if (WearTackleController.BackFromTravel)
		{
			WearTackleController.BackFromTravel = false;
			Inventory inventory = PhotonConnectionFactory.Instance.Profile.Inventory;
			List<InventoryItem> list = inventory.Where(delegate(InventoryItem x)
			{
				if (x.IsRepairable)
				{
					int? maxDurability = x.MaxDurability;
					float? num = ((maxDurability == null) ? null : new float?((float)maxDurability.Value));
					float? num2 = ((num == null) ? null : new float?(num.GetValueOrDefault() * 0.7f));
					if ((float)x.Durability < num2 && (float)x.Durability > 0f)
					{
						int num3;
						float num4;
						string text;
						inventory.PreviewRepair(x, out num3, out num4, out text);
						return num3 > 0;
					}
				}
				return false;
			}).ToList<InventoryItem>();
			if (list.Count > 0)
			{
				this._messageBox = GUITools.AddChild(MessageBoxList.Instance.gameObject, MessageBoxList.Instance.wearTacklePrefab).GetComponent<WearTackleInit>();
				this._messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
				this._messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
				this._messageBox.InitContent(list);
			}
		}
	}

	public static bool BackFromTravel;

	private WearTackleInit _messageBox;

	private const float MaxDurabilityRepair = 0.7f;
}
