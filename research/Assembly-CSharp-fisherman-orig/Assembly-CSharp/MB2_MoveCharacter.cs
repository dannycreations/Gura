using System;
using UnityEngine;

public class MB2_MoveCharacter : MonoBehaviour
{
	private void Start()
	{
		this.characterController = base.GetComponent<CharacterController>();
	}

	private void Update()
	{
		if (Time.frameCount % 500 == 0)
		{
			return;
		}
		Vector3 vector = this.target.position - base.transform.position;
		vector.Normalize();
		this.characterController.Move(vector * this.speed * Time.deltaTime);
	}

	private CharacterController characterController;

	public float speed = 5f;

	public Transform target;
}
