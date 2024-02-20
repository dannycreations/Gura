using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class InfiniteScroll : ScrollRect
{
	protected RectTransform t
	{
		get
		{
			if (this._t == null)
			{
				this._t = base.GetComponent<RectTransform>();
			}
			return this._t;
		}
	}

	protected abstract float GetSize(RectTransform item);

	protected abstract float GetDimension(Vector2 vector);

	protected abstract Vector2 GetVector(float value);

	protected abstract float GetPos(RectTransform item);

	protected abstract int OneOrMinusOne();

	private void Awake()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (this.initOnAwake)
		{
			this.Init();
		}
	}

	public void Init()
	{
		this.init = true;
		Stack<RectTransform> stack = new Stack<RectTransform>();
		IEnumerator enumerator = base.content.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				RectTransform rectTransform = (RectTransform)obj;
				if (rectTransform.gameObject.activeSelf)
				{
					stack.Push(rectTransform);
					rectTransform.gameObject.SetActive(false);
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
		this.prefabItems = stack.ToArray();
		RectTransform rectTransform2;
		for (float num = 0f; num < this.GetDimension(this.t.sizeDelta); num += this.GetSize(rectTransform2))
		{
			rectTransform2 = this.NewItemAtEnd();
		}
	}

	protected virtual void Update()
	{
		if (!Application.isPlaying || !this.init)
		{
			return;
		}
		if (this.GetDimension(base.content.sizeDelta) - this.GetDimension(base.content.localPosition) * (float)this.OneOrMinusOne() < this.GetDimension(this.t.sizeDelta))
		{
			this.NewItemAtEnd();
		}
		else if (this.GetDimension(base.content.localPosition) * (float)this.OneOrMinusOne() >= this.GetDimension(this.t.sizeDelta) * 0.5f)
		{
			IEnumerator enumerator = base.content.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					RectTransform rectTransform = (RectTransform)obj;
					if (rectTransform.gameObject.activeSelf)
					{
						if (this.GetPos(rectTransform) > this.GetDimension(this.t.sizeDelta))
						{
							Object.Destroy(rectTransform.gameObject);
							base.content.localPosition -= this.GetVector(this.GetSize(rectTransform));
							this.dragOffset -= this.GetVector(this.GetSize(rectTransform));
							this.Add(ref this.itemTypeStart);
						}
						else if (this.GetPos(rectTransform) < -(this.GetDimension(this.t.sizeDelta) + this.GetSize(rectTransform)))
						{
						}
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
		}
	}

	private RectTransform NewItemAtStart()
	{
		this.Subtract(ref this.itemTypeStart);
		RectTransform rectTransform = this.InstantiateNextItem(this.itemTypeStart);
		rectTransform.SetAsFirstSibling();
		base.content.localPosition += this.GetVector(this.GetSize(rectTransform));
		this.dragOffset += this.GetVector(this.GetSize(rectTransform));
		return rectTransform;
	}

	private RectTransform NewItemAtEnd()
	{
		RectTransform rectTransform = this.InstantiateNextItem(this.itemTypeEnd);
		this.Add(ref this.itemTypeEnd);
		return rectTransform;
	}

	private RectTransform InstantiateNextItem(int itemType)
	{
		RectTransform rectTransform = Object.Instantiate<RectTransform>(this.prefabItems[itemType]);
		rectTransform.name = this.prefabItems[itemType].name;
		rectTransform.transform.SetParent(base.content.transform, false);
		rectTransform.gameObject.SetActive(true);
		return rectTransform;
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		this.dragOffset = Vector2.zero;
		base.OnBeginDrag(eventData);
	}

	public override void OnDrag(PointerEventData eventData)
	{
		if (this.dragOffset != Vector2.zero)
		{
			this.OnEndDrag(eventData);
			this.OnBeginDrag(eventData);
			this.dragOffset = Vector2.zero;
		}
		base.OnDrag(eventData);
	}

	private void Subtract(ref int i)
	{
		i--;
		if (i == -1)
		{
			i = this.prefabItems.Length - 1;
		}
	}

	private void Add(ref int i)
	{
		i++;
		if (i == this.prefabItems.Length)
		{
			i = 0;
		}
	}

	[HideInInspector]
	public bool initOnAwake;

	private RectTransform _t;

	private RectTransform[] prefabItems;

	private int itemTypeStart;

	private int itemTypeEnd;

	private bool init;

	private Vector2 dragOffset = Vector2.zero;
}
