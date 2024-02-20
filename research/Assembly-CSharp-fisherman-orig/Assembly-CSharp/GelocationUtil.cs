using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

public class GelocationUtil
{
	internal GeoData GetMyContinent()
	{
		GeoData geoData;
		try
		{
			HttpWebRequest httpWebRequest = WebRequest.Create(new Uri("http://www.telize.com/geoip")) as HttpWebRequest;
			if (httpWebRequest != null)
			{
				httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
				using (HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse)
				{
					if (httpWebResponse != null)
					{
						using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
						{
							return JsonConvert.DeserializeObject<GeoData>(streamReader.ReadToEnd());
						}
					}
				}
			}
			geoData = null;
		}
		catch (Exception ex)
		{
			geoData = new GeoData();
		}
		return geoData;
	}
}
