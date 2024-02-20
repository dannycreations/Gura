using System;
using UnityEngine;

namespace UltraReal
{
	public class flicker : MonoBehaviour
	{
		private void Update()
		{
			base.GetComponent<Light>().intensity = this.lightIntensity * this.flickerCurve.EvaluateStep(Time.deltaTime);
		}

		public float lightIntensity = 1f;

		public AnimationCurveProperty flickerCurve;
	}
}
