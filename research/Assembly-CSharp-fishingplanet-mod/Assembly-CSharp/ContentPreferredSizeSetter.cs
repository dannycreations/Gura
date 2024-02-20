using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContentPreferredSizeSetter : ActivityStateControlled
{
	public void SetHeightOverride(float? heightToOverride)
	{
		this._heightOverride = heightToOverride;
	}

	public void OnTransformChildrenChanged()
	{
		this.updated = true;
	}

	protected void Awake()
	{
		if (this.isControlledByLayouts)
		{
			this.layouts = new List<LayoutElement>(base.GetComponentsInChildren<LayoutElement>());
		}
	}

	private void Update()
	{
		if (!base.ShouldUpdate())
		{
			return;
		}
		if (this.updated)
		{
			this.updated = false;
			this.SetPreferredSize();
		}
	}

	public void Refresh()
	{
		this.SetPreferredSize();
	}

	private void SetPreferredSize()
	{
		if (this.isControlledByLayouts && this.MyLayout != null && this.layouts != null)
		{
			bool flag = true;
			foreach (LayoutElement layoutElement in this.layouts)
			{
				if (!(layoutElement == this.MyLayout))
				{
					if (!layoutElement.ignoreLayout)
					{
						flag = false;
						break;
					}
				}
			}
			this.MyLayout.ignoreLayout = flag;
			return;
		}
		int childCount = base.transform.childCount;
		float num = 0f;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (!Mathf.Approximately(child.localScale.y, 0f) && child.gameObject.activeSelf)
			{
				float height = (child as RectTransform).rect.height;
				if (height > num)
				{
					num = height;
				}
			}
		}
		if (this._heightOverride != null)
		{
			num = this._heightOverride.Value;
		}
		(base.transform as RectTransform).SetSizeWithCurrentAnchors(1, num);
	}

	private bool updated;

	public bool isControlledByLayouts;

	private float? _heightOverride;

	[SerializeField]
	private LayoutElement MyLayout;

	private List<LayoutElement> layouts;
}
