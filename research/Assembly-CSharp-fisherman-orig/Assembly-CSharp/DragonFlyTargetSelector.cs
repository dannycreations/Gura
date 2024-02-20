using System;
using System.Collections.Generic;
using UnityEngine;

public class DragonFlyTargetSelector : MonoBehaviour
{
	private DragonFlyTargetSelector.State CurState
	{
		get
		{
			return this._curState;
		}
		set
		{
			this._curState = value;
		}
	}

	private void Start()
	{
		this.CurState = DragonFlyTargetSelector.State.NoTargets;
		this._curTarget = PlayerTargetType.NONE;
		GameObject gameObject = GameObject.Find("3D/HUD/Collider/Player/HandsRoot/FakeHandsRoot");
		if (gameObject != null)
		{
			this.player = gameObject.GetComponent<PlayerController>();
			this.player.OnActivateTarget += this.EAddTarget;
			this.player.OnUpdateTargetLocalPos += this.EUpdateTargetLocalPos;
			this.player.OnDeactivateTarget += this.ERemovedTarget;
		}
	}

	private void RoamerOnOnEscape()
	{
		if (this.CurState == DragonFlyTargetSelector.State.ActiveRoamer)
		{
			this.CurState = DragonFlyTargetSelector.State.EscapeRoamer;
		}
	}

	private void RoamerOnOnEscaped()
	{
		Object.Destroy(this._roamer.gameObject);
		this._roamer = null;
		this.CurState = DragonFlyTargetSelector.State.NoTargets;
		if (this._targets.Count > 0)
		{
			this.PrepareLaunch();
		}
	}

	private void PlayerOnOnActivateTarget(Transform rodTransform, Vector3 rodLocalPoint)
	{
		this.EAddTarget(PlayerTargetType.ROD, rodTransform, rodLocalPoint);
	}

	private void PlayerOnUpdateTargetLocalPos(Vector3 newLocalPoint)
	{
		this.EUpdateTargetLocalPos(PlayerTargetType.ROD, newLocalPoint);
	}

	private void PlayerOnOnDeactivateTarget()
	{
		this.ERemovedTarget(PlayerTargetType.ROD);
	}

	private void EAddTarget(PlayerTargetType targetType, Transform rodTransform, Vector3 rodLocalPoint)
	{
		if (this._targets.Count == 0)
		{
			this.PrepareLaunch();
		}
		this._targets[targetType] = new DragonFlyTargetSelector.Target
		{
			transform = rodTransform,
			localPoint = rodLocalPoint
		};
	}

	private void PrepareLaunch()
	{
		if (this._curState != DragonFlyTargetSelector.State.NoTargets)
		{
			return;
		}
		if (this._roamer == null)
		{
			int num = Random.Range(0, this._prefabs.Length);
			GameObject gameObject = Object.Instantiate<GameObject>(this._prefabs[num]);
			this._roamer = gameObject.GetComponent<DragonFlyController>();
			this._roamer._cameraTransform = this._cameraTransform;
			this._roamer.OnEscape += this.RoamerOnOnEscape;
			this._roamer.OnEscaped += this.RoamerOnOnEscaped;
		}
		this._curTarget = PlayerTargetType.NONE;
		this.CurState = DragonFlyTargetSelector.State.IncomingRoamer;
		this._activateRoamerAt = Time.time + this._delayToActivateRoamer;
	}

	private void EUpdateTargetLocalPos(PlayerTargetType targetType, Vector3 newLocalPoint)
	{
		this._targets[targetType].localPoint = newLocalPoint;
		if (this.CurState == DragonFlyTargetSelector.State.ActiveRoamer && this._curTarget == targetType)
		{
			this._roamer.UpdateTargetLocalPos(newLocalPoint);
		}
	}

	private void ERemovedTarget(PlayerTargetType removedType)
	{
		if (!this._targets.ContainsKey(removedType))
		{
			return;
		}
		this._targets.Remove(removedType);
		if (this._targets.Count == 0)
		{
			if (this._curState == DragonFlyTargetSelector.State.ActiveRoamer)
			{
				this._roamer.EscapeAction();
				this.CurState = DragonFlyTargetSelector.State.EscapeRoamer;
			}
			else if (this._curState != DragonFlyTargetSelector.State.EscapeRoamer)
			{
				this.CurState = DragonFlyTargetSelector.State.NoTargets;
			}
			this._curTarget = PlayerTargetType.NONE;
		}
		else if (this._curTarget == removedType)
		{
			this.SelectRandomTarget();
		}
	}

	private void SelectRandomTarget()
	{
		List<PlayerTargetType> list = new List<PlayerTargetType>(this._targets.Keys);
		int num = Random.Range(0, list.Count);
		this._curTarget = list[num];
		DragonFlyTargetSelector.Target target = this._targets[this._curTarget];
		this._roamer.SetTarget(target.transform, target.localPoint);
	}

	private int GetRandomSide()
	{
		return (Random.Range(0, 2) != 0) ? 1 : (-1);
	}

	private void Update()
	{
		if (this.CurState == DragonFlyTargetSelector.State.IncomingRoamer && this._activateRoamerAt < Time.time)
		{
			this.CurState = DragonFlyTargetSelector.State.ActiveRoamer;
			this._activateRoamerAt = 0f;
			Vector3 vector;
			vector..ctor((float)this.GetRandomSide() * this._spawnSettings._sideDist, 0f, this._spawnSettings._distToCamera);
			Vector3 vector2 = this._roamer._cameraTransform.TransformPoint(vector);
			vector2.y = this._spawnSettings._globalY;
			this._roamer.SetPosition(vector2);
			this.SelectRandomTarget();
		}
	}

	private void OnDestroy()
	{
		if (this._roamer != null)
		{
			this._roamer.OnEscape -= this.RoamerOnOnEscape;
			this._roamer.OnEscaped -= this.RoamerOnOnEscaped;
			this._roamer = null;
		}
		if (this.player != null)
		{
			this.player.OnActivateTarget -= this.EAddTarget;
			this.player.OnUpdateTargetLocalPos -= this.EUpdateTargetLocalPos;
			this.player.OnDeactivateTarget -= this.ERemovedTarget;
			this.player = null;
		}
	}

	[Tooltip("Object to calculate direction to escape from")]
	public Transform _cameraTransform;

	public GameObject[] _prefabs;

	public float _delayToActivateRoamer;

	public DragonFlyTargetSelector.TargetSpawnSettings _spawnSettings;

	private DragonFlyController _roamer;

	private PlayerController player;

	private float _activateRoamerAt;

	private Dictionary<PlayerTargetType, DragonFlyTargetSelector.Target> _targets = new Dictionary<PlayerTargetType, DragonFlyTargetSelector.Target>();

	private DragonFlyTargetSelector.State _curState;

	private PlayerTargetType _curTarget;

	[Serializable]
	public class TargetSpawnSettings
	{
		public float _distToCamera = 100f;

		public float _globalY = 0.5f;

		public float _sideDist = 50f;
	}

	private enum State
	{
		NoTargets,
		IncomingRoamer,
		ActiveRoamer,
		EscapeRoamer
	}

	private class Target
	{
		public Transform transform;

		public Vector3 localPoint;
	}
}
