using System;
using UnityEngine;

public abstract class BaseAlphaTween : MonoBehaviour
{
	public void Play()
	{
		this._starTime = DateTime.Now.Ticks;
		this._isPlaying = true;
	}

	public void Stop()
	{
		this._isPlaying = false;
	}

	protected float GetCalculateValue(float from, float to, float durat, long deltaTime)
	{
		if (durat == 0f || from == to)
		{
			return to;
		}
		if (from < to)
		{
			return Mathf.Abs(to - from) / (durat * 1000f) * (float)deltaTime;
		}
		if (from > to)
		{
			return from - Mathf.Abs(to - from) / (durat * 1000f) * (float)deltaTime;
		}
		return to;
	}

	public float From = 1f;

	public float To = 1f;

	public float Duration = 1f;

	[HideInInspector]
	protected long _starTime;

	protected bool _isPlaying;
}
