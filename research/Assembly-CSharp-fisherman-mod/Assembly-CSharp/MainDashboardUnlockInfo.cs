using System;
using TMPro;
using UnityEngine;

public class MainDashboardUnlockInfo : MonoBehaviour
{
	public void SetActive(bool flag)
	{
		this._content.gameObject.SetActive(flag);
	}

	public void SetLvlInfo(string text)
	{
		this._lvlInfo.text = text;
	}

	[SerializeField]
	private TextMeshProUGUI _lvlInfo;

	[SerializeField]
	private GameObject _content;
}
