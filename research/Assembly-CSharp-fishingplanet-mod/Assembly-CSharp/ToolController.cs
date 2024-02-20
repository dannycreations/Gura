using System;
using ObjectModel;
using UnityEngine;

public class ToolController : MonoBehaviour
{
	public float StartDelay
	{
		get
		{
			return this._startDelay;
		}
	}

	public float DestroyDelay
	{
		get
		{
			return this._destroyDelay;
		}
	}

	public bool IsDestroyingRequested { get; set; }

	internal void Start()
	{
		this.rootNode.gameObject.GetComponent<Rigidbody>().detectCollisions = false;
	}

	private void PhotonServer_OnConsumeItemFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnItemConsumed -= this.PhotonServer_OnItemConsumed;
		PhotonConnectionFactory.Instance.OnConsumeItemFailed -= this.PhotonServer_OnConsumeItemFailed;
		Object.Destroy(this);
	}

	private void PhotonServer_OnItemConsumed()
	{
		this._isDestroyTimer = true;
		PhotonConnectionFactory.Instance.OnItemConsumed -= this.PhotonServer_OnItemConsumed;
		PhotonConnectionFactory.Instance.OnConsumeItemFailed -= this.PhotonServer_OnConsumeItemFailed;
		this._isConsumed = true;
	}

	private void RunFirework()
	{
		if (FireworkController.Instance != null && base.transform != null)
		{
			FireworkController.Instance.RunFirework((Firework)this.Item, this.rootNode.position);
		}
	}

	public void PutOnTheGround()
	{
		PhotonConnectionFactory.Instance.OnItemConsumed += this.PhotonServer_OnItemConsumed;
		PhotonConnectionFactory.Instance.OnConsumeItemFailed += this.PhotonServer_OnConsumeItemFailed;
		Rigidbody component = this.rootNode.gameObject.GetComponent<Rigidbody>();
		ToolController.InitRigidbody(base.transform, component);
		this._isStartTimer = true;
		PhotonConnectionFactory.Instance.ConsumeItem(this.Item, 1);
	}

	public static void InitRigidbody(Transform root, Rigidbody body)
	{
		body.detectCollisions = true;
		body.useGravity = true;
		body.mass = 10f;
		root.parent = null;
		body.transform.localEulerAngles = new Vector3(0f, body.transform.localEulerAngles.y, 0f);
		root.localEulerAngles = new Vector3(0f, root.localEulerAngles.y, 0f);
		body.constraints = 122;
	}

	private void Update()
	{
		if (this._isConsumed && this._isStart && !this._isStarted)
		{
			this._isStarted = true;
			this.RunFirework();
		}
		if (this._isStartTimer)
		{
			this._startTimer += Time.deltaTime;
			if (this._startTimer > this._startDelay)
			{
				this._isStartTimer = false;
				this._isStart = true;
			}
		}
		if (this._isDestroyTimer)
		{
			this._destroyTimer += Time.deltaTime;
			if (this._destroyTimer > this._destroyDelay)
			{
				this._isDestroyTimer = false;
				Object.Destroy(base.gameObject);
			}
		}
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnItemConsumed -= this.PhotonServer_OnItemConsumed;
		PhotonConnectionFactory.Instance.OnConsumeItemFailed -= this.PhotonServer_OnConsumeItemFailed;
	}

	public Transform rootNode;

	[HideInInspector]
	public InventoryItem Item;

	private readonly float _startDelay = 3f;

	private float _startTimer;

	private bool _isStartTimer;

	private readonly float _destroyDelay = 25f;

	private float _destroyTimer;

	private bool _isDestroyTimer;

	private bool _isConsumed;

	private bool _isStart;

	private bool _isStarted;
}
