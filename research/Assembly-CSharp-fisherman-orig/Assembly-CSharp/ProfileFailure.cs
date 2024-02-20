using System;
using ExitGames.Client.Photon;
using Photon.Interfaces.Profile;

public class ProfileFailure : Failure
{
	public ProfileFailure(OperationResponse response)
		: base(response)
	{
	}

	public ProfileSubOperationCode SubOperation { get; set; }

	public override string FullErrorInfo
	{
		get
		{
			return base.FullErrorInfo + string.Format("\r\nSubOperation: " + Enum.GetName(typeof(ProfileSubOperationCode), this.SubOperation), new object[0]);
		}
	}
}
