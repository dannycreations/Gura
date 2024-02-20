using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainPageHandler<T> : ActivityStateControlled where T : struct, IConvertible, IComparable, IFormattable
{
	protected override void OnDestroy()
	{
		base.OnDestroy();
		base.StopAllCoroutines();
	}

	protected override void HideHelp()
	{
		base.HideHelp();
		foreach (KeyValuePair<T, MainPageItem> keyValuePair in this.CategoriesGo)
		{
			if (keyValuePair.Value != null)
			{
				keyValuePair.Value.SetParentActive(false);
			}
		}
	}

	protected override void SetHelp()
	{
		this.SetHelpActivityState();
		if (this.CategoriesGo.Count == 0)
		{
			this.InitMainData();
			this.InitUi();
			this.UpdateCategory();
			this.ResetMainMenuNavigation();
		}
		foreach (KeyValuePair<T, MainPageItem> keyValuePair in this.CategoriesGo)
		{
			if (keyValuePair.Value != null)
			{
				keyValuePair.Value.SetParentActive(true);
			}
		}
	}

	public virtual void OnClickCategory(int category)
	{
		if (Enum.IsDefined(typeof(T), category))
		{
			T t = (T)((object)category);
			if (!t.Equals(this.Category))
			{
				this.SetCategory(t);
				this.UpdateCategory();
			}
		}
	}

	protected virtual void SetCategory(T newCategory)
	{
		this.CategoryPrev = this.Category;
		this.Category = newCategory;
	}

	protected virtual void SetHelpActivityState()
	{
		base.SetHelp();
	}

	protected virtual void InitMainData()
	{
	}

	protected virtual void InitUi()
	{
	}

	protected virtual void UpdateCategory()
	{
		this.UpdateHeaders(this.Category);
		this.SetActiveCategory(this.CategoriesGo);
	}

	protected virtual void SetActiveCategory(Dictionary<T, MainPageItem> categoriesGo)
	{
		foreach (KeyValuePair<T, MainPageItem> keyValuePair in categoriesGo)
		{
			if (keyValuePair.Value != null)
			{
				MainPageItem value = keyValuePair.Value;
				T key = keyValuePair.Key;
				value.SetActive(key.Equals(this.Category));
			}
		}
	}

	protected virtual void UpdateHeaders(T cat)
	{
		if (this.Cats == null)
		{
			this.Cats = Enum.GetValues(typeof(T)).Cast<T>().ToList<T>();
		}
		List<T> list = this.CatsSkippedAnim();
		bool flag = !list.Contains(cat) && !list.Contains(this.CategoryPrev);
		for (int i = 0; i < this.Cats.Count; i++)
		{
			T t = this.Cats[i];
			if (this.HeadersSelected.ContainsKey(t))
			{
				bool isEq = t.Equals(cat);
				this.HeadersSelected[t].ForEach(delegate(GameObject p)
				{
					p.SetActive(isEq);
				});
				this.HeadersActive[t].ForEach(delegate(GameObject p)
				{
					p.SetActive(!isEq);
				});
			}
		}
		if (this.ImSelected != null)
		{
			this.ImSelected.gameObject.SetActive(flag);
			if (flag && this.Headers.ContainsKey(cat))
			{
				base.StartCoroutine(this.ImSelectedPlayAnim(cat));
			}
		}
	}

	protected IEnumerator ImSelectedPlayAnim(T cat)
	{
		yield return new WaitForEndOfFrame();
		RectTransform header = this.Headers[cat];
		ShortcutExtensions.DOKill(this.ImSelected, true);
		ShortcutExtensions.DOAnchorPos(this.ImSelected, new Vector2(header.anchoredPosition.x, this.ImSelected.anchoredPosition.y), 0.4f, false);
		MenuWidthSet mws = header.GetComponent<MenuWidthSet>();
		ShortcutExtensions.DOSizeDelta(this.ImSelected, new Vector2(mws.PreferredWidth, this.ImSelected.rect.height), 0.4f, false);
		yield break;
	}

	protected virtual List<T> CatsSkippedAnim()
	{
		return new List<T>();
	}

	protected virtual void CreateHeaderData(Toggle t, T c)
	{
		if (t == null)
		{
			return;
		}
		Transform transform = t.transform;
		this.Headers[c] = transform as RectTransform;
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			Dictionary<T, List<GameObject>> dictionary = ((!child.name.EndsWith("Selected")) ? ((!child.name.EndsWith("Active")) ? null : this.HeadersActive) : this.HeadersSelected);
			if (dictionary != null)
			{
				if (!dictionary.ContainsKey(c))
				{
					dictionary[c] = new List<GameObject>();
				}
				dictionary[c].Add(child.gameObject);
			}
		}
	}

	protected virtual void InitHeaderToggleClickLogic(Toggle toggle, T c)
	{
		toggle.onValueChanged.RemoveAllListeners();
		if (c.Equals(this.Category))
		{
			toggle.isOn = true;
		}
		toggle.group = this.HeaderToggleGroup;
		toggle.onValueChanged.AddListener(delegate(bool isOn)
		{
			if (!isOn)
			{
				return;
			}
			this.OnClickCategory(c.ToInt32(CultureInfo.InvariantCulture));
			this.HeaderPlayButtonEffect.OnSubmit();
		});
	}

	protected virtual void ResetMainMenuNavigation()
	{
	}

	[SerializeField]
	protected Transform HeaderMenu;

	[SerializeField]
	protected RectTransform ImSelected;

	[SerializeField]
	protected ToggleGroup HeaderToggleGroup;

	[SerializeField]
	protected PlayButtonEffect HeaderPlayButtonEffect;

	protected readonly Dictionary<T, MainPageItem> CategoriesGo = new Dictionary<T, MainPageItem>();

	protected readonly Dictionary<T, List<GameObject>> HeadersActive = new Dictionary<T, List<GameObject>>();

	protected readonly Dictionary<T, List<GameObject>> HeadersSelected = new Dictionary<T, List<GameObject>>();

	protected readonly Dictionary<T, RectTransform> Headers = new Dictionary<T, RectTransform>();

	protected T CategoryPrev;

	protected T Category;

	protected List<T> Cats;

	protected const float AnimTime = 0.4f;
}
