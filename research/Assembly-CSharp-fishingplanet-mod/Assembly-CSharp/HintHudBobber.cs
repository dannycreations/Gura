using System;
using UnityEngine;

public class HintHudBobber : HintColorImageChildren
{
	protected override void Update()
	{
		base.Update();
		PlayerController player = GameFactory.Player;
		if (this._shines.Count > 0 && player != null && player.HudFishingHandler != null)
		{
			RectTransform rectTransform = this._shines[0];
			bool flag = rectTransform.anchoredPosition.x >= -this.ShineWidth && rectTransform.anchoredPosition.x <= this.ShineWidth;
			player.HudFishingHandler.SetHintActive(flag);
			if (flag)
			{
				player.HudFishingHandler.SetBobberColor(this._highlightColor);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (GameFactory.Player != null && GameFactory.Player.HudFishingHandler != null)
		{
			GameFactory.Player.HudFishingHandler.SetHintActive(false);
		}
	}

	[SerializeField]
	protected float ShineWidth = 20f;
}
