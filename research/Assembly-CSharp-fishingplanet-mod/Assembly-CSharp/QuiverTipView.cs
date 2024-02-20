using System;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuiverTipView : MonoBehaviour
{
	public void Set(FeederRod rod, QuiverTip tip)
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(true);
		}
		if (tip.IsBroken)
		{
			this.Button.interactable = false;
			this.Icon.text = "\ue69c";
		}
		else
		{
			this.Button.onClick.RemoveAllListeners();
			this.Button.onClick.AddListener(delegate
			{
				PhotonConnectionFactory.Instance.SetActiveQuiverTip(rod, tip.ItemId);
			});
			this.Button.interactable = true;
			this.Icon.text = string.Empty;
		}
		this.Body.color = RodBehaviour.QuiverTipColorToRGB(tip.Color);
		this.SetActive(rod.QuiverId == tip.ItemId);
	}

	public void SetActive(bool isActive)
	{
		this.Body.fontSize = (float)((!isActive) ? 16 : 24);
	}

	[SerializeField]
	private Button Button;

	[SerializeField]
	private TextMeshProUGUI Body;

	[SerializeField]
	private TextMeshProUGUI Icon;

	private const string QuiverTipBroken = "\ue69c";

	private const int defaultSize = 16;

	private const int activeSize = 24;
}
