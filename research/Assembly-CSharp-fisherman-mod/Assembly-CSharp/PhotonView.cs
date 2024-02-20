using System;
using System.Collections.Generic;
using System.Reflection;
using Photon;
using UnityEngine;

[AddComponentMenu("Photon Networking/Photon View &v")]
public class PhotonView : MonoBehaviour
{
	public int prefix
	{
		get
		{
			if (this.prefixBackup == -1 && PhotonNetwork.networkingPeer != null)
			{
				this.prefixBackup = (int)PhotonNetwork.networkingPeer.currentLevelPrefix;
			}
			return this.prefixBackup;
		}
		set
		{
			this.prefixBackup = value;
		}
	}

	public object[] instantiationData
	{
		get
		{
			if (!this.didAwake)
			{
				this.instantiationDataField = PhotonNetwork.networkingPeer.FetchInstantiationData(this.instantiationId);
			}
			return this.instantiationDataField;
		}
		set
		{
			this.instantiationDataField = value;
		}
	}

	public int viewID
	{
		get
		{
			return this.viewIdField;
		}
		set
		{
			bool flag = this.didAwake && this.viewIdField == 0;
			this.ownerId = value / PhotonNetwork.MAX_VIEW_IDS;
			this.viewIdField = value;
			if (flag)
			{
				PhotonNetwork.networkingPeer.RegisterPhotonView(this);
			}
		}
	}

	public bool isSceneView
	{
		get
		{
			return this.CreatorActorNr == 0;
		}
	}

	public PhotonPlayer owner
	{
		get
		{
			return PhotonPlayer.Find(this.ownerId);
		}
	}

	public int OwnerActorNr
	{
		get
		{
			return this.ownerId;
		}
	}

	public bool isOwnerActive
	{
		get
		{
			return this.ownerId != 0 && PhotonNetwork.networkingPeer.mActors.ContainsKey(this.ownerId);
		}
	}

	public int CreatorActorNr
	{
		get
		{
			return this.viewIdField / PhotonNetwork.MAX_VIEW_IDS;
		}
	}

	public bool isMine
	{
		get
		{
			return this.ownerId == PhotonNetwork.player.ID || (!this.isOwnerActive && PhotonNetwork.isMasterClient);
		}
	}

	protected internal void Awake()
	{
		if (this.viewID != 0)
		{
			PhotonNetwork.networkingPeer.RegisterPhotonView(this);
			this.instantiationDataField = PhotonNetwork.networkingPeer.FetchInstantiationData(this.instantiationId);
		}
		this.didAwake = true;
	}

	public void RequestOwnership()
	{
		PhotonNetwork.networkingPeer.RequestOwnership(this.viewID, this.ownerId);
	}

	public void TransferOwnership(PhotonPlayer newOwner)
	{
		this.TransferOwnership(newOwner.ID);
	}

	public void TransferOwnership(int newOwnerId)
	{
		PhotonNetwork.networkingPeer.TransferOwnership(this.viewID, newOwnerId);
		this.ownerId = newOwnerId;
	}

	protected internal void OnApplicationQuit()
	{
		this.destroyedByPhotonNetworkOrQuit = true;
	}

	protected internal void OnDestroy()
	{
		if (!this.destroyedByPhotonNetworkOrQuit)
		{
			PhotonNetwork.networkingPeer.LocalCleanPhotonView(this);
		}
		if (!this.destroyedByPhotonNetworkOrQuit && !Application.isLoadingLevel)
		{
			if (this.instantiationId > 0)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"OnDestroy() seems to be called without PhotonNetwork.Destroy()?! GameObject: ",
					base.gameObject,
					" Application.isLoadingLevel: ",
					Application.isLoadingLevel
				}));
			}
			else if (this.viewID <= 0)
			{
				Debug.LogWarning(string.Format("OnDestroy manually allocated PhotonView {0}. The viewID is 0. Was it ever (manually) set?", this));
			}
			else if (this.isMine && !PhotonNetwork.manuallyAllocatedViewIds.Contains(this.viewID))
			{
				Debug.LogWarning(string.Format("OnDestroy manually allocated PhotonView {0}. The viewID is local (isMine) but not in manuallyAllocatedViewIds list. Use UnAllocateViewID() after you destroyed the PV.", this));
			}
		}
	}

	public void SerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		this.SerializeComponent(this.observed, stream, info);
		for (int i = 0; i < this.ObservedComponents.Count; i++)
		{
			this.SerializeComponent(this.ObservedComponents[i], stream, info);
		}
	}

	public void DeserializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		this.DeserializeComponent(this.observed, stream, info);
		for (int i = 0; i < this.ObservedComponents.Count; i++)
		{
			this.DeserializeComponent(this.ObservedComponents[i], stream, info);
		}
	}

	protected internal void DeserializeComponent(Component component, PhotonStream stream, PhotonMessageInfo info)
	{
		if (component == null)
		{
			return;
		}
		if (component is MonoBehaviour)
		{
			this.ExecuteComponentOnSerialize(component, stream, info);
		}
		else if (component is Transform)
		{
			Transform transform = (Transform)component;
			switch (this.onSerializeTransformOption)
			{
			case OnSerializeTransform.OnlyPosition:
				transform.localPosition = (Vector3)stream.ReceiveNext();
				break;
			case OnSerializeTransform.OnlyRotation:
				transform.localRotation = (Quaternion)stream.ReceiveNext();
				break;
			case OnSerializeTransform.OnlyScale:
				transform.localScale = (Vector3)stream.ReceiveNext();
				break;
			case OnSerializeTransform.PositionAndRotation:
				transform.localPosition = (Vector3)stream.ReceiveNext();
				transform.localRotation = (Quaternion)stream.ReceiveNext();
				break;
			case OnSerializeTransform.All:
				transform.localPosition = (Vector3)stream.ReceiveNext();
				transform.localRotation = (Quaternion)stream.ReceiveNext();
				transform.localScale = (Vector3)stream.ReceiveNext();
				break;
			}
		}
		else if (component is Rigidbody)
		{
			Rigidbody rigidbody = (Rigidbody)component;
			OnSerializeRigidBody onSerializeRigidBody = this.onSerializeRigidBodyOption;
			if (onSerializeRigidBody != OnSerializeRigidBody.All)
			{
				if (onSerializeRigidBody != OnSerializeRigidBody.OnlyAngularVelocity)
				{
					if (onSerializeRigidBody == OnSerializeRigidBody.OnlyVelocity)
					{
						rigidbody.velocity = (Vector3)stream.ReceiveNext();
					}
				}
				else
				{
					rigidbody.angularVelocity = (Vector3)stream.ReceiveNext();
				}
			}
			else
			{
				rigidbody.velocity = (Vector3)stream.ReceiveNext();
				rigidbody.angularVelocity = (Vector3)stream.ReceiveNext();
			}
		}
		else if (component is Rigidbody2D)
		{
			Rigidbody2D rigidbody2D = (Rigidbody2D)component;
			OnSerializeRigidBody onSerializeRigidBody2 = this.onSerializeRigidBodyOption;
			if (onSerializeRigidBody2 != OnSerializeRigidBody.All)
			{
				if (onSerializeRigidBody2 != OnSerializeRigidBody.OnlyAngularVelocity)
				{
					if (onSerializeRigidBody2 == OnSerializeRigidBody.OnlyVelocity)
					{
						rigidbody2D.velocity = (Vector2)stream.ReceiveNext();
					}
				}
				else
				{
					rigidbody2D.angularVelocity = (float)stream.ReceiveNext();
				}
			}
			else
			{
				rigidbody2D.velocity = (Vector2)stream.ReceiveNext();
				rigidbody2D.angularVelocity = (float)stream.ReceiveNext();
			}
		}
		else
		{
			Debug.LogError("Type of observed is unknown when receiving.");
		}
	}

	protected internal void SerializeComponent(Component component, PhotonStream stream, PhotonMessageInfo info)
	{
		if (component == null)
		{
			return;
		}
		if (component is MonoBehaviour)
		{
			this.ExecuteComponentOnSerialize(component, stream, info);
		}
		else if (component is Transform)
		{
			Transform transform = (Transform)component;
			switch (this.onSerializeTransformOption)
			{
			case OnSerializeTransform.OnlyPosition:
				stream.SendNext(transform.localPosition);
				break;
			case OnSerializeTransform.OnlyRotation:
				stream.SendNext(transform.localRotation);
				break;
			case OnSerializeTransform.OnlyScale:
				stream.SendNext(transform.localScale);
				break;
			case OnSerializeTransform.PositionAndRotation:
				stream.SendNext(transform.localPosition);
				stream.SendNext(transform.localRotation);
				break;
			case OnSerializeTransform.All:
				stream.SendNext(transform.localPosition);
				stream.SendNext(transform.localRotation);
				stream.SendNext(transform.localScale);
				break;
			}
		}
		else if (component is Rigidbody)
		{
			Rigidbody rigidbody = (Rigidbody)component;
			OnSerializeRigidBody onSerializeRigidBody = this.onSerializeRigidBodyOption;
			if (onSerializeRigidBody != OnSerializeRigidBody.All)
			{
				if (onSerializeRigidBody != OnSerializeRigidBody.OnlyAngularVelocity)
				{
					if (onSerializeRigidBody == OnSerializeRigidBody.OnlyVelocity)
					{
						stream.SendNext(rigidbody.velocity);
					}
				}
				else
				{
					stream.SendNext(rigidbody.angularVelocity);
				}
			}
			else
			{
				stream.SendNext(rigidbody.velocity);
				stream.SendNext(rigidbody.angularVelocity);
			}
		}
		else if (component is Rigidbody2D)
		{
			Rigidbody2D rigidbody2D = (Rigidbody2D)component;
			OnSerializeRigidBody onSerializeRigidBody2 = this.onSerializeRigidBodyOption;
			if (onSerializeRigidBody2 != OnSerializeRigidBody.All)
			{
				if (onSerializeRigidBody2 != OnSerializeRigidBody.OnlyAngularVelocity)
				{
					if (onSerializeRigidBody2 == OnSerializeRigidBody.OnlyVelocity)
					{
						stream.SendNext(rigidbody2D.velocity);
					}
				}
				else
				{
					stream.SendNext(rigidbody2D.angularVelocity);
				}
			}
			else
			{
				stream.SendNext(rigidbody2D.velocity);
				stream.SendNext(rigidbody2D.angularVelocity);
			}
		}
		else
		{
			Debug.LogError("Observed type is not serializable: " + component.GetType());
		}
	}

	protected internal void ExecuteComponentOnSerialize(Component component, PhotonStream stream, PhotonMessageInfo info)
	{
		if (component != null)
		{
			if (!this.m_OnSerializeMethodInfos.ContainsKey(component))
			{
				MethodInfo methodInfo = null;
				if (!NetworkingPeer.GetMethod(component as MonoBehaviour, PhotonNetworkingMessage.OnPhotonSerializeView.ToString(), out methodInfo))
				{
					Debug.LogError("The observed monobehaviour (" + component.name + ") of this PhotonView does not implement OnPhotonSerializeView()!");
					methodInfo = null;
				}
				this.m_OnSerializeMethodInfos.Add(component, methodInfo);
			}
			if (this.m_OnSerializeMethodInfos[component] != null)
			{
				this.m_OnSerializeMethodInfos[component].Invoke(component, new object[] { stream, info });
			}
		}
	}

	public static PhotonView Get(Component component)
	{
		return component.GetComponent<PhotonView>();
	}

	public static PhotonView Get(GameObject gameObj)
	{
		return gameObj.GetComponent<PhotonView>();
	}

	public static PhotonView Find(int viewID)
	{
		return PhotonNetwork.networkingPeer.GetPhotonView(viewID);
	}

	public override string ToString()
	{
		return string.Format("View ({3}){0} on {1} {2}", new object[]
		{
			this.viewID,
			(!(base.gameObject != null)) ? "GO==null" : base.gameObject.name,
			(!this.isSceneView) ? string.Empty : "(scene)",
			this.prefix
		});
	}

	public int ownerId;

	public int group;

	protected internal bool mixedModeIsReliable;

	public int prefixBackup = -1;

	private object[] instantiationDataField;

	protected internal object[] lastOnSerializeDataSent;

	protected internal object[] lastOnSerializeDataReceived;

	public Component observed;

	public ViewSynchronization synchronization;

	public OnSerializeTransform onSerializeTransformOption = OnSerializeTransform.PositionAndRotation;

	public OnSerializeRigidBody onSerializeRigidBodyOption = OnSerializeRigidBody.All;

	public OwnershipOption ownershipTransfer;

	public List<Component> ObservedComponents;

	private Dictionary<Component, MethodInfo> m_OnSerializeMethodInfos = new Dictionary<Component, MethodInfo>();

	[SerializeField]
	private bool ObservedComponentsFoldoutOpen = true;

	[SerializeField]
	private int viewIdField;

	public int instantiationId;

	protected internal bool didAwake;

	[SerializeField]
	protected internal bool isRuntimeInstantiated;

	protected internal bool destroyedByPhotonNetworkOrQuit;

	private MethodInfo OnSerializeMethodInfo;

	private bool failedToFindOnSerialize;
}
