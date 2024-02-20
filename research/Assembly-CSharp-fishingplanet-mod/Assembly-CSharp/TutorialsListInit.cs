using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class TutorialsListInit : MonoBehaviour
{
	private void OnEnable()
	{
		this.Clear();
		base.GetComponent<ToggleGroup>().allowSwitchOff = true;
		List<TutorialSlideBlock> slides = TutorialSlidesController.Instance.Slides;
		float num = (float)(this.ContentPanel.GetComponent<VerticalLayoutGroup>().padding.top + this.ContentPanel.GetComponent<VerticalLayoutGroup>().padding.bottom) + this.ContentPanel.GetComponent<VerticalLayoutGroup>().spacing * (float)(slides.Count - 1) + this.ListItemPrefab.GetComponent<LayoutElement>().preferredHeight * (float)slides.Count;
		if (this.navigation == null)
		{
			this.navigation = base.GetComponent<UINavigation>();
		}
		foreach (TutorialSlideBlock tutorialSlideBlock in slides)
		{
			this.AddToList(tutorialSlideBlock);
		}
		this.SetSizeForContent(num);
		this._firstInit.GetComponent<Toggle>().isOn = true;
		this._firstInit.GetComponent<SlideUnitGameObject>().OnSelected();
	}

	private void AddToList(TutorialSlideBlock slideBlock)
	{
		GameObject parent;
		if (!this.categories.TryGetValue(slideBlock.Category, out parent))
		{
			parent = GUITools.AddChild(this.ContentPanel, this.ListItemRoot);
			parent.transform.Find("Label").GetComponent<Text>().text = ScriptLocalization.Get(slideBlock.Category.ToString() + "Category");
			this.categories.Add(slideBlock.Category, parent);
			parent.GetComponentInChildren<Button>().onClick.AddListener(delegate
			{
				bool activeInHierarchy = parent.transform.Find("Openned").gameObject.activeInHierarchy;
				parent.transform.Find("Openned").gameObject.SetActive(!activeInHierarchy);
				parent.transform.Find("Closed").gameObject.SetActive(activeInHierarchy);
				IEnumerator enumerator = parent.transform.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform = (Transform)obj;
						if (transform.GetComponent<LayoutElement>() == null || !transform.GetComponent<LayoutElement>().ignoreLayout)
						{
							transform.gameObject.SetActive(activeInHierarchy);
						}
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
				this.navigation.ForceUpdate();
			});
		}
		GameObject gameObject = GUITools.AddChild(parent, this.ListItemPrefab);
		gameObject.GetComponent<Toggle>().group = base.GetComponent<ToggleGroup>();
		gameObject.GetComponent<Toggle>().isOn = false;
		gameObject.transform.Find("Label").GetComponent<Text>().text = ScriptLocalization.Get(slideBlock.Name);
		gameObject.AddComponent<SlideUnitGameObject>();
		gameObject.GetComponent<SlideUnitGameObject>().ConcreteSlideBlock = slideBlock;
		gameObject.GetComponent<SlideUnitGameObject>().OnClickSelected += this.TutorialsListInit_OnClickSelected;
		gameObject.SetActive(false);
		if (this._firstInit == null)
		{
			this._firstInit = gameObject.GetComponent<Toggle>();
		}
	}

	private void TutorialsListInit_OnClickSelected(object sender, ConcreteSlideEventArgs e)
	{
		this._currentSlideBlock = e.ConcreteSlideBlock;
		this.StartSlideShow(this._currentSlideBlock);
	}

	private void StartSlideShow(TutorialSlideBlock activeSlideBlock)
	{
		this.SlidePanel.RunSlideShow(activeSlideBlock);
	}

	private void SetSizeForContent(float height)
	{
		RectTransform component = this.ContentPanel.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(component.sizeDelta.x, Mathf.Max(height, this.ContentPanel.transform.parent.GetComponent<RectTransform>().rect.height));
		component.anchoredPosition = new Vector3(0f, 0f - height / 2f, 0f);
		GameObject gameObject = this.ContentPanel.transform.parent.Find("Scrollbar").gameObject;
		if (component.sizeDelta.y > this.ContentPanel.transform.parent.GetComponent<RectTransform>().rect.height)
		{
			gameObject.SetActive(true);
			gameObject.GetComponent<Scrollbar>().value = 1f;
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	private void Clear()
	{
		this.categories = new Dictionary<SlideCategories, GameObject>();
		for (int i = 0; i < this.ContentPanel.transform.childCount; i++)
		{
			Object.Destroy(this.ContentPanel.transform.GetChild(i).gameObject);
		}
	}

	public GameObject ListItemPrefab;

	public GameObject ListItemRoot;

	public GameObject ContentPanel;

	public SlideShowControllerInUI SlidePanel;

	private Dictionary<SlideCategories, GameObject> categories = new Dictionary<SlideCategories, GameObject>();

	private TutorialSlideBlock _currentSlideBlock;

	private Toggle _firstInit;

	private UINavigation navigation;
}
