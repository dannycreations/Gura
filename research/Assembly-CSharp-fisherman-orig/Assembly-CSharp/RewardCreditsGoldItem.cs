using System;
using TMPro;
using UnityEngine;

public class RewardCreditsGoldItem : MonoBehaviour
{
	public void Init(string credits)
	{
		this._credits.text = credits;
	}

	[SerializeField]
	private TextMeshProUGUI _credits;
}
