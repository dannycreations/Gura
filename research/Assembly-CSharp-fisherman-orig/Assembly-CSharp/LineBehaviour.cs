using System;
using System.Collections.Generic;
using System.Globalization;
using CodeStage.AntiCheat.ObscuredTypes;
using ObjectModel;
using UnityEngine;

public class LineBehaviour
{
	public LineBehaviour(LineController owner, IAssembledRod rodAssembly, GameFactory.RodSlot rodSlot)
	{
		this._owner = owner;
		this.RodSlot = rodSlot;
		this.lrLineObject = this._owner.LineObject.GetComponent<LineRenderer>();
		this.lrLineOnRodObject = this._owner.LineOnRodObject.GetComponent<LineRenderer>();
		this.lrLineLeaderObject = this._owner.LineLeaderObject.GetComponent<LineRenderer>();
		this.lrLineLeashObject = this._owner.LineLeashObject.GetComponent<LineRenderer>();
		Color color = LineBehaviour.HexToColor(rodAssembly.LineInterface.Color, LineBehaviour.DefaultLineColor);
		this.lrLineObject.startColor = color;
		this.lrLineObject.endColor = color;
		this.lrLineOnRodObject.startColor = color;
		this.lrLineOnRodObject.endColor = color;
		Color color2 = color;
		if (rodAssembly.LeaderInterface != null)
		{
			color2 = LineBehaviour.HexToColor(rodAssembly.LeaderInterface.Color, LineBehaviour.DefaultLineColor);
		}
		Color color3 = color;
		if (rodAssembly.RodTemplate != RodTemplate.Float)
		{
			color3 = color2;
		}
		this.lrLineLeaderObject.startColor = color3;
		this.lrLineLeaderObject.endColor = color3;
		Color color4 = color;
		if (rodAssembly.RodTemplate == RodTemplate.Float || rodAssembly.RodTemplate.IsSinkerRig())
		{
			color4 = color2;
		}
		this.lrLineLeashObject.startColor = color4;
		this.lrLineLeashObject.endColor = color4;
		this.SetRendererLineWidth(0.003f);
		this.Sinkers = new List<GameObject>();
		GameObject gameObject = (GameObject)Resources.Load("Tackle/Sinkers/DefaultSinker/pDefaultSinker", typeof(GameObject));
		this._sinkerRenderers = new MeshRenderer[3];
		for (int i = 0; i < 3; i++)
		{
			GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject, new Vector3(0f, -1000f, 0f), Quaternion.identity);
			this.Sinkers.Add(gameObject2);
			this._sinkerRenderers[i] = this.Sinkers[i].GetComponent<MeshRenderer>();
		}
		Line line = rodAssembly.LineInterface as Line;
		if (line != null)
		{
			this.LineLengthOnSpool = ((line.Length != null) ? ((float)line.Length.Value) : 100f);
			float num = line.MaxLoad;
			AssembledRod assembledRod = rodAssembly as AssembledRod;
			if (assembledRod != null && assembledRod.Leader != null && assembledRod.Leader.MaxLoad < num)
			{
				num = assembledRod.Leader.MaxLoad;
			}
			this.MaxLoad = num * 9.81f;
		}
	}

	public GameObject gameObject
	{
		get
		{
			return (!(this._owner != null)) ? null : this._owner.gameObject;
		}
	}

	public GameObject mainLineObject
	{
		get
		{
			return (!(this._owner != null)) ? null : this._owner.LineObject;
		}
	}

	public GameObject rodLineObject
	{
		get
		{
			return (!(this._owner != null)) ? null : this._owner.LineOnRodObject;
		}
	}

	public GameObject leaderLineObject
	{
		get
		{
			return (!(this._owner != null)) ? null : this._owner.LineLeaderObject;
		}
	}

	public Transform transform
	{
		get
		{
			return this._owner.gameObject.transform;
		}
	}

	public string InstanceId { get; set; }

	public float LineWidth { get; set; }

	public GameFactory.RodSlot RodSlot { get; private set; }

	public RodBehaviour Rod
	{
		get
		{
			return this.RodSlot.Rod;
		}
	}

	public ReelBehaviour Reel
	{
		get
		{
			return this.RodSlot.Reel;
		}
	}

	public TackleBehaviour Tackle
	{
		get
		{
			return this.RodSlot.Tackle;
		}
	}

	public List<GameObject> Sinkers { get; set; }

	public virtual bool IsSlacked
	{
		get
		{
			return false;
		}
	}

	public virtual ObscuredFloat SecuredLineLength
	{
		get
		{
			return 0f;
		}
	}

	public virtual float FullLineLength
	{
		get
		{
			return 0f;
		}
	}

	public virtual float AppliedForce { get; protected set; }

	public float MinLineLengthWithFish { get; set; }

	public float MinLineLengthOnPitch { get; set; }

	public ObscuredFloat MaxLineLength { get; set; }

	public ObscuredFloat LineLengthOnSpool { get; set; }

	public float AvailableLineLengthOnSpool
	{
		get
		{
			return this.LineLengthOnSpool - this.Rod.LineOnRodLength;
		}
	}

	public float MaxLoad { get; set; }

	public bool IsBraid { get; protected set; }

	public float SinkerMass { get; set; }

	public Vector3 LastRingLinePosition { get; protected set; }

	public void ResetAppledForce()
	{
		this.appliedForceAverager.Clear();
	}

	public float MinLineLength
	{
		get
		{
			return 0.3f;
		}
	}

	public virtual void CalculateAppliedForce()
	{
		float num = this.Rod.AdjustedAppliedForce * 1f;
		float num2 = Mathf.Min(num, this.Reel.LineAdjustedFrictionForce);
		this.AppliedForce = this.appliedForceAverager.UpdateAndGet(num2, Time.deltaTime);
	}

	public virtual bool IsTensioned
	{
		get
		{
			return false;
		}
	}

	public virtual void Init(RodOnPodBehaviour.TransitionData transitionData)
	{
		if (transitionData == null)
		{
			if (this.RodSlot.LineClips.Count > 0)
			{
				this.MaxLineLength = this.RodSlot.LineClips.Peek();
			}
			else
			{
				this.MaxLineLength = this.RodSlot.Line.AvailableLineLengthOnSpool;
			}
		}
		else
		{
			this.MaxLineLength = transitionData.reelMaxLineLength;
		}
	}

	public void Destroy()
	{
		this.gameObject.SetActive(false);
		Object.Destroy(this.gameObject);
		for (int i = 0; i < this.Sinkers.Count; i++)
		{
			if (this.Sinkers[i] != null)
			{
				this.Sinkers[i].SetActive(false);
				Object.Destroy(this.Sinkers[i]);
			}
		}
		this.Sinkers.Clear();
		this.RodSlot = null;
		this._owner = null;
	}

	protected void StartLineWidthTransition(float newTargetWidth)
	{
		this.transitionTime = 0f;
		this.initialLineWidth = this.LineWidth;
		this.targetLineWidth = newTargetWidth;
	}

	protected Vector3 GetSecondRodPoint(RodBehaviour rodBehaviour, Vector3 reelLineArcLocatorPos)
	{
		List<Transform> firstRingLocatorsProp = rodBehaviour.FirstRingLocatorsProp;
		Transform transform = rodBehaviour.LineLocatorsProp[1];
		Vector3 vector = firstRingLocatorsProp[0].position;
		float num = Vector3.Distance(reelLineArcLocatorPos, vector);
		num += Vector3.Distance(vector, transform.position);
		for (int i = 0; i < firstRingLocatorsProp.Count; i++)
		{
			Transform transform2 = firstRingLocatorsProp[i];
			float num2 = Vector3.Distance(reelLineArcLocatorPos, transform2.position) + Vector3.Distance(transform2.position, transform.position);
			if (num2 < num)
			{
				vector = transform2.position;
				num = num2;
			}
		}
		return vector;
	}

	public void SetVisibility(bool flag)
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

	public virtual void TransitToNewLineWidth()
	{
		if (this.transitionTime < 1f)
		{
			this.transitionTime += Time.deltaTime;
			this.SetRendererLineWidth(Mathf.Lerp(this.initialLineWidth, this.targetLineWidth, this.transitionTime));
		}
	}

	public static Color HexToColor(string colorCode, Color defaultColor)
	{
		if (colorCode != null && colorCode.Length == 7 && colorCode[0] == '#')
		{
			int num = int.Parse(colorCode.Substring(1, 2), NumberStyles.HexNumber);
			int num2 = int.Parse(colorCode.Substring(3, 2), NumberStyles.HexNumber);
			int num3 = int.Parse(colorCode.Substring(5, 2), NumberStyles.HexNumber);
			return new Color((float)num / 255f, (float)num2 / 255f, (float)num3 / 255f);
		}
		return defaultColor;
	}

	protected void SetRendererLineWidth(float newLineWidth)
	{
		this.LineWidth = newLineWidth;
		this.lrLineObject.SetWidth(newLineWidth, newLineWidth * 1.4f);
		this.lrLineLeaderObject.SetWidth(newLineWidth, newLineWidth);
		this.lrLineLeashObject.SetWidth(newLineWidth, newLineWidth);
	}

	public virtual void Start()
	{
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
		this.CalculateAppliedForce();
	}

	public void SetActive(bool flag)
	{
		if (this.gameObject != null)
		{
			this.gameObject.SetActive(flag);
		}
		for (int i = 0; i < this.Sinkers.Count; i++)
		{
			this.Sinkers[i].SetActive(flag);
		}
	}

	public const float FarLineWidth = 0.003f;

	public const float CloseLineWidth = 0.00075f;

	public const int LINE_POINTS_COUNT = 3;

	public const int MLINE_MIDDLE_POINT_INDEX = 0;

	public const int LINE_END_POINT_INDEX = 1;

	public const int LEADER_END_POINT_INDEX = 2;

	public static readonly Color DefaultLineColor = new Color(0.3647059f, 0.5019608f, 0.3607843f);

	protected LineController _owner;

	protected LineRenderer lrLineObject;

	protected LineRenderer lrLineOnRodObject;

	protected LineRenderer lrLineLeaderObject;

	protected LineRenderer lrLineLeashObject;

	protected float initialLineWidth;

	protected float targetLineWidth;

	protected float transitionTime;

	protected int rodLinePointsCount;

	protected MeshRenderer[] _sinkerRenderers;

	private readonly TimedAverager appliedForceAverager = new TimedAverager(0.2f, 100);

	public const float MinLeaderLength = 0.1f;
}
