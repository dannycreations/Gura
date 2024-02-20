using System;
using UnityEngine;

public class DelayedEnabler : MonoBehaviour
{
	private void Awake()
	{
		if (this._withAnimation)
		{
			this._animation = base.GetComponent<Animation>();
		}
		this._component.enabled = false;
		this._startAt = Time.time + Random.Range(0f, this._maxDelay);
	}

	private void Update()
	{
		if (this._startAt < Time.time)
		{
			base.enabled = false;
			this._component.enabled = true;
			if (this._animation != null)
			{
				this._animation.Rewind(this._animation.clip.name);
			}
		}
	}

	[SerializeField]
	private float _maxDelay = 5f;

	[SerializeField]
	private Behaviour _component;

	[SerializeField]
	private bool _withAnimation = true;

	private float _startAt;

	private Animation _animation;
}
