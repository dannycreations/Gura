using System;
using System.Linq;
using UnityEngine;

namespace TPM
{
	public class TPMFlashLightController : FlashLightController
	{
		public void SetLightConeVisibility(bool flag)
		{
			this._lightCone.SetActive(flag);
		}

		public void SetActive(bool flag)
		{
			this._isModelVisible = flag;
			this.UpdateVisibility();
		}

		public void SetLightVisibility(bool flag)
		{
			this._isLightVisible = flag;
			this.UpdateVisibility();
		}

		private void UpdateVisibility()
		{
			base.EnableLight(this._isModelVisible && FlashLightController.IsDarkTime && this._isLightVisible);
		}

		protected override void Awake()
		{
			base.Awake();
			this._remappingHandler.EReMapped += this.OnBoneRemapped;
		}

		protected override void Start()
		{
			if (PhotonConnectionFactory.Instance != null)
			{
				PhotonConnectionFactory.Instance.OnGotTime += this.OnUpdateTime;
			}
			base.EnableLight(false);
		}

		private void OnBoneRemapped(SkinnedMeshRenderer skinnedMeshRenderer)
		{
			this._rootBone = skinnedMeshRenderer.bones.FirstOrDefault((Transform t) => t.name == this._baseBoneName);
		}

		private void Update()
		{
			if (this._rootBone != null)
			{
				base.transform.position = this._rootBone.TransformPoint(this._localPos);
				base.transform.rotation = this._rootBone.rotation;
			}
		}

		protected override void OnUpdateTime(TimeSpan time)
		{
			if (this._isModelVisible && this._isLightVisible && this._rootBone != null)
			{
				base.OnUpdateTime(time);
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			this._remappingHandler.EReMapped -= this.OnBoneRemapped;
		}

		[SerializeField]
		protected BonesRemappingHandler _remappingHandler;

		[SerializeField]
		protected string _baseBoneName;

		[SerializeField]
		protected Vector3 _localPos;

		[SerializeField]
		protected GameObject _lightCone;

		private Transform _rootBone;

		private bool _isModelVisible = true;

		private bool _isLightVisible = true;
	}
}
