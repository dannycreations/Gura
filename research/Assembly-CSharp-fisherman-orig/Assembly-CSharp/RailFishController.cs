using System;
using System.Collections.Generic;
using Phy;
using UnityEngine;

public class RailFishController : MonoBehaviour
{
	private void Start()
	{
		this.ampFactor = 1f;
		this.freqFactor = 1f;
		this.prevPosition = base.transform.position;
		FishBehaviour.GetMeshZDimensions(base.transform.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh, out this.meshMinZ, out this.meshMaxZ);
		base.transform.localScale = new Vector3(this.FishScale, this.FishScale, this.FishScale);
		this.modelRoot = base.transform.Find("root");
		this.jawbone = base.transform.Find("root/head/jawbone");
		this.jawBaseAngle = this.jawbone.localEulerAngles.x;
		this.fishChord = FishBehaviour.initTransformableBones(this.modelRoot);
		this.chordLength = (this.fishChord[this.fishChord.Count - 1].basePosition.z - this.fishChord[0].basePosition.z) * this.FishScale;
		float num = 0f;
		float num2 = 1f / (float)this.bezierCurve.Order;
		for (int i = 0; i <= this.bezierCurve.Order; i++)
		{
			this.bezierCurve.AnchorPoints[i] = Vector3.forward * (this.meshMinZ + (this.meshMaxZ - this.meshMinZ) * num);
			num += num2;
		}
		this.bezierCurve.LateralScale = Vector2.one;
		for (int j = 0; j < this.bezierCurve.RightAxis.Length; j++)
		{
			this.bezierCurve.RightAxis[j] = Vector3.right;
		}
		this.wave = new Vector3[this.bezierCurve.Order + 1];
		this.debug_d = new float[this.wave.Length];
		this.debug_wavet = new float[this.wave.Length];
		this.debug_trajt = new float[this.wave.Length];
		this.waveStepsCountInv = 1f / (float)this.wave.Length;
		if (this.FollowMode)
		{
			this.rootTrajectory = new Vector3[(int)(this.chordLength / this.FollowTrajetoryStep) + 30];
			this.rootTangents = new Vector3[this.rootTrajectory.Length];
			this.rootTrajectoryCounter = 0;
			while (this.rootTrajectoryCounter < this.rootTrajectory.Length)
			{
				this.rootTrajectory[this.rootTrajectoryCounter] = base.transform.position - base.transform.forward * (float)(this.rootTrajectory.Length - this.rootTrajectoryCounter - 1) * this.FollowTrajetoryStep;
				this.rootTangents[this.rootTrajectoryCounter] = base.transform.forward;
				this.rootTrajectoryCounter++;
			}
			this.rootTrajectoryCounter = 0;
		}
		this.dp = new DebugPlotter("/monitor_output/RailFishController.adv", new string[0], true);
		this.dp_d = new DebugPlotter.Value[this.wave.Length];
		this.dp_wavet = new DebugPlotter.Value[this.wave.Length];
		this.dp_trajt = new DebugPlotter.Value[this.wave.Length];
		for (int k = 0; k < this.wave.Length; k++)
		{
			this.dp_d[k] = this.dp.AddValue("d_" + k);
			this.dp_wavet[k] = this.dp.AddValue("wavet_" + k);
			this.dp_trajt[k] = this.dp.AddValue("trajt_" + k);
		}
	}

	private void ApplyWaveAnimation()
	{
		if (!this.FollowMode)
		{
			for (int i = 0; i < this.wave.Length; i++)
			{
				this.wave[i] = Vector3.forward * (this.meshMinZ + (this.meshMaxZ - this.meshMinZ) * this.waveStepsCountInv * (float)i);
			}
		}
		this.TemporalPhase += Time.deltaTime * this.WaveTemporalFrequency * this.freqFactor;
		this.TemporalPhase %= 6.2831855f;
		this.ModulationPhase += Time.deltaTime * this.WaveModulationFrequency * this.freqFactor;
		this.ModulationPhase %= 6.2831855f;
		float num = this.WaveMaxAmplitude * Mathf.Cos(this.ModulationPhase) * this.ampFactor;
		float num2 = 0f;
		this.wave[0] += this.bezierCurve.RightAxis[0] * num * (1f - this.WaveSpatialEnvelopeFactor) * Mathf.Sin(this.TemporalPhase);
		for (int j = 1; j < this.wave.Length; j++)
		{
			num2 += 6.2831855f * this.waveStepsCountInv * this.WaveSpatialFrequency * this.freqFactor;
			this.wave[j] += this.bezierCurve.RightAxis[j - 1] * num * Mathf.Sin(this.TemporalPhase + num2) * (1f - this.WaveSpatialEnvelopeFactor * (1f - Mathf.Pow(this.waveStepsCountInv * (float)j, 2f)));
			if (this.WaveAnimNormalization)
			{
				this.wave[j] = this.wave[j - 1] + (this.wave[j] - this.wave[j - 1]).normalized * (this.meshMaxZ - this.meshMinZ) * this.waveStepsCountInv;
			}
		}
		for (int k = 0; k <= this.bezierCurve.Order; k++)
		{
			this.bezierCurve.AnchorPoints[k] = this.wave[k];
		}
	}

	public void ImmediateVelocity(float velocity)
	{
		this.filteredVelocity = velocity;
	}

	private void UpdateFollowMode()
	{
		float num = (this.rootTrajectory[this.rootTrajectoryCounter] - base.transform.position).magnitude;
		if (num >= this.FollowTrajetoryStep)
		{
			int num2 = this.rootTrajectoryCounter;
			this.rootTrajectoryCounter++;
			if (this.rootTrajectoryCounter == this.rootTrajectory.Length)
			{
				this.rootTrajectoryCounter = 0;
			}
			this.rootTrajectory[this.rootTrajectoryCounter] = base.transform.position;
			this.rootTangents[this.rootTrajectoryCounter] = (this.rootTrajectory[this.rootTrajectoryCounter] - this.rootTrajectory[num2]).normalized;
			num = 0f;
		}
		int num3 = this.rootTrajectoryCounter;
		for (int i = 1; i < this.rootTrajectory.Length; i++)
		{
			int num4 = (this.rootTrajectory.Length + this.rootTrajectoryCounter - i) % this.rootTrajectory.Length;
			Debug.DrawLine(this.rootTrajectory[num3], this.rootTrajectory[num4], Color.green);
			num3 = num4;
		}
		this.wave[0] = Vector3.zero;
		float num5 = num;
		this.debug_trajt[0] = num5;
		float num6 = 0f;
		int num7 = this.rootTrajectory.Length + this.rootTrajectoryCounter + 1;
		float num8 = 0f;
		for (int j = 1; j < this.wave.Length; j++)
		{
			num6 += (this.meshMaxZ - this.meshMinZ) * this.FishScale * this.waveStepsCountInv;
			while (num5 < num6)
			{
				num7--;
				if (num7 == this.rootTrajectoryCounter)
				{
					Debug.LogError("trace overflow");
					return;
				}
				num8 = (this.rootTrajectory[num7 % this.rootTrajectory.Length] - this.rootTrajectory[(num7 - 1) % this.rootTrajectory.Length]).magnitude;
				num5 += num8;
			}
			float num9 = (num5 - num6) / num8;
			this.wave[j] = base.transform.InverseTransformPoint(Vector3.Lerp(this.rootTrajectory[(num7 - 1) % this.rootTrajectory.Length], this.rootTrajectory[num7 % this.rootTrajectory.Length], num9));
			this.bezierCurve.RightAxis[j - 1] = -Vector3.Cross(Vector3.up, Vector3.Lerp(this.rootTangents[(num7 - 1) % this.rootTangents.Length], this.rootTangents[num7 % this.rootTrajectory.Length], num9));
			this.debug_d[j] = num9;
			this.debug_wavet[j] = num6;
			this.debug_trajt[j] = num5;
			if (this.dp != null)
			{
				this.dp_d[j].Add(num9);
				this.dp_wavet[j].Add(num6);
				this.dp_trajt[j].Add(num5);
			}
		}
		for (int k = 0; k <= this.bezierCurve.Order; k++)
		{
			this.bezierCurve.AnchorPoints[k] = this.wave[k];
		}
		if (this.AutoLocoWave)
		{
			this.filteredVelocity = Mathf.Lerp((base.transform.position - this.prevPosition).magnitude / Time.deltaTime, this.filteredVelocity, Mathf.Exp(-Time.deltaTime));
			this.prevPosition = base.transform.position;
			float num10 = Mathf.Min(this.filteredVelocity / this.NominalLocomotionVelocity, 2f);
			this.ampFactor = num10;
			this.freqFactor = num10;
		}
	}

	private void UpdateJawAnimation()
	{
		if (this.JawOpenCue)
		{
			this.jawOpenProgress = Mathf.Lerp(1f, this.jawOpenProgress, Mathf.Exp(-Time.deltaTime * this.JawOpenSpeed));
		}
		else
		{
			this.jawOpenProgress = Mathf.Lerp(0f, this.jawOpenProgress, Mathf.Exp(-Time.deltaTime * this.JawCloseSpeed));
		}
		this.jawbone.localRotation = Quaternion.Euler(-this.JawOpenAngle * this.jawOpenProgress + this.jawBaseAngle, 0f, 0f);
	}

	private void LateUpdate()
	{
		if (this.FollowMode)
		{
			this.UpdateFollowMode();
		}
		this.ApplyWaveAnimation();
		this.UpdateJawAnimation();
		for (int i = 0; i < this.fishChord.Count; i++)
		{
			this.bezierCurve.SetT((this.fishChord[i].basePosition.z - this.meshMinZ) / (this.meshMaxZ - this.meshMinZ));
			this.fishChord[i].transform.position = base.transform.TransformPoint(this.bezierCurve.CurvedCylinderTransform(this.fishChord[i].basePosition));
			this.fishChord[i].transform.rotation = this.bezierCurve.CurvedCylinderTransformRotation(this.fishChord[i].baseRotation) * base.transform.rotation;
		}
		if (this.AnchorNode != null)
		{
			this.modelRoot.localPosition -= base.transform.InverseTransformPoint(this.AnchorNode.position) * this.AnchorFactor;
		}
		if (this.Lure != null)
		{
			this.Lure.UpdateLine();
		}
	}

	public float FishScale = 1f;

	public float WaveMaxAmplitude = 0.05f;

	public float WaveModulationFrequency;

	public float WaveSpatialFrequency = 1f;

	public float WaveTemporalFrequency = -10f;

	public float WaveSpatialEnvelopeFactor = 0.8f;

	public float TemporalPhase;

	public float ModulationPhase;

	public Transform AnchorNode;

	public float AnchorFactor;

	public bool FollowMode;

	public float FollowTrajetoryStep = 0.005f;

	public bool AutoLocoWave;

	public float NominalLocomotionVelocity = 0.5f;

	public float JawOpenAngle = 25f;

	public float JawOpenSpeed = 5f;

	public float JawCloseSpeed = 50f;

	public bool JawOpenCue;

	public bool WaveAnimNormalization;

	public RailLureController Lure;

	private Transform modelRoot;

	private List<FishBehaviour.ChordNode> fishChord;

	private float meshMinZ;

	private float meshMaxZ;

	private BezierCurveWithTorsion bezierCurve = new BezierCurveWithTorsion(5);

	private Vector3[] wave;

	private float waveStepsCountInv;

	private Vector3[] rootTrajectory;

	private Vector3[] rootTangents;

	private float[] debug_d;

	private float[] debug_wavet;

	private float[] debug_trajt;

	private int rootTrajectoryCounter;

	private float chordLength;

	private float filteredVelocity;

	private Vector3 prevPosition;

	private float ampFactor;

	private float freqFactor;

	private Transform jawbone;

	private float jawOpenProgress;

	private float jawBaseAngle;

	public DebugPlotter dp;

	private DebugPlotter.Value[] dp_d;

	private DebugPlotter.Value[] dp_wavet;

	private DebugPlotter.Value[] dp_trajt;
}
