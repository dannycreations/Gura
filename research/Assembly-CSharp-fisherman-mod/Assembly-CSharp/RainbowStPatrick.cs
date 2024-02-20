using System;
using UnityEngine;

public class RainbowStPatrick : MonoBehaviour
{
	private void Awake()
	{
		MeshRenderer component = base.GetComponent<MeshRenderer>();
		component.sharedMaterial = new Material(component.material);
		this._material = component.sharedMaterial;
		this._defaultColor = this._material.GetColor("_Color");
	}

	private void OnEnable()
	{
		this._material.SetColor("_Color", new Color(this._defaultColor.r, this._defaultColor.g, this._defaultColor.b, 0f));
		this._curTimer = this._timers.AddTimer(RainbowStPatrick.Timers.FadeIn, this._fadeDuration, delegate
		{
			this._curTimer = this._timers.AddTimer(RainbowStPatrick.Timers.Live, this._liveDuration, delegate
			{
				this._curTimer = this._timers.AddTimer(RainbowStPatrick.Timers.FadeOut, this._fadeDuration, delegate
				{
					base.gameObject.SetActive(false);
				}, false);
			}, false);
		}, false);
	}

	private void Update()
	{
		this._timers.Update(Time.deltaTime);
		if (this._curTimer.Name == RainbowStPatrick.Timers.FadeIn)
		{
			this._material.SetColor("_Color", new Color(this._defaultColor.r, this._defaultColor.g, this._defaultColor.b, Mathf.Lerp(0f, this._defaultColor.a, this._curTimer.Prc)));
		}
		else if (this._curTimer.Name == RainbowStPatrick.Timers.FadeOut)
		{
			this._material.SetColor("_Color", new Color(this._defaultColor.r, this._defaultColor.g, this._defaultColor.b, Mathf.Lerp(this._defaultColor.a, 0f, this._curTimer.Prc)));
		}
	}

	[SerializeField]
	private float _fadeDuration;

	[SerializeField]
	private float _liveDuration;

	private Material _material;

	private TimerCore<RainbowStPatrick.Timers> _timers = new TimerCore<RainbowStPatrick.Timers>();

	private TimerCore<RainbowStPatrick.Timers>.Timer _curTimer;

	private Color _defaultColor;

	private enum Timers
	{
		FadeIn,
		Live,
		FadeOut
	}
}
