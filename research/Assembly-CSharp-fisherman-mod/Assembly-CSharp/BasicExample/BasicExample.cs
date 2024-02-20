using System;
using InControl;
using UnityEngine;

namespace BasicExample
{
	public class BasicExample : MonoBehaviour
	{
		private void Update()
		{
			InputDevice activeDevice = InputManager.ActiveDevice;
			base.transform.Rotate(Vector3.down, 500f * Time.deltaTime * activeDevice.LeftStickX, 0);
			base.transform.Rotate(Vector3.right, 500f * Time.deltaTime * activeDevice.LeftStickY, 0);
			Color color = ((!activeDevice.Action1.IsPressed) ? Color.white : Color.red);
			Color color2 = ((!activeDevice.Action2.IsPressed) ? Color.white : Color.green);
			base.GetComponent<Renderer>().material.color = Color.Lerp(color, color2, 0.5f);
		}
	}
}
