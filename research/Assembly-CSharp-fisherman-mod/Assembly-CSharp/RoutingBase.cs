using System;
using System.Diagnostics;
using UnityEngine;

public abstract class RoutingBase : MonoBehaviour
{
	public abstract void Play();

	public abstract void Stop();

	public abstract Vector3 Position { get; }

	public abstract Quaternion Rotation { get; }

	public string Tag
	{
		get
		{
			return base.gameObject.tag;
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool, float> EFade = delegate
	{
	};

	protected void Fade(bool flag)
	{
		this.EFade(flag, this._fadeTime);
	}

	[SerializeField]
	protected float _fadeTime = 0.5f;
}
