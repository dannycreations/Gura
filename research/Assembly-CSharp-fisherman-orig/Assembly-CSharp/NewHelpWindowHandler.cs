using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Common.Managers;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewHelpWindowHandler : MonoBehaviour
{
	public bool IsVisible
	{
		get
		{
			return this._alFade.Alpha > 0f;
		}
	}

	private void Awake()
	{
		this._helpTitle.text = ScriptLocalization.Get("Help").ToUpper();
		this._keyBindTitle.text = ScriptLocalization.Get("KeyBindingsCaption").ToUpper();
		this._primaryTitle.text = ScriptLocalization.Get("KeyPrimaryCaption").ToUpper();
		this._alternativeTitle.text = ScriptLocalization.Get("KeySecondaryCaption").ToUpper();
		this._mouseTitle.text = ScriptLocalization.Get("MouseCaption").ToUpper();
		Dictionary<ControlsActionsCategories, List<CustomPlayerAction>> actions = new Dictionary<ControlsActionsCategories, List<CustomPlayerAction>>();
		Type caType = ControlsController.ControlsActions.GetType();
		List<FieldInfo> list = (from p in typeof(ControlsActions).GetFields()
			where Attribute.IsDefined(p, typeof(ControlsActionAttribute))
			select p).ToList<FieldInfo>();
		list.ForEach(delegate(FieldInfo f)
		{
			object value = caType.GetField(f.Name).GetValue(ControlsController.ControlsActions);
			CustomPlayerAction customPlayerAction = (CustomPlayerAction)value;
			if (!actions.ContainsKey(customPlayerAction.Category))
			{
				actions.Add(customPlayerAction.Category, new List<CustomPlayerAction>());
			}
			actions[customPlayerAction.Category].Add(customPlayerAction);
		});
		foreach (KeyValuePair<ControlsActionsCategories, string> keyValuePair in this.ControlsActionsCategoriesLocalization)
		{
			if (actions.ContainsKey(keyValuePair.Key))
			{
				KeyBindingItemHandler item = GUITools.AddChild(this._rootContent, this._keyTitlePrefab).GetComponent<KeyBindingItemHandler>();
				item.Init(keyValuePair.Key, keyValuePair.Value);
				this._data.Add(item);
				actions[keyValuePair.Key].ForEach(delegate(CustomPlayerAction a)
				{
					item = GUITools.AddChild(this._rootContent, this._keyPrefab).GetComponent<KeyBindingItemHandler>();
					item.Init(a);
					this._data.Add(item);
				});
			}
		}
		ControlsController.OnBindingsChanged += this.ControlsController_OnBindingsChanged;
	}

	private void OnDestroy()
	{
		base.StopAllCoroutines();
		ControlsController.OnBindingsChanged -= this.ControlsController_OnBindingsChanged;
	}

	public void SetVisible(bool flag)
	{
		if (flag)
		{
			ControlsController.ControlsActions.BlockInput(null);
			CursorManager.ShowCursor();
			this._alFade.ShowPanel();
			PlayerController player = GameFactory.Player;
			if (player != null)
			{
				if (player.IsSailing)
				{
					this.ScrollToCategory(ControlsActionsCategories.Boating);
				}
				else if (player.IsInteractionWithRodStand || player.WithAnyRod)
				{
					this.ScrollToCategory(ControlsActionsCategories.Fishing);
				}
				else
				{
					this.ScrollToCategory(ControlsActionsCategories.Movement);
				}
			}
		}
		else
		{
			ControlsController.ControlsActions.UnBlockInput();
			CursorManager.HideCursor();
			this._alFade.HidePanel();
		}
	}

	public void Close()
	{
		KeysHandlerAction.EscapeHandler(false);
	}

	private void ScrollToCategory(ControlsActionsCategories cat)
	{
		int num = this._data.FindIndex((KeyBindingItemHandler p) => p.Category == cat);
		if (num > 0)
		{
			base.StopAllCoroutines();
			float num2 = (float)num / (float)this._data.Count;
			if (this._scrollBar.direction == 2)
			{
				num2 = 1f - num2;
			}
			base.StartCoroutine(this.Scroll(num2));
		}
	}

	private IEnumerator Scroll(float v)
	{
		yield return new WaitForEndOfFrame();
		this._scrollRect.verticalNormalizedPosition = v;
		yield break;
	}

	private void ControlsController_OnBindingsChanged()
	{
		this._data.ForEach(delegate(KeyBindingItemHandler p)
		{
			if (p.HasBindings)
			{
				p.OnBindingsChanged();
			}
		});
	}

	[SerializeField]
	private GameObject _rootContent;

	[SerializeField]
	private GameObject _keyPrefab;

	[SerializeField]
	private GameObject _keyTitlePrefab;

	[SerializeField]
	private TextMeshProUGUI _helpTitle;

	[SerializeField]
	private TextMeshProUGUI _keyBindTitle;

	[SerializeField]
	private TextMeshProUGUI _primaryTitle;

	[SerializeField]
	private TextMeshProUGUI _alternativeTitle;

	[SerializeField]
	private TextMeshProUGUI _mouseTitle;

	[SerializeField]
	private AlphaFade _alFade;

	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	private Scrollbar _scrollBar;

	public readonly Dictionary<ControlsActionsCategories, string> ControlsActionsCategoriesLocalization = new Dictionary<ControlsActionsCategories, string>
	{
		{
			ControlsActionsCategories.Movement,
			"KeyMappingMovement"
		},
		{
			ControlsActionsCategories.Fishing,
			"Fishing"
		},
		{
			ControlsActionsCategories.Boating,
			"KeyMappingBoating"
		},
		{
			ControlsActionsCategories.Misc,
			"MiscUpperCaption"
		}
	};

	private List<KeyBindingItemHandler> _data = new List<KeyBindingItemHandler>();
}
