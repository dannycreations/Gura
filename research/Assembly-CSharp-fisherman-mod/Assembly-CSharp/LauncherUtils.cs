using System;
using System.IO;
using System.Security.Cryptography;

public static class LauncherUtils
{
	public static string GetFileHash(string filename)
	{
		string text;
		using (MD5 md = MD5.Create())
		{
			using (FileStream fileStream = File.OpenRead(filename))
			{
				text = Convert.ToBase64String(md.ComputeHash(fileStream));
			}
		}
		return text;
	}

	public static void ClearPath(string path)
	{
		string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
		foreach (string text in files)
		{
			try
			{
				File.Delete(text);
			}
			catch
			{
			}
		}
		string[] directories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
		foreach (string text2 in directories)
		{
			try
			{
				Directory.Delete(text2);
			}
			catch
			{
			}
		}
	}
}
