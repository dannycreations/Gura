using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

namespace TPM
{
	public class Lure3rdBehaviour : LureBehaviour
	{
		public Lure3rdBehaviour(LureController owner, IAssembledRod rodAssembly)
			: base(owner, rodAssembly, null)
		{
			this._wasInitialized = false;
			this._hookRenderers = RenderersHelper.GetAllRenderersForObject<SkinnedMeshRenderer>(base.transform);
		}

		public override Transform TransformWithHook
		{
			get
			{
				return base.transform;
			}
		}

		public override void SetVisibility(bool isVisible)
		{
			base.SetVisibility(isVisible);
			for (int i = 0; i < this._hookRenderers.Count; i++)
			{
				this._hookRenderers[i].enabled = isVisible;
			}
		}

		public override void SetOpaque(float prc)
		{
			for (int i = 0; i < this._hookRenderers.Count; i++)
			{
				this._hookRenderers[i].material.SetFloat("_CharacterOpaque", prc);
			}
			if (this._sinkerRenderer != null)
			{
				this._sinkerRenderer.material.SetFloat("_CharacterOpaque", prc);
			}
			if (this._beadRenderer != null)
			{
				this._beadRenderer.material.SetFloat("_CharacterOpaque", prc);
			}
		}

		public override void RodSyncUpdate(Line3rdBehaviour lineBehaviour, float dtPrc)
		{
			Quaternion quaternion = Quaternion.Slerp(this._prevLureRotation, this._targetLureRotation, dtPrc);
			base.transform.rotation = quaternion;
			base.transform.position = lineBehaviour.LineEndPoint;
			if (base.SinkerObject != null && base.RodTemplate != RodTemplate.TexasRig)
			{
				base.SinkerObject.transform.rotation = quaternion;
				base.SinkerObject.transform.position = lineBehaviour.BobberPoint;
			}
		}

		public override void ServerUpdate(TackleData tackleData, bool baitVisibility, float dtPrc)
		{
			if (this._wasInitialized)
			{
				this._prevLureRotation = Quaternion.Slerp(this._prevLureRotation, this._targetLureRotation, dtPrc);
			}
			else
			{
				this._wasInitialized = true;
				this._prevLureRotation = tackleData.rotation;
			}
			this._targetLureRotation = tackleData.rotation;
		}

		private Quaternion _prevLureRotation;

		private Quaternion _targetLureRotation;

		private Quaternion _prevSinkerRotation;

		private Quaternion _targetSinkerRotation;

		private bool _wasInitialized;

		private List<SkinnedMeshRenderer> _hookRenderers;
	}
}
