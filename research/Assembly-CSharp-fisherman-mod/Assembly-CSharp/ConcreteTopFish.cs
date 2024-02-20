using System;
using Assets.Scripts.UI._2D.PlayerProfile;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ConcreteTopFish : ConcreteTop
{
	private HorizontalLayoutGroup hlg
	{
		get
		{
			if (this._hlg == null)
			{
				this._hlg = base.GetComponent<HorizontalLayoutGroup>();
			}
			return this._hlg;
		}
	}

	private void Awake()
	{
		this.hlg.enabled = false;
	}

	internal void InitItem(TopFish fish, int number)
	{
		RectTransform component = this.Place.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(component.rect.width, base.GetComponent<RectTransform>().rect.height);
		RectTransform rectTransform = component;
		Vector2 vector = Vector2.zero;
		component.pivot = vector;
		vector = vector;
		component.anchorMax = vector;
		rectTransform.anchorMin = vector;
		this.hlg.enabled = true;
		this.Number.text = number.ToString();
		this.Name.text = fish.UserName;
		this.Level.text = PlayerProfileHelper.GetPlayerLevelRank(fish);
		this.Spec.text = fish.FishName;
		this.Weight.text = MeasuringSystemManager.FishWeight((float)fish.Weight).ToString("N3");
		this.Length.text = MeasuringSystemManager.FishLength((float)fish.Length).ToString("N3");
		this.Place.text = fish.PondName;
		this.UserId = fish.UserId.ToString();
		base.Invoke("DisableHLG", 0.1f);
	}

	private void DisableHLG()
	{
		this.hlg.enabled = false;
	}

	public Text Number;

	public Text Name;

	public Text Level;

	public Text Spec;

	public Text Weight;

	public Text Length;

	public Text Place;

	private HorizontalLayoutGroup _hlg;
}
