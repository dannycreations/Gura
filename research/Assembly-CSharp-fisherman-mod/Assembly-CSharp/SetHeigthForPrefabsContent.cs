using System;
using UnityEngine;
using UnityEngine.UI;

public class SetHeigthForPrefabsContent : MonoBehaviour
{
	private void Update()
	{
		if (this._currentItems == base.transform.childCount)
		{
			return;
		}
		this._currentItems = base.transform.childCount;
		float num = (float)base.GetComponent<VerticalLayoutGroup>().padding.top;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			num += base.transform.GetChild(i).GetComponent<LayoutElement>().preferredHeight;
			num += base.GetComponent<VerticalLayoutGroup>().spacing;
		}
		num += (float)base.GetComponent<VerticalLayoutGroup>().padding.bottom;
		base.GetComponent<RectTransform>().sizeDelta = new Vector2(base.GetComponent<RectTransform>().sizeDelta.x, num);
	}

	private int _currentItems;
}
