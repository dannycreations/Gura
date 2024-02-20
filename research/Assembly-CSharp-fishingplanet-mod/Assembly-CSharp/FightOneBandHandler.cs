using System;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FightOneBandHandler : MonoBehaviour
{
	public DamageOneBandHandler DamageHandler
	{
		get
		{
			return this._damageHandler;
		}
	}

	internal void Start()
	{
		this.RodDamageGlow.FastHidePanel();
		this.ReelDamageGlow.FastHidePanel();
		this.LineDamageGlow.FastHidePanel();
		this.UpGlow.FastHidePanel();
		this.DownGlow.FastHidePanel();
		this.Indicator.fillAmount = 0f;
	}

	internal void Update()
	{
		if (GameFactory.Player.Rod == null || GameFactory.Player.Rod.AssembledRod == null)
		{
			return;
		}
		this.RefreshIndicator();
	}

	private void RefreshIndicator()
	{
		float indicatorForce = GameFactory.Player.Reel.IndicatorForce;
		this.Indicator.fillAmount = indicatorForce;
		if (GameFactory.Player.Reel.IsIndicatorOn && (GameFactory.Player.Line.IsOverloaded || GameFactory.Player.Reel.IsOverloaded || GameFactory.Player.Rod.IsOverloaded))
		{
			this.LineDamageGlow.ShowPanel();
			this.UpGlow.ShowPanel();
		}
		else
		{
			this.LineDamageGlow.HidePanel();
			this.UpGlow.HidePanel();
		}
		if (GameFactory.Player.Reel.IsIndicatorOn && GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish != null && GameFactory.Player.Tackle.Fish.Behavior == FishBehavior.Hook && !GameFactory.Player.Tackle.Fish.IsShowing && GameFactory.Player.Line.IsSlacked)
		{
			this.DownGlow.ShowPanel();
		}
		else if (this.DownGlow.IsShow || this.DownGlow.IsShowing)
		{
			this.DownGlow.HidePanel();
		}
	}

	[SerializeField]
	private DamageOneBandHandler _damageHandler;

	public Image Indicator;

	public AlphaFade RodDamageGlow;

	public AlphaFade ReelDamageGlow;

	public AlphaFade LineDamageGlow;

	public AlphaFade UpGlow;

	public AlphaFade DownGlow;
}
