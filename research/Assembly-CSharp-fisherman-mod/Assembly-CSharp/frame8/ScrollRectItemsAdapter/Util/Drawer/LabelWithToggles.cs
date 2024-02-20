using System;
using System.Collections.Generic;
using System.Diagnostics;
using frame8.Logic.Misc.Other;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util.Drawer
{
	public class LabelWithToggles : MonoBehaviour
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<int, bool> ToggleChanged;

		public bool Interactable
		{
			set
			{
				foreach (LabelWithToggle labelWithToggle in this.subItems)
				{
					if (labelWithToggle)
					{
						labelWithToggle.toggle.interactable = value;
					}
				}
			}
		}

		public LabelWithToggles Init(string mainLabelStr, params string[] names)
		{
			if (this.subItems != null)
			{
				this.DestroySubItems();
			}
			this.mainLabel.text = mainLabelStr;
			int i = 0;
			List<LabelWithToggle> list = DotNETCoreCompat.ConvertAll<string, LabelWithToggle>(names, delegate(string n)
			{
				if (string.IsNullOrEmpty(n))
				{
					return null;
				}
				LabelWithToggle component = Object.Instantiate<GameObject>(this._SubItemPrefab.gameObject).GetComponent<LabelWithToggle>();
				component.gameObject.SetActive(true);
				component.transform.SetParent(this.toggleGroup.transform, false);
				component.Init(n);
				this.toggleGroup.RegisterToggle(component.toggle);
				int copyOfI = i++;
				component.toggle.onValueChanged.AddListener(delegate(bool isOn)
				{
					if (<Init>c__AnonStorey.ToggleChanged != null)
					{
						<Init>c__AnonStorey.ToggleChanged(copyOfI, isOn);
					}
				});
				return component;
			});
			this.subItems = list.FindAll((LabelWithToggle si) => si != null).ToArray();
			return this;
		}

		private void DestroySubItems()
		{
			foreach (LabelWithToggle labelWithToggle in this.subItems)
			{
				if (labelWithToggle)
				{
					if (labelWithToggle.toggle)
					{
						labelWithToggle.toggle.onValueChanged.RemoveAllListeners();
					}
					Object.Destroy(labelWithToggle.gameObject);
				}
			}
		}

		private void OnDestroy()
		{
			this.ToggleChanged = null;
			if (this.subItems != null)
			{
				this.DestroySubItems();
			}
		}

		[SerializeField]
		private LabelWithToggle _SubItemPrefab;

		public Text mainLabel;

		public ToggleGroup toggleGroup;

		[NonSerialized]
		public LabelWithToggle[] subItems;

		private int _ToggleIndexForCurrentEvent;
	}
}
