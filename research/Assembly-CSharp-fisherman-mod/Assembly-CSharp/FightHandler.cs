using System;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FightHandler : MonoBehaviour
{
	internal void Start()
	{
		this.UpGlow.gameObject.GetComponent<AlphaFade>().FastHidePanel();
		this.DownGlow.gameObject.GetComponent<AlphaFade>().FastHidePanel();
		float num = this.MaxWeight / 5f;
		this.Sector1.color = new Color(1f, 1f, 1f, this.WeightValue / num);
		this.Sector2.color = new Color(1f, 1f, 1f, (this.WeightValue - num) / num);
		this.Sector3.color = new Color(1f, 1f, 1f, (this.WeightValue - num * 2f) / num);
		this.Sector4.color = new Color(1f, 1f, 1f, (this.WeightValue - num * 3f) / num);
		this.Sector5.color = new Color(1f, 1f, 1f, (this.WeightValue - num * 4f) / num);
	}

	internal void Update()
	{
		float num = this.MaxWeight / 5f;
		this.Sector1.color = new Color(1f, 1f, 1f, this.WeightValue / num);
		this.Sector2.color = new Color(1f, 1f, 1f, (this.WeightValue - num) / num);
		this.Sector3.color = new Color(1f, 1f, 1f, (this.WeightValue - num * 2f) / num);
		this.Sector4.color = new Color(1f, 1f, 1f, (this.WeightValue - num * 3f) / num);
		this.Sector5.color = new Color(1f, 1f, 1f, (this.WeightValue - num * 4f) / num);
		if (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish != null && GameFactory.Player.Tackle.Fish.Behavior == FishBehavior.Hook && !GameFactory.Player.Tackle.Fish.IsShowing)
		{
			if (GameFactory.Player.Rod.IsFishingForced)
			{
				this.UpGlow.color = this.ErrorColor;
				this.DownGlow.gameObject.GetComponent<AlphaFade>().ShowPanel();
			}
			else if (!GameFactory.Player.Rod.IsFishingForced && GameFactory.Player.Rod.IsForced && GameFactory.Player.Reel.IsForced)
			{
				this.UpGlow.color = this.WarningColor;
				this.UpGlow.gameObject.GetComponent<AlphaFade>().ShowPanel();
			}
			else
			{
				this.UpGlow.gameObject.GetComponent<AlphaFade>().HidePanel();
			}
		}
		else
		{
			this.UpGlow.gameObject.GetComponent<AlphaFade>().HidePanel();
		}
		if (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish != null && GameFactory.Player.Tackle.Fish.Behavior == FishBehavior.Hook && !GameFactory.Player.Tackle.Fish.IsShowing)
		{
			if (GameFactory.Player.Line.IsSlacked)
			{
				this.DownGlow.color = this.ErrorColor;
				this.DownGlow.gameObject.GetComponent<AlphaFade>().ShowPanel();
			}
			else if (!GameFactory.Player.Line.IsTensioned)
			{
				this.DownGlow.color = this.WarningColor;
				this.DownGlow.gameObject.GetComponent<AlphaFade>().ShowPanel();
			}
			else
			{
				this.DownGlow.gameObject.GetComponent<AlphaFade>().HidePanel();
			}
		}
		else
		{
			this.DownGlow.gameObject.GetComponent<AlphaFade>().HidePanel();
		}
	}

	public Image Sector1;

	public Image Sector2;

	public Image Sector3;

	public Image Sector4;

	public Image Sector5;

	public Image UpGlow;

	public Image DownGlow;

	public Color WarningColor;

	public Color ErrorColor;

	public float MaxWeight = 10f;

	public float WeightValue;
}
