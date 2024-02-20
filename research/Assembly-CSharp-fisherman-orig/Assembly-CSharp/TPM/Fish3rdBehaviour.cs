using System;
using UnityEngine;

namespace TPM
{
	public class Fish3rdBehaviour : FishBehaviour, ThirdPersonData.IFish
	{
		public Fish3rdBehaviour(FishController owner, IFish fishTemplate, GameFactory.RodSlot slot)
			: base(owner, fishTemplate, slot)
		{
			this._wasInitialized = false;
			base.initChord();
			this._waterDisturber = new FishWaterDiturber(base.transform, this._owner.modelSize, false);
			this.bezierPPropIds = new int[6];
			this.rightAxisPropIds = new int[6];
			for (int i = 0; i <= 5; i++)
			{
				this.bezierPPropIds[i] = Shader.PropertyToID("_BezierP" + i);
				this.rightAxisPropIds[i] = Shader.PropertyToID("_RightAxis" + i);
			}
		}

		public int Id { get; set; }

		public TPMFishState state { get; private set; }

		public void SetVisibility(bool flag)
		{
			if (this.fishMeshRenderer != null)
			{
				this.fishMeshRenderer.enabled = flag;
			}
			this._waterDisturber.SetVisibility(flag);
		}

		public void SetOpaque(float prc)
		{
			if (this.fishMaterial != null)
			{
				this.fishMaterial.SetFloat("_CharacterOpaque", prc);
			}
		}

		public void SyncUpdate(TackleBehaviour tackle, GripSettings grip, float additionalYaw, float dtPrc)
		{
			if (base.transform == null)
			{
				return;
			}
			Vector3 vector = Vector3.Lerp(this._prevFishBackward, this._targetFishBackward, dtPrc);
			Vector3 vector2 = Vector3.Lerp(this._prevFishBackward2, this._targetFishBackward2, dtPrc);
			Vector3 vector3 = Vector3.Lerp(this._prevFishRight, this._targetFishRight, dtPrc);
			Vector3 vector4 = Vector3.Lerp(this._prevPosition, this._targetPosition, dtPrc);
			if (grip.IsVisible && this.state == TPMFishState.ShowBig)
			{
				base.transform.position = grip.Fish.position;
				vector3 = Quaternion.AngleAxis(additionalYaw, Vector3.up) * vector3;
			}
			else if (this.state == TPMFishState.ShowSmall)
			{
				base.transform.position = tackle.HookAnchor.position;
				vector3 = Quaternion.AngleAxis(additionalYaw, Vector3.up) * vector3;
			}
			else
			{
				base.transform.position = vector4;
			}
			float num = Mathf.Abs(base.meshHeadZ - base.meshTailZ);
			this.bezierCurve.AnchorPoints[0] = Vector3.zero;
			this.bezierCurve.AnchorPoints[1] = this.bezierCurve.AnchorPoints[0] + Vector3.Slerp(vector, vector2, 0.2f) * 0.2f * num;
			this.bezierCurve.AnchorPoints[2] = this.bezierCurve.AnchorPoints[0] + Vector3.Slerp(vector, vector2, 0.4f) * 0.4f * num;
			this.bezierCurve.AnchorPoints[3] = this.bezierCurve.AnchorPoints[2] + Vector3.Slerp(vector, vector2, 0.6f) * 0.2f * num;
			this.bezierCurve.AnchorPoints[4] = this.bezierCurve.AnchorPoints[2] + Vector3.Slerp(vector, vector2, 0.8f) * 0.4f * num;
			this.bezierCurve.AnchorPoints[5] = this.bezierCurve.AnchorPoints[2] + vector2 * 0.6f * num;
			Quaternion quaternion = Quaternion.FromToRotation(vector, Vector3.Slerp(vector, vector2, 0.25f));
			Vector3[] array = new Vector3[5];
			array[0] = vector3;
			for (int i = 1; i < 5; i++)
			{
				array[i] = quaternion * array[i - 1];
				this.bezierCurve.RightAxis[i] = array[i];
			}
			base.updateTransformableBones();
			if (grip.IsVisible && this.state == TPMFishState.ShowBig)
			{
				base.transform.position += grip.Fish.position - base.Grip.position;
			}
			else if (this.state == TPMFishState.ShowSmall)
			{
				base.transform.position += base.transform.position - base.Mouth.position;
			}
			this._waterDisturber.Update();
		}

		public void UpdateFish(ThirdPersonData.FishData fish, float dtPrc)
		{
			if (this._wasInitialized)
			{
				this._prevPosition = Vector3.Lerp(this._prevPosition, this._targetPosition, dtPrc);
				this._prevFishBackward = Vector3.Lerp(this._prevFishBackward, this._targetFishBackward, dtPrc);
				this._prevFishBackward2 = Vector3.Lerp(this._prevFishBackward2, this._targetFishBackward2, dtPrc);
				this._prevFishRight = Vector3.Lerp(this._prevFishRight, this._targetFishRight, dtPrc);
			}
			else
			{
				this._wasInitialized = true;
				this._prevPosition = fish.position;
				this._prevFishBackward = fish.backward;
				this._prevFishBackward2 = fish.backward2;
				this._prevFishRight = fish.right;
			}
			this._targetPosition = fish.position;
			this._targetFishBackward = fish.backward;
			this._targetFishBackward2 = fish.backward2;
			this._targetFishRight = fish.right;
			this.state = fish.state;
		}

		private Vector3 _prevPosition;

		private Vector3 _targetPosition;

		private Vector3 _prevFishBackward;

		private Vector3 _targetFishBackward;

		private Vector3 _prevFishBackward2;

		private Vector3 _targetFishBackward2;

		private Vector3 _prevFishRight;

		private Vector3 _targetFishRight;

		private bool _wasInitialized;

		private FishWaterDiturber _waterDisturber;

		private int[] bezierPPropIds;

		private int[] rightAxisPropIds;
	}
}
