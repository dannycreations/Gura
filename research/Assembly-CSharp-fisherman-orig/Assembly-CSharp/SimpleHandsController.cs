using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SimpleHandsController : MonoBehaviour
{
	private void Start()
	{
		this.anim = base.GetComponent<Animator>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(13))
		{
			this.anim.SetTrigger(this.pitchingHash);
		}
		else if (Input.GetKeyDown(278))
		{
			this.anim.Play("Idle");
		}
		else if (Input.GetKeyDown(32))
		{
			int fullPathHash = this.anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
			if (fullPathHash == this.pitchStateHash || fullPathHash == this.blendTestStateHash)
			{
				this.anim.SetFloat("Roll", 1f);
				this.anim.SetFloat("ThrowIn", 0.05f);
				this.anim.SetTrigger(this.blendHash);
			}
		}
	}

	private Animator anim;

	private int pitchingHash = Animator.StringToHash("StartPitching");

	private int blendHash = Animator.StringToHash("BlendTestTrigger");

	private int pitchStateHash = Animator.StringToHash("Base Layer.IdlePitch");

	private int blendTestStateHash = Animator.StringToHash("Base Layer.BlendTest");
}
