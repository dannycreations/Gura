using System;
using UnityEngine;

namespace Boats
{
	public class FollowBoneMouseController : CameraMouseLook
	{
		protected override void Awake()
		{
			base.Awake();
			this.originalRotation = Quaternion.identity;
		}

		public void Activate(Transform rootBone)
		{
			base.enabled = true;
			this._originalRootBone = base.transform.parent;
			this.ReAttachAndKeepRotation(rootBone);
			Vector3 eulerAngles = (Quaternion.Inverse(this.originalRotation) * base.transform.localRotation).eulerAngles;
			float num = Math3d.ClampAngleTo180(eulerAngles.y);
			if (num >= this.minimumX && num <= this.maximumX)
			{
				this.rotationX = num;
				this._yawToAdjust = 0f;
			}
			else
			{
				this.rotationX = 0f;
				this._yawToAdjust = num;
			}
			float num2 = -Math3d.ClampAngleTo180(eulerAngles.x);
			if (num2 >= this.minimumY && num2 <= this.maximumY)
			{
				this.rotationY = num2;
				this._pitchToAdjust = 0f;
			}
			else
			{
				this.rotationY = 0f;
				this._pitchToAdjust = num2;
			}
		}

		public void Deactivate()
		{
			base.enabled = false;
			this.ReAttachAndKeepRotation(this._originalRootBone);
		}

		private void ReAttachAndKeepRotation(Transform parent)
		{
			Quaternion rotation = base.transform.rotation;
			base.transform.SetParent(parent, true);
			base.transform.rotation = rotation;
		}

		private Transform _originalRootBone;
	}
}
