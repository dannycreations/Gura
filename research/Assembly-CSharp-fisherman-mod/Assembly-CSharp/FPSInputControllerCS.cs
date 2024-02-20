using System;
using UnityEngine;

[RequireComponent(typeof(CharacterMotorCS))]
[AddComponentMenu("Character/FPS Input Controller")]
public class FPSInputControllerCS : MonoBehaviour
{
	private void Awake()
	{
		this.motor = base.GetComponent<CharacterMotorCS>();
	}

	private void Update()
	{
		Vector3 vector;
		vector..ctor(ControlsController.ControlsActions.Move.X, 0f, ControlsController.ControlsActions.Move.Y);
		if (vector != Vector3.zero)
		{
			float num = vector.magnitude;
			vector /= num;
			num = Mathf.Min(1f, num);
			num *= num;
			vector *= num;
		}
		this.motor.inputMoveDirection = base.transform.rotation * vector;
		this.motor.inputJump = ControlsController.ControlsActions.Jump.IsPressed;
	}

	private CharacterMotorCS motor;
}
