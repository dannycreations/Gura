using System;
using ExitGames.Client.Photon;

public class TransactionFailure : Failure
{
	public TransactionFailure(OperationResponse response)
		: base(response)
	{
	}
}
