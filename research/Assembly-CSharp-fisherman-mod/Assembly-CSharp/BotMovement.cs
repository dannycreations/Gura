using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DebugThirdPersonsController))]
public class BotMovement : MonoBehaviour
{
	private void Start()
	{
		this.controller = base.gameObject.GetComponent<DebugThirdPersonsController>();
		this.tmpObj = new GameObject();
	}

	private void Update()
	{
		this.UpdatePlayersTransform();
	}

	private void UpdatePlayersTransform()
	{
		foreach (KeyValuePair<int, Player3dController> keyValuePair in this.controller.Players)
		{
			Player3dController value = keyValuePair.Value;
			int key = keyValuePair.Key;
			Transform transform = this.tmpObj.transform;
			transform.position = value.transform.position;
			transform.rotation = value.transform.rotation;
			transform.localScale = value.transform.localScale;
			float num = this.speed * Time.deltaTime;
			Ray ray;
			ray..ctor(value.transform.position, value.transform.forward);
			if (Physics.SphereCast(ray, this.botWidth, num))
			{
				transform.Rotate(0f, this.obstacleRotationAngle, 0f);
			}
			else
			{
				transform.Translate(0f, 0f, num);
			}
			this.controller.onNewTransform(key, transform.position, transform.rotation);
		}
	}

	public float speed = 1f;

	public float botWidth = 1.22f;

	public float obstacleRotationAngle = 5f;

	private DebugThirdPersonsController controller;

	private GameObject tmpObj;
}
