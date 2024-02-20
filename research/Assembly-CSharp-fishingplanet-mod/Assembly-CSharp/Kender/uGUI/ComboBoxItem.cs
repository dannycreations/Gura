using System;
using UnityEngine;

namespace Kender.uGUI
{
	[Serializable]
	public class ComboBoxItem
	{
		public ComboBoxItem(string caption)
		{
			this._caption = caption;
		}

		public ComboBoxItem(Sprite image)
		{
			this._image = image;
		}

		public ComboBoxItem(string caption, bool disabled)
		{
			this._caption = caption;
			this._isDisabled = disabled;
		}

		public ComboBoxItem(Sprite image, bool disabled)
		{
			this._image = image;
			this._isDisabled = disabled;
		}

		public ComboBoxItem(string caption, Sprite image, bool disabled)
		{
			this._caption = caption;
			this._image = image;
			this._isDisabled = disabled;
		}

		public ComboBoxItem(string caption, Sprite image, bool disabled, Action onSelect)
		{
			this._caption = caption;
			this._image = image;
			this._isDisabled = disabled;
			this.OnSelect = onSelect;
		}

		public ComboBoxItem(string caption, Sprite image, Action onSelect)
		{
			this._caption = caption;
			this._image = image;
			this.OnSelect = onSelect;
		}

		public ComboBoxItem(string caption, Action onSelect)
		{
			this._caption = caption;
			this.OnSelect = onSelect;
		}

		public ComboBoxItem(Sprite image, Action onSelect)
		{
			this._image = image;
			this.OnSelect = onSelect;
		}

		public string Caption
		{
			get
			{
				return this._caption;
			}
			set
			{
				this._caption = value;
				if (this.OnUpdate != null)
				{
					this.OnUpdate();
				}
			}
		}

		public Sprite Image
		{
			get
			{
				return this._image;
			}
			set
			{
				this._image = value;
				if (this.OnUpdate != null)
				{
					this.OnUpdate();
				}
			}
		}

		public bool IsDisabled
		{
			get
			{
				return this._isDisabled;
			}
			set
			{
				this._isDisabled = value;
				if (this.OnUpdate != null)
				{
					this.OnUpdate();
				}
			}
		}

		public virtual void AdditionalAction(GameObject go)
		{
		}

		[SerializeField]
		private string _caption;

		[SerializeField]
		private Sprite _image;

		[SerializeField]
		private bool _isDisabled;

		public Action OnSelect;

		internal Action OnUpdate;
	}
}
