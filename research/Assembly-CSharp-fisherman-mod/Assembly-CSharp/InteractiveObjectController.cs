using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

public class InteractiveObjectController : MonoBehaviour
{
	internal void Start()
	{
		GameFactory.InteractiveObjectController = this;
		PhotonConnectionFactory.Instance.OnGotInteractiveObjects += this.PhotonServerOnGotInteractiveObjects;
		PhotonConnectionFactory.Instance.GetInteractiveObjects(StaticUserData.CurrentPond.PondId);
		PhotonConnectionFactory.Instance.OnInteractedWithObject += this.PhotonServer_OnInteractedWithObject;
		PhotonConnectionFactory.Instance.OnInteractWithObjectFailed += this.PhotonServer_OnInteractWithObjectFailed;
		this._objectsIds = new HashSet<int>();
	}

	internal void Update()
	{
		if (ControlsController.ControlsActions.InteractObject.WasPressed && this._selectedObject != null && this._canInteract && TimeHelper.UtcTime() > this._selectedObject.InteractionStart && TimeHelper.UtcTime() < this._selectedObject.InteractionEnd && this.CheckCanInteract())
		{
			this._canInteract = false;
			PhotonConnectionFactory.Instance.InteractWithObject(this._selectedObject);
		}
	}

	private void PhotonServerOnGotInteractiveObjects(IList<InteractiveObject> objects)
	{
		this._objects = objects;
		this.Refresh(this._objects);
	}

	private void Refresh(IList<InteractiveObject> objects)
	{
		this.Clear();
		this._canInteract = true;
		if (objects == null)
		{
			return;
		}
		for (int i = 0; i < objects.Count; i++)
		{
			if (PhotonConnectionFactory.Instance.Profile.CanShowObject(objects[i]))
			{
				GameObject gameObject = this.CreateObject(objects[i]);
				InteractiveObjectComponent component = gameObject.GetComponent<InteractiveObjectComponent>();
				component.OnCreate(PhotonConnectionFactory.Instance.Profile.CanInteractWithObject(component.obj) && PhotonConnectionFactory.Instance.Profile.ChekcInteractionIsOnTime(component.obj), !this._objectsIds.Contains(objects[i].ObjectId));
				this._objectsIds.Add(objects[i].ObjectId);
			}
		}
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotInteractiveObjects -= this.PhotonServerOnGotInteractiveObjects;
	}

	private GameObject CreateObject(InteractiveObject obj)
	{
		GameObject gameObject = (GameObject)Resources.Load(obj.Asset, typeof(GameObject));
		if (gameObject == null)
		{
			throw new PrefabException(string.Format("Interactive object: {0} prefab can't instantiate", obj.Asset));
		}
		GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject);
		InteractiveObjectComponent interactiveObjectComponent = gameObject2.AddComponent<InteractiveObjectComponent>();
		interactiveObjectComponent.obj = obj;
		gameObject2.transform.parent = base.transform.parent;
		gameObject2.transform.localPosition = obj.Config.Position.ToVector3();
		gameObject2.transform.localScale = obj.Config.Scale.ToVector3();
		gameObject2.transform.localEulerAngles = obj.Config.Rotation.ToVector3();
		this._listOfObjects.Add(gameObject2);
		return gameObject2;
	}

	private void Clear()
	{
		for (int i = 0; i < this._listOfObjects.Count; i++)
		{
			Object.Destroy(this._listOfObjects[i]);
		}
	}

	public void CheckInteraction(Ray ray)
	{
		Debug.DrawRay(ray.origin, ray.direction * 100f, Color.magenta);
		RaycastHit raycastHit;
		if (Physics.Raycast(ray.origin, ray.direction, ref raycastHit, 5f, GlobalConsts.InteractiveObjectsMask))
		{
			if (!this._isShowMessage)
			{
				this._isShowMessage = true;
				this._selectedComponent = raycastHit.transform.gameObject.GetComponent<InteractiveObjectComponent>();
				if (this._selectedComponent != null)
				{
					this._selectedObject = this._selectedComponent.obj;
				}
				if (TimeHelper.UtcTime() > this._selectedObject.InteractionStart && TimeHelper.UtcTime() < this._selectedObject.InteractionEnd && this.CheckCanInteract())
				{
					ShowHudElements.Instance.InteractiveObjectShowPanel(null);
					this._selectedComponent.OnSelectedInteractionReady();
				}
				else if (TimeHelper.UtcTime() < this._selectedObject.InteractionStart)
				{
					ShowHudElements.Instance.InteractiveObjectShowPanel(this._selectedObject.BeforeMessage);
					this._selectedComponent.OnSelectedBeforeInteractionStart();
				}
				else if ((InfoMessageController.Instance == null || !InfoMessageController.Instance.IsActive) && MessageFactory.InfoMessagesQueue.Count == 0 && TimeHelper.UtcTime() > this._selectedObject.InteractionStart && TimeHelper.UtcTime() < this._selectedObject.InteractionEnd && !this.CheckCanInteract())
				{
					ShowHudElements.Instance.InteractiveObjectShowPanel(this._selectedObject.IteractedMessage);
					this._selectedComponent.OnSelectedInteractionComplete();
				}
				else if (TimeHelper.UtcTime() > this._selectedObject.InteractionEnd)
				{
					ShowHudElements.Instance.InteractiveObjectShowPanel(this._selectedObject.AfterMessage);
					this._selectedComponent.OnSelectedAfterInteractionFinish();
				}
			}
		}
		else if (this._isShowMessage)
		{
			this._isShowMessage = false;
			ShowHudElements.Instance.InteractiveObjectHidePanel(false);
			this._selectedObject = null;
			if (this._selectedComponent != null)
			{
				this._selectedComponent.OnUnselected();
				this._selectedComponent = null;
			}
		}
	}

	public bool InteractionActived
	{
		get
		{
			return this._isShowMessage;
		}
	}

	private void PhotonServer_OnInteractWithObjectFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.GetInteractiveObjects(StaticUserData.CurrentPond.PondId);
	}

	private void PhotonServer_OnInteractedWithObject(IEnumerable<InventoryItem> itemsGained, int? amount, string currency)
	{
		PhotonConnectionFactory.Instance.GetInteractiveObjects(StaticUserData.CurrentPond.PondId);
		if (this._isShowMessage)
		{
			this._isShowMessage = false;
			ShowHudElements.Instance.InteractiveObjectHidePanel(false);
			this._selectedObject = null;
		}
	}

	private bool CheckCanInteract()
	{
		return PhotonConnectionFactory.Instance.Profile.CanInteractWithObject(this._selectedObject);
	}

	private bool _isShowMessage;

	private InteractiveObject _selectedObject;

	private InteractiveObjectComponent _selectedComponent;

	private bool _canInteract = true;

	private List<GameObject> _listOfObjects = new List<GameObject>();

	private IList<InteractiveObject> _objects;

	private HashSet<int> _objectsIds;
}
