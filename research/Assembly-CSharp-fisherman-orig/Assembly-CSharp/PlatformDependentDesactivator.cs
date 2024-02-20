using System;
using UnityEngine;

public class PlatformDependentDesactivator : MonoBehaviour
{
	private void OnEnable()
	{
		if (!this.PC)
		{
			base.gameObject.SetActive(false);
		}
		if (this.TPMReplaysOnly)
		{
			base.gameObject.SetActive(false);
		}
	}

	[SerializeField]
	private bool PC = true;

	[SerializeField]
	private bool Console;

	[SerializeField]
	private bool XboxOnly;

	[SerializeField]
	private bool PS4Only;

	[SerializeField]
	private bool _hideOnWinmd;

	[SerializeField]
	private bool _showOnWinmd;

	[SerializeField]
	private bool TPMReplaysOnly;

	[SerializeField]
	private bool RetailPS4Only;

	[SerializeField]
	private bool RetailXboxOnly;
}
