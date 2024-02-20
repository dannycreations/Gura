using System;
using UnityEngine;

public abstract class TackleControllerBase : MonoBehaviour
{
	public PlayerController PlayerController
	{
		get
		{
			return this._pController;
		}
	}

	public GameFactory.RodSlot RodSlot { get; protected set; }

	public TackleBehaviour Behaviour
	{
		get
		{
			return this._behaviour;
		}
	}

	public virtual TackleBehaviour SetBehaviour(UserBehaviours behaviourType, IAssembledRod rodAssembly, GameFactory.RodSlot slot, RodOnPodBehaviour.TransitionData transitionData = null)
	{
		this.RodSlot = slot;
		return null;
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
		this.OnUpdate();
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
		this.RodSlot = null;
		this._pController = null;
	}

	protected virtual void OnUpdate()
	{
	}

	public Transform topLineAnchor;

	public Transform bottomLineAnchor;

	protected PlayerController _pController;

	protected TackleBehaviour _behaviour;
}
