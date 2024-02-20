using System;
using UnityEngine;

public class SetDefaultValueForHandleScroll : MonoBehaviour
{
	private void Start()
	{
		RectTransform component = base.GetComponent<RectTransform>();
		if (this.SetX)
		{
			component.anchoredPosition = new Vector2(this.X, component.anchoredPosition.y);
		}
		if (this.SetY)
		{
			component.anchoredPosition = new Vector2(component.anchoredPosition.x, this.Y);
		}
		if (this.SetLeft)
		{
			component.offsetMin = new Vector2(this.Left, component.offsetMin.y);
		}
		if (this.SetTop)
		{
			component.offsetMax = new Vector2(component.offsetMax.x, this.Top);
		}
		if (this.SetRight)
		{
			component.offsetMax = new Vector2(this.Right, component.offsetMax.y);
		}
		if (this.SetBottom)
		{
			component.offsetMin = new Vector2(component.offsetMin.x, this.Bottom);
		}
	}

	public bool SetX;

	public float X;

	public bool SetY;

	public float Y;

	public bool SetTop;

	public float Top;

	public bool SetLeft;

	public float Left;

	public bool SetRight;

	public float Right;

	public bool SetBottom;

	public float Bottom;
}
