using System;
using System.Collections.Generic;
using Phy;
using UnityEngine;

namespace TPM
{
	public class Rod3rdBehaviour : RodBehaviour
	{
		public Rod3rdBehaviour(RodController controller, IAssembledRod rodAssembly, GameFactory.RodSlot rodSlot, bool isMain, string playerName)
			: base(controller, rodAssembly, rodSlot)
		{
			this._playerName = playerName;
			this._wasInitialized = false;
			this._prevPreprocessedPoints = new List<Vector3>(base.RodPointsCount);
			this._targetPreprocessedPoints = new List<Vector3>(base.RodPointsCount);
			this._preprocessedPoints = new List<Vector3>(base.RodPointsCount);
			for (int i = 0; i < base.RodPointsCount; i++)
			{
				this._prevPreprocessedPoints.Add(Vector3.zero);
				this._targetPreprocessedPoints.Add(Vector3.zero);
				this._preprocessedPoints.Add(Vector3.zero);
			}
			this._meshRenderers = RenderersHelper.GetAllRenderersForObject<MeshRenderer>(base.transform);
			if (isMain)
			{
				GameObject gameObject = new GameObject
				{
					name = "rodTransformHelper"
				};
				this._tHelper = gameObject.transform;
			}
			else
			{
				this._tHelper = base.transform;
			}
		}

		public Transform ServerRodTransform
		{
			get
			{
				return this._tHelper;
			}
		}

		public void SetVisibility(bool flag)
		{
			for (int i = 0; i < this._meshRenderers.Count; i++)
			{
				this._meshRenderers[i].enabled = flag;
			}
		}

		public void SetOpaque(float prc)
		{
			for (int i = 0; i < this._meshRenderers.Count; i++)
			{
				this._meshRenderers[i].material.SetFloat("_CharacterOpaque", prc);
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			if (this._tHelper != null)
			{
				Object.Destroy(this._tHelper.gameObject);
			}
		}

		public Vector3 GetRodPeakPosition()
		{
			List<Transform> lineLocatorsProp = base.LineLocatorsProp;
			return lineLocatorsProp[lineLocatorsProp.Count - 1].position;
		}

		public void ServerUpdate(TPMAssembledRod rodAssembly, float dtPrc)
		{
			if (rodAssembly == null)
			{
				this._wasInitialized = false;
				return;
			}
			if (this.segmentsLength == null || this.segmentsLength.Length != rodAssembly.RodPoints.Count)
			{
				this.segmentsLength = new float[rodAssembly.RodPoints.Count];
			}
			TransformData rodTransformData = rodAssembly.RodTransformData;
			List<Vector3> rodPoints = rodAssembly.RodPoints;
			Vector3 vector = rodAssembly.lineData.mainAndLeaderPoints[1];
			Vector3 vector2 = base.transform.TransformPoint(rodPoints[rodPoints.Count - 1]);
			Vector3 vector3 = rodPoints[rodPoints.Count - 1];
			Vector3 normalized = (vector - base.transform.position).normalized;
			float num = Vector3.Angle((rodPoints[1] - rodPoints[0]).normalized, (rodPoints[rodPoints.Count - 1] - rodPoints[rodPoints.Count - 2]).normalized) * 0.017453292f;
			float num2 = Vector3.Angle(base.transform.forward, normalized) * 0.017453292f;
			float num3 = Mathf.Min(num, num2);
			float num4 = num3 / num2;
			Vector3 vector4 = base.transform.InverseTransformDirection(normalized);
			Vector3 vector5 = Vector3.Slerp(Vector3.forward, base.transform.InverseTransformDirection(normalized), num4);
			Vector3 vector6 = vector5 * rodPoints[rodPoints.Count - 1].magnitude;
			Vector3 vector7 = Vector3.Slerp(Vector3.forward, (base.transform.InverseTransformPoint(vector) - vector6).normalized, num4);
			for (int i = 1; i < this.segmentsLength.Length; i++)
			{
				this.segmentsLength[i] = (rodPoints[i] - rodPoints[i - 1]).magnitude;
			}
			for (int j = 1; j < rodPoints.Count; j++)
			{
				float num5 = (float)j / (float)(rodPoints.Count - 1);
				rodPoints[j] = rodPoints[j - 1] + Vector3.Slerp(Vector3.forward, vector5, num5) * this.segmentsLength[j];
			}
			if (rodPoints.Count != base.RodPointsCount)
			{
				LogHelper.Error("TPM User::{5} Test::Rod3rdBehaviour.Invalid rod replacement {0}(Slot:{1}) -> {2}(Slot:{3}). _wasInitialized = {4}", new object[]
				{
					rodAssembly.RodInterface.Asset,
					rodAssembly.RodInterface.Slot,
					this._rodAssembly.RodInterface.Asset,
					this._rodAssembly.RodInterface.Slot,
					this._wasInitialized,
					this._playerName
				});
				this._wasInitialized = false;
				base.RodPointsCount = rodPoints.Count;
				this._prevPreprocessedPoints.Clear();
				this._targetPreprocessedPoints.Clear();
				this._preprocessedPoints.Clear();
				for (int k = 0; k < base.RodPointsCount; k++)
				{
					this._prevPreprocessedPoints.Add(Vector3.zero);
					this._targetPreprocessedPoints.Add(Vector3.zero);
					this._preprocessedPoints.Add(Vector3.zero);
				}
			}
			if (this._wasInitialized)
			{
				for (int l = 0; l < base.RodPointsCount; l++)
				{
					this._prevPreprocessedPoints[l] = Vector3.Lerp(this._prevPreprocessedPoints[l], this._targetPreprocessedPoints[l], dtPrc);
					this._targetPreprocessedPoints[l] = rodPoints[l];
				}
				this._prevPosition = Vector3.Lerp(this._prevPosition, this._targetPosition, dtPrc);
				this._targetPosition = rodTransformData.position;
				this._prevRotation = Quaternion.Slerp(this._prevRotation, this._targetRotation, dtPrc);
				this._targetRotation = rodTransformData.rotation;
			}
			else
			{
				this._wasInitialized = true;
				for (int m = 0; m < base.RodPointsCount; m++)
				{
					this._prevPreprocessedPoints[m] = rodPoints[m];
					this._targetPreprocessedPoints[m] = rodPoints[m];
				}
				this._prevPosition = rodTransformData.position;
				this._targetPosition = rodTransformData.position;
				this._prevRotation = rodTransformData.rotation;
				this._targetRotation = rodTransformData.rotation;
			}
		}

		private List<Vector3> getInitialBendPoints()
		{
			Transform transform = this._owner.segment.lastTransform;
			this._owner.segment.rodLength = 0f;
			bool flag = false;
			while (!flag)
			{
				this._owner.segment.ChainLength++;
				this._owner.segment.rodLength += (transform.position - transform.parent.position).magnitude;
				transform = transform.parent;
				flag = transform == this._owner.segment.firstTransform;
			}
			BendingSegment segment = this._owner.segment;
			List<Vector3> list = new List<Vector3>();
			RodSegmentConfig rodSegmentConfig = RodObject.DecodeRodTemplate(segment.action);
			RodSegmentConfig quiver = RodSegmentConfig.Quiver;
			float num = 0f;
			if (quiver != null)
			{
				num = quiver.GetNormalizeLengthFactor();
			}
			Vector3 vector = segment.firstTransform.localPosition;
			for (int i = 0; i < base.RodPointsCount; i++)
			{
				if (i < rodSegmentConfig.Config.Length)
				{
					SegmentConfig segmentConfig = rodSegmentConfig.Config[i];
					vector += Vector3.forward * (segment.rodLength - segment.quiverLength) * segmentConfig.SegmentLength;
					list.Add(vector);
				}
				else
				{
					SegmentConfig segmentConfig2 = quiver.Config[i - rodSegmentConfig.Config.Length];
					vector += Vector3.forward * segment.quiverLength * segmentConfig2.SegmentLength * num;
					list.Add(vector);
				}
			}
			return list;
		}

		public Vector3 ServerTipPos
		{
			get
			{
				return this._serverTipPos;
			}
		}

		public void SyncUpdate(float dtPrc)
		{
			this._tHelper.rotation = Quaternion.Slerp(this._prevRotation, this._targetRotation, dtPrc);
			this._tHelper.position = Vector3.Lerp(this._prevPosition, this._targetPosition, dtPrc);
			for (int i = 0; i < base.RodPointsCount; i++)
			{
				this._preprocessedPoints[i] = Vector3.Lerp(this._prevPreprocessedPoints[i], this._targetPreprocessedPoints[i], dtPrc);
			}
			this._serverTipPos = this._tHelper.TransformPoint(this._preprocessedPoints[this._preprocessedPoints.Count - 1]);
			if (this.Rings == null)
			{
				base.InitProceduralBend(this.getInitialBendPoints());
			}
			base.UpdateProceduralBend(this._preprocessedPoints);
			if (base.Bell != null)
			{
				base.Bell.OnLateUpdate();
			}
		}

		private List<Vector3> _prevPreprocessedPoints;

		private List<Vector3> _targetPreprocessedPoints;

		private List<Vector3> _preprocessedPoints;

		private Vector3 _prevPosition;

		private Vector3 _targetPosition;

		private Quaternion _prevRotation;

		private Quaternion _targetRotation;

		private bool _wasInitialized;

		private Transform _tHelper;

		private readonly List<MeshRenderer> _meshRenderers;

		private readonly string _playerName;

		private float[] segmentsLength;

		private Vector3 _serverTipPos;
	}
}
