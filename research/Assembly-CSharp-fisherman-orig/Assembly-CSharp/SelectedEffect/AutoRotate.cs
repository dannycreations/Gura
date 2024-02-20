using System;
using UnityEngine;

namespace SelectedEffect
{
	public class AutoRotate : MonoBehaviour
	{
		private void Update()
		{
			float num = Time.deltaTime * this.m_Speed;
			base.transform.Rotate(num, num, 0f);
		}

		public float m_Speed = 30f;
	}
}
