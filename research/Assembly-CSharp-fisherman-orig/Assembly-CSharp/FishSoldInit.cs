using System;
using System.Collections.Generic;
using System.Globalization;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FishSoldInit : MonoBehaviour
{
	public void Init(List<Fish> fishes, int? money, int? gold, float? exp, int fishCount)
	{
		this.Clear();
		float num = 0f;
		if (fishes != null && fishes.Count > 0)
		{
			num = fishes.SumFloat((Fish p) => p.Weight);
		}
		Debug.Log("Fish count in fish cage: " + fishes.Count);
		this.TotalFishes.text = string.Format("{0} ({1} {2})", fishes.Count.ToString(CultureInfo.InvariantCulture), MeasuringSystemManager.FishWeight(num).ToString("N3"), MeasuringSystemManager.FishWeightSufix());
		FishCageContents fishCage = PhotonConnectionFactory.Instance.Profile.FishCage;
		float num2 = 1f;
		if (fishCage != null)
		{
			num2 = (float)(this.FishListContent.GetComponent<VerticalLayoutGroup>().padding.top + this.FishListContent.GetComponent<VerticalLayoutGroup>().padding.bottom) + this.FishListContent.GetComponent<VerticalLayoutGroup>().spacing * (float)(fishCage.Fish.Count - 1) + this.FishPrefab.GetComponent<LayoutElement>().preferredHeight * (float)fishCage.Fish.Count;
		}
		this.SetSizeForContent(num2);
		int num3 = 0;
		if (fishCage != null)
		{
			foreach (CaughtFish caughtFish in fishCage.Fish)
			{
				GameObject gameObject = GUITools.AddChild(this.FishListContent, this.FishPrefab);
				gameObject.GetComponent<ConcreteFishInMissionResult>().InitItem(caughtFish, fishCount);
				num3++;
			}
			if (fishCage.Fish.Count > 0)
			{
				this.NoFishText.SetActive(false);
			}
			else
			{
				this.NoFishText.GetComponent<Text>().text = ScriptLocalization.Get("NoFishesText");
			}
		}
		else
		{
			this.NoFishText.GetComponent<Text>().text = ScriptLocalization.Get("NoFishkeeperText");
		}
		this.SilversTotal.text = ((money == null) ? "0" : money.ToString());
		this.GoldsTotal.text = ((gold == null) ? "0" : gold.ToString());
		this.ExpTotal.text = ((exp == null) ? "0" : ((int)exp.Value).ToString());
		if (fishCount == -1)
		{
			this._goldCaption.SetActive(false);
			this._moneyCaption.SetActive(false);
			this._fishesCaption.anchoredPosition = new Vector2(this._fishesCaption.anchoredPosition.x + 200f, this._fishesCaption.anchoredPosition.y);
			this._xpCaption.anchoredPosition = new Vector2(this._xpCaption.anchoredPosition.x + 200f, this._xpCaption.anchoredPosition.y);
		}
	}

	private void SetSizeForContent(float height)
	{
		RectTransform component = this.FishListContent.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(component.sizeDelta.x, Mathf.Max(height, this.FishListContent.transform.parent.GetComponent<RectTransform>().rect.height));
		component.anchoredPosition = new Vector3(0f, 0f - height / 2f, 0f);
		Scrollbar fishesScrollbar = this.FishesScrollbar;
		if (component.sizeDelta.y > this.FishListContent.transform.parent.GetComponent<RectTransform>().rect.height)
		{
			fishesScrollbar.gameObject.SetActive(true);
			fishesScrollbar.GetComponent<Scrollbar>().value = 1f;
		}
		else
		{
			fishesScrollbar.gameObject.SetActive(false);
		}
	}

	private void Clear()
	{
		if (this.FishListContent != null)
		{
			for (int i = 0; i < this.FishListContent.transform.childCount; i++)
			{
				Object.Destroy(this.FishListContent.transform.GetChild(i).gameObject);
			}
		}
	}

	[SerializeField]
	private GameObject _goldCaption;

	[SerializeField]
	private GameObject _moneyCaption;

	[SerializeField]
	private RectTransform _fishesCaption;

	[SerializeField]
	private RectTransform _xpCaption;

	public Text TotalFishes;

	public Text SilversTotal;

	public Text GoldsTotal;

	public Text ExpTotal;

	public GameObject FishListContent;

	public Scrollbar FishesScrollbar;

	public GameObject FishPrefab;

	public GameObject NoFishText;

	private const float FishReleasedOffsetX = 200f;

	private const string fishesFormatString = "{0} ({1} {2})";
}
