using System;

public class ExternalFailure : Failure
{
	public override string FullErrorInfo
	{
		get
		{
			return base.ErrorMessage;
		}
	}
}
