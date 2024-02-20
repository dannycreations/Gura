using System;
using UnityEngine;

namespace Leaderboard
{
	public abstract class Top100CommonPage : TabPage
	{
		protected abstract Top100Proxy CurrentProxy { get; }

		protected abstract void InitProxy();

		protected void ShowLoading(bool show)
		{
			this.LoadingPanel.gameObject.SetActive(show);
		}

		protected void ShowEmpty(bool show)
		{
			this.EmptyPanel.gameObject.SetActive(show);
		}

		protected override void Awake()
		{
			base.Awake();
			this.InitProxy();
		}

		protected override void Update()
		{
			base.Update();
			if (base.IsShow || base.IsShowing)
			{
				this.SelectorCanvasGroup.interactable = !this.CurrentProxy.IsLoading;
			}
		}

		public override void RefreshPage()
		{
			if (!this.CurrentProxy.IsLoading)
			{
				this.ShowLoading(true);
				this.CurrentProxy.SendRequest();
			}
		}

		protected void CancelLoading()
		{
			this.CurrentProxy.CancelRequest();
			this.ShowLoading(false);
		}

		protected virtual void OnGetLeaderboardsFailed()
		{
		}

		protected override void OnDestroy()
		{
			this.CancelLoading();
			base.OnDestroy();
		}

		protected bool _firstLaunch = true;

		[SerializeField]
		protected CanvasGroup SelectorCanvasGroup;

		[SerializeField]
		protected GameObject LoadingPanel;

		[SerializeField]
		protected GameObject EmptyPanel;
	}
}
