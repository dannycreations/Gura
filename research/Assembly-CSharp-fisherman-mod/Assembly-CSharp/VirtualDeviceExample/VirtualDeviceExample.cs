using System;
using InControl;
using UnityEngine;

namespace VirtualDeviceExample
{
	public class VirtualDeviceExample : MonoBehaviour
	{
		private void OnEnable()
		{
			this.virtualDevice = new VirtualDevice();
			InputManager.OnSetup += delegate
			{
				InputManager.AttachDevice(this.virtualDevice);
			};
		}

		private void OnDisable()
		{
			InputManager.DetachDevice(this.virtualDevice);
		}

		private void Update()
		{
			InputDevice activeDevice = InputManager.ActiveDevice;
			this.leftObject.transform.Rotate(Vector3.down, 500f * Time.deltaTime * activeDevice.LeftStickX, 0);
			this.leftObject.transform.Rotate(Vector3.right, 500f * Time.deltaTime * activeDevice.LeftStickY, 0);
			this.rightObject.transform.Rotate(Vector3.down, 500f * Time.deltaTime * activeDevice.RightStickX, 0);
			this.rightObject.transform.Rotate(Vector3.right, 500f * Time.deltaTime * activeDevice.RightStickY, 0);
			Color color = Color.white;
			if (activeDevice.Action1.IsPressed)
			{
				color = Color.green;
			}
			if (activeDevice.Action2.IsPressed)
			{
				color = Color.red;
			}
			if (activeDevice.Action3.IsPressed)
			{
				color = Color.blue;
			}
			if (activeDevice.Action4.IsPressed)
			{
				color = Color.yellow;
			}
			this.leftObject.GetComponent<Renderer>().material.color = color;
		}

		public GameObject leftObject;

		public GameObject rightObject;

		private VirtualDevice virtualDevice;
	}
}
