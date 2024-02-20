using System;
using UnityEngine;
using UnityEngine.UI;

public class ColorPulsation : MonoBehaviour
{
	private void OnEnable()
	{
		this._timeStart = Time.time + this.PulseTime * 0.5f;
	}

	private void OnDisable()
	{
		this.SetColor(this.StartColor);
	}

	protected virtual void Start()
	{
		this._imageToPulse = base.GetComponent<Image>();
		if (this._imageToPulse != null)
		{
			this.StartColor = this._imageToPulse.color;
		}
	}

	protected void Update()
	{
		float num = Mathf.PingPong(Mathf.Abs(Time.time - this._timeStart) * 2f / this.PulseTime, 1f);
		Color color = Color.Lerp(this.StartColor, new Color(this.StartColor.r, this.StartColor.g, this.StartColor.b, this.MinAlpha), num);
		this.SetColor(color);
	}

	protected virtual void SetColor(Color newColor)
	{
		if (this._imageToPulse != null)
		{
			this._imageToPulse.color = newColor;
		}
	}

	public virtual void SetImage(Image im)
	{
		this._imageToPulse = im;
	}

	[SerializeField]
	public Color StartColor;

	[SerializeField]
	[Range(0f, 1f)]
	public float MinAlpha = 0.5f;

	[SerializeField]
	public float PulseTime = 2f;

	private Image _imageToPulse;

	private float _timeStart;
}
