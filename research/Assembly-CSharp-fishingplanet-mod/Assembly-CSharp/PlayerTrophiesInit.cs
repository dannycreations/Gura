using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTrophiesInit : MonoBehaviour
{
	public void Init(IDictionary<int, FishStat> fishStats, bool is3DInit = false)
	{
		TrophiesInit.FillFishStat(this._list, fishStats);
		this.ShowItems(is3DInit);
	}

	private void ShowItems(bool is3DInit = false)
	{
		this.TrophyCount.text = string.Format("({0})", this._list.Count);
		for (int i = 0; i < this._list.Count; i++)
		{
			GameObject gameObject = GUITools.AddChild(this.ContentPanel, this.TrophyPrefab);
			gameObject.GetComponent<ConcreteTrophyCompactInit>().Refresh(this._list[i], is3DInit);
			Image component = gameObject.GetComponent<Image>();
			component.enabled = i % 2 == 0;
			component.color = this.ListLineColor;
		}
	}

	public void Clear()
	{
		for (int i = 0; i < this.ContentPanel.transform.childCount; i++)
		{
			Object.Destroy(this.ContentPanel.transform.GetChild(i).gameObject);
		}
	}

	public GameObject ContentPanel;

	public GameObject TrophyPrefab;

	public Text TrophyCount;

	private List<FishStat> _list = new List<FishStat>();

	public Color ListLineColor;
}
