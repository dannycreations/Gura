using System;
using UnityEngine;

namespace TPM
{
	public class Float3rdBehaviour : FloatBehaviour
	{
		public Float3rdBehaviour(FloatController owner, IAssembledRod rodAssembly)
			: base(owner, rodAssembly, null)
		{
			this._wasInitialized = false;
			HookController component = this.hookObject.GetComponent<HookController>();
			component.enabled = false;
			this.hookObject.transform.localScale *= FloatBehaviour.GetHookScaling(rodAssembly.HookInterface.HookSize, component.hookModelSize);
			this._hookRenderer = RenderersHelper.GetAllRenderersForObject<SkinnedMeshRenderer>(this.hookObject.transform)[0];
			this._baitRenderer = RenderersHelper.GetRendererForObject(this.baitObject.transform);
			this._bobberRenderer = RenderersHelper.GetRenderer(base.transform);
		}

		public override Transform TransformWithHook
		{
			get
			{
				return this.hookObject.transform;
			}
		}

		public override void SetVisibility(bool isVisible)
		{
			base.SetVisibility(isVisible);
			this._hookRenderer.enabled = isVisible;
			this._baitRenderer.enabled = isVisible;
			this._bobberRenderer.enabled = isVisible;
		}

		public override void SetOpaque(float prc)
		{
			this._hookRenderer.material.SetFloat("_CharacterOpaque", prc);
			this._baitRenderer.material.SetFloat("_CharacterOpaque", prc);
			this._bobberRenderer.material.SetFloat("_CharacterOpaque", prc);
		}

		public override void RodSyncUpdate(Line3rdBehaviour lineBehaviour, float dtPrc)
		{
			base.transform.rotation = Quaternion.Slerp(this._prevBobberRotation, this._targetBobberRotation, dtPrc);
			base.transform.position = lineBehaviour.BobberPoint;
			this.hookObject.transform.rotation = Quaternion.Slerp(this._prevHookRotation, this._targetHookRotation, dtPrc);
			this.hookObject.transform.position = lineBehaviour.LineEndPoint;
		}

		public override void ServerUpdate(TackleData tackleData, bool baitVisibility, float dtPrc)
		{
			this.baitObject.SetActive(baitVisibility);
			if (this._wasInitialized)
			{
				this._prevBobberRotation = Quaternion.Slerp(this._prevBobberRotation, this._targetBobberRotation, dtPrc);
				this._prevHookRotation = Quaternion.Slerp(this._prevHookRotation, this._targetHookRotation, dtPrc);
			}
			else
			{
				this._wasInitialized = true;
				this._prevBobberRotation = tackleData.rotation;
				this._prevHookRotation = tackleData.hookRotation;
			}
			this._targetBobberRotation = tackleData.rotation;
			this._targetHookRotation = tackleData.hookRotation;
		}

		private Quaternion _prevBobberRotation;

		private Quaternion _targetBobberRotation;

		private Quaternion _prevHookRotation;

		private Quaternion _targetHookRotation;

		private bool _wasInitialized;

		private SkinnedMeshRenderer _baitRenderer;

		private SkinnedMeshRenderer _hookRenderer;

		private Renderer _bobberRenderer;
	}
}
