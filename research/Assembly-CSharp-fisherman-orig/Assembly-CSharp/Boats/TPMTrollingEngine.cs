using System;
using TPM;
using UnityEngine;

namespace Boats
{
	public class TPMTrollingEngine
	{
		public TPMTrollingEngine(Animation animations)
		{
			this._animations = animations;
		}

		public void ServerUpdate(ThirdPersonData data)
		{
			if (data.BoolParameters[8])
			{
				if (!this._isInWater)
				{
					this._isInWater = true;
					this._animations.Play("SetOn");
				}
			}
			else if (this._isInWater)
			{
				this._isInWater = false;
				this._animations.Play("SetOff");
			}
		}

		private Animation _animations;

		private bool _isInWater = true;
	}
}
