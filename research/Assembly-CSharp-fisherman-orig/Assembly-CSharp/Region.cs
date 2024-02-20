using System;

public class Region
{
	public static CloudRegionCode Parse(string codeAsString)
	{
		codeAsString = codeAsString.ToLower();
		CloudRegionCode cloudRegionCode = CloudRegionCode.none;
		if (Enum.IsDefined(typeof(CloudRegionCode), codeAsString))
		{
			cloudRegionCode = (CloudRegionCode)Enum.Parse(typeof(CloudRegionCode), codeAsString);
		}
		return cloudRegionCode;
	}

	public override string ToString()
	{
		return string.Format("'{0}' \t{1}ms \t{2}", this.Code, this.Ping, this.HostAndPort);
	}

	public CloudRegionCode Code;

	public string HostAndPort;

	public int Ping;
}
