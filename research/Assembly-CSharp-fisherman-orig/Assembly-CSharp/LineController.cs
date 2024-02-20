using System;
using TPM;
using UnityEngine;

public class LineController : MonoBehaviour
{
	public LineBehaviour Behaviour
	{
		get
		{
			return this._behaviour;
		}
	}

	public LineBehaviour SetBehaviour(UserBehaviours behaviour, IAssembledRod rodAssembly, GameFactory.RodSlot slot)
	{
		if (behaviour == UserBehaviours.FirstPerson)
		{
			this._behaviour = new Line1stBehaviour(this, rodAssembly, slot);
		}
		else if (behaviour == UserBehaviours.ThirdPerson)
		{
			this._behaviour = new Line3rdBehaviour(this, rodAssembly, slot);
		}
		else if (behaviour == UserBehaviours.None)
		{
			this._behaviour = new LineOnPodBehaviour(this, rodAssembly, slot);
		}
		return this._behaviour;
	}

	public PlayerController PlayerController
	{
		get
		{
			return this._pController;
		}
	}

	private void Awake()
	{
		this._pController = Player3dHelper.GetPlayerController();
	}

	private void Start()
	{
		if (this._behaviour != null)
		{
			this._behaviour.Start();
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
		this._behaviour = null;
		this._pController = null;
	}

	public float DebugNoiseAmp;

	public GameObject LineObject;

	public GameObject LineOnRodObject;

	public GameObject LineLeaderObject;

	public GameObject LineLeashObject;

	public GameObject RodViz;

	private LineBehaviour _behaviour;

	private PlayerController _pController;
}
