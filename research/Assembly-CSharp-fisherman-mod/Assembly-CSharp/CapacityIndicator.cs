using System;
using UnityEngine;
using UnityEngine.UI;

public class CapacityIndicator : MonoBehaviour
{
	private void FixedUpdate()
	{
		if (Mathf.Abs(this.percentage - this._prev) > Mathf.Epsilon)
		{
			this.SetPrecentage(this.percentage);
		}
	}

	public void SetCapacity(int count, int capacity)
	{
		float num = (float)count / (float)capacity;
		this.Filler.fillAmount = num;
		num = Mathf.Clamp01(num);
		Color color;
		Color color2;
		if (num < this.yellowPosition)
		{
			float num2 = num / Mathf.Max(0.01f, this.yellowPosition);
			color = Color.Lerp(this.EmptyColor, this.MidColor, num2);
			color2 = Color.Lerp(Color.white, this.MidColor, num2);
		}
		else
		{
			float num3 = (num - this.yellowPosition) / Mathf.Max(0.01f, 1f - this.yellowPosition);
			color = Color.Lerp(this.MidColor, this.FullColor, num3);
			color2 = color;
		}
		this.Filler.color = color;
		if (this.Icon != null)
		{
			this.Icon.color = ((Mathf.Abs(1f - num) >= Mathf.Epsilon) ? Color.white : color2);
		}
		this._prev = num;
		this.percentage = num;
		this.Value.text = string.Format("<color=#{0}>{1}</color> / {2}", ColorUtility.ToHtmlStringRGB(color2), count, capacity);
	}

	private void SetPrecentage(float percent)
	{
		this.Filler.fillAmount = percent;
		percent = Mathf.Clamp01(percent);
		Color color;
		Color color2;
		if (percent < this.yellowPosition)
		{
			float num = percent / Mathf.Max(0.01f, this.yellowPosition);
			color = Color.Lerp(this.EmptyColor, this.MidColor, num);
			color2 = Color.Lerp(Color.white, this.MidColor, num);
		}
		else
		{
			float num2 = (percent - this.yellowPosition) / Mathf.Max(0.01f, 1f - this.yellowPosition);
			color = Color.Lerp(this.MidColor, this.FullColor, num2);
			color2 = color;
		}
		this.Filler.color = color;
		if (this.Icon != null)
		{
			this.Icon.color = ((Mathf.Abs(1f - percent) >= Mathf.Epsilon) ? Color.white : color2);
		}
		this._prev = this.percentage;
		this.percentage = percent;
	}

	[SerializeField]
	private Text Icon;

	[SerializeField]
	private Text Value;

	[SerializeField]
	private Image Filler;

	[SerializeField]
	private Color FullColor;

	[SerializeField]
	private Color MidColor;

	[SerializeField]
	private Color EmptyColor;

	[Range(0f, 1f)]
	public float percentage;

	[Range(0f, 1f)]
	public float yellowPosition = 0.4f;

	private float _prev;
}
