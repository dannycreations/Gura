using System;
using ObjectModel;
using UnityEngine;

namespace TPM
{
	public class Feeder3rdBehaviour : FeederBehaviour
	{
		public Feeder3rdBehaviour(FeederController controller, IAssembledRod rodAssembly)
			: base(controller, rodAssembly, null)
		{
			this._wasInitialized = false;
			HookController component = this.hookObject.GetComponent<HookController>();
			component.enabled = false;
			this.hookObject.transform.localScale *= FloatBehaviour.GetHookScaling(rodAssembly.HookInterface.HookSize, component.hookModelSize);
			this._hookRenderer = RenderersHelper.GetAllRenderersForObject<SkinnedMeshRenderer>(this.hookObject.transform)[0];
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
		}

		public override void SetOpaque(float prc)
		{
			this._hookRenderer.material.SetFloat("_CharacterOpaque", prc);
			this._baitRenderer.material.SetFloat("_CharacterOpaque", prc);
			this._feederRenderer.material.SetFloat("_CharacterOpaque", prc);
			if (this._chumRenderer != null)
			{
				this._chumRenderer.material.SetFloat("_CharacterOpaque", prc);
			}
		}

		public override void RodSyncUpdate(Line3rdBehaviour lineBehaviour, float dtPrc)
		{
			this.hookObject.transform.rotation = Quaternion.Slerp(this._prevHookRotation, this._targetHookRotation, dtPrc);
			this.hookObject.transform.position = lineBehaviour.LineEndPoint;
			base.transform.rotation = Quaternion.Slerp(this._prevFeederRotation, this._targetFeederRotation, dtPrc);
			base.transform.position = lineBehaviour.BobberPoint;
			if (base.Controller.SecondaryTackleObject != null)
			{
				if ((this._rodAssembly.FeederInterface as TPMFeeder).PvaForm.Value == PvaFeederForm.Bag)
				{
					base.Controller.SecondaryTackleObject.transform.position = base.transform.position;
					base.Controller.SecondaryTackleObject.transform.rotation = base.transform.rotation;
				}
				else
				{
					base.Controller.SecondaryTackleObject.transform.position = this.hookObject.transform.position;
					base.Controller.SecondaryTackleObject.transform.rotation = this.hookObject.transform.rotation;
				}
			}
		}

		public override void ServerUpdate(TackleData tackleData, bool baitVisibility, float dtPrc)
		{
			this.baitObject.SetActive(baitVisibility);
			if (this._wasInitialized)
			{
				this._prevHookRotation = Quaternion.Slerp(this._prevHookRotation, this._targetHookRotation, dtPrc);
				this._prevFeederRotation = Quaternion.Slerp(this._prevFeederRotation, this._targetFeederRotation, dtPrc);
			}
			else
			{
				this._wasInitialized = true;
				this._prevFeederRotation = tackleData.rotation;
				this._prevHookRotation = tackleData.hookRotation;
			}
			this._targetFeederRotation = tackleData.rotation;
			this._targetHookRotation = tackleData.hookRotation;
		}

		private Quaternion _prevHookRotation;

		private Quaternion _targetHookRotation;

		private Quaternion _prevFeederRotation;

		private Quaternion _targetFeederRotation;

		private bool _wasInitialized;
	}
}
