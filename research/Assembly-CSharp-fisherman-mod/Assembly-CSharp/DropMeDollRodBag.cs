using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropMeDollRodBag : DropMeDoll
{
	public override void OnDrop(PointerEventData data)
	{
		base.OnDrop(data);
		MonoBehaviour.print("DropMeDollRodBag ondrop");
		if (InitRods.Instance != null)
		{
			InitRods.Instance.Refresh(null);
		}
	}
}
