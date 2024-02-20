using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Newtonsoft.Json;
using ObjectModel;
using Photon.Interfaces.Tournaments;

public class UserCompetitionFailure : Failure
{
	public UserCompetitionFailure(OperationResponse response)
		: base(response)
	{
		if (response.Parameters.ContainsKey(1))
		{
			this.SubOperation = (UserCompetitionSubOperationCode)response[1];
		}
		if (response.Parameters.ContainsKey(45))
		{
			this.UserCompetitionErrorCode = (UserCompetitionErrorCode)response[45];
		}
		if (response.Parameters.ContainsKey(47))
		{
			this.NextOperationAvailability = new DateTime?(new DateTime((long)response[47]));
		}
		if (response.Parameters.ContainsKey(10))
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			this.UserCompetition = JsonConvert.DeserializeObject<UserCompetition>(text, SerializationHelper.JsonSerializerSettings);
		}
		if (response.Parameters.ContainsKey(49))
		{
			byte[] array2 = (byte[])response.Parameters[49];
			string text2 = CompressHelper.DecompressString(array2);
			this.Tournaments = JsonConvert.DeserializeObject<List<TournamentBrief>>(text2, SerializationHelper.JsonSerializerSettings);
		}
	}

	public UserCompetitionSubOperationCode SubOperation { get; set; }

	public UserCompetitionErrorCode UserCompetitionErrorCode { get; set; }

	public DateTime? NextOperationAvailability { get; set; }

	public UserCompetitionPublic UserCompetition { get; set; }

	public List<TournamentBrief> Tournaments { get; set; }

	public override string FullErrorInfo
	{
		get
		{
			return base.FullErrorInfo + string.Format("\r\nSubOperation: " + Enum.GetName(typeof(UserCompetitionSubOperationCode), this.SubOperation), new object[0]);
		}
	}
}
