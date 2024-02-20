using System;
using System.Collections.Generic;
using UnityEngine;

public class SchoolController : MonoBehaviour, ISize
{
	public List<SchoolChild> Roamers
	{
		get
		{
			return this._roamers;
		}
	}

	public float MinFishScale
	{
		get
		{
			return (!this.isCommonFishScaleUsed) ? this._minScale : this.commonMinFishScale;
		}
	}

	public float MaxFishScale
	{
		get
		{
			return (!this.isCommonFishScaleUsed) ? this._maxScale : this.commonMaxFishScale;
		}
	}

	public void SetWidth(float width)
	{
		this._areaHalfWidth = width * 0.5f;
	}

	public void SetDepth(float depth)
	{
		this._areaCoastDist = depth;
	}

	public void Init(float minScale, float maxScale, bool sharedFish, bool canGenerateFish, List<SchoolChild> roamers)
	{
		this._curWaypointLocalPos = this._posOffset;
		this._areaWaypoints = new AreaWaypoints(this, this._groupInitialPosSidePrc, this._groupInitialPosCoastPrc);
		this._schoolSpeed = Random.Range(1f, this._childSpeedMultipler);
		this._autoRandomPosition = this._autoRandomPosition && this._activeChildren > 0;
		if (!this.isFishScaleOverridable)
		{
			this.isCommonFishScaleUsed = true;
			this.commonMinFishScale = minScale;
			this.commonMaxFishScale = maxScale;
		}
		if (canGenerateFish)
		{
			this._roamers = new List<SchoolChild>();
			this.AddFish(this._childAmount);
		}
		else
		{
			this._roamers = roamers;
			if (roamers != null && roamers.Count > 0)
			{
				this._groupTransform = roamers[0].transform.parent;
			}
		}
		this._sharedFish = sharedFish;
		this.SetLogicFlag(false, false);
	}

	public void Start()
	{
		this._timer = this.RandomWaypointTime();
	}

	public void Update()
	{
		if (this._autoRandomPosition)
		{
			this._timer -= Time.deltaTime;
			if (this._timer < 0f)
			{
				this._timer = this.RandomWaypointTime();
				this.SetRandomWaypointPosition();
			}
		}
	}

	public void SetLogicFlag(bool isEnabled, bool teleportFish = false)
	{
		base.gameObject.SetActive(isEnabled);
		if (!this._sharedFish)
		{
			if (this._groupTransform != null)
			{
				this._groupTransform.gameObject.SetActive(isEnabled);
			}
		}
		else
		{
			if (isEnabled)
			{
				for (int i = 0; i < this._roamers.Count; i++)
				{
					this._roamers[i]._spawner = this;
				}
				this.SetRandomWaypointPosition();
				if (teleportFish)
				{
					for (int j = 0; j < this._roamers.Count; j++)
					{
						this._roamers[j].RegeneratePosition();
					}
				}
			}
			if (this._groupTransform != null)
			{
				this._groupTransform.gameObject.SetActive(isEnabled);
			}
		}
	}

	public void AddFish(int amount)
	{
		if (this._groupChildToNewTransform && this._groupTransform == null)
		{
			this._groupTransform = new GameObject(base.transform.name + " Fish Container").transform;
			this._groupTransform.position = base.transform.position;
		}
		for (int i = 0; i < amount; i++)
		{
			int num = Random.Range(0, this._childPrefab.Length);
			SchoolChild schoolChild = Object.Instantiate<SchoolChild>(this._childPrefab[num]);
			schoolChild._spawner = this;
			this._roamers.Add(schoolChild);
			this.AddChildToParent(schoolChild.transform);
		}
	}

	public void AddChildToParent(Transform obj)
	{
		if (this._groupChildToSchool)
		{
			obj.parent = base.transform;
		}
		else if (this._groupChildToNewTransform)
		{
			obj.parent = this._groupTransform;
		}
	}

	public void RemoveFish(int amount)
	{
		SchoolChild schoolChild = this._roamers[this._roamers.Count - 1];
		this._roamers.RemoveAt(this._roamers.Count - 1);
		Object.Destroy(schoolChild.gameObject);
	}

	public void UpdateFishAmount()
	{
		if (this._childAmount >= 0 && this._childAmount < this._roamers.Count)
		{
			this.RemoveFish(1);
			return;
		}
		if (this._childAmount > this._roamers.Count)
		{
			this.AddFish(1);
			return;
		}
	}

	public void SetRandomWaypointPosition()
	{
		this._schoolSpeed = Random.Range(1f, this._childSpeedMultipler);
		this._areaWaypoints.SetRandomMainWaypoint();
		if (this._forceChildWaypoints)
		{
			for (int i = 0; i < this._roamers.Count; i++)
			{
				this._roamers[i].Wander(Random.value * this._forcedRandomDelay);
			}
		}
	}

	public Vector3 GenerateFishWaypoint()
	{
		return this._areaWaypoints.GenerateFishWaypoint();
	}

	public float RandomWaypointTime()
	{
		return Random.Range(this._randomPositionTimerMin, this._randomPositionTimerMax);
	}

	private void OnDestroy()
	{
		this._groupTransform = null;
		this._roamers = null;
	}

	public void OnDrawGizmos()
	{
		this._areaWaypoints.DrawGizmos();
	}

	public static void DrawGizmo(SchoolController.DrawGizmoDelegate f, Color color, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		Gizmos.color = color;
		Matrix4x4 matrix = Gizmos.matrix;
		Matrix4x4 matrix4x = Matrix4x4.TRS(position, rotation, scale);
		Gizmos.matrix *= matrix4x;
		f(Vector3.zero, Vector3.one);
		Gizmos.matrix = matrix;
	}

	public static void Log(string text)
	{
		Debug.Log(string.Format("UserLog   {0}       {1}", DateTime.Now.ToString("hh:mm:ss.ff"), text));
	}

	public static void Log(string textPattern, params object[] args)
	{
		Debug.Log(string.Format("UserLog   {0}       {1}", DateTime.Now.ToString("hh:mm:ss.ff"), string.Format(textPattern, args)));
	}

	public SchoolChild[] _childPrefab;

	public bool _groupChildToNewTransform;

	public Transform _groupTransform;

	public string _groupName = string.Empty;

	public bool _groupChildToSchool;

	public int _childAmount = 250;

	public float _spawnSphere = 3f;

	public float _spawnSphereDepth = 3f;

	public float _spawnSphereHeight = 1.5f;

	public float _childSpeedMultipler = 2f;

	public float _minSpeed = 6f;

	public float _maxSpeed = 10f;

	public AnimationCurve _speedCurveMultiplier = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 1f),
		new Keyframe(1f, 1f)
	});

	public float _minDamping = 1f;

	public float _maxDamping = 2f;

	public float _waypointDistance = 1f;

	public float _minAnimationSpeed = 2f;

	public float _maxAnimationSpeed = 4f;

	public float _randomPositionTimerMax = 10f;

	public float _randomPositionTimerMin = 4f;

	public float _acceleration = 0.025f;

	public float _brake = 0.01f;

	public float _positionSphere = 25f;

	public float _positionSphereDepth = 5f;

	public float _positionSphereHeight = 5f;

	public bool _childTriggerPos;

	public bool _forceChildWaypoints;

	public bool _autoRandomPosition;

	public float _forcedRandomDelay = 1.5f;

	public float _schoolSpeed;

	private List<SchoolChild> _roamers;

	public Vector3 _curWaypointLocalPos;

	public Vector3 _posOffset;

	public bool _avoidance;

	public float _avoidAngle = 0.35f;

	public float _avoidDistance = 1f;

	public float _avoidSpeed = 75f;

	public float _stopDistance = 0.5f;

	public float _stopSpeedMultiplier = 2f;

	public LayerMask _avoidanceMask = -1;

	public bool _push;

	public float _pushDistance;

	public float _pushForce = 5f;

	public SchoolBubbles _bubbles;

	public int _updateDivisor = 1;

	public float _newDelta;

	public int _updateCounter;

	public int _activeChildren;

	public float _areaCoastDist;

	public float _areaHalfWidth = 2f;

	public float _areaAngle;

	public float _groupWidthPrc = 0.5f;

	public float _groupCoastDistPrc = 0.5f;

	public float _groupInitialPosSidePrc;

	public float _groupInitialPosCoastPrc;

	private AreaWaypoints _areaWaypoints;

	public float _minScale = 0.7f;

	public float _maxScale = 1f;

	public bool isFishScaleOverridable;

	private bool isCommonFishScaleUsed;

	private float commonMinFishScale;

	private float commonMaxFishScale;

	private float _timer;

	private bool _sharedFish;

	public delegate void DrawGizmoDelegate(Vector3 pos, Vector3 size);
}
