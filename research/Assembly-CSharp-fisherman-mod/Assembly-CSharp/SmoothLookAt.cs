using System;
using UnityEngine;

public class SmoothLookAt : MonoBehaviour
{
	private void OnEnable()
	{
		this._myTransform = base.transform;
		this.originalRotation = this._myTransform.rotation;
	}

	private void FixedUpdate()
	{
		if (!TutorialManager.end && TutorialManager.follow)
		{
			if (this.target)
			{
				Quaternion quaternion = Quaternion.LookRotation(this.target.transform.position - this._myTransform.position);
				this._myTransform.rotation = Quaternion.Slerp(this._myTransform.rotation, quaternion, Time.deltaTime * this.CameraSettings.damping);
			}
		}
		else
		{
			if (this.CameraSettings.returnToOriginalRotation)
			{
				this._myTransform.rotation = Quaternion.Slerp(this._myTransform.rotation, this.originalRotation, Time.deltaTime * 15f);
			}
			if (TutorialManager.end)
			{
				Object.Destroy(base.GetComponent<SmoothLookAt>(), this.CameraSettings.destroyScriptTime);
			}
		}
	}

	[HideInInspector]
	public GameObject target;

	private Quaternion originalRotation;

	public SmoothLookAt.CameraClass CameraSettings;

	private Transform _myTransform;

	[Serializable]
	public class CameraClass
	{
		public float damping = 2f;

		public bool returnToOriginalRotation = true;

		[HideInInspector]
		public float destroyScriptTime = 2f;
	}
}
