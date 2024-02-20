using System;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.Util.PullToRefresh
{
	public class PullToRefreshGizmo : MonoBehaviour
	{
		public virtual bool IsShown
		{
			get
			{
				return this._IsShown;
			}
			set
			{
				this._IsShown = value;
				base.gameObject.SetActive(this._IsShown);
			}
		}

		public virtual void Awake()
		{
		}

		public virtual void OnPull(float power)
		{
		}

		public virtual void OnRefreshed(bool autoHide)
		{
			if (autoHide)
			{
				this.IsShown = false;
			}
		}

		public virtual void OnRefreshCancelled()
		{
			this.IsShown = false;
		}

		private bool _IsShown;
	}
}
