using System;
using UnityEngine;

public class DamageOneBandHandler : DamageBandsHandlerBase
{
	private void Update()
	{
		if (!this.IsEnabled || base.IsHintActive)
		{
			if (base.IsHintActive && this._cgs != null && this._cgs.Length > 0 && this._cgs[0].alpha < 1f)
			{
				for (int i = 0; i < this._cgs.Length; i++)
				{
					this._cgs[i].alpha = 1f;
				}
			}
			return;
		}
		this.UpdateCg(GameFactory.Player.Reel.ForceRuller != RullerTackleType.None, this.FrameCG, 0.2f);
		this.Init();
		this.Refresh(this.Items, this.DurabilityImages, this.DamageTexts, this._cgs, this.RullerTypes, new float?(0.2f));
	}

	protected override void Init()
	{
		base.Init();
		if (this._cgs == null)
		{
			this._cgs = new CanvasGroup[] { this.RodCG, this.ReelCG, this.LineCG };
		}
	}

	public CanvasGroup RodCG;

	public CanvasGroup ReelCG;

	public CanvasGroup LineCG;

	public CanvasGroup FrameCG;

	private const float MinAlpha = 0.2f;

	private CanvasGroup[] _cgs;
}
