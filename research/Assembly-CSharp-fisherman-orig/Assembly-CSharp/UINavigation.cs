using System;
using System.Collections.Generic;
using System.Linq;
using InControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UINavigation : ActivityStateControlled
{
	public Selectable CurrentSelected
	{
		get
		{
			return this.currentSelected;
		}
	}

	public int Layer
	{
		get
		{
			return this._visibleLayer;
		}
	}

	public Selectable[] Selectables
	{
		get
		{
			return this.selectables;
		}
	}

	public void SetUpdateRegion(UINavigation.Bindings b, bool flag)
	{
		this.updateRegion[b] = flag;
	}

	public void Reset()
	{
		for (int i = 0; i < this.selectables.Length; i++)
		{
			Toggle toggle = this.selectables[i] as Toggle;
			if (toggle != null)
			{
				toggle.isOn = false;
			}
		}
	}

	public void SetBindingSelectable(UINavigation.Bindings b, Selectable s)
	{
		HotkeyBinding hotkeyBinding = null;
		if (b == UINavigation.Bindings.Left)
		{
			hotkeyBinding = this._leftBinding;
		}
		else if (b == UINavigation.Bindings.Right)
		{
			hotkeyBinding = this._rightBinding;
		}
		else if (b == UINavigation.Bindings.Up)
		{
			hotkeyBinding = this._upBinding;
		}
		else if (b == UINavigation.Bindings.Down)
		{
			hotkeyBinding = this._downBinding;
		}
		if (hotkeyBinding != null)
		{
			hotkeyBinding.Selectable = s;
		}
	}

	public void SelectLast()
	{
		if (this.currentSelected != null)
		{
			UINavigation.SetSelectedGameObject(this.currentSelected.gameObject);
		}
	}

	public void SetConcreteActiveForce(GameObject o)
	{
		if (this.region == null)
		{
			this.UpdateSelectables();
		}
		if (this.selectables != null)
		{
			for (int i = 0; i < this.selectables.Length; i++)
			{
				if (this.selectables[i].gameObject.Equals(o))
				{
					this.currentSelected = this.selectables[i];
					break;
				}
			}
		}
	}

	public void SetFirstUpperActive()
	{
		if (this.region == null)
		{
			this.UpdateSelectables();
		}
		if (this.selectables != null)
		{
			Selectable selectable = (from s in this.selectables
				where s.transform.lossyScale != Vector3.zero && s.IsInteractable() && s.enabled && s.gameObject.activeInHierarchy
				select s into p
				orderby p.transform.GetSiblingIndex()
				select p).FirstOrDefault<Selectable>();
			if (selectable != null)
			{
				this.currentSelected = selectable;
				UINavigation.SetSelectedGameObject(this.currentSelected.gameObject);
			}
			else
			{
				LogHelper.Warning("___kocha UINavigation:SetFirstUpperActive - can't found any selectable", new object[0]);
			}
		}
	}

	public void SetFirstActiveForce()
	{
		if (this.region == null)
		{
			this.UpdateSelectables();
		}
		if (this.selectables != null)
		{
			int num = 0;
			int num2 = int.MaxValue;
			for (int i = 0; i < this.selectables.Length; i++)
			{
				int siblingIndex = this.selectables[i].transform.GetSiblingIndex();
				if (siblingIndex < num2)
				{
					num = i;
					num2 = siblingIndex;
				}
			}
			this.currentSelected = this.selectables[num];
		}
	}

	public void SetFirstActive()
	{
		if (this.needUpdate)
		{
			this.UpdateSelectables();
		}
		if (this.region == null)
		{
			return;
		}
		this.currentSelected = null;
		this.UpdateCurrentSelected();
	}

	public static void PauseForLayersLess(int layer)
	{
		for (int i = 0; i < UINavigation.allNavigation.Count; i++)
		{
			if (UINavigation.allNavigation[i]._visibleLayer < layer)
			{
				UINavigation.allNavigation[i].paused = true;
			}
		}
	}

	public static void UnpauseForLayersGreater(int layer)
	{
		for (int i = 0; i < UINavigation.allNavigation.Count; i++)
		{
			if (UINavigation.allNavigation[i]._visibleLayer >= layer)
			{
				UINavigation.allNavigation[i].paused = false;
			}
		}
	}

	private void Awake()
	{
		UINavigation.allNavigation.Add(this);
		this.referencedSelectables = this.selectables.Where((Selectable x) => x != null).ToArray<Selectable>();
		if (this.firtsGO != null)
		{
			this.currentSelected = this.firtsGO.GetComponent<Selectable>();
			UINavigation.SetSelectedGameObject(this.currentSelected.gameObject);
		}
	}

	protected override void Start()
	{
		base.Start();
		if (this.selectablesRoot != null)
		{
			ChildrenChangedListener[] componentsInChildren = this.selectablesRoot.GetComponentsInChildren<ChildrenChangedListener>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].OnChildrenChanged += this.OnTransformChildrenChanged;
			}
		}
		if (this.selectablesAlternativeRoot != null)
		{
			ChildrenChangedListener[] componentsInChildren2 = this.selectablesAlternativeRoot.GetComponentsInChildren<ChildrenChangedListener>(true);
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].OnChildrenChanged += this.OnTransformChildrenChanged;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UINavigation.allNavigation.Remove(this);
	}

	private void OnTransformChildrenChanged()
	{
		this.needUpdate = true;
	}

	public void ForceUpdate()
	{
		this.needUpdate = true;
	}

	public void ForceUpdateImmediately()
	{
		this.UpdateSelectables();
	}

	protected override void SetHelp()
	{
		HelpLinePanel.SetActionHelp(this._leftBinding);
		HelpLinePanel.SetActionHelp(this._rightBinding);
		HelpLinePanel.SetActionHelp(this._upBinding);
		HelpLinePanel.SetActionHelp(this._downBinding);
		if (!this._blockableChecked)
		{
			this._blockableRegion = base.GetComponentInParent<BlockableRegion>();
			this._blockableChecked = true;
		}
		if (this._blockableRegion != null)
		{
			this._visibleLayer = this._blockableRegion.Layer;
		}
		if (this._visibleLayer < BlockableRegion.CurrentLayer)
		{
			this.paused = true;
		}
	}

	protected override void HideHelp()
	{
		if (this.currentSelected != null && EventSystem.current != null && !EventSystem.current.alreadySelecting)
		{
			if (this.selectables.Any((Selectable p) => p != null && p.gameObject != null && p.gameObject.Equals(EventSystem.current.currentSelectedGameObject)))
			{
				UINavigation.SetSelectedGameObject(null);
			}
		}
		if (!this._rememberLast)
		{
			this.currentSelected = null;
		}
		for (int i = 0; i < this.selectables.Length; i++)
		{
			Toggle toggle = this.selectables[i] as Toggle;
			if (toggle != null && this._enableToggleOnChoose && this._toggleOffOnDisable)
			{
				toggle.isOn = false;
				this.currentSelected = null;
			}
		}
		HelpLinePanel.HideActionHelp(this._leftBinding);
		HelpLinePanel.HideActionHelp(this._rightBinding);
		HelpLinePanel.HideActionHelp(this._upBinding);
		HelpLinePanel.HideActionHelp(this._downBinding);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.OnTransformChildrenChanged();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	public void SetNavigationType(TransitionRegion.ContentNavigation nav)
	{
		this._contentNavigation = nav;
		if (this.region == null)
		{
			this.PreheatSelectables();
			return;
		}
		this.region.SetNavigationType(nav);
	}

	public void PreheatSelectables()
	{
		if (this.selectablesRoot != null)
		{
			List<Selectable> list = new List<Selectable>();
			this.Add(this.selectablesRoot, list);
			if (this.selectablesAlternativeRoot != null)
			{
				this.Add(this.selectablesAlternativeRoot, list);
			}
			this.selectables = new Selectable[this.referencedSelectables.Length];
			Array.Copy(this.referencedSelectables, 0, this.selectables, 0, this.referencedSelectables.Length);
			Array.Resize<Selectable>(ref this.selectables, this.selectables.Length + list.Count);
			Array.Copy(list.ToArray(), 0, this.selectables, this.selectables.Length - list.Count, list.Count);
		}
		if (this.region == null)
		{
			this.region = new TransitionRegion(this.selectables, this._contentNavigation, null, false);
		}
		else
		{
			this.region.UpdateContent(this.selectables);
		}
	}

	private void Add(Transform transformSelectables, List<Selectable> container)
	{
		foreach (Selectable selectable in transformSelectables.GetComponentsInChildren<Selectable>())
		{
			Type type = selectable.GetType();
			if (type != typeof(Scrollbar) && type != typeof(Slider) && (!this._skipBButtons || type != typeof(BorderedButton)) && selectable.GetComponent<IgnoredSelectable>() == null)
			{
				container.Add(selectable);
			}
		}
	}

	private void UpdateSelectables()
	{
		this.PreheatSelectables();
		if (this._selectOnEnable)
		{
			this.UpdateCurrentSelected();
		}
		this.DisableSelectables(true);
		this.needUpdate = false;
	}

	public void SetPaused(bool pause)
	{
		this.paused = pause;
	}

	private void AssignEdgeReleased(OneAxisInputControl press)
	{
		this.edgeReleaseDetector = press;
		this.edgePressedTime = Time.time + this.edgePressTime;
	}

	private void Update()
	{
		if (this.paused || InputModuleManager.GameInputType != InputModuleManager.InputType.GamePad || !base.ShouldUpdate())
		{
			if (!this.paused && this.helpInited)
			{
				base.InitHelp(false);
			}
			return;
		}
		if (!this.paused && !this.helpInited)
		{
			base.InitHelp(true);
		}
		if (this.needUpdate)
		{
			this.UpdateSelectables();
			return;
		}
		Selectable selectable = null;
		OneAxisInputControl oneAxisInputControl = ((InputManager.ActiveDevice.GetControl(this._leftBinding.Hotkey).Value <= InputManager.ActiveDevice.GetControl(this._rightBinding.Hotkey).Value) ? InputManager.ActiveDevice.GetControl(this._rightBinding.Hotkey) : InputManager.ActiveDevice.GetControl(this._leftBinding.Hotkey));
		oneAxisInputControl = ((oneAxisInputControl.Value <= InputManager.ActiveDevice.GetControl(this._downBinding.Hotkey).Value) ? InputManager.ActiveDevice.GetControl(this._downBinding.Hotkey) : oneAxisInputControl);
		oneAxisInputControl = ((oneAxisInputControl.Value <= InputManager.ActiveDevice.GetControl(this._upBinding.Hotkey).Value) ? InputManager.ActiveDevice.GetControl(this._upBinding.Hotkey) : oneAxisInputControl);
		OneAxisInputControl oneAxisInputControl2 = ((InputManager.ActiveDevice.GetControl(this._leftBinding.AlternativeHotkey).Value <= InputManager.ActiveDevice.GetControl(this._rightBinding.AlternativeHotkey).Value) ? InputManager.ActiveDevice.GetControl(this._rightBinding.AlternativeHotkey) : InputManager.ActiveDevice.GetControl(this._leftBinding.AlternativeHotkey));
		oneAxisInputControl2 = ((oneAxisInputControl2.Value <= InputManager.ActiveDevice.GetControl(this._downBinding.AlternativeHotkey).Value) ? InputManager.ActiveDevice.GetControl(this._downBinding.AlternativeHotkey) : oneAxisInputControl2);
		oneAxisInputControl2 = ((oneAxisInputControl2.Value <= InputManager.ActiveDevice.GetControl(this._upBinding.AlternativeHotkey).Value) ? InputManager.ActiveDevice.GetControl(this._upBinding.AlternativeHotkey) : oneAxisInputControl2);
		oneAxisInputControl = ((oneAxisInputControl.Value <= oneAxisInputControl2.Value) ? oneAxisInputControl2 : oneAxisInputControl);
		if (oneAxisInputControl.WasPressed || oneAxisInputControl.WasRepeated)
		{
			this.UpdateFromEventSystemSelected();
			if (this.currentSelected == null)
			{
				this.UpdateCurrentSelected();
				return;
			}
			if (oneAxisInputControl == InputManager.ActiveDevice.GetControl(this._leftBinding.Hotkey) || oneAxisInputControl == InputManager.ActiveDevice.GetControl(this._leftBinding.AlternativeHotkey))
			{
				if (this.updateRegion[UINavigation.Bindings.Left])
				{
					this.UpdateRegion();
				}
				selectable = this.currentSelected.navigation.selectOnLeft;
				if (selectable == null || selectable == this.currentSelected || !selectable.isActiveAndEnabled)
				{
					this.AssignEdgeReleased(oneAxisInputControl);
				}
			}
			if (oneAxisInputControl == InputManager.ActiveDevice.GetControl(this._rightBinding.Hotkey) || oneAxisInputControl == InputManager.ActiveDevice.GetControl(this._rightBinding.AlternativeHotkey))
			{
				if (this.updateRegion[UINavigation.Bindings.Right])
				{
					this.UpdateRegion();
				}
				selectable = this.currentSelected.navigation.selectOnRight;
				if (selectable == null || selectable == this.currentSelected || !selectable.isActiveAndEnabled)
				{
					this.AssignEdgeReleased(oneAxisInputControl);
				}
			}
			if (oneAxisInputControl == InputManager.ActiveDevice.GetControl(this._upBinding.Hotkey) || oneAxisInputControl == InputManager.ActiveDevice.GetControl(this._upBinding.AlternativeHotkey))
			{
				if (this.updateRegion[UINavigation.Bindings.Up])
				{
					this.UpdateRegion();
				}
				selectable = this.currentSelected.navigation.selectOnUp;
				if (selectable == null || selectable == this.currentSelected || !selectable.isActiveAndEnabled)
				{
					this.AssignEdgeReleased(oneAxisInputControl);
				}
			}
			if (oneAxisInputControl == InputManager.ActiveDevice.GetControl(this._downBinding.Hotkey) || oneAxisInputControl == InputManager.ActiveDevice.GetControl(this._downBinding.AlternativeHotkey))
			{
				if (this.updateRegion[UINavigation.Bindings.Down])
				{
					this.UpdateRegion();
				}
				selectable = this.currentSelected.navigation.selectOnDown;
				if (selectable == null || selectable == this.currentSelected)
				{
					this.AssignEdgeReleased(oneAxisInputControl);
				}
			}
			if (selectable != null && selectable.isActiveAndEnabled)
			{
				Toggle toggle = this.currentSelected as Toggle;
				bool flag = toggle != null && toggle.group != null;
				bool flag2 = true;
				if (this.currentSelected is Toggle && this._enableToggleOnChoose)
				{
					if (flag)
					{
						flag2 = toggle.group.allowSwitchOff;
						toggle.group.allowSwitchOff = true;
					}
					(this.currentSelected as Toggle).isOn = false;
				}
				this.currentSelected = selectable;
				if (this.currentSelected is Toggle && this._enableToggleOnChoose)
				{
					(this.currentSelected as Toggle).isOn = true;
				}
				UINavigation.SetSelectedGameObject(selectable.gameObject);
				if (flag && this._enableToggleOnChoose)
				{
					toggle.group.allowSwitchOff = flag2;
				}
			}
			else if (this.currentSelected != null)
			{
				UINavigation.SetSelectedGameObject(this.currentSelected.gameObject);
			}
		}
		if (this.edgeReleaseDetector != null && (this.edgeReleaseDetector.WasReleased || Time.time >= this.edgePressedTime))
		{
			if ((this.edgeReleaseDetector == InputManager.ActiveDevice.GetControl(this._leftBinding.Hotkey) || this.edgeReleaseDetector == InputManager.ActiveDevice.GetControl(this._leftBinding.AlternativeHotkey)) && this.OnLeftEdgeReached != null)
			{
				this.OnLeftEdgeReached(this.currentSelected);
			}
			if ((this.edgeReleaseDetector == InputManager.ActiveDevice.GetControl(this._rightBinding.Hotkey) || this.edgeReleaseDetector == InputManager.ActiveDevice.GetControl(this._rightBinding.AlternativeHotkey)) && this.OnRightEdgeReached != null)
			{
				this.OnRightEdgeReached(this.currentSelected);
			}
			if ((this.edgeReleaseDetector == InputManager.ActiveDevice.GetControl(this._upBinding.Hotkey) || this.edgeReleaseDetector == InputManager.ActiveDevice.GetControl(this._upBinding.AlternativeHotkey)) && this.OnTopReached != null)
			{
				this.OnTopReached(this.currentSelected);
			}
			if ((this.edgeReleaseDetector == InputManager.ActiveDevice.GetControl(this._downBinding.Hotkey) || this.edgeReleaseDetector == InputManager.ActiveDevice.GetControl(this._downBinding.AlternativeHotkey)) && this.OnBottomReached != null)
			{
				this.OnBottomReached(this.currentSelected);
			}
			this.edgeReleaseDetector = null;
		}
	}

	private void UpdateRegion()
	{
		if (this.region != null)
		{
			bool flag = this._axisX != InputControlType.None && this._axisY != InputControlType.None;
			bool flag2 = this._axis2X != InputControlType.None && this._axis2Y != InputControlType.None;
			if (flag || flag2)
			{
				Vector3 vector = Vector3.zero;
				if (flag)
				{
					vector += new Vector3(InputManager.ActiveDevice.GetControl(this._axisX).Value, InputManager.ActiveDevice.GetControl(this._axisY).Value, 0f);
				}
				if (flag2)
				{
					vector += new Vector3(InputManager.ActiveDevice.GetControl(this._axis2X).Value, InputManager.ActiveDevice.GetControl(this._axis2Y).Value, 0f);
				}
				vector.Normalize();
				this.region.ForceUpdateWithAxis(this.currentSelected, vector);
			}
			else
			{
				this.region.ForceUpdate(this.currentSelected);
			}
		}
	}

	public void UpdateFromEventSystemSelected()
	{
		for (int i = 0; i < this.selectables.Length; i++)
		{
			if (this.selectables[i] != null && object.ReferenceEquals(this.selectables[i].gameObject, EventSystem.current.currentSelectedGameObject))
			{
				this.currentSelected = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
			}
		}
	}

	private void UpdateCurrentSelected()
	{
		if (InputModuleManager.GameInputType != InputModuleManager.InputType.GamePad)
		{
			return;
		}
		this.DisableSelectables(false);
		for (int i = 0; i < this.selectables.Length; i++)
		{
			Toggle toggle = this.selectables[i] as Toggle;
			if (toggle != null && toggle.isOn && toggle.interactable && this._enableToggleOnChoose)
			{
				this.currentSelected = toggle;
				UINavigation.SetSelectedGameObject(this.currentSelected.gameObject);
			}
		}
		if (this.currentSelected == null)
		{
			GameObject gameObject = ((this.region != null) ? this.region.GetFirstGameObject(this._rememberLast) : null);
			if (gameObject != null)
			{
				this.currentSelected = gameObject.GetComponent<Selectable>();
				if (this._enableToggleOnChoose && this.currentSelected is Toggle)
				{
					(this.currentSelected as Toggle).isOn = true;
				}
				UINavigation.SetSelectedGameObject(this.currentSelected.gameObject);
			}
		}
	}

	private void DisableSelectables(bool disable)
	{
		if (this.disableSelectables)
		{
			for (int i = 0; i < this.selectables.Length; i++)
			{
				this.selectables[i].interactable = !disable;
			}
		}
	}

	public static void SetSelectedGameObject(GameObject go)
	{
		if (EventSystem.current != null)
		{
			LogHelper.Log(string.Concat(new object[]
			{
				"UINavigation:SetSelectedGameObject new:",
				go,
				" prev:",
				EventSystem.current.currentSelectedGameObject
			}));
			EventSystem.current.SetSelectedGameObject(go);
		}
	}

	public static GameObject CurrentSelectedGo
	{
		get
		{
			return (!(EventSystem.current != null)) ? null : EventSystem.current.currentSelectedGameObject;
		}
	}

	private Selectable currentSelected;

	[SerializeField]
	private Transform selectablesRoot;

	[SerializeField]
	private Transform selectablesAlternativeRoot;

	[SerializeField]
	private Transform firtsGO;

	private TransitionRegion region;

	[SerializeField]
	private Selectable[] selectables = new Selectable[0];

	[SerializeField]
	private bool disableSelectables;

	[SerializeField]
	private HotkeyBinding _leftBinding;

	[SerializeField]
	private HotkeyBinding _rightBinding;

	[SerializeField]
	private HotkeyBinding _upBinding;

	[SerializeField]
	private HotkeyBinding _downBinding;

	[SerializeField]
	private TransitionRegion.ContentNavigation _contentNavigation = TransitionRegion.ContentNavigation.None;

	[SerializeField]
	private InputControlType _axisX;

	[SerializeField]
	private InputControlType _axisY;

	[SerializeField]
	private InputControlType _axis2X;

	[SerializeField]
	private InputControlType _axis2Y;

	[SerializeField]
	private bool _rememberLast;

	[SerializeField]
	private bool _enableToggleOnChoose = true;

	[SerializeField]
	private bool _selectOnEnable;

	[SerializeField]
	private bool _toggleOffOnDisable = true;

	[SerializeField]
	private bool _skipBButtons;

	public Action<Selectable> OnBottomReached;

	public Action<Selectable> OnTopReached;

	public Action<Selectable> OnLeftEdgeReached;

	public Action<Selectable> OnRightEdgeReached;

	[SerializeField]
	private bool _useBindingsCheck;

	private int _visibleLayer;

	private bool needUpdate;

	private bool paused;

	private Selectable[] referencedSelectables = new Selectable[0];

	private static List<UINavigation> allNavigation = new List<UINavigation>();

	private readonly Dictionary<UINavigation.Bindings, bool> updateRegion = new Dictionary<UINavigation.Bindings, bool>
	{
		{
			UINavigation.Bindings.Down,
			true
		},
		{
			UINavigation.Bindings.Up,
			true
		},
		{
			UINavigation.Bindings.Left,
			true
		},
		{
			UINavigation.Bindings.Right,
			true
		}
	};

	private BlockableRegion _blockableRegion;

	private bool _blockableChecked;

	private OneAxisInputControl edgeReleaseDetector;

	private float edgePressedTime;

	private float edgePressTime = 0.15f;

	public enum Bindings : byte
	{
		Left,
		Right,
		Up,
		Down
	}
}
