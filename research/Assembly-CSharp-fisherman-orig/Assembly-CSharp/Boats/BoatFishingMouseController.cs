using System;
using UnityEngine;

namespace Boats
{
	public class BoatFishingMouseController : CameraMouseLookForObjectWithCollider
	{
		public void TakeControll(IBoatController owner)
		{
			this._owner = owner;
			this.ResetOriginalRotation(Quaternion.identity);
		}

		public void ReleaseControll()
		{
			this._owner = null;
		}

		public void BlockInput(bool flag)
		{
			this._isInputBlocked = flag;
			if (flag)
			{
				this.velocity = Vector2.zero;
			}
		}

		public void SetActive(bool flag)
		{
			this._startEulerY = base.transform.parent.localEulerAngles.y + this._owner.Rotation.eulerAngles.y - base.transform.rotation.eulerAngles.y;
			base.enabled = flag;
			if (base.enabled)
			{
				this._startSmothingFrom = Time.time;
			}
		}

		protected override void Update()
		{
			if (!this._isInputBlocked)
			{
				base.Update();
			}
			else
			{
				base.SetRotation(this.originalRotation * base.YawRotation * base.PitchRotation);
			}
			Quaternion quaternion = Quaternion.Euler(base.transform.localEulerAngles.x, base.transform.parent.localEulerAngles.y + this._owner.Rotation.eulerAngles.y - this._startEulerY, 0f);
			float num = Time.time - this._startSmothingFrom;
			if (num > 0.5f)
			{
				base.transform.rotation = quaternion;
			}
			else
			{
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, num / 0.5f);
			}
		}

		private const float TRANSITION_SMOTHING_DURATION = 0.5f;

		private float _startSmothingFrom;

		private float _startEulerY;

		protected IBoatController _owner;

		private bool _isInputBlocked;
	}
}
