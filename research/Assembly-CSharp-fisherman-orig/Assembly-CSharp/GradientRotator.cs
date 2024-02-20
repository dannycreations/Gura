using System;
using Coffee.UIExtensions;
using UnityEngine;

public class GradientRotator : MonoBehaviour
{
	private void Start()
	{
		this.gradient = base.GetComponent<UIGradient>();
	}

	private void FixedUpdate()
	{
		this.time += this.Speed * Time.fixedDeltaTime;
		if (this.time > 180f)
		{
			this.time -= 360f;
		}
		this.gradient.rotation = this.time;
	}

	private UIGradient gradient;

	public float Speed = 5f;

	private float time;
}
