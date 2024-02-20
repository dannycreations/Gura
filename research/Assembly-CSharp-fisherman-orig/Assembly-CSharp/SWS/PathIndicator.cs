using System;
using System.Collections;
using UnityEngine;

namespace SWS
{
	public class PathIndicator : MonoBehaviour
	{
		private void Start()
		{
			this.pSys = base.GetComponentInChildren<ParticleSystem>();
			base.StartCoroutine("EmitParticles");
		}

		private IEnumerator EmitParticles()
		{
			yield return new WaitForEndOfFrame();
			for (;;)
			{
				float rot = (base.transform.eulerAngles.y + this.modRotation) * 0.017453292f;
				this.pSys.startRotation = rot;
				this.pSys.Emit(1);
				yield return new WaitForSeconds(0.2f);
			}
			yield break;
		}

		public float modRotation;

		private ParticleSystem pSys;
	}
}
