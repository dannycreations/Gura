using System;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.Util.PullToRefresh
{
	public class PullToRefreshRotateGizmo : PullToRefreshGizmo
	{
		public override bool IsShown
		{
			get
			{
				return base.IsShown;
			}
			set
			{
				base.IsShown = value;
				base.transform.localRotation = Quaternion.Euler(this._InitialLocalRotation);
				if (!value)
				{
					this._WaitingForManualHide = false;
				}
			}
		}

		public override void Awake()
		{
			base.Awake();
			this._InitialLocalRotation = base.transform.localRotation.eulerAngles;
		}

		private void Update()
		{
			if (this._WaitingForManualHide)
			{
				this.SetLocalZRotation((base.transform.localEulerAngles.z - Time.deltaTime * this._AutoRotationDegreesPerSec) % 360f);
			}
		}

		public override void OnPull(float power)
		{
			base.OnPull(power);
			float num = Mathf.Clamp01(power);
			float num2 = Mathf.Max(0f, power - 1f);
			float num3 = num2 * (1f - this._ExcessPullRotationDamping);
			this.SetLocalZRotation((this._InitialLocalRotation.z - 360f * (num + num3)) % 360f);
			base.transform.position = this.LerpUnclamped(this._StartingPoint.position, this._EndingPoint.position, 2f - 2f / (1f + num));
		}

		public override void OnRefreshCancelled()
		{
			base.OnRefreshCancelled();
			this._WaitingForManualHide = false;
		}

		public override void OnRefreshed(bool autoHide)
		{
			base.OnRefreshed(autoHide);
			this._WaitingForManualHide = !autoHide;
		}

		private Vector3 LerpUnclamped(Vector3 from, Vector3 to, float t)
		{
			return (1f - t) * from + t * to;
		}

		private void SetLocalZRotation(float zRotation)
		{
			Vector3 eulerAngles = base.transform.localRotation.eulerAngles;
			eulerAngles.z = zRotation;
			base.transform.localRotation = Quaternion.Euler(eulerAngles);
		}

		[SerializeField]
		private RectTransform _StartingPoint;

		[SerializeField]
		private RectTransform _EndingPoint;

		[SerializeField]
		[Range(0f, 1f)]
		private float _ExcessPullRotationDamping = 0.95f;

		[SerializeField]
		private float _AutoRotationDegreesPerSec = 200f;

		private bool _WaitingForManualHide;

		private Vector3 _InitialLocalRotation;
	}
}
