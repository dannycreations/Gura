using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

public class MissionObjects : MonoBehaviour
{
	private void Start()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			MOControllerBase component = base.transform.GetChild(i).GetComponent<MOControllerBase>();
			if (component != null)
			{
				component.Init();
				this._objects[component.ResourceKey] = component;
				if (component.IsHideOnStart)
				{
					component.Activate(false);
				}
			}
		}
		ClientMissionsManager.Instance.Added += this.OnObjectAdded;
		ClientMissionsManager.Instance.Removed += this.OnObjectRemoved;
		ClientMissionsManager.Instance.Interacted += this.OnObjectInteracted;
		ClientMissionsManager.Instance.ActiveStateChanged += this.OnObjectStateChanged;
		ClientMissionsManager.Instance.InteractionChanged += this.OnInteractionChanged;
	}

	private void OnObjectAdded(MissionInteractiveObject obj)
	{
		if (this._objects.ContainsKey(obj.ResourceKey))
		{
			this._objects[obj.ResourceKey].Activate(true);
		}
		else
		{
			LogHelper.Error("Can't add unregistered object {0}", new object[] { obj.ResourceKey });
		}
	}

	private void OnObjectRemoved(MissionInteractiveObject obj)
	{
		if (this._objects.ContainsKey(obj.ResourceKey))
		{
			this._objects[obj.ResourceKey].Activate(false);
		}
		else
		{
			LogHelper.Error("Can't remove unregistered object {0}", new object[] { obj.ResourceKey });
		}
	}

	private void OnObjectStateChanged(MissionInteractiveObject obj)
	{
		if (this._objects.ContainsKey(obj.ResourceKey))
		{
			this._objects[obj.ResourceKey].OnChanged(obj);
		}
		else
		{
			LogHelper.Error("Can't change unregistered object {0}", new object[] { obj.ResourceKey });
		}
	}

	private void OnInteractionChanged(MissionInteractiveObject obj, string interaction, bool added)
	{
		if (this._objects.ContainsKey(obj.ResourceKey))
		{
			this._objects[obj.ResourceKey].OnInteractionChanged(obj, interaction, added);
		}
		else
		{
			LogHelper.Error("Can't update interaction for unregistered object {0}", new object[] { obj.ResourceKey });
		}
	}

	private void OnObjectInteracted(MissionInteractiveObject obj, MissionInteractiveObject.AllowedInteraction interaction)
	{
		MonoBehaviour.print("OnObjectInteracted");
		if (this._objects.ContainsKey(obj.ResourceKey))
		{
			this._objects[obj.ResourceKey].OnInteracted(obj, interaction);
		}
		else
		{
			LogHelper.Error("Can't update interaction for unregistered object {0}", new object[] { obj.ResourceKey });
		}
	}

	private void OnDestroy()
	{
		ClientMissionsManager.Instance.Added -= this.OnObjectAdded;
		ClientMissionsManager.Instance.Removed -= this.OnObjectRemoved;
		ClientMissionsManager.Instance.Interacted -= this.OnObjectInteracted;
		ClientMissionsManager.Instance.ActiveStateChanged -= this.OnObjectStateChanged;
		ClientMissionsManager.Instance.InteractionChanged -= this.OnInteractionChanged;
	}

	private Dictionary<string, MOControllerBase> _objects = new Dictionary<string, MOControllerBase>();
}
