using System;
using UnityEngine;

namespace cakeslice
{
	public class Rotate : MonoBehaviour
	{
		private void Start()
		{
		}

		private void Update()
		{
			base.transform.Rotate(Vector3.up, Time.deltaTime * 20f);
			this.timer -= Time.deltaTime;
			if (this.timer < 0f)
			{
				this.timer = 1f;
			}
		}

		private float timer;

		private const float time = 1f;
	}
}
