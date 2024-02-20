using System;
using UnityEngine;

namespace SelectedEffect
{
	[RequireComponent(typeof(Camera))]
	public class FreeCamera : MonoBehaviour
	{
		private void Start()
		{
			QualitySettings.antiAliasing = 8;
		}

		private void Update()
		{
			Vector3 zero = Vector3.zero;
			this.Move(this.m_ForwardButton, ref zero, base.transform.forward);
			this.Move(this.m_BackwardButton, ref zero, -base.transform.forward);
			this.Move(this.m_RightButton, ref zero, base.transform.right);
			this.Move(this.m_LeftButton, ref zero, -base.transform.right);
			this.Move(this.m_UpButton, ref zero, base.transform.up);
			this.Move(this.m_DownButton, ref zero, -base.transform.up);
			base.transform.position += zero * this.m_MoveSpeed * Time.deltaTime;
			if (Input.GetMouseButton(0))
			{
				Vector3 eulerAngles = base.transform.eulerAngles;
				eulerAngles.x += -Input.GetAxis("Mouse Y") * 359f * this.m_RotateSpeed;
				eulerAngles.y += Input.GetAxis("Mouse X") * 359f * this.m_RotateSpeed;
				base.transform.eulerAngles = eulerAngles;
			}
		}

		private void Move(KeyCode key, ref Vector3 moveTo, Vector3 dir)
		{
			if (Input.GetKey(key))
			{
				moveTo = dir;
			}
		}

		private void OnGUI()
		{
			GUI.Box(new Rect(10f, 10f, 260f, 25f), "Selected Effect --- Outline Demo");
		}

		public float m_MoveSpeed;

		public float m_RotateSpeed;

		public KeyCode m_ForwardButton = 119;

		public KeyCode m_BackwardButton = 115;

		public KeyCode m_RightButton = 100;

		public KeyCode m_LeftButton = 97;

		public KeyCode m_UpButton = 113;

		public KeyCode m_DownButton = 101;
	}
}
