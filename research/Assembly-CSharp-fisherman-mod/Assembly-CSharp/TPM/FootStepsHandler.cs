using System;
using System.Diagnostics;
using UnityEngine;

namespace TPM
{
	public class FootStepsHandler : MonoBehaviour
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event FootStepsHandler.HitGroundDelegate OnHitGround = delegate
		{
		};

		public void HitGround()
		{
			if (this.nextEventPossibleAt < Time.time)
			{
				this.nextEventPossibleAt = Time.time + this._ignoreDelay;
				this.OnHitGround();
			}
		}

		[SerializeField]
		private float _ignoreDelay = 0.3f;

		private float nextEventPossibleAt;

		public delegate void HitGroundDelegate();
	}
}
