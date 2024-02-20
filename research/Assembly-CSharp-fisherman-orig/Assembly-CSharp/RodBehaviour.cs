using System;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Mono.Simd;
using Mono.Simd.Math;
using ObjectModel;
using Phy;
using UnityEngine;

public abstract class RodBehaviour : IOutline
{
	public RodBehaviour(RodController controller, IAssembledRod rodAssembly, GameFactory.RodSlot slot)
	{
		this._outlineWidthID = Shader.PropertyToID("_OutlineWidth");
		this._outlineColorID = Shader.PropertyToID("_OutlineColor");
		this.RodSlot = slot;
		this._owner = controller;
		this._rodAssembly = rodAssembly;
		this._isBaitcasting = rodAssembly.ReelType == ReelTypes.Baitcasting;
		Debug.LogWarning("RodBehaviour.ctor " + this.InstanceID);
		this._renderers = RenderersHelper.GetAllRenderersForObject<Renderer>(this.transform);
		this.rodMaterials = new List<Material>();
		this.rodMeshRenderers = new List<MeshFilter>();
		Shader shader = Shader.Find("M_Special/Rod IBL OUT");
		IEnumerator enumerator = this.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				Renderer component = transform.gameObject.GetComponent<Renderer>();
				if (component != null)
				{
					MeshFilter component2 = transform.gameObject.GetComponent<MeshFilter>();
					if (component2 != null)
					{
						Material[] materials = component.materials;
						for (int i = 0; i < materials.Length; i++)
						{
							materials[i].shader = shader;
							materials[i].SetFloat(this._outlineWidthID, 0f);
						}
						component.materials = materials;
						for (int j = 0; j < materials.Length; j++)
						{
							this._outlines.Add(component.materials[j]);
						}
						this.rodMaterials.Add(component.material);
						if (this.IsQuiver)
						{
							component.material.EnableKeyword("QUIVER_ON");
						}
						this.rodMeshRenderers.Add(component2);
					}
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
		this.Segment.action = rodAssembly.RodInterface.Action;
		Rod rod = this._rodAssembly.RodInterface as Rod;
		if (rod != null)
		{
			this.maxLoad = rod.MaxLoad * 9.81f;
		}
		this.SetupSegment();
		this.RodPointsCount = RodObject.DecodeRodTemplate(this.Segment.action).Config.Length;
		if (this.IsQuiver)
		{
			this.RodPointsCount += RodSegmentConfig.Quiver.Config.Length;
		}
		MeshFilter component3 = RenderersHelper.GetRendererForObject<Renderer>(this.transform).GetComponent<MeshFilter>();
		this._distFromRootToBackTip = -component3.mesh.bounds.min.z;
		this.RodPodTransitionEndTimestamp = -3f;
		this.RodPodTransitionPreviousTimestamp = -30f;
	}

	public GameObject gameObject
	{
		get
		{
			return this._owner.gameObject;
		}
	}

	public Transform transform
	{
		get
		{
			return (!(this._owner != null)) ? null : this._owner.transform;
		}
	}

	public int InstanceID
	{
		get
		{
			return this._owner.gameObject.GetInstanceID();
		}
	}

	public int RodPointsCount { get; protected set; }

	public float Length { get; set; }

	public BendingSegment Segment
	{
		get
		{
			return this._owner.segment;
		}
	}

	public IAssembledRod RodAssembly
	{
		get
		{
			return this._rodAssembly;
		}
	}

	public bool IsQuiver
	{
		get
		{
			return this._rodAssembly.QuiverTipInterface != null;
		}
	}

	public RodObject RodObject { get; protected set; }

	public float DistFromRootToBackTip
	{
		get
		{
			return this._distFromRootToBackTip;
		}
	}

	public GameFactory.RodSlot RodSlot { get; private set; }

	public virtual bool IsFishingForced
	{
		get
		{
			return false;
		}
	}

	public ObscuredFloat LineOnRodLength { get; set; }

	public virtual Mass TackleTipMass
	{
		get
		{
			return null;
		}
	}

	public TackleBehaviour Tackle
	{
		get
		{
			return this.RodSlot.Tackle;
		}
	}

	public ReelBehaviour Reel
	{
		get
		{
			return this.RodSlot.Reel;
		}
	}

	public LineBehaviour Line
	{
		get
		{
			return this.RodSlot.Line;
		}
	}

	public BellBehaviour Bell
	{
		get
		{
			return this.RodSlot.Bell;
		}
	}

	public float ReelDamper { get; protected set; }

	public float AppliedForce { get; protected set; }

	public void ActivateOutline(bool flag)
	{
		float num = ((!flag) ? 0f : 0.023f);
		for (int i = 0; i < this._outlines.Count; i++)
		{
			this._outlines[i].SetFloat(this._outlineWidthID, num);
		}
	}

	public void SetOutlineColor(Color color)
	{
		for (int i = 0; i < this._outlines.Count; i++)
		{
			this._outlines[i].SetColor(this._outlineColorID, color);
		}
	}

	public virtual void Destroy()
	{
		Object.Destroy(this.gameObject);
		this.Clean();
	}

	public virtual void Clean()
	{
		if (this.RodSlot != null)
		{
			this.RodSlot.Clear();
		}
		this.RodSlot = null;
		this._rodAssembly = null;
		this._owner = null;
	}

	public bool IsOngoingRodPodTransition { get; private set; }

	public float RodPodTransitionEndTimestamp { get; private set; }

	public float RodPodTransitionPreviousTimestamp { get; private set; }

	public void OnRodPodTransitionStart()
	{
		this.IsOngoingRodPodTransition = true;
	}

	public void OnRodPodTransitionFinish()
	{
		this.IsOngoingRodPodTransition = false;
		this.RodPodTransitionPreviousTimestamp = this.RodPodTransitionEndTimestamp;
		this.RodPodTransitionEndTimestamp = Time.time;
	}

	public bool IsFishHooked
	{
		get
		{
			return this.Tackle != null && this.Tackle.Fish != null;
		}
	}

	public virtual float AdjustedAppliedForce
	{
		get
		{
			float num = 0f;
			if (this.RodSlot.Sim.LineTipMass != null)
			{
				num = this.RodSlot.Sim.LineTipMass.AvgForce.magnitude;
			}
			if (this.IsTelescopic && this.Tackle.Fish == null)
			{
				num *= 0.2f;
			}
			if (this.Tackle.IsClipStrike)
			{
				num *= 1f + 4f * Mathf.Clamp01(this._owner.transform.forward.y);
			}
			if (!this.Tackle.IsHitched && !this.IsFishHooked)
			{
				num *= 0.2f;
			}
			return num;
		}
	}

	public virtual void ResetAppliedForce()
	{
		this.appliedForceAverager.Clear();
		this.Reel.ResetAppledForce();
		this.Line.ResetAppledForce();
	}

	protected float CalcReelDamper(float currentAngle)
	{
		float num = Mathf.Min(currentAngle, 80f);
		float num2 = 0.65f + Mathf.Atan(1f - num * 0.017453292f) / 2.2f;
		if (currentAngle >= 100f && this.Reel.IsReeling && this.Reel.CurrentSpeed > 0f)
		{
			float num3 = Mathf.Clamp01((currentAngle - 100f) / 50f);
			num2 = Mathf.Lerp(num2, 1f, num3);
		}
		return num2;
	}

	protected float CalcStaticDamper(float forceApplicationAngle)
	{
		if (Mathf.Abs(forceApplicationAngle) > 90f)
		{
			return 1f;
		}
		return Mathf.Sin(forceApplicationAngle * 0.017453292f);
	}

	public virtual void CalculateAppliedForce()
	{
		float num = Vector3.Angle(this._owner.segment.firstTransform.forward, this.RodObject.TipMass.AvgForce.normalized);
		float num2 = this.AdjustedAppliedForce * 1f;
		float num3 = Mathf.Min(num2 * this.CalcStaticDamper(num), this.Reel.LineAdjustedFrictionForce);
		this.AppliedForce = this.appliedForceAverager.UpdateAndGet(num3, Time.deltaTime);
	}

	public Vector3 CurrentUnbendTipPosition
	{
		get
		{
			return this._owner.segment.firstTransform.position + this._owner.segment.firstTransform.rotation * this.localTipPosition;
		}
	}

	public virtual Vector3 CurrentTipPosition
	{
		get
		{
			return Vector3.zero;
		}
	}

	public RodController Controller
	{
		get
		{
			return this._owner;
		}
	}

	public bool IsBaitcasting
	{
		get
		{
			return this._isBaitcasting;
		}
	}

	public float MaxLoad
	{
		get
		{
			return this.maxLoad;
		}
	}

	public static Color QuiverTipColorToRGB(QuiverTipColor colorName)
	{
		Color color = Color.white;
		switch (colorName)
		{
		case QuiverTipColor.White:
			color = Color.white;
			break;
		case QuiverTipColor.Red:
			color = Color.red;
			break;
		case QuiverTipColor.Orange:
			color..ctor(1f, 0.455f, 0.058f);
			break;
		case QuiverTipColor.Yellow:
			color = Color.yellow;
			break;
		case QuiverTipColor.Green:
			color = Color.green;
			break;
		case QuiverTipColor.Blue:
			color = Color.blue;
			break;
		case QuiverTipColor.Purple:
			color = Color.magenta;
			break;
		}
		return color;
	}

	public List<Transform> LineLocatorsProp
	{
		get
		{
			return (!this._isBaitcasting) ? this._owner.LineLocators : this._owner.BaitLineLocators;
		}
	}

	public List<Transform> FirstRingLocatorsProp
	{
		get
		{
			return this._owner.FirstRingLocators;
		}
	}

	public Transform RodTipLocator { get; protected set; }

	public float RodTipMoveSpeed { get; protected set; }

	public float TipMoveSpeedFromTackle { get; protected set; }

	public float ModelLength { get; private set; }

	public virtual Vector3 RootPositionCorrection(Vector3 point, float groundHeight)
	{
		return Vector3.zero;
	}

	public virtual Vector3 RootRotationCorrect(Vector3 point)
	{
		return point;
	}

	public virtual Vector3 PositionCorrection(Mass m, bool needRotationCorrect = true)
	{
		return m.Position;
	}

	public virtual Vector3 LineLocatorCorrection()
	{
		return Vector3.zero;
	}

	public virtual void Start()
	{
		this.localTipPosition = this._owner.segment.firstTransform.InverseTransformPoint(this._owner.segment.lastTransform.position);
	}

	public virtual void Init(RodOnPodBehaviour.TransitionData transitionData = null)
	{
		if (this._owner.rootNode == null)
		{
			this._owner.rootNode = this.transform;
		}
		this.ModelLength = Vector3.Distance(this._owner.rootNode.position, this._owner.segment.lastTransform.position);
		this.RodTipLocator = this.LineLocatorsProp[this.LineLocatorsProp.Count - 1];
	}

	public virtual void FixedUpdate()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void LateUpdate()
	{
	}

	public virtual void OnLateUpdate()
	{
	}

	public virtual void LateUpdateBeforeSim()
	{
	}

	public virtual void LateUpdateAfterSim()
	{
	}

	protected void InitProceduralBend(IList<Vector3> pts)
	{
		this.rodShaderPointsCountID = Shader.PropertyToID("PointsCount");
		this.rodShaderBinomialID = Shader.PropertyToID("Binomial");
		this.rodShaderDerivativeBinomialID = Shader.PropertyToID("DerivativeBinomial");
		this.rodShaderAnchorPointsID = Shader.PropertyToID("AnchorPoints");
		this.rodShaderQuiverColorID = Shader.PropertyToID("_QuiverColor");
		this.Rings = new RodBehaviour.LocatorRing[this.LineLocatorsProp.Count];
		for (int i = 0; i < this.Rings.Length; i++)
		{
			this.Rings[i] = new RodBehaviour.LocatorRing();
			this.Rings[i].locatorUnbentPosition = this.gameObject.transform.InverseTransformPoint(this.LineLocatorsProp[i].position);
			Vector3 vector = Vector3.zero;
			int num = 0;
			for (int j = 0; j < this.LineLocatorsProp[i].parent.childCount; j++)
			{
				Transform child = this.LineLocatorsProp[i].parent.GetChild(j);
				if (child.childCount == 0)
				{
					num++;
					vector += this.gameObject.transform.InverseTransformPoint(child.position);
				}
			}
			this.Rings[i].centerUnbentPosition = vector / (float)num;
			this.Rings[i].radius = (this.Rings[i].locatorUnbentPosition - this.Rings[i].centerUnbentPosition).magnitude;
		}
		this.FirstRingLocatorsUnbentPositions = new Vector3[this.FirstRingLocatorsProp.Count];
		for (int k = 0; k < this.FirstRingLocatorsUnbentPositions.Length; k++)
		{
			this.FirstRingLocatorsUnbentPositions[k] = this.gameObject.transform.InverseTransformPoint(this.FirstRingLocatorsProp[k].position);
		}
		this.bezierCurve = new BezierCurve(this.RodPointsCount - 1);
		this.rodPointsArray = new Vector4[this.bezierCurve.Order + 1];
		for (int l = 0; l < this.RodPointsCount; l++)
		{
			this.bezierCurve.AnchorPoints[l] = new Vector3(0f, 0f, pts[l].z);
		}
		int num2 = 512;
		this.bezierCurve.BuildReparametrizationCurveMap(num2, 128);
		float num3 = 1f / (this.bezierCurve.AnchorPoints[this.bezierCurve.Order].z - this.bezierCurve.AnchorPoints[0].z);
		for (int m = 0; m < this.rodMaterials.Count; m++)
		{
			MeshFilter meshFilter = this.rodMeshRenderers[m];
			Color[] colors = meshFilter.sharedMesh.colors;
			Vector3[] vertices = meshFilter.sharedMesh.vertices;
			for (int n = 0; n < colors.Length; n++)
			{
				float num4 = (vertices[n].z - this.bezierCurve.AnchorPoints[0].z) * num3;
				if (num4 > 0f && colors[n].r > 0f)
				{
					float num5 = this.bezierCurve.SampleReparametrizationCurveMap(num4);
					colors[n].r = num5;
				}
				else
				{
					colors[n].r = 0f;
				}
			}
			meshFilter.sharedMesh.colors = colors;
		}
		for (int num6 = 0; num6 < this.Rings.Length; num6++)
		{
			float num7 = (this.Rings[num6].locatorUnbentPosition.z - this.bezierCurve.AnchorPoints[0].z) * num3;
			if (num7 > 0f)
			{
				this.Rings[num6].tparam = this.bezierCurve.SampleReparametrizationCurveMap(num7);
			}
		}
	}

	public void UpdateProceduralBend(IList<Vector3> pts)
	{
		int num = Mathf.Min(new int[]
		{
			pts.Count,
			this.RodPointsCount,
			this.bezierCurve.NumberOfPoints
		});
		for (int i = 0; i < num; i++)
		{
			if (i >= 3)
			{
				this.bezierCurve.AnchorPoints[i] = pts[i];
			}
		}
		List<Transform> lineLocatorsProp = this.LineLocatorsProp;
		for (int j = 0; j < lineLocatorsProp.Count; j++)
		{
			this.bezierCurve.SetT(this.Rings[j].tparam);
			lineLocatorsProp[j].position = this.gameObject.transform.TransformPoint(this.bezierCurve.CurvedCylinderTransform(this.Rings[j].locatorUnbentPosition));
		}
		this.bezierCurve.SetT(this.Rings[0].tparam);
		for (int k = 0; k < this._owner.FirstRingLocators.Count; k++)
		{
			this._owner.FirstRingLocators[k].position = this.gameObject.transform.TransformPoint(this.bezierCurve.CurvedCylinderTransform(this.FirstRingLocatorsUnbentPositions[k]));
		}
		for (int l = 0; l < this.rodMaterials.Count; l++)
		{
			this.UpdateRodShader(this.rodMaterials[l]);
		}
		for (int m = 0; m < this._outlines.Count; m++)
		{
			this.UpdateRodShader(this._outlines[m]);
		}
	}

	public void UpdateProceduralBend(Vector3[] pts, Vector3[] locatorsPos, Vector3[] firstRingPos)
	{
		for (int i = 0; i < this.RodPointsCount; i++)
		{
			if (i < 3)
			{
				this.bezierCurve.AnchorPoints[i] = new Vector3(0f, 0f, pts[i].z);
			}
			else
			{
				this.bezierCurve.AnchorPoints[i] = pts[i];
			}
		}
		List<Transform> lineLocatorsProp = this.LineLocatorsProp;
		for (int j = 0; j < lineLocatorsProp.Count; j++)
		{
			lineLocatorsProp[j].position = this.gameObject.transform.TransformPoint(locatorsPos[j]);
		}
		this.bezierCurve.SetT(this.Rings[0].tparam);
		for (int k = 0; k < this._owner.FirstRingLocators.Count; k++)
		{
			this._owner.FirstRingLocators[k].position = this.gameObject.transform.TransformPoint(firstRingPos[k]);
		}
		for (int l = 0; l < this.rodMaterials.Count; l++)
		{
			this.UpdateRodShader(this.rodMaterials[l]);
		}
		for (int m = 0; m < this._outlines.Count; m++)
		{
			this.UpdateRodShader(this._outlines[m]);
		}
	}

	public float GetTParam(float position)
	{
		float z = this.bezierCurve.AnchorPoints[0].z;
		return this.bezierCurve.SampleReparametrizationCurveMap((position - z) / (this.bezierCurve.AnchorPoints[this.bezierCurve.Order].z - z));
	}

	public Vector3 GetBezierPoint(float t)
	{
		return this.transform.TransformPoint(this.bezierCurve.Point(t));
	}

	public Quaternion GetBezierRotation(float t)
	{
		return this.transform.rotation * this.bezierCurve.Rotation(t);
	}

	public int RingsCount
	{
		get
		{
			return this.Rings.Length;
		}
	}

	public Vector3 GetRingCenterLocalUnbent(int ringIndex)
	{
		return this.Rings[ringIndex].centerUnbentPosition;
	}

	public Vector3 GetRingCenter(int ringIndex)
	{
		this.bezierCurve.SetT(this.Rings[ringIndex].tparam);
		return this.gameObject.transform.TransformPoint(this.bezierCurve.CurvedCylinderTransform(this.Rings[ringIndex].centerUnbentPosition));
	}

	public Vector3 GetNearestPointOnRing(Vector3 sourcePoint, Vector3 destPoint, int ringIndex, bool debugDraw = false)
	{
		RodBehaviour.LocatorRing locatorRing = this.Rings[ringIndex];
		this.bezierCurve.SetT(locatorRing.tparam);
		Vector3 vector = this.gameObject.transform.TransformPoint(this.bezierCurve.CurvedCylinderTransform(locatorRing.centerUnbentPosition));
		Vector3 normalized = this.gameObject.transform.TransformDirection(this.bezierCurve.Derivative()).normalized;
		Vector3 zero = Vector3.zero;
		Math3d.LinePlaneIntersection(out zero, sourcePoint, (destPoint - sourcePoint).normalized, normalized, vector);
		if ((zero - vector).magnitude <= this.Rings[ringIndex].radius)
		{
			return zero;
		}
		return vector + (zero - vector).normalized * locatorRing.radius;
	}

	private void UpdateRodShader(Material material)
	{
		if (material)
		{
			int num = this.bezierCurve.Order + 1;
			for (int i = 0; i < num; i++)
			{
				this.rodPointsArray[i] = this.bezierCurve.AnchorPoints[i];
			}
			material.SetInt(this.rodShaderPointsCountID, num);
			material.SetFloatArray(this.rodShaderBinomialID, this.bezierCurve.Binomial);
			material.SetFloatArray(this.rodShaderDerivativeBinomialID, this.bezierCurve.DerivativeBinomial);
			material.SetVectorArray(this.rodShaderAnchorPointsID, this.rodPointsArray);
			if (this.IsQuiver)
			{
				material.SetColor(this.rodShaderQuiverColorID, RodBehaviour.QuiverTipColorToRGB(this.QuiverTipColorName));
			}
		}
	}

	public virtual void TriggerHapticPulseOnRod(float motor, float duration)
	{
	}

	protected void SetupSegment()
	{
		this._owner.segment.AngleH = 0f;
		this._owner.segment.AngleV = 0f;
		this._owner.segment.ChainLength = 1;
		Transform transform = this._owner.segment.lastTransform;
		float z = this._owner.transform.InverseTransformPoint(transform.position).z;
		float num = z;
		IEnumerator enumerator = transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform2 = (Transform)obj;
				float z2 = this._owner.transform.InverseTransformPoint(transform2.position).z;
				if (z2 > num)
				{
					num = z2;
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
		this._owner.segment.rodLength = 0f;
		bool flag = false;
		while (!flag)
		{
			this._owner.segment.ChainLength++;
			this._owner.segment.rodLength += (transform.position - transform.parent.position).magnitude;
			transform = transform.parent;
			flag = transform == this._owner.segment.firstTransform;
		}
		this._owner.segment.rodLength += Mathf.Max(0f, num - z);
		this._owner.segment.Nodes = new Transform[this._owner.segment.ChainLength];
		this._owner.segment.CumulativeLengths = new float[this._owner.segment.ChainLength];
		float num2 = this._owner.segment.rodLength;
		transform = this._owner.segment.lastTransform;
		for (int i = this._owner.segment.ChainLength - 1; i >= 0; i--)
		{
			this._owner.segment.Nodes[i] = transform;
			this._owner.segment.CumulativeLengths[i] = num2;
			num2 -= (transform.position - transform.parent.position).magnitude;
			transform = transform.parent;
		}
	}

	public static void UpdateMassEnvironment(Mass m)
	{
		if (m.IgnoreEnvironment)
		{
			return;
		}
		if (m.Position.y >= 0f)
		{
			m.WindVelocity = WeatherController.Instance.SceneFX.WindVelocity.AsPhyVector(ref m.WindVelocity);
		}
		else
		{
			m.WindVelocity = Vector4f.Zero;
		}
		if (m.Position.y > m.Radius || m.GroundHeight > -m.Radius)
		{
			m.FlowVelocity = Vector4f.Zero;
		}
		else if (GameFactory.WaterFlow != null)
		{
			Vector3 streamSpeed = GameFactory.WaterFlow.GetStreamSpeed(m.Position - Vector3.up * m.Radius);
			m.FlowVelocity = streamSpeed.AsPhyVector(ref m.FlowVelocity);
		}
	}

	public static float CorrectLineLengthForPodTransition(Vector3 lineTipPosition, Vector3 rodTipPosition, Vector3 rodRootPosition, float rodLength, float lineLength)
	{
		float magnitude = (lineTipPosition - rodTipPosition).magnitude;
		Vector3 vector = rodRootPosition + Vector3.up * rodLength;
		float magnitude2 = (lineTipPosition - vector).magnitude;
		return Mathf.Max(new float[] { magnitude2, magnitude, lineLength });
	}

	private const float OUTLINE_WIDTH = 0.023f;

	public const int ROD_POINTS_COUNT = 6;

	public const int ROD_HANDLE_POINTS_COUNT = 3;

	public const float WaterStreamForceAmplifier = 1f;

	public bool IsTelescopic;

	protected List<Renderer> _renderers;

	protected IAssembledRod _rodAssembly;

	public QuiverTipColor QuiverTipColorName;

	private float _distFromRootToBackTip;

	private readonly TimedAverager appliedForceAverager = new TimedAverager(0.2f, 100);

	private const float MinReelDamperAngle = 80f;

	private const float MinReelDamperBoostAngle = 100f;

	private const float MaxReelDamperBoostAngle = 150f;

	private Vector3 localTipPosition;

	protected RodController _owner;

	protected bool _isBaitcasting;

	protected float maxLoad;

	protected RodBehaviour.LocatorRing[] Rings;

	protected Vector3[] FirstRingLocatorsUnbentPositions;

	protected BezierCurve bezierCurve;

	protected Vector4[] rodPointsArray;

	protected List<MeshFilter> rodMeshRenderers;

	protected List<Material> rodMaterials;

	protected List<Material> _outlines = new List<Material>();

	protected int rodShaderPointsCountID;

	protected int rodShaderBinomialID;

	protected int rodShaderDerivativeBinomialID;

	protected int rodShaderAnchorPointsID;

	protected int rodShaderQuiverColorID;

	protected int _outlineWidthID;

	protected int _outlineColorID;

	public int RodOnPodTpmId;

	public class LocatorRing
	{
		public LocatorRing()
		{
		}

		public LocatorRing(RodBehaviour.LocatorRing s)
		{
			this.centerUnbentPosition = s.centerUnbentPosition;
			this.locatorUnbentPosition = s.locatorUnbentPosition;
			this.tparam = s.tparam;
			this.radius = s.radius;
		}

		public Vector3 centerUnbentPosition;

		public Vector3 locatorUnbentPosition;

		public float tparam;

		public float radius;
	}
}
