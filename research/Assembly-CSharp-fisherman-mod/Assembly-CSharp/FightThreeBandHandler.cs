using System;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FightThreeBandHandler : MonoBehaviour
{
	public DamageThreeBandsHandler DamageHandler
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
		this.RodUpGlow.FastHidePanel();
		this.RodDownGlow.FastHidePanel();
		this.ReelUpGlow.FastHidePanel();
		this.ReelDownGlow.FastHidePanel();
		this.LineUpGlow.FastHidePanel();
		this.LineDownGlow.FastHidePanel();
	}

	internal void Update()
	{
		if (GameFactory.Player.Rod == null || GameFactory.Player.Rod.AssembledRod == null)
		{
			return;
		}
		this.RefreshRodIndicator();
		this.RefreshReelIndicator();
		this.RefreshLineIndicator();
	}

	private void RefreshRodIndicator()
	{
		if (GameFactory.Player.Reel.IsIndicatorOn)
		{
			float num = GameFactory.Player.Rod.AppliedForce / GameFactory.Player.Rod.MaxLoad;
			this.RodIndicator.fillAmount = num;
		}
		else
		{
			this.RodIndicator.fillAmount = 0f;
		}
		if (GameFactory.Player.Reel.IsIndicatorOn && GameFactory.Player.Rod.IsOverloaded)
		{
			this.RodDamageGlow.ShowPanel();
			this.RodUpGlow.ShowPanel();
		}
		else
		{
			if (this.RodDamageGlow.IsShow)
			{
				this.RodDamageGlow.HidePanel();
			}
			if (this.RodUpGlow.IsShow)
			{
				this.RodUpGlow.HidePanel();
			}
		}
	}

	private void RefreshReelIndicator()
	{
		if (GameFactory.Player.Reel.IsIndicatorOn)
		{
			float num = GameFactory.Player.Reel.AppliedForce / GameFactory.Player.Reel.MaxLoad;
			this.ReelIndicator.fillAmount = num;
		}
		else
		{
			this.ReelIndicator.fillAmount = 0f;
		}
		if (GameFactory.Player.Reel.IsIndicatorOn && GameFactory.Player.Reel.IsOverloaded)
		{
			this.ReelDamageGlow.ShowPanel();
			this.ReelUpGlow.ShowPanel();
		}
		else
		{
			if (this.ReelDamageGlow.IsShow)
			{
				this.ReelDamageGlow.HidePanel();
			}
			if (this.ReelUpGlow.IsShow)
			{
				this.ReelUpGlow.HidePanel();
			}
		}
	}

	private void RefreshLineIndicator()
	{
		if (GameFactory.Player.Reel.IsIndicatorOn)
		{
			float num = GameFactory.Player.Line.AppliedForce / GameFactory.Player.Line.MaxLoad;
			this.LineIndicator.fillAmount = num;
		}
		else
		{
			this.LineIndicator.fillAmount = 0f;
		}
		if (GameFactory.Player.Reel.IsIndicatorOn && GameFactory.Player.Line.IsOverloaded)
		{
			this.LineDamageGlow.ShowPanel();
			this.LineUpGlow.ShowPanel();
		}
		else
		{
			if (this.LineDamageGlow.IsShow)
			{
				this.LineDamageGlow.HidePanel();
			}
			if (this.LineUpGlow.IsShow)
			{
				this.LineUpGlow.HidePanel();
			}
		}
		if (GameFactory.Player.Reel.IsIndicatorOn && GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish != null && GameFactory.Player.Tackle.Fish.Behavior == FishBehavior.Hook && !GameFactory.Player.Tackle.Fish.IsShowing && GameFactory.Player.Line.IsSlacked)
		{
			this.RodDownGlow.ShowPanel();
			this.ReelDownGlow.ShowPanel();
			this.LineDownGlow.ShowPanel();
		}
		else if (this.RodDownGlow.IsShow || this.RodDownGlow.IsShowing)
		{
			if (this.RodDownGlow.IsShow)
			{
				this.RodDownGlow.HidePanel();
			}
			if (this.ReelDownGlow.IsShow)
			{
				this.ReelDownGlow.HidePanel();
			}
			if (this.LineDownGlow.IsShow)
			{
				this.LineDownGlow.HidePanel();
			}
		}
	}

	[SerializeField]
	private DamageThreeBandsHandler _damageHandler;

	public Image RodIndicator;

	public Image ReelIndicator;

	public Image LineIndicator;

	public AlphaFade RodDamageGlow;

	public AlphaFade ReelDamageGlow;

	public AlphaFade LineDamageGlow;

	public AlphaFade RodUpGlow;

	public AlphaFade RodDownGlow;

	public AlphaFade ReelUpGlow;

	public AlphaFade ReelDownGlow;

	public AlphaFade LineUpGlow;

	public AlphaFade LineDownGlow;
}
