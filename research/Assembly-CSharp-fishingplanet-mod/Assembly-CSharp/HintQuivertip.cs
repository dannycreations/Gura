using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintQuivertip : HintColorImageChildren
{
	protected override void Init()
	{
		base.FillData<Image>(this.Data, null, true, null);
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < base.transform.parent.childCount; i++)
		{
			Transform child = base.transform.parent.GetChild(i);
			if (!child.Equals(base.transform) && child.childCount > 0)
			{
				base.FillData<Image>(this.Data, null, false, child);
				for (int j = 0; j < child.childCount; j++)
				{
					list.Add(child.GetChild(j));
				}
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			RectTransform component = Object.Instantiate<GameObject>(this._shinePrefab, list[k]).GetComponent<RectTransform>();
			component.gameObject.SetActive(false);
			this._shines.Add(component);
		}
	}

	protected override void UpdateVisual(bool isDestroy = false)
	{
		bool flag = this.shouldShow && !isDestroy;
		foreach (KeyValuePair<Image, Color> keyValuePair in this.Data)
		{
			keyValuePair.Key.color = ((!flag) ? keyValuePair.Value : this._highlightColor);
		}
		if (flag)
		{
			this._shines.ForEach(delegate(RectTransform p)
			{
				p.gameObject.SetActive(true);
			});
		}
	}
}
