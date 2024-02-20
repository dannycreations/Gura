using System;
using UnityEngine;

namespace TPM
{
	public class GripSettings : MonoBehaviour
	{
		public Transform Fish
		{
			get
			{
				return this._fish;
			}
		}

		public bool IsVisible
		{
			get
			{
				return this._renderer.enabled;
			}
		}

		private void Awake()
		{
			this._renderer = RenderersHelper.GetRendererForObject(base.transform);
			this.UpdateVisibility();
		}

		public void SetGameVisibility(bool flag)
		{
			this._gameVisibility = flag;
			this.UpdateVisibility();
		}

		public void SetPlayerVisibility(bool flag)
		{
			this._playerVisibility = flag;
			this.UpdateVisibility();
		}

		private void UpdateVisibility()
		{
			this._renderer.enabled = this._gameVisibility && this._playerVisibility;
		}

		public void SetOpaque(float prc)
		{
			this._renderer.material.SetFloat("_CharacterOpaque", prc);
		}

		public void ChangeRoot(Transform root)
		{
			this.ChangeRoot(root, Vector3.zero);
		}

		public void ChangeRoot(Transform root, Vector3 localPosition)
		{
			base.transform.parent = root;
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
		}

		[SerializeField]
		private Transform _fish;

		private SkinnedMeshRenderer _renderer;

		private bool _gameVisibility;

		private bool _playerVisibility;
	}
}
