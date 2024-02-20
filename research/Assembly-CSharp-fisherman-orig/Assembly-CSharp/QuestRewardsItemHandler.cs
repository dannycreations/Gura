using System;
using UnityEngine;
using UnityEngine.UI;

public class QuestRewardsItemHandler : MonoBehaviour
{
	public void Init(string xp = null, string gold = null, string silver = null)
	{
		this.SetValue(this._xp, this._xpText, xp, null);
		this.SetValue(this._goldText.gameObject, this._goldText, gold, "\ue62c");
		this.SetValue(this._silverText.gameObject, this._silverText, silver, "\ue62b");
	}

	private void SetValue(GameObject go, Text text, string value, string ico)
	{
		bool flag = !string.IsNullOrEmpty(value);
		if (flag)
		{
			float num;
			flag = float.TryParse(value, out num);
			if (flag)
			{
				flag = num > 0f;
			}
		}
		go.SetActive(flag);
		if (go.activeSelf)
		{
			text.text = ((!string.IsNullOrEmpty(ico)) ? string.Format("{1} {0}", value, ico) : value);
		}
	}

	[SerializeField]
	private GameObject _xp;

	[SerializeField]
	private Text _xpText;

	[SerializeField]
	private Text _goldText;

	[SerializeField]
	private Text _silverText;
}
