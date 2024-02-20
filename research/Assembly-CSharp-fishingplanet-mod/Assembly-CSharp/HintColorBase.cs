using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HintColorBase : ManagedHintObject
{
	protected virtual void Awake()
	{
		if (this._colorPulsation != null)
		{
			this._highlightColor = this._colorPulsation.StartColor;
		}
		this.Init();
	}

	protected override void Show()
	{
		base.Show();
		this.UpdateVisual(false);
	}

	protected override void Hide()
	{
		base.Hide();
		this.UpdateVisual(false);
	}

	protected override void OnDestroy()
	{
		this.ClearShines();
		this.UpdateVisual(true);
		base.OnDestroy();
	}

	protected virtual void ClearShines()
	{
		for (int i = 0; i < this._shines.Count; i++)
		{
			Object.Destroy(this._shines[i].gameObject);
		}
		this._shines.Clear();
	}

	protected virtual void Init()
	{
	}

	protected IEnumerator Test()
	{
		yield return new WaitForSeconds(5f);
		this.Show();
		yield break;
	}

	protected virtual void UpdateVisual(bool isDestroy = false)
	{
	}

	protected void FillData<T>(Dictionary<T, Color> data, Action<T> fillFunc, bool inParent, Transform tsform = null) where T : MaskableGraphic
	{
		if (tsform == null)
		{
			tsform = base.gameObject.transform;
		}
		Transform transform = ((!inParent) ? tsform : tsform.parent);
		for (int i = 0; i < transform.childCount; i++)
		{
			T component = transform.GetChild(i).GetComponent<T>();
			if (component != null)
			{
				data.Add(component, component.color);
				if (fillFunc != null)
				{
					fillFunc(component);
				}
			}
		}
	}

	protected void CloneFontMaterial(TextMeshProUGUI t)
	{
		t.fontSharedMaterial = Object.Instantiate<Material>(t.fontSharedMaterial);
	}

	[SerializeField]
	protected float MoveTime = 3f;

	[SerializeField]
	protected GameObject _shinePrefab;

	[SerializeField]
	protected ColorPulsation _colorPulsation;

	protected List<RectTransform> _shines = new List<RectTransform>();

	protected Color _highlightColor;
}
