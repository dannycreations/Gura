using System;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class AchivInit : MonoBehaviour
{
	public int AchivementId { get; private set; }

	public void Refresh(StatAchievement achiv)
	{
		this.AchivementId = achiv.AchivementId;
		if (achiv.CurrentStage == null)
		{
			this.Icon.color = new Color(this.Icon.color.r, this.Icon.color.g, this.Icon.color.b, 0.2f);
			this.Name.color = new Color(this.Name.color.r, this.Name.color.g, this.Name.color.b, 0.2f);
			if (this.Desc != null)
			{
				this.Desc.color = new Color(this.Desc.color.r, this.Desc.color.g, this.Desc.color.b, 0.2f);
			}
		}
		this.Name.text = string.Format("{0} {1}", achiv.Name, (achiv.CurrentStage == null) ? string.Empty : string.Format(achiv.CurrentStage.Name.ToString(), achiv.CurrentStage.Count));
		if (this.Desc != null)
		{
			this.Desc.text = achiv.Desc;
		}
		this.IconLdbl.Image = this.Icon;
		if (achiv.CurrentStage != null)
		{
			this.IconLdbl.Load((achiv.CurrentStage.SignBID == null) ? string.Empty : string.Format("Textures/Inventory/{0}", achiv.CurrentStage.SignBID.ToString()));
		}
		else
		{
			this.IconLdbl.Load((achiv.NextStage.SignBID == null) ? string.Empty : string.Format("Textures/Inventory/{0}", achiv.NextStage.SignBID.ToString()));
		}
		PopupAchivmentsInfo componentInChildren = base.GetComponentInChildren<PopupAchivmentsInfo>();
		if (componentInChildren != null && achiv.NextStage != null)
		{
			componentInChildren.OnSelected.RemoveAllListeners();
			componentInChildren.OnSelected.AddListener(delegate
			{
				PhotonConnectionFactory.Instance.ChangeSelectedElement(GameElementType.Achievement, null, new int?(achiv.AchivementId));
			});
			string text = this.TranslateUnits(achiv.NextStage.Unit, achiv.CurrentCount);
			string text2 = this.TranslateUnits(achiv.NextStage.Unit, achiv.NextStage.Count);
			componentInChildren.PopupValue = string.Format("<b>{0} ({1}/{2})</b>", achiv.Name, text, text2);
			string text3 = string.Empty;
			if (achiv.NextStage.Experience != null && achiv.NextStage.Experience != 0)
			{
				string text4 = text3;
				text3 = string.Concat(new object[]
				{
					text4,
					"\ue62d ",
					achiv.NextStage.Experience,
					" "
				});
			}
			if (achiv.NextStage.Money1 != null)
			{
				if (achiv.NextStage.Currency1 == "SC")
				{
					string text4 = text3;
					text3 = string.Concat(new object[]
					{
						text4,
						"\ue62b ",
						achiv.NextStage.Money1.Value,
						" "
					});
				}
				else
				{
					string text4 = text3;
					text3 = string.Concat(new object[]
					{
						text4,
						"\ue62c ",
						achiv.NextStage.Money1.Value,
						" "
					});
				}
			}
			if (achiv.NextStage.Money2 != null)
			{
				if (achiv.NextStage.Currency2 == "SC")
				{
					string text4 = text3;
					text3 = string.Concat(new object[]
					{
						text4,
						"\ue62b ",
						achiv.NextStage.Money2.Value,
						" "
					});
				}
				else
				{
					string text4 = text3;
					text3 = string.Concat(new object[]
					{
						text4,
						"\ue62c ",
						achiv.NextStage.Money2.Value,
						" "
					});
				}
			}
			if (text3 != string.Empty || (achiv.NextStage.ItemRewards != null && achiv.NextStage.ItemRewards.Length != 0))
			{
				string text5 = this.TranslateUnits(achiv.NextStage.Unit, achiv.NextStage.Count);
				componentInChildren.PopupValueDescription = string.Format(string.Format("{0} \n<b>{1}</b>:", achiv.NextStage.Desc, ScriptLocalization.Get("RevardAchivementPopup")), text5);
			}
			else
			{
				componentInChildren.PopupValueDescription = string.Format("{0} \n", achiv.NextStage.Desc);
			}
			componentInChildren.Rewards = achiv.NextStage.ItemRewards;
			componentInChildren.ProductRewards = achiv.NextStage.ProductRewards;
			componentInChildren.RewardValue = text3;
			componentInChildren.RewardID = achiv.NextStage.StageId;
		}
		else if (base.GetComponentInChildren<PopupAchivmentsInfo>() != null)
		{
			base.GetComponentInChildren<PopupAchivmentsInfo>().enabled = false;
		}
	}

	private string TranslateUnits(string unit, int count)
	{
		string text = count.ToString();
		if (!string.IsNullOrEmpty(unit))
		{
			if (unit == "kg")
			{
				text = MeasuringSystemManager.FishWeight((float)count).ToString("0.0");
			}
			else if (unit == "g")
			{
				text = MeasuringSystemManager.FishWeight((float)count * 0.001f).ToString("0.0");
			}
			else if (unit == "m")
			{
				text = MeasuringSystemManager.LineLength((float)count).ToString("0.0");
			}
			else if (unit == "dm")
			{
				text = MeasuringSystemManager.LineLength((float)count * 0.1f).ToString("0.0");
			}
		}
		return text;
	}

	public Image Icon;

	private ResourcesHelpers.AsyncLoadableImage IconLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Text Name;

	public Text Desc;
}
