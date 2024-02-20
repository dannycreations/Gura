using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Friction6SpeedHandler : MonoBehaviour
{
	protected virtual void Awake()
	{
		this.Speeds = new GameObject[] { this.Speed0, this.Speed1, this.Speed2, this.Speed3, this.Speed4 };
		this.Sectors = new GameObject[] { this.Sector1, this.Sector2, this.Sector3, this.Sector4, this.Sector5, this.Sector6 };
	}

	private void Start()
	{
		this.UpdateSpeed();
		this.UpdateFriction();
	}

	public void DoFade(float alpha, float time)
	{
		this.Fade(this.Speeds, alpha, time, this.TweenerSpeeds);
	}

	public void DoFadeSectors(float alpha, float time)
	{
		this.Fade(this.Sectors, alpha, time, this.TweenerSectors);
	}

	protected virtual void Fade(GameObject[] objects, float alpha, float time, List<Tweener> tweenerContainer)
	{
		this.StopFades(tweenerContainer);
		for (int i = 0; i < objects.Length; i++)
		{
			CanvasGroup component = objects[i].GetComponent<CanvasGroup>();
			if (component.gameObject.activeSelf)
			{
				tweenerContainer.Add(ShortcutExtensions.DOFade(component, alpha, time));
			}
			else
			{
				component.alpha = alpha;
			}
		}
	}

	protected virtual void StopFades(List<Tweener> tweenerContainer)
	{
		for (int i = 0; i < tweenerContainer.Count; i++)
		{
			TweenExtensions.Kill(tweenerContainer[i], false);
		}
		tweenerContainer.Clear();
	}

	protected virtual void UpdateSpeed()
	{
		if (GameFactory.Player.Reel == null)
		{
			return;
		}
		this._currentReelSpeed = GameFactory.Player.Reel.CurrentReelSpeedSection;
		for (int i = 0; i < this.Speeds.Length; i++)
		{
			this.Speeds[i].SetActive(this._currentReelSpeed == i);
		}
	}

	private void Update()
	{
		if (GameFactory.Player.Reel == null)
		{
			return;
		}
		if (GameFactory.Player.Reel.CurrentReelSpeedSection != this._currentReelSpeed)
		{
			this.UpdateSpeed();
		}
		if (GameFactory.Player.Reel.CurrentFrictionSection != this._currentFrictionSpeed)
		{
			this.UpdateFriction();
		}
		if (GameFactory.Player.Reel.IsFrictioning)
		{
			this.Glow.alpha = Mathf.Min(1f, this.Glow.alpha + Time.deltaTime * 4f);
		}
		else
		{
			this.Glow.alpha = Mathf.Max(0f, this.Glow.alpha - Time.deltaTime * 4f);
		}
	}

	protected virtual void UpdateFriction()
	{
		if (GameFactory.Player.Reel == null)
		{
			return;
		}
		this._currentFrictionSpeed = GameFactory.Player.Reel.CurrentFrictionSection;
		this.Sector1.SetActive(false);
		this.Sector2.SetActive(false);
		this.Sector3.SetActive(false);
		this.Sector4.SetActive(false);
		this.Sector5.SetActive(false);
		this.Sector6.SetActive(false);
		if ((byte)this._currentFrictionSpeed > 0)
		{
			this.Sector1.SetActive(true);
		}
		if ((byte)this._currentFrictionSpeed > 1)
		{
			this.Sector2.SetActive(true);
		}
		if ((byte)this._currentFrictionSpeed > 2)
		{
			this.Sector3.SetActive(true);
		}
		if ((byte)this._currentFrictionSpeed > 3)
		{
			this.Sector4.SetActive(true);
		}
		if ((byte)this._currentFrictionSpeed > 4)
		{
			this.Sector5.SetActive(true);
		}
		if ((byte)this._currentFrictionSpeed > 5)
		{
			this.Sector6.SetActive(true);
		}
	}

	public GameObject Speed0;

	public GameObject Speed1;

	public GameObject Speed2;

	public GameObject Speed3;

	public GameObject Speed4;

	public CanvasGroup Glow;

	public GameObject Sector1;

	public GameObject Sector2;

	public GameObject Sector3;

	public GameObject Sector4;

	public GameObject Sector5;

	public GameObject Sector6;

	private int _currentReelSpeed;

	protected int _currentFrictionSpeed;

	protected GameObject[] Speeds;

	protected GameObject[] Sectors;

	protected List<Tweener> TweenerSpeeds = new List<Tweener>();

	protected List<Tweener> TweenerSectors = new List<Tweener>();
}
