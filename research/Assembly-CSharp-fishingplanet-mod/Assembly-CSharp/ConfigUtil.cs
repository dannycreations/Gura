using System;
using System.IO;

public class ConfigUtil
{
	public void SetServerConnection(string file_path)
	{
		FileInfo fileInfo = new FileInfo(file_path);
		if (!fileInfo.Exists)
		{
			return;
		}
		using (StreamReader streamReader = new StreamReader(file_path))
		{
			string text = streamReader.ReadLine();
			string text2 = streamReader.ReadLine();
			if (text2.Trim() == "Console=true")
			{
				ConfigUtil.IsConsole = true;
			}
			string text3 = streamReader.ReadToEnd();
			if (!string.IsNullOrEmpty(text))
			{
				StaticUserData.ServerConnectionString = text;
			}
			if (!string.IsNullOrEmpty(text3))
			{
				PaymentHelper.PaymentBaseUrl = text3;
			}
		}
	}

	public static bool IsConsole;
}
