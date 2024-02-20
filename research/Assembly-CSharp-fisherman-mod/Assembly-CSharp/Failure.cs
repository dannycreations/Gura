using System;
using ExitGames.Client.Photon;
using Photon.Interfaces;

public class Failure
{
	public Failure()
	{
	}

	public Failure(OperationResponse response)
	{
		this.Operation = response.OperationCode;
		this.ErrorCode = response.ReturnCode;
		this.ErrorMessage = response.DebugMessage;
	}

	public OperationCode Operation { get; set; }

	public ErrorCode ErrorCode { get; set; }

	public string ErrorMessage { get; set; }

	public virtual string FullErrorInfo
	{
		get
		{
			return string.Format("SERVER FAILURE \r\n Operation: {0}, \r\nError code: {1} \r\nMessage: {2}", Enum.GetName(typeof(OperationCode), this.Operation), Enum.GetName(typeof(ErrorCode), this.ErrorCode), this.ErrorMessage);
		}
	}
}
