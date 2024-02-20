using System;
using System.Collections.Generic;
using System.Globalization;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using InControl;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class DayDetailsResultInit : MonoBehaviour
{
	public void Update()
	{
		if (base.GetComponent<AlphaFade>() != null && (ControlsController.ControlsActions.Space.WasPressedMandatory || ControlsController.ControlsActions.GetMouseButtonDownMandatory(0) || InputManager.ActiveDevice.Action2.WasPressed) && !base.GetComponent<AlphaFade>().IsHiding)
		{
			base.GetComponent<AlphaFade>().HidePanel();
			base.GetComponent<EventAction>().DoAction();
		}
	}

	public virtual void InitMissionResult(PeriodStats result)
	{
		this.Clear();
		if (result.Achievements == null || result.Achievements.Count == 0)
		{
			this.NoAchivText.SetActive(true);
		}
		else
		{
			this.NoAchivText.SetActive(false);
		}
		Text silversReward = this.SilversReward;
		int? fishSilver = result.FishSilver;
		silversReward.text = ((fishSilver == null) ? 0 : fishSilver.Value).ToString(CultureInfo.InvariantCulture);
		Text goldsReward = this.GoldsReward;
		int? fishGold = result.FishGold;
		goldsReward.text = ((fishGold == null) ? 0 : fishGold.Value).ToString(CultureInfo.InvariantCulture);
		int num = ((result.Penalties == null) ? 0 : ((int)StatPenalty.GetSumOfPenalty(result.Penalties, "GC")));
		if (num != 0)
		{
			Debug.LogErrorFormat("There is gold penalty {0} which is not shown", new object[] { num });
		}
		this.Penalties.text = ((result.Penalties == null) ? 0 : ((int)StatPenalty.GetSumOfPenalty(result.Penalties, "GC"))).ToString(CultureInfo.InvariantCulture);
		this.TotalFishes.text = result.FishCount.ToString(CultureInfo.InvariantCulture);
		this.TrophyFishes.text = result.TrophyCount.ToString(CultureInfo.InvariantCulture);
		this.UniqueFishes.text = result.UniqueCount.ToString(CultureInfo.InvariantCulture);
		this.Escapes.text = result.Escapes.ToString(CultureInfo.InvariantCulture);
		this.Hitches.text = result.Hitches.ToString(CultureInfo.InvariantCulture);
		this.LineBreaks.text = result.LineBreaks.ToString(CultureInfo.InvariantCulture);
		this.OtherBreaks.text = result.OtherBreaks.ToString(CultureInfo.InvariantCulture);
		this.Images.Clear();
		if (result.Achievements != null)
		{
			if (result.Achievements.Count > 0)
			{
				this.Achiv1.SetActive(true);
				this.Achiv1.transform.Find("Name").GetComponent<Text>().text = result.Achievements[0].Name;
				this.Images.Add(new ResourcesHelpers.AsyncLoadableImage());
				this.Images[0].Image = this.Achiv1.transform.Find("Image").GetComponent<Image>();
				this.Images[0].Load(string.Format("Textures/Inventory/{0}", result.Achievements[0].CurrentStage.SignBID));
			}
			if (result.Achievements.Count > 1)
			{
				this.Achiv2.SetActive(true);
				this.Achiv2.transform.Find("Name").GetComponent<Text>().text = result.Achievements[1].Name;
				this.Images.Add(new ResourcesHelpers.AsyncLoadableImage());
				this.Images[1].Image = this.Achiv2.transform.Find("Image").GetComponent<Image>();
				this.Images[1].Load(string.Format("Textures/Inventory/{0}", result.Achievements[1].CurrentStage.SignBID));
			}
			if (result.Achievements.Count > 2)
			{
				this.Achiv3.SetActive(true);
				this.Achiv3.transform.Find("Name").GetComponent<Text>().text = result.Achievements[2].Name;
				this.Images.Add(new ResourcesHelpers.AsyncLoadableImage());
				this.Images[2].Image = this.Achiv3.transform.Find("Image").GetComponent<Image>();
				this.Images[2].Load(string.Format("Textures/Inventory/{0}", result.Achievements[2].CurrentStage.SignBID));
			}
			if (result.Achievements.Count > 3)
			{
				this.Achiv4.SetActive(true);
				this.Achiv4.transform.Find("Name").GetComponent<Text>().text = result.Achievements[3].Name;
				this.Images.Add(new ResourcesHelpers.AsyncLoadableImage());
				this.Images[3].Image = this.Achiv4.transform.Find("Image").GetComponent<Image>();
				this.Images[3].Load(string.Format("Textures/Inventory/{0}", result.Achievements[3].CurrentStage.SignBID));
			}
			if (result.Achievements.Count > 4)
			{
				this.Achiv5.SetActive(true);
				this.Achiv5.transform.Find("Name").GetComponent<Text>().text = result.Achievements[4].Name;
				this.Images.Add(new ResourcesHelpers.AsyncLoadableImage());
				this.Images[4].Image = this.Achiv4.transform.Find("Image").GetComponent<Image>();
				this.Images[4].Load(string.Format("Textures/Inventory/{0}", result.Achievements[4].CurrentStage.SignBID));
			}
		}
		this.FillFishInKeepnet();
		this.FillDayResult(result);
	}

	protected virtual void FillDayResult(PeriodStats result)
	{
		this.ExpTotal.text = result.Experience.ToString(CultureInfo.InvariantCulture);
		int num = 0;
		int? fishSilver = result.FishSilver;
		if (fishSilver != null)
		{
			num += result.FishSilver.Value;
		}
		int? silver = result.Silver;
		if (silver != null)
		{
			num += result.Silver.Value;
		}
		if (result.Penalties != null)
		{
			num -= (int)StatPenalty.GetSumOfPenalty(result.Penalties, "SC");
		}
		int num2 = 0;
		int? fishGold = result.FishGold;
		if (fishGold != null)
		{
			num2 += result.FishGold.Value;
		}
		int? gold = result.Gold;
		if (gold != null)
		{
			num2 += result.Gold.Value;
		}
		if (result.Penalties != null)
		{
			num2 -= (int)StatPenalty.GetSumOfPenalty(result.Penalties, "GC");
		}
		this.SilversTotal.text = num.ToString(CultureInfo.InvariantCulture);
		this.GoldsTotal.text = num2.ToString(CultureInfo.InvariantCulture);
	}

	protected virtual void FillFishInKeepnet()
	{
		FishCageContents fishCage = PhotonConnectionFactory.Instance.Profile.FishCage;
		float num = 1f;
		if (fishCage != null)
		{
			num = (float)(this.FishListContent.GetComponent<VerticalLayoutGroup>().padding.top + this.FishListContent.GetComponent<VerticalLayoutGroup>().padding.bottom) + this.FishListContent.GetComponent<VerticalLayoutGroup>().spacing * (float)(fishCage.Fish.Count - 1) + this.FishPrefab.GetComponent<LayoutElement>().preferredHeight * (float)fishCage.Fish.Count;
		}
		this.SetSizeForContent(num);
		int num2 = 0;
		if (fishCage != null)
		{
			foreach (CaughtFish caughtFish in fishCage.Fish)
			{
				GameObject gameObject = GUITools.AddChild(this.FishListContent, this.FishPrefab);
				if (num2 % 2 != 0)
				{
					gameObject.GetComponent<Image>().enabled = false;
				}
				gameObject.GetComponent<ConcreteFishInMissionResult>().InitItem(caughtFish, fishCage.Fish.Count);
				num2++;
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
		this.Achiv1.SetActive(false);
		this.Achiv2.SetActive(false);
		this.Achiv3.SetActive(false);
		this.Achiv4.SetActive(false);
		this.Achiv5.SetActive(false);
		if (this.FishListContent != null)
		{
			for (int i = 0; i < this.FishListContent.transform.childCount; i++)
			{
				Object.Destroy(this.FishListContent.transform.GetChild(i).gameObject);
			}
		}
	}

	public Text SilversReward;

	public Text GoldsReward;

	public Text Penalties;

	public Text TotalFishes;

	public Text TrophyFishes;

	public Text UniqueFishes;

	public Text Escapes;

	public Text Hitches;

	public Text LineBreaks;

	public Text OtherBreaks;

	public GameObject NoAchivText;

	public GameObject Achiv1;

	public GameObject Achiv2;

	public GameObject Achiv3;

	public GameObject Achiv4;

	public GameObject Achiv5;

	private List<ResourcesHelpers.AsyncLoadableImage> Images = new List<ResourcesHelpers.AsyncLoadableImage>();

	public Text SilversTotal;

	public Text GoldsTotal;

	public Text ExpTotal;

	public GameObject FishListContent;

	public Scrollbar FishesScrollbar;

	public GameObject FishPrefab;

	public GameObject NoFishText;
}
