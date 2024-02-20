using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace DeltaDNA
{
	public class Popup : IPopup
	{
		public Popup()
			: this(new Dictionary<string, object>())
		{
		}

		public Popup(Dictionary<string, object> options)
		{
			object obj;
			if (options.TryGetValue("name", out obj))
			{
				this._name = (string)obj;
			}
			object obj2;
			if (options.TryGetValue("depth", out obj2))
			{
				this._depth = (int)obj2;
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler BeforePrepare;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler AfterPrepare;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler BeforeShow;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler BeforeClose;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler AfterClose;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<PopupEventArgs> Dismiss;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<PopupEventArgs> Action;

		public Dictionary<string, object> Configuration { get; private set; }

		public bool IsReady { get; private set; }

		public bool IsShowing { get; private set; }

		public void Prepare(Dictionary<string, object> configuration)
		{
			try
			{
				if (this.BeforePrepare != null)
				{
					this.BeforePrepare(this, new EventArgs());
				}
				this._gameObject = new GameObject(this._name);
				SpriteMap spriteMap = this._gameObject.AddComponent<SpriteMap>();
				spriteMap.Init(configuration);
				spriteMap.LoadResource(delegate
				{
					this.IsReady = true;
					if (this.AfterPrepare != null)
					{
						this.AfterPrepare(this, new EventArgs());
					}
				});
				this._spritemap = spriteMap;
				this.Configuration = configuration;
			}
			catch (Exception ex)
			{
				Logger.LogError("Preparing popup configuration failed: " + ex.Message);
			}
		}

		public void Show()
		{
			if (this.IsReady)
			{
				try
				{
					if (this.BeforeShow != null)
					{
						this.BeforeShow(this, new EventArgs());
					}
					object obj;
					if (this.Configuration.TryGetValue("shim", out obj))
					{
						Dictionary<string, object> dictionary = obj as Dictionary<string, object>;
						ShimLayer shimLayer = this._gameObject.AddComponent<ShimLayer>();
						shimLayer.Init(this, dictionary, this._depth);
					}
					object obj2;
					if (this.Configuration.TryGetValue("layout", out obj2))
					{
						Dictionary<string, object> dictionary2 = obj2 as Dictionary<string, object>;
						object obj3;
						if (dictionary2.TryGetValue("landscape", out obj3) || dictionary2.TryGetValue("portrait", out obj3))
						{
							Dictionary<string, object> dictionary3 = obj3 as Dictionary<string, object>;
							BackgroundLayer backgroundLayer = this._gameObject.AddComponent<BackgroundLayer>();
							backgroundLayer.Init(this, dictionary3, this._spritemap.GetBackground(), this._depth - 1);
							ButtonsLayer buttonsLayer = this._gameObject.AddComponent<ButtonsLayer>();
							buttonsLayer.Init(this, dictionary3, this._spritemap.GetButtons(), backgroundLayer, this._depth - 2);
							this.IsShowing = true;
						}
						else
						{
							Logger.LogError("No layout orientation found.");
						}
					}
					else
					{
						Logger.LogError("No layout found.");
					}
				}
				catch (Exception ex)
				{
					Logger.LogError("Showing popup failed: " + ex.Message);
				}
			}
		}

		public void Close()
		{
			if (this.IsShowing)
			{
				if (this.BeforeClose != null)
				{
					this.BeforeClose(this, new EventArgs());
				}
				foreach (Layer layer in this._gameObject.GetComponents<Layer>())
				{
					Object.Destroy(layer);
				}
				if (this.AfterClose != null)
				{
					this.AfterClose(this, new EventArgs());
				}
				this.IsShowing = false;
			}
		}

		public void OnDismiss(PopupEventArgs eventArgs)
		{
			if (this.Dismiss != null)
			{
				this.Dismiss(this, eventArgs);
			}
		}

		public void OnAction(PopupEventArgs eventArgs)
		{
			if (this.Action != null)
			{
				this.Action(this, eventArgs);
			}
		}

		private GameObject _gameObject;

		private SpriteMap _spritemap;

		private string _name = "Popup";

		private int _depth;
	}
}
