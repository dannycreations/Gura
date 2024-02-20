using System;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleController : MonoBehaviour
{
	internal void Start()
	{
		Object.DontDestroyOnLoad(this);
		base.GetComponent<CanvasGroup>().alpha = 0f;
		PhotonConnectionFactory.Instance.OnMoveFailed += this.OnMoveFailed;
		PhotonConnectionFactory.Instance.OnInventoryUpdatedFailure += this.OnInventoryUpdatedFailure;
		PhotonConnectionFactory.Instance.OnOperationFailed += this.OnOperationFailed;
		PhotonConnectionFactory.Instance.OnProfileOperationFailed += this.OnProfileOperationFailed;
		PhotonConnectionFactory.Instance.OnTransactionFailed += this.OnTransactionFailed;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnMoveFailed -= this.OnMoveFailed;
		PhotonConnectionFactory.Instance.OnInventoryUpdatedFailure -= this.OnInventoryUpdatedFailure;
		PhotonConnectionFactory.Instance.OnOperationFailed -= this.OnOperationFailed;
		PhotonConnectionFactory.Instance.OnProfileOperationFailed -= this.OnProfileOperationFailed;
		PhotonConnectionFactory.Instance.OnTransactionFailed -= this.OnTransactionFailed;
	}

	internal void Update()
	{
		if (!ConfigUtil.IsConsole)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void AddError(string errorText)
	{
		if (ConfigUtil.IsConsole)
		{
			base.GetComponent<CanvasGroup>().alpha = 1f;
			this.ErrorList.text = string.Format("{0}", errorText);
		}
	}

	private void OnInventoryUpdatedFailure(Failure failure)
	{
		this.AddError(failure.FullErrorInfo);
	}

	private void OnTransactionFailed(TransactionFailure failure)
	{
		this.AddError(failure.FullErrorInfo);
	}

	private void OnOperationFailed(Failure failure)
	{
		if (failure.Operation != 156)
		{
			this.AddError(failure.FullErrorInfo);
		}
	}

	private void OnProfileOperationFailed(ProfileFailure failure)
	{
		this.AddError(failure.FullErrorInfo);
	}

	private void OnMoveFailed(Failure failure)
	{
		this.AddError(failure.FullErrorInfo);
	}

	public Text ErrorList;
}
