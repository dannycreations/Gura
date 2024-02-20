using System;
using UnityEngine;

public class SchoolBubbles : MonoBehaviour
{
	public void Start()
	{
		if (this._bubbleParticles == null)
		{
			base.transform.GetComponent<ParticleSystem>();
		}
	}

	public void EmitBubbles(Vector3 pos, float amount)
	{
		float num = amount * this._speedEmitMultiplier;
		if (num < 1f)
		{
			return;
		}
		this._bubbleParticles.transform.position = pos;
		this._bubbleParticles.Emit(Mathf.Clamp((int)(amount * this._speedEmitMultiplier), this._minBubbles, this._maxBubbles));
	}

	public ParticleSystem _bubbleParticles;

	public float _emitEverySecond = 0.01f;

	public float _speedEmitMultiplier = 0.25f;

	public int _minBubbles;

	public int _maxBubbles = 5;
}
