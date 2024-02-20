using System;
using ExitGames.Client.Photon;
using Photon.Interfaces;
using Photon.Interfaces.Sys;

public class SysOpFailure : Failure
{
	public SysOpFailure(OperationResponse response)
		: base(response)
	{
		base.Operation = response.OperationCode;
		this.SubOperation = (SysSubOperationCode)response[1];
		base.ErrorCode = response.ReturnCode;
		base.ErrorMessage = response.DebugMessage;
	}

	public override string FullErrorInfo
	{
		get
		{
			return string.Format("Sys operation failed \r\n Operation: {0}, \r\n SubOperation: {1}, \r\nError code: {2} \r\nMessage: {3}", new object[]
			{
				Enum.GetName(typeof(OperationCode), base.Operation),
				Enum.GetName(typeof(SysSubOperationCode), this.SubOperation),
				Enum.GetName(typeof(ErrorCode), base.ErrorCode),
				base.ErrorMessage
			});
		}
	}

	public SysSubOperationCode SubOperation;
}
