using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(TPMMovementController))]
public class TPMDebugInput : MonoBehaviour
{
	private void Awake()
	{
		this.startPosition = base.transform.position;
		this.m_Character = base.GetComponent<TPMMovementController>();
	}

	private void Start()
	{
	}

	private void FixedUpdate()
	{
		if (this._recordStopAt > Time.time)
		{
			this.m_Move = Vector3.forward * 3f;
		}
		else
		{
			if (!this._wasSpeedCalculated)
			{
				this._wasSpeedCalculated = true;
				float magnitude = (base.transform.position - this.startPosition).magnitude;
			}
			float axis = CrossPlatformInputManager.GetAxis("Horizontal");
			float axis2 = CrossPlatformInputManager.GetAxis("Vertical");
			this.m_Move = axis2 * Vector3.forward + axis * Vector3.right;
		}
		this.m_Character.Move(this.m_Move);
	}

	private TPMMovementController m_Character;

	private float RECORD_TIME = 5f;

	private Vector3 m_Move;

	private float _recordStopAt;

	private bool _wasSpeedCalculated;

	private Vector3 startPosition;
}
