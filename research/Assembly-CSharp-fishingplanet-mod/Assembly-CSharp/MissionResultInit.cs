using System;
using System.Collections.Generic;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class MissionResultInit : DayResultInit
{
	protected override void Awake()
	{
		base.Awake();
		string text = "YourAccountCaption";
		this._textRewards.text = ScriptLocalization.Get(text);
	}

	private void Clear()
	{
		this.Achiv1.SetActive(false);
		this.Achiv2.SetActive(false);
		this.Achiv3.SetActive(false);
		this.Achiv4.SetActive(false);
		this.Achiv5.SetActive(false);
		this.Achiv6.SetActive(false);
	}

	public override void Init(PeriodStats result)
	{
		base.Init(result);
		this.Clear();
		this.Images.Clear();
		if (result.Achievements != null)
		{
			if (result.Achievements.Count > 0)
			{
				this.Achiv1.SetActive(true);
				StatAchievement statAchievement = result.Achievements[0];
				string text = statAchievement.Name;
				if (statAchievement.CurrentStage != null)
				{
					text = text + "\n" + statAchievement.CurrentStage.Name;
				}
				this.Achiv1.transform.Find("Name").GetComponent<Text>().text = text;
				this.Images.Add(new ResourcesHelpers.AsyncLoadableImage());
				this.Images[0].Image = this.Achiv1.transform.Find("Image").GetComponent<Image>();
				this.Images[0].Load(string.Format("Textures/Inventory/{0}", result.Achievements[0].CurrentStage.SignBID));
			}
			if (result.Achievements.Count > 1)
			{
				this.Achiv2.SetActive(true);
				StatAchievement statAchievement2 = result.Achievements[1];
				string text2 = statAchievement2.Name;
				if (statAchievement2.CurrentStage != null)
				{
					text2 = text2 + "\n" + statAchievement2.CurrentStage.Name;
				}
				this.Achiv2.transform.Find("Name").GetComponent<Text>().text = text2;
				this.Images.Add(new ResourcesHelpers.AsyncLoadableImage());
				this.Images[1].Image = this.Achiv2.transform.Find("Image").GetComponent<Image>();
				this.Images[1].Load(string.Format("Textures/Inventory/{0}", result.Achievements[1].CurrentStage.SignBID));
			}
			if (result.Achievements.Count > 2)
			{
				this.Achiv3.SetActive(true);
				StatAchievement statAchievement3 = result.Achievements[2];
				string text3 = statAchievement3.Name;
				if (statAchievement3.CurrentStage != null)
				{
					text3 = text3 + "\n" + statAchievement3.CurrentStage.Name;
				}
				this.Achiv3.transform.Find("Name").GetComponent<Text>().text = text3;
				this.Images.Add(new ResourcesHelpers.AsyncLoadableImage());
				this.Images[2].Image = this.Achiv3.transform.Find("Image").GetComponent<Image>();
				this.Images[2].Load(string.Format("Textures/Inventory/{0}", result.Achievements[2].CurrentStage.SignBID));
			}
			if (result.Achievements.Count > 3)
			{
				this.Achiv4.SetActive(true);
				StatAchievement statAchievement4 = result.Achievements[3];
				string text4 = statAchievement4.Name;
				if (statAchievement4.CurrentStage != null)
				{
					text4 = text4 + "\n" + statAchievement4.CurrentStage.Name;
				}
				this.Achiv4.transform.Find("Name").GetComponent<Text>().text = text4;
				this.Images.Add(new ResourcesHelpers.AsyncLoadableImage());
				this.Images[3].Image = this.Achiv4.transform.Find("Image").GetComponent<Image>();
				this.Images[3].Load(string.Format("Textures/Inventory/{0}", result.Achievements[3].CurrentStage.SignBID));
			}
			if (result.Achievements.Count > 4)
			{
				this.Achiv5.SetActive(true);
				StatAchievement statAchievement5 = result.Achievements[4];
				string text5 = statAchievement5.Name;
				if (statAchievement5.CurrentStage != null)
				{
					text5 = text5 + "\n" + statAchievement5.CurrentStage.Name;
				}
				this.Achiv5.transform.Find("Name").GetComponent<Text>().text = text5;
				this.Images.Add(new ResourcesHelpers.AsyncLoadableImage());
				this.Images[4].Image = this.Achiv5.transform.Find("Image").GetComponent<Image>();
				this.Images[4].Load(string.Format("Textures/Inventory/{0}", result.Achievements[4].CurrentStage.SignBID));
			}
			if (result.Achievements.Count > 5)
			{
				this.Achiv6.SetActive(true);
				StatAchievement statAchievement6 = result.Achievements[5];
				string text6 = statAchievement6.Name;
				if (statAchievement6.CurrentStage != null)
				{
					text6 = text6 + "\n" + statAchievement6.CurrentStage.Name;
				}
				this.Achiv6.transform.Find("Name").GetComponent<Text>().text = text6;
				this.Images.Add(new ResourcesHelpers.AsyncLoadableImage());
				this.Images[5].Image = this.Achiv6.transform.Find("Image").GetComponent<Image>();
				this.Images[5].Load(string.Format("Textures/Inventory/{0}", result.Achievements[5].CurrentStage.SignBID));
			}
		}
	}

	[SerializeField]
	private RectTransform _leftRuller;

	[SerializeField]
	private RectTransform _rightRuller;

	[SerializeField]
	private RectTransform _challenges;

	[SerializeField]
	private RectTransform _achivementsCaption;

	[SerializeField]
	private RectTransform _achivsPanel;

	[SerializeField]
	private Text _textRewards;

	public GameObject Achiv1;

	public GameObject Achiv2;

	public GameObject Achiv3;

	public GameObject Achiv4;

	public GameObject Achiv5;

	public GameObject Achiv6;

	private List<ResourcesHelpers.AsyncLoadableImage> Images = new List<ResourcesHelpers.AsyncLoadableImage>();
}
