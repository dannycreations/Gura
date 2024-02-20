using System;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ConcreteFishInMissionResult : MonoBehaviour
{
	public void InitItem(ConcreteFishInMissionResult.ConcreteFishResultData o)
	{
		if (o.FishCount == -1 && this.Money != null)
		{
			if (this.MoneySuffix != null)
			{
				this.MoneySuffix.gameObject.SetActive(false);
			}
			if (o.WeightAlignmentMiddleRight)
			{
				this.Weight.alignment = 5;
			}
		}
		this.Name.text = o.Name;
		this.Weight.text = string.Format("{0} {1}", MeasuringSystemManager.FishWeight(o.Weight).ToString("N3"), MeasuringSystemManager.FishWeightSufix());
		this.SetMoney(o.GoldCost, o.SilverCost);
		if (this._fishIco != null)
		{
			this.ImageLdbl.Load(o.ThumbnailBID, this._fishIco, "Textures/Inventory/{0}");
		}
		if (this._fishLength != null)
		{
			this._fishLength.text = string.Format("{0} {1}", MeasuringSystemManager.FishLength(o.Length).ToString("N3"), MeasuringSystemManager.FishLengthSufix());
		}
		if (o.ChangeColorForScoringFish && o.Score <= 0f)
		{
			this.Name.color = new Color(this.Name.color.r, this.Name.color.g, this.Name.color.b, 0.4f);
			this.Weight.color = new Color(this.Weight.color.r, this.Weight.color.g, this.Weight.color.b, 0.4f);
			this._fishLength.color = new Color(this._fishLength.color.r, this._fishLength.color.g, this._fishLength.color.b, 0.4f);
		}
	}

	public void InitItem(CaughtFish fish, int fishCount)
	{
		this.InitItem(new ConcreteFishInMissionResult.ConcreteFishResultData
		{
			FishCount = fishCount,
			GoldCost = fish.Fish.GoldCost,
			Length = fish.Fish.Length,
			Name = fish.Fish.Name,
			SilverCost = fish.Fish.SilverCost,
			ThumbnailBID = fish.Fish.ThumbnailBID,
			Weight = fish.Fish.Weight,
			WeightAlignmentMiddleRight = true
		});
	}

	private void SetMoney(float? goldCost, float? silverCost)
	{
		if (this.Money != null)
		{
			if (this.MoneySuffix == null)
			{
				if (goldCost == null && silverCost == null)
				{
					this.Money.text = "-";
				}
				else if (goldCost != null)
				{
					this.Money.text = string.Format("{0} {1}", goldCost, "\ue62c");
				}
				else
				{
					this.Money.text = string.Format("{0} {1}", silverCost, "\ue62b");
				}
			}
			else if (goldCost != null)
			{
				this.Money.text = string.Format("{0}", goldCost);
				this.MoneySuffix.text = "\ue62c";
			}
			else
			{
				this.Money.text = string.Format("{0}", silverCost);
				this.MoneySuffix.text = "\ue62b";
			}
		}
	}

	[SerializeField]
	private Image _fishIco;

	[SerializeField]
	private Text _fishLength;

	public Text Name;

	public Text Weight;

	public Text Money;

	public Text MoneySuffix;

	protected ResourcesHelpers.AsyncLoadableImage ImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	private const float ScoringFishAlpha = 0.4f;

	public class ConcreteFishResultData
	{
		public string Name { get; set; }

		public float Weight { get; set; }

		public float Length { get; set; }

		public float? GoldCost { get; set; }

		public float? SilverCost { get; set; }

		public int? ThumbnailBID { get; set; }

		public int FishCount { get; set; }

		public float Score { get; set; }

		public bool ChangeColorForScoringFish { get; set; }

		public bool WeightAlignmentMiddleRight { get; set; }
	}
}
