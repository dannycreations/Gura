using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardPlaceItem : MonoBehaviour
{
	public void Init(int place, string credits)
	{
		this._credits.text = credits;
		this._place.text = place.ToString();
		if (place < 4)
		{
			this._cup.overrideSprite = this._cups[place - 1];
			this._cup.gameObject.SetActive(true);
			this._place.text = string.Format("<b>{0}</b>", this._place.text);
		}
	}

	[SerializeField]
	private TextMeshProUGUI _place;

	[SerializeField]
	private TextMeshProUGUI _credits;

	[SerializeField]
	private Image _cup;

	[SerializeField]
	private Sprite[] _cups;
}
