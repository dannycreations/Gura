using System;
using UnityEngine;

[RequireComponent(typeof(DebugThirdPersonsController))]
public class ClientBotsGenerator : MonoBehaviour
{
	private void Start()
	{
		this.controller = base.gameObject.GetComponent<DebugThirdPersonsController>();
		this.player = GameObject.Find("3D/HUD/Player");
	}

	private void Update()
	{
		if (Input.GetKeyUp(270))
		{
			this.AddBot();
		}
		if (Input.GetKeyUp(269))
		{
			this.DelBot();
		}
		if (Input.GetKeyUp(127))
		{
			this.ClearBots();
		}
	}

	private void AddBot()
	{
		this.curIndex++;
		this.controller.OnPlayerModelEnter(this.curIndex, "bot" + this.curIndex, this.player.transform.position + new Vector3(Random.Range(-this.START_R, this.START_R), 0f, Random.Range(-this.START_R, this.START_R)));
	}

	private void DelBot()
	{
		if (this.curIndex > 0)
		{
			this.controller.OnPlayerModelLeave(this.curIndex--);
		}
	}

	private void ClearBots()
	{
		while (this.curIndex > 0)
		{
			this.DelBot();
		}
	}

	public float START_R = 3.5f;

	private DebugThirdPersonsController controller;

	private int curIndex;

	private GameObject player;
}
