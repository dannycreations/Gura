using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InControl;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionsController : MonoBehaviour
{
	public TransitionsBuilder CurrentBuilder { get; set; }

	public static TransitionsController Instance { get; private set; }

	private void Awake()
	{
		this.Initialization();
	}

	private void Initialization()
	{
		if (TransitionsController.Instance != null)
		{
			Object.Destroy(TransitionsController.Instance.gameObject);
		}
		TransitionsController.Instance = this;
		this.transitionRegion = null;
		this.topRegion = null;
		this.blockableRegions = new HashSet<Transform>();
		this.CurrentBuilder = null;
		this.topTransitions = null;
		Object.DontDestroyOnLoad(this);
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			InControlInputModule inControlInputModule = Object.FindObjectOfType<InControlInputModule>();
			inControlInputModule.OnMove = (Action)Delegate.Combine(inControlInputModule.OnMove, new Action(this.Rebuild));
		}
		SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnLevelWasLoadedNew);
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.OnLevelWasLoadedNew);
	}

	private void UpdateSelectables()
	{
		if (this.CurrentBuilder != null)
		{
			this.CurrentBuilder.UpdateContent();
		}
	}

	public void BlockAllByRegion(Transform regionRoot)
	{
		this.blockableRegions.Add(regionRoot);
		base.StartCoroutine(this.BlockByRegionDelayed(regionRoot));
	}

	private IEnumerator BlockByRegionDelayed(Transform regionRoot)
	{
		yield return new WaitForEndOfFrame();
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			this.CreateActiveRegion(regionRoot, TransitionRegion.ContentNavigation.HorizontalAndVertical);
			if (this.CurrentBuilder != null)
			{
				this.CurrentBuilder.OnDeselect();
				this.CurrentBuilder = null;
			}
		}
		yield break;
	}

	private void OnLevelWasLoadedNew(Scene scene, LoadSceneMode mode)
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			InControlInputModule inControlInputModule = Object.FindObjectOfType<InControlInputModule>();
			if (inControlInputModule != null)
			{
				InControlInputModule inControlInputModule2 = inControlInputModule;
				inControlInputModule2.OnMove = (Action)Delegate.Combine(inControlInputModule2.OnMove, new Action(this.Rebuild));
			}
		}
	}

	public TransitionRegion CreateActiveRegion(Transform regionRoot, TransitionRegion.ContentNavigation navigation)
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			List<Selectable> list = new List<Selectable>();
			foreach (Selectable selectable in regionRoot.GetComponentsInChildren<Selectable>(true))
			{
				if (selectable.GetType() != typeof(Scrollbar) && selectable.GetType() != typeof(Slider))
				{
					list.Add(selectable);
				}
			}
			this.transitionRegion = new TransitionRegion(list.ToArray(), navigation, null, false);
			return this.transitionRegion;
		}
		return null;
	}

	public void UnblockByRegion(Transform regionRoot)
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			if (!this.blockableRegions.Remove(regionRoot))
			{
				return;
			}
			this.CloseActiveRegion();
			if (this.blockableRegions.Count == 0)
			{
				if (this.topRegion != null)
				{
					GameObject firstGameObject = this.topRegion.GetFirstGameObject(true);
					if (firstGameObject != null)
					{
						this.CurrentBuilder = firstGameObject.GetComponent<TransitionsBuilder>();
						this.CurrentBuilder.OnSelect();
					}
				}
			}
			else
			{
				Transform transform = this.blockableRegions.First<Transform>();
				this.blockableRegions.Remove(transform);
				this.BlockAllByRegion(transform);
			}
		}
	}

	public void CloseActiveRegion()
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			if (this.transitionRegion == null)
			{
				return;
			}
			this.transitionRegion.Close();
			this.transitionRegion = null;
			if (this.CurrentBuilder != null)
			{
				this.CurrentBuilder.SelectGameObject();
			}
		}
	}

	public void RebuildBuilders(TransitionsBuilder builder, bool added = true)
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			if (this.topRegion == null)
			{
				this.topTransitions = new Selectable[] { builder.cachedSelectable };
				this.topRegion = new TransitionRegion(this.topTransitions, TransitionRegion.ContentNavigation.WidthAndHeightBased, null, false);
			}
			if (added)
			{
				if (!Array.Exists<Selectable>(this.topTransitions, (Selectable item) => item == builder.GetComponent<Selectable>()))
				{
					Array.Resize<Selectable>(ref this.topTransitions, this.topTransitions.Length + 1);
					this.topTransitions[this.topTransitions.Length - 1] = builder.cachedSelectable;
				}
			}
			else
			{
				int num = Array.FindIndex<Selectable>(this.topTransitions, (Selectable current) => current == builder.GetComponent<Selectable>());
				if (num >= 0)
				{
					this.topTransitions[num] = this.topTransitions[this.topTransitions.Length - 1];
					Array.Resize<Selectable>(ref this.topTransitions, this.topTransitions.Length - 1);
				}
			}
			this.topRegion.UpdateContent(this.topTransitions);
			if ((this.CurrentBuilder == null || (this.CurrentBuilder == builder && !added)) && this.transitionRegion == null)
			{
				if (this.topRegion.GetFirstGameObject(true) != null)
				{
					this.CurrentBuilder = this.topRegion.GetFirstGameObject(true).GetComponent<TransitionsBuilder>();
					this.CurrentBuilder.OnSelect();
				}
				else
				{
					this.CurrentBuilder = null;
					if (EventSystem.current != null)
					{
						EventSystem.current.SetSelectedGameObject(null);
					}
				}
			}
		}
	}

	private void Rebuild()
	{
		if (this.transitionRegion != null)
		{
			this.transitionRegion.ForceUpdate((!(EventSystem.current.currentSelectedGameObject == null)) ? EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>() : null);
		}
		else if (this.CurrentBuilder != null)
		{
			this.CurrentBuilder.ForceUpdate();
		}
	}

	private void Update()
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			if (this.transitionRegion == null)
			{
				if (this.CurrentBuilder != null)
				{
					this.CurrentBuilder.UpdateBuilder();
				}
			}
			if (this.CurrentBuilder == null)
			{
				return;
			}
			this.topRegion.ForceUpdate(this.CurrentBuilder.GetComponent<Selectable>());
			Selectable selectable = null;
			OneAxisInputControl oneAxisInputControl = ((InputManager.ActiveDevice.LeftStick.Down.Value <= InputManager.ActiveDevice.LeftStick.Left.Value) ? InputManager.ActiveDevice.LeftStick.Left : InputManager.ActiveDevice.LeftStick.Down);
			oneAxisInputControl = ((oneAxisInputControl.Value <= InputManager.ActiveDevice.LeftStick.Right.Value) ? InputManager.ActiveDevice.LeftStick.Right : oneAxisInputControl);
			oneAxisInputControl = ((oneAxisInputControl.Value <= InputManager.ActiveDevice.LeftStick.Up.Value) ? InputManager.ActiveDevice.LeftStick.Up : oneAxisInputControl);
			if (!oneAxisInputControl.WasPressed && !oneAxisInputControl.WasRepeated)
			{
				return;
			}
			if (oneAxisInputControl == InputManager.ActiveDevice.LeftStick.Down)
			{
				selectable = this.CurrentBuilder.GetComponent<Selectable>().navigation.selectOnDown;
			}
			else if (oneAxisInputControl == InputManager.ActiveDevice.LeftStick.Left)
			{
				selectable = this.CurrentBuilder.GetComponent<Selectable>().navigation.selectOnLeft;
			}
			else if (oneAxisInputControl == InputManager.ActiveDevice.LeftStick.Right)
			{
				selectable = this.CurrentBuilder.GetComponent<Selectable>().navigation.selectOnRight;
			}
			else if (oneAxisInputControl == InputManager.ActiveDevice.LeftStick.Up)
			{
				selectable = this.CurrentBuilder.GetComponent<Selectable>().navigation.selectOnUp;
			}
			if (!object.ReferenceEquals(selectable, null))
			{
				TransitionsBuilder component = selectable.GetComponent<TransitionsBuilder>();
				this.CurrentBuilder.OnDeselect();
				component.OnSelect();
				this.CurrentBuilder = component;
				if (this.transitionRegion != null)
				{
					this.transitionRegion.Close();
					this.transitionRegion = null;
				}
			}
		}
	}

	private Selectable[] topTransitions;

	private TransitionRegion topRegion;

	private TransitionRegion transitionRegion;

	private HashSet<Transform> blockableRegions = new HashSet<Transform>();
}
