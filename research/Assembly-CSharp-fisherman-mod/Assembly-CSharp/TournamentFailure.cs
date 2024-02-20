using System;
using ExitGames.Client.Photon;
using Newtonsoft.Json;
using ObjectModel;
using Photon.Interfaces;
using Photon.Interfaces.Tournaments;

public class TournamentFailure : Failure
{
	public TournamentFailure(OperationResponse response)
		: base(response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		base.Operation = response.OperationCode;
		if (response.Parameters.ContainsKey(1))
		{
			this.SubOperation = (TournamentSubOperationCode)response[1];
		}
		base.ErrorCode = response.ReturnCode;
		base.ErrorMessage = response.DebugMessage;
		if (response.Parameters.ContainsKey(48))
		{
			byte[] array = (byte[])response[48];
			string text = CompressHelper.DecompressString(array);
			this.UserCompetition = JsonConvert.DeserializeObject<UserCompetitionPublic>(text);
		}
	}

	public override string FullErrorInfo
	{
		get
		{
			return string.Format("Tournament operation failed \r\n Operation: {0}, \r\n SubOperation: {1}, \r\nError code: {2} \r\nMessage: {3}", new object[]
			{
				Enum.GetName(typeof(OperationCode), base.Operation),
				Enum.GetName(typeof(TournamentSubOperationCode), this.SubOperation),
				Enum.GetName(typeof(ErrorCode), base.ErrorCode),
				base.ErrorMessage
			});
		}
	}

	public TournamentSubOperationCode SubOperation;

	public UserCompetitionPublic UserCompetition;
}
