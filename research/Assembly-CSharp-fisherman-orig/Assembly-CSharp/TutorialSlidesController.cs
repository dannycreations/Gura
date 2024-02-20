using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using DG.Tweening;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSlidesController : MonoBehaviour
{
	public List<TutorialSlideBlock> Slides
	{
		get
		{
			return (!(this._slidesContainerInit == null)) ? this._slidesContainerInit.TutorialSlides : null;
		}
	}

	internal void Start()
	{
		if (TutorialSlidesController.Instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		TutorialSlidesController.Instance = this;
		this._sequence = DOTween.Sequence();
		this._textSequence = DOTween.Sequence();
		Object.DontDestroyOnLoad(this);
		this._slidesContainerInit = (TutorialSlidesContainer)Resources.Load("Tutorial/SlidesContainer", typeof(TutorialSlidesContainer));
		this._slidesContainer = Object.Instantiate<TutorialSlidesContainer>(this._slidesContainerInit);
		for (int i = 0; i < this._slidesContainer.TutorialSlides.Count; i++)
		{
			this._slidesContainer.TutorialSlides[i].SetUpTriggers();
		}
		base.GetComponent<AlphaFade>().FastHidePanel();
		base.GetComponent<AlphaFade>().OnHide.AddListener(delegate
		{
			this.SlidePanel.SetActive(false);
		});
		this.SlidePanel.SetActive(false);
	}

	internal void Update()
	{
		if (!DashboardTabSetter.IsTutorialSlidesEnabled)
		{
			return;
		}
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile != null && profile.Settings != null)
		{
			bool flag = profile.Settings.ContainsKey("SlideTutorial");
			if (!flag)
			{
				this._isInit = false;
			}
			if (!this._isInit)
			{
				List<int> list = new List<int>();
				if (flag)
				{
					string text = profile.Settings["SlideTutorial"];
					if (!string.IsNullOrEmpty(text))
					{
						list = text.Split(new char[] { ',' }).Select(new Func<string, int>(Convert.ToInt32)).ToList<int>();
					}
					else
					{
						PhotonConnectionFactory.Instance.PinError(new Exception("profile settings SlideTutorial null!!!!"));
						profile.Settings["SlideTutorial"] = "0";
					}
				}
				else
				{
					profile.Settings.Add("SlideTutorial", "0");
				}
				this.DeleteDoneSlideUnites(list);
				this._isInit = true;
			}
		}
		if (SettingsManager.ShowSlides && !this.IsShowing && this._slidesContainer.TutorialSlides.Count > 0 && (StaticUserData.CurrentPond == null || StaticUserData.CurrentPond.PondId != 2) && InfoMessageController.Instance.currentMessage == null)
		{
			MenuPrefabsList menuPrefabsList = this._menuHelpers.MenuPrefabsList;
			if (menuPrefabsList != null && !menuPrefabsList.IsLoadingFormActive && !menuPrefabsList.IsTravelingFormActive && !menuPrefabsList.IsStartFormActive)
			{
				this.UpdateTriggers();
				this.CheckShoudStart();
			}
		}
	}

	private void InitSlidesResources(AssetBundle ab)
	{
		TutorialSlidesController.TutorialSlidesBundle = ab;
		this.ShowSlides.isOn = true;
		this.SlidePanel.SetActive(true);
		this.Title.text = ScriptLocalization.Get(this._currentSlideBlock.Name).ToUpper();
		IEnumerator enumerator = this.Content.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				Object.Destroy(transform.gameObject);
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
		TutorialSlideReferences tutorialSlideReferences = Object.Instantiate<TutorialSlideReferences>(this.Slide);
		tutorialSlideReferences.transform.parent = this.Content;
		tutorialSlideReferences.transform.localScale = Vector3.one;
		for (int i = 0; i < this._currentSlideBlock.Slides.Count; i++)
		{
			tutorialSlideReferences = Object.Instantiate<TutorialSlideReferences>(this.Slide);
			tutorialSlideReferences.InitSlide(this._currentSlideBlock.Slides[i], false);
			tutorialSlideReferences.transform.parent = this.Content;
			tutorialSlideReferences.transform.localScale = Vector3.one;
		}
		tutorialSlideReferences = Object.Instantiate<TutorialSlideReferences>(this.Slide);
		tutorialSlideReferences.transform.parent = this.Content;
		tutorialSlideReferences.transform.localScale = Vector3.one;
		this._currentSlide = 0;
		this.SetMessage(false);
		this._slideCount = this._currentSlideBlock.Slides.Count - 1;
		this.NextSlide.interactable = this._currentSlide < this._slideCount;
		this.PreviousSlide.interactable = this._currentSlide > 0;
		this.SetColor();
		base.GetComponent<AlphaFade>().ShowPanel();
		if (this._slideCount > 0)
		{
			this.Content.GetComponent<RectTransform>().anchoredPosition = new Vector2((float)(this._slideCount * this._slideWidth / 2), this.Content.GetComponent<RectTransform>().anchoredPosition.y);
		}
		this._position = this.Content.GetComponent<RectTransform>().anchoredPosition;
		ControlsController.ControlsActions.BlockAxis();
		CursorManager.ShowCursor();
	}

	private void SetColor()
	{
		for (int i = 0; i < this.Next.Length; i++)
		{
			this.Next[i].color = ((this._currentSlide >= this._slideCount) ? this.gray : Color.white);
		}
		for (int j = 0; j < this.Prev.Length; j++)
		{
			this.Prev[j].color = ((this._currentSlide <= 0) ? this.gray : Color.white);
		}
	}

	private void UpdateTriggers()
	{
		for (int i = 0; i < this._slidesContainer.TutorialSlides.Count; i++)
		{
			TutorialSlideBlock tutorialSlideBlock = this._slidesContainer.TutorialSlides[i];
			for (int j = 0; j < tutorialSlideBlock.Triggers.Count; j++)
			{
				tutorialSlideBlock.Triggers[j].Update();
			}
		}
	}

	private void CheckShoudStart()
	{
		if (InfoMessageController.Instance.currentMessage != null || MessageBoxList.Instance.currentMessage != null || MessageFactory.InfoMessagesQueue.Count != 0 || MessageFactory.MessageBoxQueue.Count != 0)
		{
			return;
		}
		for (int i = 0; i < this._slidesContainer.TutorialSlides.Count; i++)
		{
			TutorialSlideBlock tutorialSlideBlock = this._slidesContainer.TutorialSlides[i];
			int j;
			for (j = 0; j < tutorialSlideBlock.Triggers.Count; j++)
			{
				if (!tutorialSlideBlock.Triggers[j].IsTriggering())
				{
					break;
				}
			}
			if (j == tutorialSlideBlock.Triggers.Count)
			{
				this.StartSlideShow(tutorialSlideBlock);
				return;
			}
		}
	}

	private void StartSlideShow(TutorialSlideBlock activeSlide)
	{
		this._currentSlideBlock = activeSlide;
		this.IsShowing = true;
		base.StartCoroutine(AssetBundleManager.LoadAssetBundleFromDisk(activeSlide.ID.ToString(), new Action<AssetBundle>(this.InitSlidesResources)));
	}

	public void ShowNextSlide()
	{
		this._position -= new Vector2((float)this._slideWidth, 0f);
		TweenSettingsExtensions.Append(this._sequence, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this.Content.GetComponent<RectTransform>(), this._position, this._duration, false), 4));
		this._currentSlide++;
		this.SetMessage(true);
		this.SetColor();
		this.NextSlide.interactable = this._currentSlide < this._slideCount;
		this.PreviousSlide.interactable = this._currentSlide > 0;
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.SlideClip, SettingsManager.InterfaceVolume);
	}

	public void ShowPrevSlide()
	{
		this._position += new Vector2((float)this._slideWidth, 0f);
		TweenSettingsExtensions.Append(this._sequence, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this.Content.GetComponent<RectTransform>(), this._position, this._duration, false), 4));
		this._currentSlide--;
		this.SetMessage(true);
		this.SetColor();
		this.NextSlide.interactable = this._currentSlide < this._slideCount;
		this.PreviousSlide.interactable = this._currentSlide > 0;
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.SlideClip, SettingsManager.InterfaceVolume);
	}

	private void SetMessage(bool useTweener)
	{
		string key = this._currentSlideBlock.Slides[this._currentSlide].PrimaryText.text;
		List<string> array = new List<string>();
		for (int i = 0; i < this._currentSlideBlock.Slides[this._currentSlide].PrimaryText.KeyInputType.Length; i++)
		{
			array.Add(this._currentSlideBlock.Slides[this._currentSlide].PrimaryText.KeyInputType[i].ToString());
		}
		for (int j = 0; j < this._currentSlideBlock.Slides[this._currentSlide].PrimaryText.MouseInputType.Length; j++)
		{
			array.Add(ScriptLocalization.Get(HotkeyIcons.MouseMappings[this._currentSlideBlock.Slides[this._currentSlide].PrimaryText.MouseInputType[j]]));
		}
		if (useTweener)
		{
			TweenExtensions.Kill(this._textSequence, false);
			TweenSettingsExtensions.Append(this._textSequence, TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOFade(this.Message.GetComponent<CanvasGroup>(), 0f, this._duration), delegate
			{
				if (!string.IsNullOrEmpty(key))
				{
					this.Message.text = string.Format(ScriptLocalization.Get(key).Replace("<br>", "\n"), array.ToArray());
				}
			}));
			TweenSettingsExtensions.Append(this._textSequence, TweenSettingsExtensions.SetDelay<Tweener>(ShortcutExtensions.DOFade(this.Message.GetComponent<CanvasGroup>(), 1f, this._duration * 4f), this._duration));
		}
		else if (!string.IsNullOrEmpty(key))
		{
			this.Message.text = string.Format(ScriptLocalization.Get(key).Replace("<br>", "\n"), array.ToArray());
		}
	}

	private void DeleteDoneSlideUnites(List<int> doneList)
	{
		this._slidesContainer.TutorialSlides.RemoveAll((TutorialSlideBlock slide) => doneList.Contains(slide.ID));
	}

	public void CloseSlideShow()
	{
		if (this._currentSlideBlock != null)
		{
			Dictionary<string, string> settings;
			(settings = PhotonConnectionFactory.Instance.Profile.Settings)["SlideTutorial"] = settings["SlideTutorial"] + string.Format(",{0}", this._currentSlideBlock.ID);
			this._slidesContainer.TutorialSlides.Remove(this._currentSlideBlock);
			base.StartCoroutine("SaveProfileSettings");
		}
		base.GetComponent<AlphaFade>().HidePanel();
		if (!this.ShowSlides.isOn)
		{
			SettingsManager.ShowSlides = false;
		}
		this.IsShowing = false;
		this.Content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, this.Content.GetComponent<RectTransform>().anchoredPosition.y);
		if (TutorialSlidesController.TutorialSlidesBundle != null)
		{
			TutorialSlidesController.TutorialSlidesBundle.Unload(true);
		}
		ControlsController.ControlsActions.UnBlockAxis();
		CursorManager.HideCursor();
	}

	public void ChangeToggle()
	{
		this.ShowSlides.isOn = !this.ShowSlides.isOn;
	}

	internal void SaveProfileSettings()
	{
		PhotonConnectionFactory.Instance.UpdateProfileSettings(PhotonConnectionFactory.Instance.Profile.Settings);
	}

	private void OnDestroy()
	{
		if (TutorialSlidesController.TutorialSlidesBundle != null)
		{
			TutorialSlidesController.TutorialSlidesBundle.Unload(true);
		}
	}

	public static int FishCounter;

	private TutorialSlidesContainer _slidesContainer;

	private TutorialSlidesContainer _slidesContainerInit;

	private bool _isInit;

	public bool IsShowing;

	private TutorialSlideBlock _currentSlideBlock;

	public GameObject SlidePanel;

	public TutorialSlideReferences Slide;

	public Transform Content;

	public Text Title;

	public Text Message;

	public Text[] Next;

	public Text[] Prev;

	public Toggle ShowSlides;

	public static TutorialSlidesController Instance;

	public static AssetBundle TutorialSlidesBundle;

	public Button PreviousSlide;

	public Button NextSlide;

	private int _currentSlide;

	private int _slideCount;

	private int _slideWidth = 1000;

	private float _duration = 0.5f;

	private Color gray = new Color(0.3019608f, 0.3019608f, 0.3019608f);

	private Vector2 _position;

	private Sequence _sequence;

	private Sequence _textSequence;

	private MenuHelpers _menuHelpers = new MenuHelpers();

	private const string SlidesKey = "SlideTutorial";
}
