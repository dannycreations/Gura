using System;
using System.Collections.Generic;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;
using ObjectModel;
using Phy;
using UnityEngine;

public class Line1stBehaviour : LineBehaviour, ILineController
{
	public Line1stBehaviour(LineController owner, IAssembledRod rodAssembly, GameFactory.RodSlot rodSlot)
		: base(owner, rodAssembly, rodSlot)
	{
		Line line = rodAssembly.LineInterface as Line;
		base.IsBraid = line.ItemSubType == ItemSubTypes.BraidLine;
		this.Extensibility = ((line.Extensibility != 0f) ? line.Extensibility : 0.1f);
		this.Thickness = ((line.Thickness != 0f) ? line.Thickness : 0.1f);
		this._serverMainAndLeaderPoints = new Vector3[3];
		this._sinkersFirstPoint = Vector3.zero;
	}

	public Vector3[] ServerMainAndLeaderPoints
	{
		get
		{
			return this._serverMainAndLeaderPoints;
		}
	}

	public Vector3 SinkersFirstPoint
	{
		get
		{
			return this._sinkersFirstPoint;
		}
	}

	public bool TerrainHit
	{
		get
		{
			return this._terrainHit;
		}
	}

	public float IndicatedLineLength
	{
		get
		{
			if (base.Reel.IsIndicatorOn)
			{
				return base.Rod.LineOnRodLength + this.LineLength;
			}
			return 0f;
		}
	}

	public new void SetVisibility(bool flag)
	{
		this.lrLineOnRodObject.enabled = flag;
		this.lrLineObject.enabled = flag;
		this.lrLineLeaderObject.enabled = flag;
		this.lrLineLeashObject.enabled = flag;
		for (int i = 0; i < this._sinkerRenderers.Length; i++)
		{
			this._sinkerRenderers[i].enabled = flag;
		}
	}

	public override void Init(RodOnPodBehaviour.TransitionData transitionData)
	{
		base.Init(transitionData);
		this.rodLinePointsCount = base.Rod.RingsCount + 1;
		this.lrLineOnRodObject.positionCount = this.rodLinePointsCount;
		this.lineOnRodPoints = new Vector3[base.Rod.RingsCount + 2];
		this._lineBezierCurve = new BezierCurve(2);
	}

	public float Extensibility { get; set; }

	public float Thickness { get; set; }

	public float LineLength
	{
		get
		{
			return Mathf.Min(base.RodSlot.Sim.FinalLineLength, base.MaxLineLength);
		}
	}

	public override ObscuredFloat SecuredLineLength
	{
		get
		{
			return Mathf.Min(base.RodSlot.Sim.FinalLineLength, base.MaxLineLength);
		}
	}

	public override float FullLineLength
	{
		get
		{
			return base.RodSlot.Sim.FullLineLength;
		}
	}

	public bool IsOverloaded
	{
		get
		{
			return this.AppliedForce >= base.MaxLoad;
		}
	}

	public void ResetToMinLength()
	{
		base.RodSlot.Sim.ResetLineToMinLength();
	}

	public Mass GetKinematicMass()
	{
		Mass mass = this.lineObject.Masses[this.lineObject.Masses.Count - 1];
		mass.IsKinematic = true;
		mass.StopMass();
		return mass;
	}

	public Spring GetKinematicMassSpring()
	{
		Spring spring = this.lineObject.Springs[this.lineObject.Springs.Count - 2];
		spring.SpringLength = 0.05f;
		return spring;
	}

	public void ReleaseKinematicMass(Mass kinematicLineMass)
	{
		kinematicLineMass.IsKinematic = false;
		kinematicLineMass.StopMass();
	}

	public void SetLineSpringConstant(float springConstant)
	{
		for (int i = 0; i < this.lineObject.Springs.Count; i++)
		{
			this.lineObject.Springs[i].SpringConstant = springConstant;
		}
		for (int j = 0; j < this.leaderObject.Springs.Count; j++)
		{
			this.leaderObject.Springs[j].SpringConstant = springConstant;
		}
		if (this.leashObject != null)
		{
			for (int k = 0; k < this.leashObject.Springs.Count; k++)
			{
				this.leashObject.Springs[k].SpringConstant = springConstant;
			}
		}
	}

	public float GetLineTensionFactor()
	{
		float num = 0f;
		for (int i = 0; i < this.lineObject.Springs.Count; i++)
		{
			Spring spring = this.lineObject.Springs[i];
			if (spring.SpringLength > 0f)
			{
				num += ((spring.Mass1.Position - spring.Mass2.Position).magnitude - spring.SpringLength) / spring.SpringLength;
			}
		}
		return Mathf.Max(num / (float)this.lineObject.Springs.Count, 0f);
	}

	public float GetLinePhysicsLength()
	{
		float num = 0f;
		for (int i = 0; i < this.lineObject.Springs.Count; i++)
		{
			Spring spring = this.lineObject.Springs[i];
			num += (spring.Mass2.Position - spring.Mass1.Position).magnitude;
		}
		return num;
	}

	public float GetLineAirStrain()
	{
		float num = 0f;
		Vector3 vector = Vector3.zero;
		Mass mass = base.RodSlot.Sim.RodTipMass;
		while (mass.NextSpring != null)
		{
			if ((mass != base.RodSlot.Sim.RodTipMass && mass.Type != Mass.MassType.Line) || mass.IsKinematic)
			{
				break;
			}
			Spring nextSpring = mass.NextSpring;
			vector = nextSpring.Mass2.Position;
			if (nextSpring.Mass1.Position.y > 0f && nextSpring.Mass2.Position.y < 0f)
			{
				vector = Vector3.Lerp(nextSpring.Mass1.Position, nextSpring.Mass2.Position, nextSpring.Mass1.Position.y / (nextSpring.Mass1.Position.y - nextSpring.Mass2.Position.y));
				num += (nextSpring.Mass1.Position - vector).magnitude;
				break;
			}
			num += (nextSpring.Mass2.Position - nextSpring.Mass1.Position).magnitude;
			mass = mass.NextSpring.Mass2;
		}
		return (base.RodSlot.Sim.RodTipMass.Position - vector).magnitude / num;
	}

	public void GradualFinalLineLengthChange(float length, int steps)
	{
		if (steps > 0)
		{
			this.gradualFinalLineLength = length;
			this.gradualFinalLineLengthChangeSteps = steps - 1;
			float num = (this.gradualFinalLineLength - base.RodSlot.Sim.CurrentLineLength) / (float)steps;
			base.RodSlot.Sim.FinalLineLength = base.RodSlot.Sim.CurrentLineLength + num;
		}
	}

	public SpringDrivenObject LineObj
	{
		get
		{
			return this.lineObject;
		}
	}

	public SpringDrivenObject LeaderObj
	{
		get
		{
			return this.leaderObject;
		}
	}

	public SpringDrivenObject LeashObj
	{
		get
		{
			return this.leashObject;
		}
	}

	public bool IsLineAvailable
	{
		get
		{
			return this.SecuredLineLength > base.MinLineLength;
		}
	}

	public void CreateLine(Vector3 lineTipPosition, float extraLength = 0f)
	{
		base.RodSlot.Sim.AddLine(lineTipPosition, extraLength);
	}

	public void RefreshObjectsFromSim()
	{
		this.lineObject = (SpringDrivenObject)base.RodSlot.Sim.Objects.First((PhyObject i) => i.Type == PhyObjectType.Line);
		this.leaderObject = (SpringDrivenObject)base.RodSlot.Sim.Objects.FirstOrDefault((PhyObject i) => i.Type == PhyObjectType.Leader);
		this.leashObject = (SpringDrivenObject)base.RodSlot.Sim.Objects.FirstOrDefault((PhyObject i) => i.Type == PhyObjectType.Leash);
		this.sinkerObject = base.RodSlot.Sim.Objects.FirstOrDefault((PhyObject i) => i.Type == PhyObjectType.Sinker);
		if (this.sinkerObject == null)
		{
			return;
		}
		if (this.sinkerObject.Masses.Count != 3 || base.Sinkers.Count != 3)
		{
			throw new PrefabConfigException("Wrong sinker count");
		}
		for (int j = 0; j < 3; j++)
		{
			GameObject gameObject = base.Sinkers[j];
			Mass mass = this.sinkerObject.Masses[j];
			gameObject.transform.localScale *= Mathf.Pow(mass.MassValue / base.SinkerMass, 0.33333334f);
		}
	}

	public override void OnLateUpdate()
	{
		base.OnLateUpdate();
		this.UpdateLineOnRod();
		this.UpdateMainLine();
		this.UpdateExtraLine();
		this.CreateWaterDisturbance();
		this.GradualFinalLineLengthChange(this.gradualFinalLineLength, this.gradualFinalLineLengthChangeSteps);
		Vector3 vector = this._serverMainAndLeaderPoints[2];
		if (vector.y <= 0.01f)
		{
			this._terrainHit = true;
		}
		else if (this._rayCastUpdatesLeft == 0)
		{
			this._rayCastUpdatesLeft = 5;
			this._terrainHit = Math3d.GetMaskedRayContactPoint(vector + this.RAY_CAST_DIST, vector - this.RAY_CAST_DIST, GlobalConsts.GroundObstacleMask) != null;
		}
		else
		{
			this._rayCastUpdatesLeft -= 1;
		}
		if (StaticUserData.RodInHand.IsRodDisassembled)
		{
			return;
		}
		this.CheckLineLengthHack();
	}

	private void CheckLineLengthHack()
	{
		if (Math.Abs(this.SecuredLineLength - this.LineLength) > 0.5f)
		{
			InjectionDetecting.Instance.DetectedLineLengthHack(this.LineLength, this.SecuredLineLength);
		}
	}

	public override void CalculateAppliedForce()
	{
		base.CalculateAppliedForce();
		if (base.Rod.RodAssembly.IsRodDisassembled || base.Tackle.IsShowing)
		{
			this.AppliedForce = 0f;
		}
	}

	public Vector3[] GetMainLineSpline(int resampledCount)
	{
		if (resampledCount == 0)
		{
			resampledCount = this.lrLineObject.positionCount;
		}
		Vector3[] array = new Vector3[resampledCount];
		if (resampledCount == this.lrLineObject.positionCount)
		{
			Array.Copy(this.mainLineSplineCache, array, resampledCount);
		}
		else
		{
			float num = 0f;
			float[] array2 = new float[this.lrLineObject.positionCount - 1];
			for (int i = 1; i < this.lrLineObject.positionCount; i++)
			{
				num += (this.mainLineSplineCache[i] - this.mainLineSplineCache[i - 1]).magnitude;
				array2[i - 1] = num;
			}
			float num2 = num / (float)resampledCount;
			int num3 = 0;
			int num4 = -1;
			float num5 = 0f;
			array[0] = this.mainLineSplineCache[0];
			while (num5 < num)
			{
				num3++;
				float num6 = num2 * (float)num3;
				while (num5 < num6 && num4 < array2.Length - 1)
				{
					num4++;
					num5 = array2[num4];
				}
				if (num3 < array.Length && num4 > 0)
				{
					float num7 = array2[num4 - 1];
					array[num3] = Vector3.Lerp(this.mainLineSplineCache[num4], this.mainLineSplineCache[num4 + 1], (num6 - num7) / (num5 - num7));
				}
			}
			for (int j = num3 + 1; j < array.Length; j++)
			{
				array[j] = this.mainLineSplineCache[this.lrLineObject.positionCount - 1];
			}
		}
		return array;
	}

	public void SetOnPod(RodPodController rodPod, int rodPodSlot)
	{
		this.isOnPod = rodPod != null;
		this.rodPod = rodPod;
		this.lineTransitionStartTime = Time.time;
		this.firstRing = 0;
		if (this.isOnPod)
		{
			this.podLineSpline = this.GetMainLineSpline(101);
			this.signalizerObject = rodPod.GetSignalizer(rodPodSlot);
			if (this.signalizerObject != null)
			{
				float z = base.Rod.transform.InverseTransformPoint(this.signalizerObject.ClipPosition).z;
				for (int i = 0; i < base.Rod.RingsCount; i++)
				{
					float z2 = base.Rod.GetRingCenterLocalUnbent(i).z;
					if (z2 >= z)
					{
						this.firstRing = i;
						break;
					}
				}
			}
		}
		else
		{
			this.signalizerObject = null;
			this.firstRing = 0;
		}
	}

	private void UpdateMainLine()
	{
		if (this.lineObject == null || base.Rod == null)
		{
			return;
		}
		Vector3 lineEndPosition = base.Tackle.LineEndPosition;
		this.tensionFactor = Mathf.Lerp(this.GetLineTensionFactor() * 10000f, this.tensionFactor, Mathf.Pow(0.5f, Time.deltaTime * 2f));
		this._owner.DebugNoiseAmp = Mathf.Exp(-this.tensionFactor) * 0f;
		this.surfaceCrossingPoint = null;
		if (base.RodSlot.SimThread != null && !this.isOnPod)
		{
			this.lrLineObject.positionCount = 1 + base.RodSlot.SimThread.BuildBezier5CatmullRomComposite(base.RodSlot.Sim.RodTipMass.UID, base.Tackle.phyObject.UID, this._owner.DebugNoiseAmp, 0f, ref this.mainLineSplineCache, 60, 5, base.Rod.LineLocatorCorrection());
			if (this.lrLineObject.positionCount <= 1)
			{
				return;
			}
			bool flag = (!base.Tackle.FishIsShowing && !base.Tackle.ItemIsShowing) || base.Tackle.IsShowingComplete;
			Vector3 vector = base.Rod.RootPositionCorrection(this.mainLineSplineCache[0], base.RodSlot.Sim.RodTipMass.GroundHeight);
			Vector3 vector2 = base.Rod.RootPositionCorrection(this.mainLineSplineCache[this.lrLineObject.positionCount - 2], base.RodSlot.Sim.LineTipMass.GroundHeight);
			if (flag)
			{
				Vector3 vector3 = this.mainLineSplineCache[0] + vector;
				Vector3 vector4 = this.mainLineSplineCache[this.lrLineObject.positionCount - 2] + vector2;
				vector += base.Rod.RootRotationCorrect(vector3) - vector3;
				vector2 += base.Rod.RootRotationCorrect(vector4) - vector4;
			}
			int num = 0;
			int num2 = this.lrLineObject.positionCount - 1;
			for (int i = num; i < num2; i++)
			{
				float num3 = (float)i / (float)(this.lrLineObject.positionCount - 2);
				this.mainLineSplineCache[i] += Vector3.Lerp(vector, vector2, 2f * num3 - num3 * num3);
				if (i > 0)
				{
					Vector3? vector5 = this.surfaceCrossingPoint;
					if (vector5 == null && this.mainLineSplineCache[i].y < 0f && this.mainLineSplineCache[i - 1].y >= 0f)
					{
						this.surfaceCrossingPoint = new Vector3?(Vector3.Lerp(this.mainLineSplineCache[i - 1], this.mainLineSplineCache[i], this.mainLineSplineCache[i - 1].y / (this.mainLineSplineCache[i - 1].y - this.mainLineSplineCache[i].y)));
					}
				}
			}
			this.mainLineSplineCache[this.lrLineObject.positionCount - 1] = lineEndPosition;
			if ((!base.Tackle.FishIsShowing && !base.Tackle.ItemIsShowing && base.Tackle.Swivel == null && !base.Tackle.IsSwimbait) || base.Tackle.TackleType == FishingRodSimulation.TackleType.CarpClassic || base.Tackle.TackleType == FishingRodSimulation.TackleType.CarpMethod || base.Tackle.TackleType == FishingRodSimulation.TackleType.CarpPVABag || base.Tackle.TackleType == FishingRodSimulation.TackleType.CarpPVAStick)
			{
				this.lrLineObject.positionCount--;
			}
			this.lrLineObject.SetPositions(this.mainLineSplineCache);
		}
		else
		{
			base.RodSlot.SimThread.ResetBezier5CatmullRomComposite();
			Vector3 position = base.Rod.LineLocatorsProp[base.Rod.LineLocatorsProp.Count - 1].position;
			this._lineBezierCurve.AnchorPoints[0] = position;
			this._lineBezierCurve.AnchorPoints[2] = lineEndPosition;
			this._lineBezierCurve.AnchorPoints[1] = (this._lineBezierCurve.AnchorPoints[0] + this._lineBezierCurve.AnchorPoints[2]) * 0.5f;
			float magnitude = (position - lineEndPosition).magnitude;
			float y = this._lineBezierCurve.AnchorPoints[0].y;
			float y2 = this._lineBezierCurve.AnchorPoints[2].y;
			this._lineBezierCurve.AnchorPoints[1].y = Mathf.Lerp(y2, (y + y2) * 0.5f, Mathf.Min(1f, Mathf.Exp(magnitude - this.LineLength)));
			this.lrLineObject.positionCount = 101;
			this.lrLineObject.SetPosition(0, position);
			for (int j = 0; j < 100; j++)
			{
				float num4 = (float)(j + 1) / 99f;
				this.lrLineObject.SetPosition(j + 1, Vector3.Lerp(this._lineBezierCurve.Point(num4), this.podLineSpline[j] + (position - this.podLineSpline[0]), Mathf.Exp(this.lineTransitionStartTime - Time.time)));
			}
		}
		Vector3[] serverMainAndLeaderPoints = this._serverMainAndLeaderPoints;
		int num5 = 0;
		Vector3? vector6 = this.surfaceCrossingPoint;
		serverMainAndLeaderPoints[num5] = ((vector6 == null) ? this.lrLineObject.GetPosition(this.lrLineObject.positionCount / 2) : this.surfaceCrossingPoint.Value);
		this._serverMainAndLeaderPoints[1] = lineEndPosition;
	}

	private bool hasSignalizerAttached
	{
		get
		{
			return this.signalizerObject != null && (GameFactory.Player == null || (GameFactory.Player.State != typeof(ReplaceRodOnPodTakeAndPut) && GameFactory.Player.State != typeof(ReplaceRodOnPodOut)));
		}
	}

	private void UpdateLineOnRod()
	{
		bool hasSignalizerAttached = this.hasSignalizerAttached;
		int num = 0;
		if (hasSignalizerAttached)
		{
			num = 2;
			this.lrLineOnRodObject.positionCount = base.Rod.RingsCount + 1 + num - this.firstRing;
		}
		else
		{
			this.lrLineOnRodObject.positionCount = base.Rod.RingsCount + 1;
			this.firstRing = 0;
		}
		Vector3 position = base.Reel.ReelHandler.LineArcLocator.transform.position;
		Vector3 position2 = base.RodSlot.Sim.RodTipMass.NextSpring.Mass2.NextSpring.Mass2.Position;
		Vector3 vector = position;
		Vector3 vector2 = base.Rod.GetNearestPointOnRing(position2, vector, base.Rod.RingsCount - 1, false);
		this.lrLineOnRodObject.SetPosition(0, position);
		if (hasSignalizerAttached)
		{
			this.lrLineOnRodObject.SetPosition(1, this.signalizerObject.ClipPosition);
			this.lrLineOnRodObject.SetPosition(2, this.signalizerObject.NestPosition);
			vector = this.signalizerObject.NestPosition;
		}
		this.lrLineOnRodObject.SetPosition(base.Rod.RingsCount + num - this.firstRing, base.Rod.RodTipLocator.position);
		this.lineOnRodPoints[0] = position;
		this.lineOnRodPoints[this.lineOnRodPoints.Length - 1 - this.firstRing] = position2;
		for (int i = base.Rod.RingsCount - 1; i >= this.firstRing; i--)
		{
			Vector3 nearestPointOnRing = base.Rod.GetNearestPointOnRing(vector2, vector, i, false);
			this.lineOnRodPoints[i + 1 - this.firstRing] = nearestPointOnRing;
			vector2 = nearestPointOnRing;
			if (i == base.Rod.RingsCount - 1)
			{
				base.LastRingLinePosition = nearestPointOnRing;
			}
		}
		for (int j = 1 + this.firstRing; j < base.Rod.RingsCount + 1; j++)
		{
			Vector3 nearestPointOnRing2 = base.Rod.GetNearestPointOnRing(this.lineOnRodPoints[j - 1 - this.firstRing], this.lineOnRodPoints[j + 1 - this.firstRing], j - 1, false);
			this.lineOnRodPoints[j - this.firstRing] = nearestPointOnRing2;
			if (j == base.Rod.RingsCount)
			{
				base.LastRingLinePosition = nearestPointOnRing2;
			}
		}
		for (int k = 1 + this.firstRing; k < base.Rod.RingsCount + 1; k++)
		{
			this.lrLineOnRodObject.SetPosition(k + num - this.firstRing, this.lineOnRodPoints[k - this.firstRing]);
		}
	}

	private void UpdateExtraLine()
	{
		if (this.leaderObject == null)
		{
			this._serverMainAndLeaderPoints[2] = this._serverMainAndLeaderPoints[1];
			return;
		}
		Transform leaderEndAnchor = base.Tackle.LeaderEndAnchor;
		if (base.Tackle.IsVisibleLeader)
		{
			this.DrawExtraLine(this.leaderObject.Masses, this.lrLineLeaderObject, base.Tackle.LeaderTopPosition, leaderEndAnchor.position);
		}
		if (leaderEndAnchor != null)
		{
			this._serverMainAndLeaderPoints[2] = leaderEndAnchor.position;
		}
		if (this.leashObject != null)
		{
			this.DrawExtraLine(this.leashObject.Masses, this.lrLineLeashObject, base.Tackle.LeashTopPosition, base.Tackle.LeashEndPosition);
			if (base.Tackle.TackleType == FishingRodSimulation.TackleType.Float && base.Tackle.leashEndAnchor != null)
			{
				this._serverMainAndLeaderPoints[2] = base.Tackle.LeashEndPosition;
			}
		}
		if (this.sinkerObject == null)
		{
			this._sinkersFirstPoint = this.leaderObject.Masses[0].Position;
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			GameObject gameObject = base.Sinkers[i];
			Mass mass = this.sinkerObject.Masses[i];
			gameObject.SetActive((!base.Tackle.IsShowing || i != 2) && GameFactory.Player.State != typeof(PlayerDrawOut));
			gameObject.transform.position = base.Rod.PositionCorrection(mass, true);
		}
		this._sinkersFirstPoint = this.sinkerObject.Masses[0].Position;
	}

	private void DrawExtraLine(IList<Mass> lineParts, LineRenderer lineRenderer, Vector3 startPosition, Vector3 endPosition)
	{
		this.extraLineMassPositions.Clear();
		this.extraLineMassPositions.Add(startPosition);
		bool flag = base.Tackle.IsShowingComplete || (!base.Tackle.FishIsShowing && !base.Tackle.ItemIsShowing);
		for (int i = 0; i < lineParts.Count; i++)
		{
			Mass mass = lineParts[i];
			this.extraLineMassPositions.Add(base.Rod.PositionCorrection(mass, flag));
			if (i > 0)
			{
				Vector3? vector = this.surfaceCrossingPoint;
				if (vector == null && this.extraLineMassPositions[i].y < 0f && this.extraLineMassPositions[i - 1].y >= 0f)
				{
					this.surfaceCrossingPoint = new Vector3?(Vector3.Lerp(this.extraLineMassPositions[i - 1], this.extraLineMassPositions[i], this.extraLineMassPositions[i - 1].y / (this.extraLineMassPositions[i - 1].y - this.extraLineMassPositions[i].y)));
				}
			}
			if (mass.IsKinematic && (base.Tackle.FishIsShowing || base.Tackle.ItemIsShowing))
			{
				this.extraLineMassPositions.Add(base.Rod.PositionCorrection(mass, flag));
				break;
			}
		}
		Vector3? vector2 = this.surfaceCrossingPoint;
		if (vector2 == null)
		{
			for (int j = 1; j < base.RodSlot.Sim.TackleObject.Masses.Count; j++)
			{
				if (j > 0)
				{
					Vector3 position = base.RodSlot.Sim.TackleObject.Masses[j].Position;
					Vector3 position2 = base.RodSlot.Sim.TackleObject.Masses[j - 1].Position;
					if (position.y < 0f && position2.y >= 0f)
					{
						this.surfaceCrossingPoint = new Vector3?(Vector3.Lerp(position2, position, position2.y / (position2.y - position.y)));
						break;
					}
				}
			}
		}
		SplineBuilder.BuildHermite(this.extraLineMassPositions, endPosition, lineRenderer);
	}

	private void CreateWaterDisturbance()
	{
		if (GameFactory.Water != null)
		{
			Vector3? vector = this.surfaceCrossingPoint;
			if (vector != null)
			{
				GameFactory.Water.AddWaterDisturb(this.surfaceCrossingPoint.Value, 0.01f, WaterDisturbForce.XXSmall);
			}
		}
	}

	private Vector3? GetWaterSufaceCrossingPoint()
	{
		for (int i = 0; i < this.lineObject.Masses.Count - 1; i++)
		{
			Mass mass = this.lineObject.Masses[i];
			Mass mass2 = this.lineObject.Masses[i + 1];
			float y = mass.Position.y;
			float y2 = mass2.Position.y;
			if (y > 0f && y2 < 0f)
			{
				return new Vector3?(Vector3.Lerp(mass.Position, mass2.Position, y / (y - y2)));
			}
		}
		return null;
	}

	public override bool IsTensioned
	{
		get
		{
			Mass rodTipMass = base.RodSlot.Sim.RodTipMass;
			Mass lineTensionDetectorMass = base.RodSlot.Sim.LineTensionDetectorMass;
			return Vector3.Distance(rodTipMass.Position, lineTensionDetectorMass.Position) >= base.RodSlot.Sim.CurrentLineTensionSegmentLength * 1f;
		}
	}

	public override bool IsSlacked
	{
		get
		{
			return !base.Rod.IsOngoingRodPodTransition && (Time.time < base.Rod.RodPodTransitionEndTimestamp || Time.time >= base.Rod.RodPodTransitionEndTimestamp + 3f || Time.time <= base.Rod.RodPodTransitionPreviousTimestamp + 30f) && !this.IsTensioned && this.AppliedForce / base.MaxLoad < 0.05f;
		}
	}

	public void ResetLineWidthChange(float newTargetWidth)
	{
		base.StartLineWidthTransition(newTargetWidth);
	}

	private const float WATER_DETECTION_PRECISION = 0.2f;

	private Vector3[] _serverMainAndLeaderPoints;

	private Vector3 _sinkersFirstPoint;

	private float tensionFactor;

	private float gradualFinalLineLength;

	private int gradualFinalLineLengthChangeSteps;

	private SpringDrivenObject lineObject;

	private SpringDrivenObject leaderObject;

	private SpringDrivenObject leashObject;

	private PhyObject sinkerObject;

	private float noiseAmplitude;

	private bool armed;

	private Vector3[] mainLineSplineCache = new Vector3[1000];

	private Vector3[] podLineSpline;

	private const int BezierSegmentPoints = 60;

	private const int CatmullRomPointsPerSegment = 5;

	private const byte UPDATES_TO_RAY_CAST = 5;

	private Vector3 RAY_CAST_DIST = new Vector3(0f, 0.05f, 0f);

	private bool _terrainHit;

	private byte _rayCastUpdatesLeft;

	private BezierCurve _lineBezierCurve;

	private float lineTransitionStartTime;

	private bool isOnPod;

	private RodPodController rodPod;

	private RodStandSignalizer signalizerObject;

	private int firstRing;

	private Vector3[] lineOnRodPoints;

	private readonly List<Vector3> extraLineMassPositions = new List<Vector3>();

	private const float DisturbaceRadius = 0.01f;

	private const WaterDisturbForce Disturbance = WaterDisturbForce.XXSmall;

	private Vector3? surfaceCrossingPoint;

	public const float LineTensionLength = 1f;

	public const float RodPodTransitionLineSlackExtraTime = 3f;

	public const float RodPodTransitionLineSlackExtraTimeCooldown = 30f;
}
