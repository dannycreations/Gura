using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

public class QuiverTipController : MonoBehaviour
{
	private void Awake()
	{
		this.quiverTips = new List<QuiverTipView>(base.GetComponentsInChildren<QuiverTipView>(true));
	}

	public void Initialize(FeederRod rod)
	{
		int i = 0;
		if (this.quiverTips == null)
		{
			this.Awake();
		}
		if (rod == null || rod.QuiverTips == null)
		{
			while (i < this.quiverTips.Count)
			{
				this.quiverTips[i].gameObject.SetActive(false);
				i++;
			}
			return;
		}
		foreach (QuiverTip quiverTip in rod.QuiverTips)
		{
			if (this.quiverTips.Count <= i)
			{
				this.quiverTips.Add(Object.Instantiate<QuiverTipView>(this.quiverTips[0], base.transform));
			}
			QuiverTipView quiverTipView = this.quiverTips[i++];
			quiverTipView.Set(rod, quiverTip);
		}
		while (i < this.quiverTips.Count)
		{
			this.quiverTips[i].gameObject.SetActive(false);
			i++;
		}
	}

	private List<QuiverTipView> quiverTips;
}
