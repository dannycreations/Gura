using System;
using System.Collections.Generic;
using UnityEngine;

internal class FishPhotoBehaviour : FishBehaviour
{
	public FishPhotoBehaviour(FishController owner, IFish fishTemplate, GameFactory.RodSlot slot)
		: base(owner, fishTemplate, slot)
	{
		this._modelMeshRenderer = RenderersHelper.GetRendererForObject(base.transform);
		this.InitFishBend();
	}

	private void InitFishBend()
	{
		if (this._modelMeshRenderer != null)
		{
			this.furDisplacementPropertyID = Shader.PropertyToID("Displacement");
			for (int i = 0; i < this._modelMeshRenderer.sharedMaterials.Length; i++)
			{
				if (this._modelMeshRenderer.sharedMaterials[i].HasProperty(this.furDisplacementPropertyID))
				{
					this.furMaterial = this._modelMeshRenderer.sharedMaterials[i];
					break;
				}
			}
			float num = 0f;
			float num2 = 1f / (float)this.bezierCurve.Order;
			for (int j = 0; j <= this.bezierCurve.Order; j++)
			{
				this.bezierCurve.AnchorPoints[j] = Vector3.forward * (this.fishMinZ + (this.fishMaxZ - this.fishMinZ) * num);
				num += num2;
			}
			for (int k = 0; k < this.bezierCurve.RightAxis.Length; k++)
			{
				this.bezierCurve.RightAxis[k] = Vector3.right;
			}
			this.fishChord = new List<Transform>();
			Transform transform = base.transform.Find("root");
			while (transform != null)
			{
				this.fishChord.Add(transform);
				transform = transform.Find("bone" + this.fishChord.Count);
			}
			transform = this.fishChord[this.fishChord.Count - 1].Find("tail_mid");
			if (transform != null)
			{
				this.fishChord.Add(transform);
				transform = this.fishChord[this.fishChord.Count - 1].Find("tail");
				if (transform != null)
				{
					this.fishChord.Add(transform);
				}
			}
			this.fishBaseChordPos = new Vector3[this.fishChord.Count];
			this.fishBaseChordRot = new Quaternion[this.fishChord.Count];
			for (int l = 0; l < this.fishChord.Count; l++)
			{
				this.fishBaseChordPos[l] = base.transform.InverseTransformPoint(this.fishChord[l].position);
				this.fishBaseChordRot[l] = this.fishChord[l].rotation * Quaternion.Inverse(base.transform.rotation);
				if (l == 0 || this.fishMinZ > this.fishBaseChordPos[l].z)
				{
					this.fishMinZ = this.fishBaseChordPos[l].z;
				}
				if (l == 0 || this.fishMaxZ < this.fishBaseChordPos[l].z)
				{
					this.fishMaxZ = this.fishBaseChordPos[l].z;
				}
			}
			this.chordDirectionSign = true;
		}
	}

	public override void LateUpdate()
	{
		this.UpdateFishBend();
	}

	private void UpdateFishBend()
	{
		if (this._modelMeshRenderer != null)
		{
			if (this.furMaterial != null)
			{
				this.furMaterial.SetVector(this.furDisplacementPropertyID, new Vector3(Mathf.PerlinNoise(0f, Time.time) - 0.5f, Mathf.PerlinNoise(Time.time, 0f) - 0.5f, Mathf.PerlinNoise(Time.time, Time.time) - 0.5f) * 2f);
			}
			float num = Mathf.Exp(-Time.deltaTime);
			this._owner.leftHandLeanDelta = Mathf.Clamp(this._owner.LeftHandRoot.position.y - this._owner._leftHand.position.y, -0.2f, 0.2f) * 10f;
			this._owner.rightHandLeanDelta = Mathf.Clamp(this._owner.RightHandRoot.position.y - this._owner._rightHand.position.y, -0.2f, 0.2f) * 10f;
			this.leftHandDeltaAccumulated += this._owner.leftHandLeanDelta * Time.deltaTime;
			this.rightHandDeltaAccumulated += this._owner.rightHandLeanDelta * Time.deltaTime;
			Vector3[] array = new Vector3[this.bezierCurve.Order + 1];
			float num2 = ((!this.chordDirectionSign) ? this.rightHandDeltaAccumulated : this.leftHandDeltaAccumulated);
			float num3 = ((!this.chordDirectionSign) ? this.leftHandDeltaAccumulated : this.rightHandDeltaAccumulated);
			Vector3 vector;
			vector..ctor(0f, -num2 / base.transform.localScale.y, this.fishMinZ);
			Vector3 vector2;
			vector2..ctor(0f, -num3 / base.transform.localScale.y, this.fishMaxZ);
			Vector3 vector3 = Vector3.Lerp(vector, vector2, 1f / (float)array.Length);
			Vector3 vector4 = Vector3.Lerp(vector, vector2, ((float)array.Length - 1f) / (float)array.Length);
			float num4 = 6f;
			float num5 = num4 * Mathf.Abs(this.fishMaxZ - this.fishMinZ) / 6.2831855f;
			float num6 = Vector3.Distance(vector3, vector4) * 0.5f;
			float num7 = Mathf.Sqrt(num5 * num5 - num6 * num6);
			Vector3 normalized = Vector3.Cross((vector4 - vector3).normalized, Vector3.right).normalized;
			Vector3 vector5 = (vector3 + vector4) * 0.5f - normalized * num7;
			for (int i = 0; i < array.Length; i++)
			{
				float num8 = (float)i / (float)(array.Length - 1);
				float num9 = ((!this.chordDirectionSign) ? (1f - num8) : num8);
				float num10 = (num8 - 0.5f) * 2f * 3.1415927f / num4;
				array[i] = vector5 + Quaternion.AngleAxis(num10 * 57.29578f, Vector3.right) * normalized * num5;
			}
			Vector3 vector6 = array[0] - array[1];
			vector6 = Quaternion.AngleAxis(this.headFallAngle, Vector3.left) * Quaternion.AngleAxis(this.headSwingAngle, Vector3.up) * vector6;
			array[0] = array[1] + vector6;
			Vector3 vector7 = array[array.Length - 1] - array[array.Length - 2];
			vector7 = Quaternion.AngleAxis(this.tailFallAngle, Vector3.right) * Quaternion.AngleAxis(this.tailSwingAngle, Vector3.up) * vector7;
			array[array.Length - 1] = array[array.Length - 2] + vector7;
			for (int j = 0; j <= this.bezierCurve.Order; j++)
			{
				this.bezierCurve.AnchorPoints[j] = array[j];
			}
			this.headSwingAngle = Mathf.Sin(Time.time * 5f) * 5f;
			this.tailSwingAngle = -Mathf.Sin(Time.time * 5f) * 50f;
			for (int k = 0; k < this.bezierCurve.RightAxis.Length; k++)
			{
				this.bezierCurve.RightAxis[k] = Vector3.right;
			}
			Vector3[] array2 = new Vector3[this.fishChord.Count];
			for (int l = 0; l < this.fishChord.Count; l++)
			{
				this.bezierCurve.SetT((this.fishBaseChordPos[l].z - this.fishMinZ) / (this.fishMaxZ - this.fishMinZ));
				array2[l] = base.transform.TransformPoint(this.bezierCurve.CurvedCylinderTransform(this.fishBaseChordPos[l]));
			}
			for (int m = 0; m < this.fishChord.Count; m++)
			{
				this.bezierCurve.SetT((this.fishBaseChordPos[m].z - this.fishMinZ) / (this.fishMaxZ - this.fishMinZ));
				this.fishChord[m].position = array2[m];
				this.fishChord[m].rotation = base.transform.rotation * (this.bezierCurve.CurvedCylinderTransformRotation(Quaternion.identity) * this.fishBaseChordRot[m]);
			}
			Vector3 vector8 = (this._owner._leftHand.position + this._owner._rightHand.position) * 0.5f - this._owner.CenterRoot.position;
			vector8.y = 0f;
			base.transform.position += vector8;
		}
	}

	private const float FishBendAmp = 0.06f;

	private const float FishBendFreq = 3f;

	private const float MaxLeanVelocity = 0.2f;

	private const float LeanVelocityMult = 10f;

	private Vector3[] fishBaseChordPos;

	private Quaternion[] fishBaseChordRot;

	private new List<Transform> fishChord;

	private float fishMinZ;

	private float fishMaxZ;

	private new int furDisplacementPropertyID;

	private new Material furMaterial;

	private SkinnedMeshRenderer _modelMeshRenderer;

	private float tailSwingAngle;

	private float tailFallAngle;

	private float headSwingAngle;

	private float headFallAngle;

	private const float tailSwingAmplitude = 50f;

	private const float headSwingAmplitude = 5f;

	private const float fishVerticalOffset = 0f;

	private float leftHandDeltaAccumulated;

	private float rightHandDeltaAccumulated;

	private bool chordDirectionSign;
}
