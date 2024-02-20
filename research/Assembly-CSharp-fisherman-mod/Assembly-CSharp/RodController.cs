using System;
using System.Collections.Generic;
using TPM;
using UnityEngine;

public class RodController : MonoBehaviour
{
	public RodBehaviour Behaviour
	{
		get
		{
			return this._behaviour;
		}
	}

	public PlayerController PlayerController
	{
		get
		{
			return this._pController;
		}
	}

	public ReelTypes ReelType
	{
		get
		{
			return this._behaviour.RodAssembly.ReelType;
		}
	}

	public float BackTipDistance
	{
		get
		{
			return this._behaviour.DistFromRootToBackTip;
		}
	}

	private void Awake()
	{
		this._pController = Player3dHelper.GetPlayerController();
		this.prefabFirstRingLocatorPosition = new Vector3[this.FirstRingLocators.Count];
		this.prefabLineLocatorPosition = new Vector3[this.LineLocators.Count];
		this.prefabBaitLineLocatorPosition = new Vector3[this.BaitLineLocators.Count];
		for (int i = 0; i < this.FirstRingLocators.Count; i++)
		{
			this.prefabFirstRingLocatorPosition[i] = this.FirstRingLocators[i].localPosition;
		}
		for (int j = 0; j < this.LineLocators.Count; j++)
		{
			this.prefabLineLocatorPosition[j] = this.LineLocators[j].localPosition;
		}
		for (int k = 0; k < this.BaitLineLocators.Count; k++)
		{
			this.prefabBaitLineLocatorPosition[k] = this.BaitLineLocators[k].localPosition;
		}
	}

	public void ResetLocators()
	{
		for (int i = 0; i < this.FirstRingLocators.Count; i++)
		{
			this.FirstRingLocators[i].localPosition = this.prefabFirstRingLocatorPosition[i];
		}
		for (int j = 0; j < this.LineLocators.Count; j++)
		{
			this.LineLocators[j].localPosition = this.prefabLineLocatorPosition[j];
		}
		for (int k = 0; k < this.BaitLineLocators.Count; k++)
		{
			this.BaitLineLocators[k].localPosition = this.prefabBaitLineLocatorPosition[k];
		}
	}

	public RodBehaviour SetBehaviour(UserBehaviours behaviourType, IAssembledRod rodAssembly, GameFactory.RodSlot slot, bool isMain, RodOnPodBehaviour.TransitionData td = null, string playerName = null)
	{
		if (this._behaviour == null)
		{
			if (behaviourType == UserBehaviours.FirstPerson)
			{
				this._behaviour = new Rod1stBehaviour(this, rodAssembly as AssembledRod, slot);
			}
			else if (behaviourType == UserBehaviours.ThirdPerson)
			{
				this._behaviour = new Rod3rdBehaviour(this, rodAssembly, slot, isMain, playerName);
			}
			else if (behaviourType == UserBehaviours.RodPod)
			{
				this._behaviour = new RodOnPodBehaviour(this, td, slot);
			}
		}
		return this._behaviour;
	}

	private void Start()
	{
		if (this._behaviour != null)
		{
			this._behaviour.Start();
		}
	}

	private void FixedUpdate()
	{
		if (this._behaviour != null)
		{
			this._behaviour.FixedUpdate();
		}
	}

	private void Update()
	{
		if (this._behaviour != null)
		{
			this._behaviour.Update();
		}
	}

	private void LateUpdate()
	{
		if (this._behaviour != null)
		{
			this._behaviour.LateUpdate();
		}
	}

	private void OnDestroy()
	{
		if (this.Behaviour != null)
		{
			this.Behaviour.Clean();
		}
		this._behaviour = null;
		this._pController = null;
	}

	public bool DebugSnapshotLog;

	public bool SimThreadDebugTrigger;

	public Transform rootNode;

	public BendingSegment segment;

	public List<Transform> LineLocators;

	public List<Transform> BaitLineLocators;

	public float weight = 0.2f;

	public Vector3 lastForceOnTip;

	public Vector3 lastForceOnHand;

	public Vector3 cachedForceOnTip;

	public Vector3 cachedForceOnHand;

	public float cachedAccelOnTip;

	public float cachedAccelOnHand;

	public List<Transform> FirstRingLocators;

	private RodBehaviour _behaviour;

	private RodBehaviour _behaviourBackup;

	private PlayerController _pController;

	private Vector3[] prefabFirstRingLocatorPosition;

	private Vector3[] prefabLineLocatorPosition;

	private Vector3[] prefabBaitLineLocatorPosition;
}
