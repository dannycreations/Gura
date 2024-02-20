using System;
using System.Diagnostics;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Common;
using I2.Loc;
using InControl;
using UnityEngine;
using UnityEngine.UI;

public class BindingInit : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event Action BindingChanged;

	private void Update()
	{
		if (ControlsController.ControlsActions.UICancel.WasPressed)
		{
			Guid? guid = BindingInit.bindingSelected;
			if (guid != null && BindingInit.bindingSelected == this.guid && !BindingInit.EscPressed)
			{
				this._action.StopListeningForBinding();
				BindingInit.EscPressed = true;
				this.CancelChangeBinding();
			}
		}
	}

	public void Init(BindingSource binding, CustomPlayerAction action, BindingSourceHelper.BindingType bindingType, int number = 1)
	{
		this.guid = Guid.NewGuid();
		this._binding = binding;
		this._action = action;
		this._bindingType = bindingType;
		this._number = number;
		if (this._binding != null)
		{
			if (this._binding.BindingSourceType == BindingSourceType.DeviceBindingSource)
			{
				DeviceBindingSource deviceBindingSource = (DeviceBindingSource)this._binding;
				this.InputNameText.text = deviceBindingSource.Control.ToString();
			}
			else
			{
				this.InputNameText.text = this._binding.Name;
			}
		}
	}

	public void SelectField()
	{
		Guid? guid = BindingInit.bindingSelected;
		if (guid == null && this._binding != null)
		{
			this.CloseIcon.SetActive(true);
		}
	}

	public void DeselectField()
	{
		Guid? guid = BindingInit.bindingSelected;
		if (guid == null || BindingInit.EscPressed)
		{
			this.CloseIcon.SetActive(false);
		}
	}

	public void DeleteBinding()
	{
		if (this._bindingType == BindingSourceHelper.BindingType.Controller)
		{
			return;
		}
		this._action.RemoveBinding(this._binding);
		this._binding = null;
		this.CloseIcon.SetActive(false);
		this.InputNameText.text = string.Empty;
		if (BindingInit.BindingChanged != null)
		{
			BindingInit.BindingChanged();
		}
	}

	public void ChangeBinding()
	{
		if (this._bindingType == BindingSourceHelper.BindingType.Controller)
		{
			return;
		}
		this.MessagePanel = GUITools.AddChild(InfoMessageController.Instance.gameObject, this.MessagePanelPrefab);
		this.MessagePanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this.MessagePanel.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		if (this._binding != null)
		{
			this._action.ListenForBindingReplacing(this._binding);
		}
		else
		{
			this._action.ListenForBinding();
		}
		BindingInit.bindingSelected = new Guid?(this.guid);
		BindingListenOptions listenOptions = ControlsController.ControlsActions.ListenOptions;
		listenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Combine(listenOptions.OnBindingAdded, new Action<PlayerAction, BindingSource>(this.UpdateBinding));
		BindingListenOptions listenOptions2 = ControlsController.ControlsActions.ListenOptions;
		listenOptions2.OnBindingRejected = (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)Delegate.Combine(listenOptions2.OnBindingRejected, new Action<PlayerAction, BindingSource, BindingSourceRejectionType>(this.ShowBindingRejectedMessage));
		ControlsController.ControlsActions.ListenOptions.OnBindingFound = delegate(PlayerAction act, BindingSource bin)
		{
			if (bin.BindingSourceType == BindingSourceType.DeviceBindingSource)
			{
				DeviceBindingSource b = (DeviceBindingSource)bin;
				if (Array.Exists<InputControlType>(this.unsupportedControls, (InputControlType element) => element == b.Control))
				{
					this.MessagePanel.transform.Find("BindingRejectedText").GetComponent<Text>().text = ScriptLocalization.Get("ControllerUnsuportedButtons");
					this.MessagePanel.transform.Find("BindingRejectedText").gameObject.SetActive(true);
					return false;
				}
			}
			if (!(bin.Name != "Escape") || ((!(bin.DeviceName == "Keyboard") || this._bindingType != BindingSourceHelper.BindingType.Keyboard) && (!(bin.DeviceName == "Mouse") || this._bindingType != BindingSourceHelper.BindingType.Mouse) && (!(bin.DeviceName == string.Empty) || this._bindingType != BindingSourceHelper.BindingType.Controller)))
			{
				return false;
			}
			if (this.duplicateConfirm)
			{
				this.duplicateConfirm = false;
				return true;
			}
			if (act.Owner.HasBinding(bin))
			{
				this.duplicateConfirm = true;
				this.MessageBox = GUITools.AddChild(InfoMessageController.Instance.gameObject, this.MessageBoxPrefab).GetComponent<MessageBox>();
				this.MessageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
				this.MessageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
				MessageBox component = this.MessageBox.GetComponent<MessageBox>();
				component.Message = ScriptLocalization.Get("AlreadyUsingKey");
				component.ConfirmButtonText = ScriptLocalization.Get("ConfirmCaption");
				component.CancelButtonText = ScriptLocalization.Get("CancelButton");
				component.confirmButton.GetComponent<Button>().onClick.AddListener(delegate
				{
					this.ConfirmDuplicateBinding(bin, act);
				});
				component.declineButton.GetComponent<Button>().onClick.AddListener(delegate
				{
					this.DeclineDuplicateBinding(bin, act);
				});
				act.StopListeningForBinding();
				return false;
			}
			return true;
		};
	}

	public void ConfirmDuplicateBinding(BindingSource bin, PlayerAction act)
	{
		if (act.Owner.ListenOptions.ReplaceBinding == null)
		{
			this._action.AddBinding(bin);
		}
		else
		{
			this._action.ReplaceBinding(act.Owner.ListenOptions.ReplaceBinding, bin);
		}
		this.UpdateBinding(this._action, bin);
		if (this.MessageBox != null)
		{
			this.MessageBox.Close();
		}
	}

	private void DeclineDuplicateBinding(BindingSource bin, PlayerAction act)
	{
		if (this.MessageBox != null)
		{
			this.MessageBox.Close();
		}
		if (this._binding != null)
		{
			this._action.ListenForBindingReplacing(this._binding);
		}
		else
		{
			this._action.ListenForBinding();
		}
	}

	private void CancelChangeBinding()
	{
		this.DeselectField();
		BindingInit.bindingSelected = null;
		BindingInit.EscPressed = false;
		MonoBehaviour.print(this.MessagePanel);
		this.CloseMessagePanel();
	}

	private void CloseMessagePanel()
	{
		BindingListenOptions listenOptions = ControlsController.ControlsActions.ListenOptions;
		listenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Remove(listenOptions.OnBindingAdded, new Action<PlayerAction, BindingSource>(this.UpdateBinding));
		BindingListenOptions listenOptions2 = ControlsController.ControlsActions.ListenOptions;
		listenOptions2.OnBindingRejected = (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)Delegate.Remove(listenOptions2.OnBindingRejected, new Action<PlayerAction, BindingSource, BindingSourceRejectionType>(this.ShowBindingRejectedMessage));
		ControlsController.ControlsActions.ListenOptions.OnBindingFound = null;
		Object.Destroy(this.MessagePanel);
	}

	private void UpdateBinding(PlayerAction action, BindingSource binding)
	{
		if (binding.DeviceName == "Keyboard")
		{
			binding.Number = this._number;
		}
		this._binding = binding;
		this.CloseMessagePanel();
		this.InputNameText.text = binding.Name;
		if (this._binding.BindingSourceType == BindingSourceType.DeviceBindingSource)
		{
			DeviceBindingSource deviceBindingSource = (DeviceBindingSource)this._binding;
			this.InputNameText.text = deviceBindingSource.Control.ToString();
		}
		else
		{
			this.InputNameText.text = this._binding.Name;
		}
		BindingInit.bindingSelected = null;
		this.DeselectField();
		if (BindingInit.BindingChanged != null)
		{
			BindingInit.BindingChanged();
		}
	}

	private void ShowBindingRejectedMessage(PlayerAction action, BindingSource binding, BindingSourceRejectionType reason)
	{
		this.MessagePanel.GetComponent<MessagePanelInit>().BindingRejectedText.SetActive(true);
	}

	public Text InputNameText;

	public GameObject CloseIcon;

	public GameObject MessagePanelPrefab;

	public GameObject MessageBoxPrefab;

	private GameObject MessagePanel;

	private MessageBox MessageBox;

	private BindingSource _binding;

	private CustomPlayerAction _action;

	private static Guid? bindingSelected;

	private static bool EscPressed;

	private BindingSourceHelper.BindingType _bindingType;

	private MenuHelpers helpers = new MenuHelpers();

	public Guid guid;

	public int _number;

	private bool duplicateConfirm;

	private InputControlType[] unsupportedControls = new InputControlType[]
	{
		InputControlType.Back,
		InputControlType.Start,
		InputControlType.System,
		InputControlType.Options,
		InputControlType.Pause,
		InputControlType.Menu,
		InputControlType.Share,
		InputControlType.Home,
		InputControlType.View,
		InputControlType.Power,
		InputControlType.Command
	};
}
