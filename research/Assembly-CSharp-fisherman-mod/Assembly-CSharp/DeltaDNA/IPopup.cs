﻿using System;
using System.Collections.Generic;

namespace DeltaDNA
{
	public interface IPopup
	{
		event EventHandler BeforePrepare;

		event EventHandler AfterPrepare;

		event EventHandler BeforeShow;

		event EventHandler BeforeClose;

		event EventHandler AfterClose;

		event EventHandler<PopupEventArgs> Dismiss;

		event EventHandler<PopupEventArgs> Action;

		void Prepare(Dictionary<string, object> configuration);

		void Show();

		void Close();

		void OnDismiss(PopupEventArgs eventArgs);

		void OnAction(PopupEventArgs eventArgs);
	}
}
