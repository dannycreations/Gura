using System;
using DG.Tweening;
using SWS;
using UnityEngine;

public class AnimalsMoveAnimator : MoveAnimator
{
	protected override void Start()
	{
		this.animator = base.GetComponentInChildren<Animator>();
	}

	public void SetNewMover(splineMove mover)
	{
		this.sMove = mover;
	}

	protected override void OnAnimatorMove()
	{
		float num = 0f;
		float num2 = 0f;
		if (this.sMove && this.sMove.tween != null && TweenExtensions.IsPlaying(this.sMove.tween))
		{
			num = this.sMove.speed;
			num2 = (base.transform.eulerAngles.y - this.lastRotY) * 10f;
			this.lastRotY = base.transform.eulerAngles.y;
		}
		else if (Mathf.Abs(this.lastRotY - base.transform.eulerAngles.y) > 1f)
		{
			base.transform.eulerAngles = new Vector3(base.transform.eulerAngles.x, this.lastRotY, base.transform.eulerAngles.z);
		}
		this.animator.SetFloat("Speed", num);
		this.animator.SetFloat("Direction", num2, 0.15f, Time.deltaTime);
	}
}
