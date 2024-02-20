using System;
using Newtonsoft.Json;

namespace Updater.Core
{
	public class FileInfoEx
	{
		public FileInfoEx()
		{
		}

		public FileInfoEx(string basePath, string filename, string hash)
		{
			this.FullFilePath = filename;
			this.RelativeFilePath = filename.Substring(basePath.Length, filename.Length - basePath.Length);
			this.Id = this.RelativeFilePath.ToLower();
			this.Hash = hash;
		}

		[JsonIgnore]
		public string Id { get; set; }

		public string FullFilePath { get; set; }

		public string RelativeFilePath { get; set; }

		public string Hash { get; set; }
	}
}
