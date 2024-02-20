using System;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class MissionReceivedInit : MonoBehaviour
{
	private void Awake()
	{
		this.Caption.text = ScriptLocalization.Get("NewQuestReceivedCaption");
	}

	public void Init(HintMessage message)
	{
		this.Name.text = message.MissionName;
		if (message.ImageId != null)
		{
			this.IconLdbl.Image = this.Icon;
			this.IconLdbl.Load(string.Format("Textures/Inventory/{0}", message.ImageId.Value));
		}
		this.DescValue.text = message.DescriptionFormatted;
	}

	public void Init(MissionOnClient missionInfo)
	{
		this.Name.text = missionInfo.Name;
		if (missionInfo.ThumbnailBID != null)
		{
			this.IconLdbl.Image = this.Icon;
			this.IconLdbl.Load(string.Format("Textures/Inventory/{0}", missionInfo.ThumbnailBID.Value));
		}
		string text = string.Empty;
		foreach (MissionTaskOnClient missionTaskOnClient in missionInfo.Tasks)
		{
			if (text != string.Empty)
			{
				text += "\n";
			}
			text = text + " - " + missionTaskOnClient.Name;
		}
		this.DescValue.text = text;
	}

	public Image Icon;

	private ResourcesHelpers.AsyncLoadableImage IconLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Text Caption;

	public Text Name;

	public Text DescValue;

	private MissionOnClient _mission;
}
