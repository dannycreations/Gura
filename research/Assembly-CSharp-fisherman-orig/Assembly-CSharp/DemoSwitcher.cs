using System;
using UnityEngine;

public class DemoSwitcher : MonoBehaviour
{
	private void Start()
	{
		base.transform.position = this.camPosTransfroms[0].position;
		base.transform.rotation = this.camPosTransfroms[0].rotation;
		this.target = this.camPosTransfroms[0];
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(10f, 10f, 100f, 30f), "Last"))
		{
			this.index--;
			if (this.index < 0)
			{
				this.index = this.camPosTransfroms.Length - 1;
			}
			this.target = this.camPosTransfroms[this.index];
		}
		if (GUI.Button(new Rect((float)(Screen.width - 110), 10f, 100f, 30f), "Next"))
		{
			this.index++;
			this.index %= this.camPosTransfroms.Length;
			this.target = this.camPosTransfroms[this.index];
		}
	}

	private void Update()
	{
		base.transform.position = Vector3.Slerp(base.transform.position, this.target.position, Time.deltaTime * this.speed);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, this.target.rotation, Time.deltaTime * this.speed);
	}

	public Transform cameraTransform;

	public Transform[] camPosTransfroms;

	public Transform target;

	public float speed = 3f;

	public int index;
}
