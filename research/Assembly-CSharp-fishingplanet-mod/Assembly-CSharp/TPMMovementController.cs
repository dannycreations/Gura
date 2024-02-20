using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class TPMMovementController : MonoBehaviour
{
	private void Start()
	{
		this.m_Animator = base.GetComponent<Animator>();
		this.m_Animator.speed = this.m_AnimSpeedMultiplier;
		this.m_Rigidbody = base.GetComponent<Rigidbody>();
		this.m_Rigidbody.constraints = 112;
	}

	public void Move(Vector3 move)
	{
		if (move.magnitude > 1f)
		{
			move.Normalize();
		}
		if (move.z > 0f)
		{
			this.m_TurnAmount = Mathf.Atan2(move.x, move.z);
			this.m_ForwardAmount = move.z;
		}
		else if (move.z < 0f)
		{
			this.m_TurnAmount = -Mathf.Atan2(move.x, -move.z);
			this.m_ForwardAmount = move.z;
		}
		else
		{
			this.m_TurnAmount = Mathf.Atan2(move.x, move.z);
			this.m_ForwardAmount = 0f;
		}
		this.ApplyExtraTurnRotation();
		this.m_Animator.SetFloat("Forward", this.m_ForwardAmount, 0.1f, Time.deltaTime);
		this.m_Animator.SetFloat("Turn", this.m_TurnAmount, 0.1f, Time.deltaTime);
	}

	private void ApplyExtraTurnRotation()
	{
		float num = Mathf.Lerp(this.m_StationaryTurnSpeed, this.m_MovingTurnSpeed, this.m_ForwardAmount);
		base.transform.Rotate(0f, this.m_TurnAmount * num * Time.deltaTime, 0f);
	}

	public void OnAnimatorMove()
	{
		if (Time.deltaTime > 0f)
		{
			Vector3 vector = this.m_Animator.deltaPosition * this.m_MoveSpeedMultiplier / Time.deltaTime;
			vector.y = this.m_Rigidbody.velocity.y;
			this.m_Rigidbody.velocity = vector;
		}
	}

	public float m_MovingTurnSpeed = 360f;

	public float m_StationaryTurnSpeed = 180f;

	public float m_MoveSpeedMultiplier = 1f;

	public float m_AnimSpeedMultiplier = 1f;

	private Rigidbody m_Rigidbody;

	private Animator m_Animator;

	private float m_TurnAmount;

	private float m_ForwardAmount;

	private Vector3 m_GroundNormal;
}
