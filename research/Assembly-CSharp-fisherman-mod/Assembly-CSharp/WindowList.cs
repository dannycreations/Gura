using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class WindowList : WindowBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<int> OnSelected = delegate(int i)
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<List<int>> OnMultiSelected = delegate(List<int> i)
	{
	};

	public void Init(WindowList.WindowListContainer c)
	{
		this.Data = c.Data;
		this.FillView(c);
	}

	protected void FillView(WindowList.WindowListContainerBase c)
	{
		if (this.Title != null)
		{
			this.Title.text = c.Title;
		}
		if (this.DataTitle != null)
		{
			this.DataTitle.text = c.DataTitle;
		}
		if (this.DescTitle != null)
		{
			this.DescTitle.text = c.DescTitle;
		}
		this.SelectedIndex = c.Index;
		this.IsMultiSelect = c.Indexes != null;
		if (this.IsMultiSelect)
		{
			for (int i = 0; i < c.Indexes.Count; i++)
			{
				c.Indexes[i] = c.Indexes[i] + 1;
			}
			if (c.Indexes.Count == this.Data.Count)
			{
				c.Indexes.Add(0);
			}
			this.Data.Insert(0, new WindowList.WindowListElem
			{
				Name = ScriptLocalization.Get("AllCaption"),
				Desc = (string.IsNullOrEmpty(c.AllDescCaption) ? this.Data[0].Desc : c.AllDescCaption)
			});
		}
		this.Indexes = c.Indexes;
		WindowList.CreateList(this.Data, this.ItemsRoot, this.ItemPrefab, this.Items, this.ItemsRoot.GetComponent<ToggleGroup>(), this.SelectedIndex, new Action<int>(this.SetActiveItem), new Action<int>(this.SetActiveItem), c.Indexes, new Action(this.OnDblClick));
		if (this.DescValue != null)
		{
			this.DescValue.text = this.Data[this.SelectedIndex].Desc;
		}
		this.UpdateImg(this.SelectedIndex);
		this.ScrollToSelectedItem();
	}

	public static void CreateList(List<WindowList.WindowListElem> data, GameObject itemsRoot, GameObject itemPrefab, List<IWindowListItem> items, ToggleGroup tg, int index, Action<int> onSelect, Action<int> onOk, List<int> indexes = null, Action onAccept = null)
	{
		int selectedIndex = index;
		bool isMultiSelect = indexes != null;
		for (int i = 0; i < data.Count; i++)
		{
			GameObject gameObject = GUITools.AddChild(itemsRoot, itemPrefab);
			IWindowListItem component = gameObject.GetComponent<IWindowListItem>();
			component.Init(data[i].Name, tg, data[i].Interactable, data[i].RadioId);
			if (isMultiSelect)
			{
				component.SetToggle(indexes.Contains(i));
				component.SetActive(indexes.Contains(i));
			}
			else
			{
				component.SetToggle(i == index);
				component.SetActive(i == index);
			}
			int i1 = i;
			component.OnSelect += delegate
			{
				if (!isMultiSelect || SettingsManager.InputType == InputModuleManager.InputType.Mouse)
				{
					onSelect(i1);
				}
				if (!isMultiSelect && SettingsManager.InputType == InputModuleManager.InputType.Mouse)
				{
					if (WindowList.CurDblClickTime <= 0.5f && selectedIndex == i1 && onAccept != null)
					{
						onAccept();
					}
					WindowList.CurDblClickTime = 0f;
					selectedIndex = i1;
				}
			};
			component.OnOk += delegate
			{
				onOk(i1);
			};
			items.Add(component);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!this.IsMultiSelect && SettingsManager.InputType == InputModuleManager.InputType.Mouse)
		{
			WindowList.CurDblClickTime += Time.deltaTime;
		}
	}

	protected override void AcceptActionCalled()
	{
		if (this.IsMultiSelect)
		{
			List<int> list = new List<int>();
			List<IWindowListItem> range = this.Items.GetRange(1, this.Items.Count - 1);
			for (int i = 0; i < range.Count; i++)
			{
				if (range[i].IsActive)
				{
					list.Add(i);
				}
			}
			this.OnMultiSelected(list);
		}
		else
		{
			this.OnSelected(this.SelectedIndex);
		}
	}

	protected virtual void OnDblClick()
	{
		this.Accept();
	}

	protected virtual void SetActiveItem(int i)
	{
		if (this.IsMultiSelect)
		{
			bool flag = !this.Items[i].IsActive;
			this.Items[i].SetActive(flag);
			if (i == 0)
			{
				if (!flag)
				{
					if (!this.Items.GetRange(1, this.Items.Count - 1).All((IWindowListItem p) => p.IsActive))
					{
						goto IL_AC;
					}
				}
				for (int j = 1; j < this.Items.Count; j++)
				{
					this.Items[j].SetActive(flag);
				}
				IL_AC:;
			}
			else if (!flag && this.Items[0].IsActive)
			{
				this.Items[0].SetActive(false);
			}
			if (i > 0 && !flag && this.Items[i].RadioId > 0)
			{
				for (int k = 1; k < this.Items.Count; k++)
				{
					if (k != i && this.Items[k].RadioId == this.Items[i].RadioId)
					{
						this.Items[k].SetActive(true);
					}
				}
			}
			if (this.Items.All((IWindowListItem p) => !p.IsActive))
			{
				IWindowListItem windowListItem = this.Items.FirstOrDefault((IWindowListItem p) => p.RadioId > 0);
				if (windowListItem != null)
				{
					windowListItem.SetActive(true);
				}
			}
			this.OkBtn.interactable = this.Items.Any((IWindowListItem p) => p.IsActive);
		}
		else
		{
			this.Items[this.SelectedIndex].SetToggle(false);
			this.Items[this.SelectedIndex].SetActive(false);
			this.Items[i].SetActive(true);
		}
		this.SelectedIndex = i;
		if (this.DescValue != null)
		{
			this.DescValue.text = this.Data[i].Desc;
		}
		this.UpdateImg(i);
	}

	protected virtual void UpdateImg(int i)
	{
		if (this.Img == null)
		{
			return;
		}
		if (this.ImgLoadable.Image != this.Img)
		{
			this.ImgLoadable.Image = this.Img;
		}
		this.Img.gameObject.SetActive(!string.IsNullOrEmpty(this.Data[i].ImgPath));
		if (this.Img.gameObject.activeSelf)
		{
			this.ImgLoadable.Load(this.Data[i].ImgPath);
		}
		this.DescValue.rectTransform.sizeDelta = new Vector2(this.DescValue.rectTransform.rect.width, (!this.Img.gameObject.activeSelf) ? 528f : 274f);
	}

	protected IEnumerator Scrolling(ScrollRect sr, float v)
	{
		yield return new WaitForEndOfFrame();
		sr.verticalNormalizedPosition = v;
		yield break;
	}

	protected void ScrollToSelectedItem()
	{
		if (!this.IsMultiSelect)
		{
			int siblingIndex = this.Items[this.SelectedIndex].GetSiblingIndex();
			float num = (float)siblingIndex / (float)this.Items.Count;
			if (this.Scroll.direction == 2)
			{
				num = 1f - num;
			}
			base.StartCoroutine(this.Scrolling(this.ScrollRect, num));
		}
	}

	protected void Clear()
	{
		this.Items.ForEach(delegate(IWindowListItem p)
		{
			p.Remove();
		});
		this.Items.Clear();
		this.ItemsRoot.transform.DetachChildren();
	}

	[SerializeField]
	protected Image Img;

	protected ResourcesHelpers.AsyncLoadableImage ImgLoadable = new ResourcesHelpers.AsyncLoadableImage();

	[SerializeField]
	protected Text Title;

	[SerializeField]
	protected Text DataTitle;

	[SerializeField]
	protected Text DescTitle;

	[SerializeField]
	protected Text DescValue;

	[SerializeField]
	protected GameObject ItemsRoot;

	[SerializeField]
	protected GameObject ItemPrefab;

	[SerializeField]
	protected Scrollbar Scroll;

	[SerializeField]
	protected ScrollRect ScrollRect;

	protected const float DblClickTime = 0.5f;

	protected static float CurDblClickTime;

	protected int SelectedIndex;

	protected List<IWindowListItem> Items = new List<IWindowListItem>();

	protected List<WindowList.WindowListElem> Data;

	protected bool IsMultiSelect;

	protected List<int> Indexes;

	public class WindowListElem
	{
		public WindowListElem()
		{
			this.Interactable = true;
		}

		public string Name { get; set; }

		public string Desc { get; set; }

		public string ImgPath { get; set; }

		public bool Interactable { get; set; }

		public int RadioId { get; set; }
	}

	public class Titles
	{
		public string Title { get; set; }

		public string DataTitle { get; set; }

		public string DescTitle { get; set; }
	}

	public class WindowListDataGetter<T>
	{
		public Func<T, string> LocName { get; set; }

		public Func<T, string> LocDesc { get; set; }

		public Func<T, string> GetImgPath { get; set; }

		public Func<T, bool> Interactable { get; set; }

		public Func<T, int> RadioId { get; set; }
	}

	public class WindowListContainerBase
	{
		public string Title { get; set; }

		public string DataTitle { get; set; }

		public string DescTitle { get; set; }

		public int Index { get; set; }

		public string AllDescCaption { get; set; }

		public List<int> Indexes { get; set; }
	}

	public class WindowListContainer : WindowList.WindowListContainerBase
	{
		public List<WindowList.WindowListElem> Data { get; set; }
	}
}
