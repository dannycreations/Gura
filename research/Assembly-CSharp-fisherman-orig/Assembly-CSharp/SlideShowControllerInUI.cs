using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using InControl;
using UnityEngine;
using UnityEngine.UI;

public class SlideShowControllerInUI : MonoBehaviour
{
	private void OnDisable()
	{
		this._loadingCoroutines.Values.ToList<Coroutine>().ForEach(new Action<Coroutine>(base.StopCoroutine));
		this._loadingCoroutines.Clear();
		this._currentSlideBlock = null;
		if (this._currentLoadedBundle != null)
		{
			this._currentLoadedBundle.Unload(true);
		}
	}

	public void RunSlideShow(TutorialSlideBlock activeSlideBlock)
	{
		if (this._currentSlideBlock != null && activeSlideBlock.ID == this._currentSlideBlock.ID)
		{
			return;
		}
		this._isChangedSlide = true;
		if (this._currentSlideBlock != null)
		{
			if (this.NextSlide != null)
			{
				this.DestroySlide(this.NextSlide);
			}
			if (this.CurrentSlide != null)
			{
				AlphaFade component = this.CurrentSlide.GetComponent<AlphaFade>();
				component.HideFinished += this.SlideShowControllerInUI_HideFinished;
				component.HidePanel();
			}
		}
		this._isShowing = true;
		this._currentSlideBlock = activeSlideBlock;
		this._currentSlideId = 0;
		if (this._currentLoadedBundle == null || this._currentLoadedBundle.name != this._currentSlideBlock.ID.ToString())
		{
			if (this._currentLoadedBundle != null)
			{
				this._currentLoadedBundle.Unload(true);
			}
			InfoMessageController.Instance.StartCoroutine(AssetBundleManager.LoadAssetBundleFromDisk(this._currentSlideBlock.ID.ToString(), new Action<AssetBundle>(this.ShowSlide)));
		}
		else
		{
			this.ShowSlide(this._currentLoadedBundle);
		}
	}

	private void SlideShowControllerInUI_HideFinished(object sender, EventArgsAlphaFade e)
	{
		this.DestroySlide(e.gameObject);
	}

	private GameObject CreateSlide(TutorialSlide slide)
	{
		GameObject slideInstance = GUITools.AddChild(this.SlidePlace, this.SlidePrefab);
		slideInstance.name = Guid.NewGuid().ToString();
		slideInstance.GetComponent<AlphaFade>().FastHidePanel();
		string text = slide.PrimaryImage.path;
		string text2 = text;
		this.LocalizePath(slide, ref text, TutorialSlideReferences.LocalizationAdditions);
		this.PlatformPathAdd(slide, ref text);
		if (!this._currentLoadedBundle.Contains(text))
		{
			text = text2;
			this.LocalizePath(slide, ref text, TutorialSlideReferences.LocalizationAdditionsFallback);
			this.PlatformPathAdd(slide, ref text);
		}
		this._loadingCoroutines[slideInstance.name] = base.StartCoroutine(AssetBundleManager.LoadFromExisting(this._currentLoadedBundle, text, delegate(Object ret)
		{
			if (this._loadingCoroutines.ContainsKey(slideInstance.name))
			{
				this._loadingCoroutines.Remove(slideInstance.name);
			}
			slideInstance.GetComponent<Image>().overrideSprite = ret as Sprite;
			RectTransform component2 = slideInstance.GetComponent<RectTransform>();
			component2.anchoredPosition3D = Vector3.zero;
			component2.sizeDelta = Vector2.zero;
		}));
		List<string> list = new List<string>();
		list.AddRange(slide.PrimaryText.KeyInputType.Select((Key p) => p.ToString()));
		list.AddRange(slide.PrimaryText.MouseInputType.Select((Mouse p) => ScriptLocalization.Get(HotkeyIcons.MouseMappings[p])));
		Text component = slideInstance.transform.Find("Text").GetComponent<Text>();
		try
		{
			component.text = string.Format(ScriptLocalization.Get(slide.PrimaryText.text).Replace("<br>", "\n"), list.ToArray());
		}
		catch (FormatException ex)
		{
			LogHelper.Error("___kocha SlideShowControllerInUI:CreateSlide - FormatException for :{0} array.Count:{1}", new object[]
			{
				slide.PrimaryText.text,
				list.Count
			});
		}
		return slideInstance;
	}

	private void ShowSlide(AssetBundle currentBundle)
	{
		if (!base.isActiveAndEnabled || currentBundle.name != this._currentSlideBlock.ID.ToString())
		{
			currentBundle.Unload(true);
			return;
		}
		this._isChangedSlide = true;
		this._currentLoadedBundle = currentBundle;
		this.NextSlide = this.CreateSlide(this._currentSlideBlock.Slides[this._currentSlideId]);
		this.NextSlide.GetComponent<AlphaFade>().ShowFinished += this.SlideShowController_HideFinished;
		if (this.CurrentSlide != null)
		{
			this.CurrentSlide.GetComponent<AlphaFade>().HidePanel();
		}
		this.NextSlide.GetComponent<AlphaFade>().ShowPanel();
	}

	private void SlideShowController_HideFinished(object sender, EventArgs e)
	{
		this.NextSlide.GetComponent<AlphaFade>().ShowFinished -= this.SlideShowController_HideFinished;
		if (this.CurrentSlide != null)
		{
			this.DestroySlide(this.CurrentSlide);
		}
		this.CurrentSlide = this.NextSlide;
		this.NextButton.GetComponent<Button>().interactable = this._currentSlideId < this._currentSlideBlock.Slides.Count - 1;
		this.PreviousButton.GetComponent<Button>().interactable = this._currentSlideId > 0;
		this._isChangedSlide = false;
	}

	private void ShowNextSlide()
	{
		this._currentSlideId++;
		this.ShowSlide(this._currentLoadedBundle);
	}

	public void NextButtonAction()
	{
		if (!this._isChangedSlide && this._currentSlideId < this._currentSlideBlock.Slides.Count - 1)
		{
			this.ShowNextSlide();
		}
	}

	public void PreviusButtonAction()
	{
		if (!this._isChangedSlide && this._currentSlideId > 0)
		{
			this.PreviousNextSlide();
		}
	}

	private void PreviousNextSlide()
	{
		this._currentSlideId--;
		this.ShowSlide(this._currentLoadedBundle);
	}

	private void DestroySlide(GameObject slide)
	{
		if (this._loadingCoroutines.ContainsKey(slide.name))
		{
			base.StopCoroutine(this._loadingCoroutines[slide.name]);
			this._loadingCoroutines.Remove(slide.name);
		}
		Object.Destroy(slide);
	}

	private void PlatformPathAdd(TutorialSlide slide, ref string pathToSprite)
	{
		if (slide.PrimaryImage.platformDependent)
		{
			pathToSprite += "_pc";
		}
	}

	private void LocalizePath(TutorialSlide slide, ref string pathToSprite, Dictionary<int, string> localization)
	{
		if (slide.PrimaryImage.localized)
		{
			int num = pathToSprite.IndexOf('_') + 1;
			pathToSprite = pathToSprite.Insert((num >= 0) ? num : 0, localization[ChangeLanguage.GetCurrentLanguage.Id]);
		}
	}

	public GameObject SlidePlace;

	public GameObject SlidePrefab;

	public Button NextButton;

	public Button PreviousButton;

	private TutorialSlideBlock _currentSlideBlock;

	private int _currentSlideId;

	private GameObject CurrentSlide;

	private GameObject NextSlide;

	private bool _lockMouse;

	private bool _isShowing;

	private bool _isChangedSlide;

	private AssetBundle _currentLoadedBundle;

	private readonly Dictionary<string, Coroutine> _loadingCoroutines = new Dictionary<string, Coroutine>();
}
