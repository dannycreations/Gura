using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Common.Managers;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using InControl;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
	public static bool EnableBackToLobby
	{
		get
		{
			return TutorialController.Step > 23;
		}
	}

	public static int Step
	{
		get
		{
			string text = TutorialController.CurrentStep;
			if (!string.IsNullOrEmpty(text))
			{
				int num = text.IndexOf('_');
				if (num != -1)
				{
					text = text.Substring(0, num);
				}
				string text2 = new string(text.Where(new Func<char, bool>(char.IsNumber)).ToArray<char>());
				int num2;
				if (int.TryParse(text2, out num2))
				{
					return num2;
				}
			}
			return -1;
		}
	}

	public static string CurrentStep
	{
		get
		{
			return TutorialController._currentStep;
		}
		set
		{
			TutorialController._currentStep = value;
		}
	}

	private void Start()
	{
		if (StaticUserData.CurrentPond.PondId == 2)
		{
			MenuPrefabsSpawner[] array = Resources.FindObjectsOfTypeAll<MenuPrefabsSpawner>();
			if (array != null)
			{
				array[0].Init();
			}
		}
		else
		{
			for (int i = 0; i < this._tutorialObjects.Length; i++)
			{
				Object.Destroy(this._tutorialObjects[i]);
			}
			Object.Destroy(base.gameObject);
		}
	}

	private void StartTutorial()
	{
		PlayerPrefs.DeleteKey("SentSystemInfo2");
		SysInfoHelper.SendSystemInfo();
		if (StaticUserData.CurrentPond.PondId != 2)
		{
			base.gameObject.SetActive(false);
		}
		TutorialController.CurrentStep = "Step0";
		TutorialController.CurrentStepFinished = true;
		this.TutorialInit();
	}

	private void Update()
	{
		if (TutorialController._pondHelpers != null && TutorialController._pondHelpers.PondControllerList.Game3DPond.activeSelf && !this._wasStarted)
		{
			this._wasStarted = true;
			this.StartTutorial();
		}
		if (!this._wasStarted)
		{
			return;
		}
		if (this._panel != null && !this._isShowing && !this._isHiding && !this._isShortMessageShow)
		{
			this._fullMessageTimer += Time.deltaTime;
			if (this._fullMessageTimer > this._newStep.TimeShowingFullMessage)
			{
				this.ShowShortPanel(this._newStep);
			}
		}
		if (this._panel == null && !TutorialController.CurrentStepFinished)
		{
			this._isShowing = true;
		}
		if (this._isShowing)
		{
			this._currentFullShowTime += Time.deltaTime;
			float num = ((!this._newStep.IsWaitingHintMessage()) ? (this._currentFullShowTime / this._showFullFadeTime) : 0f);
			if (this._panel != null)
			{
				this._panel.GetComponent<CanvasGroup>().alpha = num;
				if (this._panel.GetComponent<CanvasGroup>().alpha >= 1f)
				{
					this._isShowing = false;
				}
			}
			else
			{
				this._isShowing = false;
			}
		}
		if (this._isHiding && this._hidePanel != null)
		{
			this._currentFullHideTime += Time.deltaTime;
			float num2 = this._currentFullHideTime / this._hideFullFadeTime;
			this._hidePanel.GetComponent<CanvasGroup>().alpha = 1f - num2;
			if (this._hidePanel.GetComponent<CanvasGroup>().alpha <= 0f)
			{
				Object.Destroy(this._hidePanel);
				this._isHiding = false;
			}
		}
		if (!TutorialController.CurrentStepFinished)
		{
			this._newStep.StepUpdate();
		}
		if ((ControlsController.ControlsActions.UICancel.WasPressedMandatory || ControlsController.ControlsActions.HelpGamepad.WasPressedMandatory) && this.ExitEnabled && !KeysHandlerAction.HelpShown && TutorialController.CurrentStep != "Step24" && this.ExitInGamePanel != null)
		{
			this.ExitInGamePanel.SetActive(!this.ExitInGamePanel.activeSelf);
		}
	}

	private bool ExitEnabled
	{
		get
		{
			PlayerController player = GameFactory.Player;
			if (player == null)
			{
				return true;
			}
			MenuPrefabsList menuPrefabsList = new MenuHelpers().MenuPrefabsList;
			return menuPrefabsList == null || (player.State != typeof(PlayerShowFishLineIn) && (player.Tackle == null || !player.Tackle.IsFishHooked) && !player.IsTackleThrown && !player.CantOpenInventory && !menuPrefabsList.IsLoadingFormActive && !menuPrefabsList.IsTravelingFormActive);
		}
	}

	private void RecordStep(string stepName)
	{
		if (PhotonConnectionFactory.Instance != null)
		{
			string text = stepName;
			int num = text.IndexOf('_');
			if (num > 0)
			{
				text = text.Substring(0, num);
			}
			text = TutorialController.digitRegex.Replace(text, delegate(Match match)
			{
				if (match.Length >= 2)
				{
					return match.Value;
				}
				return match.Value.PadLeft(2, '0');
			});
			if (!this.passedTutorialSteps.Contains(text))
			{
				this.passedTutorialSteps.Add(text);
				PhotonConnectionFactory.Instance.SetTutorialStep(text);
			}
		}
	}

	private void TutorialInit()
	{
		List<Selectable> list;
		if (this.ExitInGamePanel != null)
		{
			list = (from p in this.ExitInGamePanel.GetComponentsInChildren<BorderedButton>(true)
				select (p)).ToList<Selectable>();
		}
		else
		{
			list = new List<Selectable>();
		}
		List<Selectable> list2 = list;
		this._sceneTutorialSteps = new List<TutorialStep>();
		TutorialStep[] array = Object.FindObjectsOfType<TutorialStep>();
		foreach (TutorialStep tutorialStep in array)
		{
			tutorialStep.SetSelectablesForSkip(list2);
			tutorialStep.StartTriggering += this.step_StartTriggering;
			tutorialStep.EndTriggering += this.step_EndTriggering;
			this._sceneTutorialSteps.Add(tutorialStep);
		}
		Debug.Log("Steps count: " + array.Count<TutorialStep>());
	}

	private void step_EndTriggering(object sender, TutorialEventArgs e)
	{
		TutorialController.CurrentStepFinished = true;
		if (this._panel != null)
		{
			this._isShortMessageShow = false;
			this.HidePanel(this._panel);
		}
		if (e.tutorialStep.IsActive)
		{
			e.tutorialStep.DoEndAction();
		}
	}

	private void step_StartTriggering(object sender, TutorialEventArgs e)
	{
		TutorialController.CurrentStepFinished = false;
		this._isHiding = false;
		if (this._panel != null)
		{
			Object.Destroy(this._panel);
			this._panel = null;
		}
		if (this._hidePanel != null)
		{
			Object.Destroy(this._hidePanel);
		}
		this._isShortMessageShow = false;
		if (e.tutorialStep.TimeShowingFullMessage > 0f)
		{
			this._panel = GUITools.AddChild(this.TutorialPanel, this.PanelPrefab);
			this.ShowPanel(e.tutorialStep);
		}
		else
		{
			this._newStep = e.tutorialStep;
			this.ShowShortPanel(this._newStep);
		}
		if (!e.tutorialStep.IsActive)
		{
			e.tutorialStep.DoStartAction();
		}
	}

	private void ShowPanel(TutorialStep step)
	{
		this._panel.transform.Find("Caption").Find("Text").GetComponent<Text>()
			.text = string.Format(ScriptLocalization.Get(step.Title), new object[0]);
		if (step.BigControlIcon != InputControlType.None && InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			if (step.UseIconBigMessage)
			{
				this._panel.transform.Find("Image").gameObject.SetActive(false);
				this._panel.transform.Find("Icon").gameObject.SetActive(true);
				this._panel.transform.Find("Icon").GetComponent<Text>().text = HotkeyIcons.KeyMappings[step.BigControlIcon];
			}
			else
			{
				this._panel.transform.Find("Image").gameObject.SetActive(true);
				this._panel.transform.Find("Icon").gameObject.SetActive(false);
				this._panel.transform.Find("Image").GetComponent<Image>().overrideSprite = step.Image;
			}
			if (step.AppendIconToBigMessage)
			{
				this._panel.transform.Find("Text").GetComponent<Text>().text = string.Format(ScriptLocalization.Get(step.Message + "Gamepad"), '\n', HotkeyIcons.KeyMappings[step.BigControlIcon]);
			}
			else
			{
				this._panel.transform.Find("Text").GetComponent<Text>().text = string.Format(ScriptLocalization.Get(step.Message), '\n');
			}
		}
		else
		{
			this._panel.transform.Find("Image").gameObject.SetActive(true);
			this._panel.transform.Find("Icon").gameObject.SetActive(false);
			this._panel.transform.Find("Image").GetComponent<Image>().overrideSprite = step.Image;
			this._panel.transform.Find("Text").GetComponent<Text>().text = string.Format(ScriptLocalization.Get(step.Message), '\n');
		}
		if (!string.IsNullOrEmpty(step.ButtonText))
		{
			Text component = this._panel.transform.Find("Image/Text").GetComponent<Text>();
			component.gameObject.SetActive(true);
			component.font = step.ButtonFont;
			component.text = ((!step.IsButtonTextUppercase) ? ScriptLocalization.Get(step.ButtonText) : ScriptLocalization.Get(step.ButtonText).ToUpper());
			component.fontSize = step.ButtonTextSize;
			component.fontStyle = step.ButtonTextStyle;
			component.resizeTextMaxSize = step.ButtonTextSize;
			RectTransform component2 = component.GetComponent<RectTransform>();
			component2.offsetMin = new Vector2((float)step.Left, 0f);
			component2.offsetMax = new Vector2((float)(-(float)step.Right), 0f);
		}
		else
		{
			this._panel.transform.Find("Image/Text").gameObject.SetActive(false);
		}
		this._panel.GetComponent<CanvasGroup>().alpha = 0f;
		this._currentFullShowTime = 0f;
		this._fullMessageTimer = 0f;
		this._newStep = step;
		this._isShowing = true;
		this._isShortMessageShow = false;
		this.RecordStep(step.Name + "_Full");
	}

	private void ShowShortPanel(TutorialStep step)
	{
		this._isShortMessageShow = true;
		this.HidePanel(this._panel);
		float num = 1f;
		if (this._newStep.ShortMessagePosition == ShortMessagePosition.Top)
		{
			this.ShortPanelPrefab.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
			this.ShortPanelPrefab.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
			this.ShortPanelPrefab.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
		}
		if (this._newStep.ShortMessagePosition == ShortMessagePosition.Bottom)
		{
			this.ShortPanelPrefab.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0f);
			this.ShortPanelPrefab.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0f);
			this.ShortPanelPrefab.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
		}
		if (string.IsNullOrEmpty(step.ShortMessage))
		{
			return;
		}
		if (PhotonConnectionFactory.Instance.IsAuthenticated && (PhotonConnectionFactory.Instance.IsConnectedToGameServer || PhotonConnectionFactory.Instance.IsConnectedToMaster))
		{
			this.RecordStep(step.Name + "_Short");
		}
		this._panel = GUITools.AddChild(this.TutorialPanel, this.ShortPanelPrefab);
		this._panel.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, num, 0f);
		if (step.ShortControlIcon != InputControlType.None && InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			if (step.UseIconSmallMessage)
			{
				this._panel.transform.Find("Image").gameObject.SetActive(false);
				this._panel.transform.Find("Icon").gameObject.SetActive(true);
				this._panel.transform.Find("Icon").GetComponent<Text>().text = HotkeyIcons.KeyMappings[step.ShortControlIcon];
			}
			else
			{
				this._panel.transform.Find("Image").gameObject.SetActive(true);
				this._panel.transform.Find("Icon").gameObject.SetActive(false);
				this._panel.transform.Find("Image").GetComponent<Image>().overrideSprite = step.Image;
			}
			if (step.AppendIconToSmallMessage)
			{
				this._panel.transform.Find("Text").GetComponent<Text>().text = string.Format(ScriptLocalization.Get(step.ShortMessage + "Gamepad"), '\n', HotkeyIcons.KeyMappings[step.ShortControlIcon]);
			}
			else
			{
				this._panel.transform.Find("Text").GetComponent<Text>().text = string.Format(ScriptLocalization.Get(step.ShortMessage), '\n');
			}
		}
		else
		{
			this._panel.transform.Find("Image").gameObject.SetActive(true);
			this._panel.transform.Find("Icon").gameObject.SetActive(false);
			this._panel.transform.Find("Image").GetComponent<Image>().overrideSprite = step.Image;
			this._panel.transform.Find("Text").GetComponent<Text>().text = string.Format(ScriptLocalization.Get(step.ShortMessage), '\n');
		}
		if (!string.IsNullOrEmpty(step.ButtonText))
		{
			Text component = this._panel.transform.Find("Image/Text").GetComponent<Text>();
			component.gameObject.SetActive(true);
			component.font = step.ButtonFont;
			component.text = ((!step.IsButtonTextUppercase) ? ScriptLocalization.Get(step.ButtonText) : ScriptLocalization.Get(step.ButtonText).ToUpper());
			component.fontSize = step.ButtonTextSize;
			component.fontStyle = step.ButtonTextStyle;
			component.resizeTextMaxSize = (int)((float)step.ButtonTextSize * 0.6f);
			RectTransform component2 = component.GetComponent<RectTransform>();
			component2.offsetMin = new Vector2((float)step.Left, 0f);
			component2.offsetMax = new Vector2((float)(-(float)step.Right), 0f);
		}
		else
		{
			this._panel.transform.Find("Image/Text").gameObject.SetActive(false);
		}
		this._panel.GetComponent<CanvasGroup>().alpha = 0f;
		this._currentFullShowTime = 0f;
		this._isShowing = true;
	}

	private void HidePanel(GameObject panel)
	{
		if (!this._isHiding && panel != null)
		{
			this._currentFullHideTime = 0f;
			this._hidePanel = panel;
			this._isHiding = true;
		}
	}

	private static PondHelpers _pondHelpers = new PondHelpers();

	private IList<TutorialStep> _sceneTutorialSteps = new List<TutorialStep>();

	private const int MinStepForBackToLobby = 23;

	private static string _currentStep;

	public static bool CurrentStepFinished = false;

	public static int FishCatchedCount = 0;

	public GameObject PanelPrefab;

	public GameObject ShortPanelPrefab;

	public GameObject TutorialPanel;

	[SerializeField]
	private GameObject[] _tutorialObjects = new GameObject[0];

	private GameObject _panel;

	private bool _isShowing;

	private bool _isHiding;

	private TutorialStep _newStep;

	private GameObject _hidePanel;

	private float _hideFullFadeTime = 1f;

	private float _currentFullHideTime;

	private float _showFullFadeTime = 0.5f;

	private float _currentFullShowTime;

	private float _fullMessageTimer;

	private bool _isShortMessageShow;

	public GameObject ExitInGamePanel;

	private bool _wasStarted;

	private static readonly Regex digitRegex = new Regex("\\d+");

	private readonly HashSet<string> passedTutorialSteps = new HashSet<string>();
}
