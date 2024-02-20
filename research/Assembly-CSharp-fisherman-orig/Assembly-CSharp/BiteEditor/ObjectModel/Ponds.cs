using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace BiteEditor.ObjectModel
{
	public class Ponds
	{
		public Ponds(Ponds previousInstance, FishData[] fishData)
		{
			if (fishData != null)
			{
				Settings.InitFish(fishData);
			}
			if (!Directory.Exists("Assets\\BiteSystem\\Maps\\"))
			{
				throw new InvalidOperationException(string.Format("BiteEditor.Ponds: Can't find directory {0}", "Assets\\BiteSystem\\Maps\\"));
			}
			DirectoryInfo directoryInfo = new DirectoryInfo("Assets\\BiteSystem\\Maps\\");
			FileInfo[] files = directoryInfo.GetFiles("*.srv");
			JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
			jsonSerializerSettings.Converters.Add(new DataConverter());
			foreach (FileInfo fileInfo in files)
			{
				this._pondsSettings.Add(fileInfo);
				Pond pond2;
				if (previousInstance != null && !previousInstance.WasSettingChanged(fileInfo))
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);
					Pond pond = previousInstance.FindPondByFileName(fileNameWithoutExtension);
					pond2 = (Pond)pond.Clone();
				}
				else
				{
					pond2 = JsonConvert.DeserializeObject<Pond>(File.ReadAllText(fileInfo.FullName), jsonSerializerSettings);
					pond2.FinishInitialization();
				}
				this._ponds[pond2.Name] = pond2;
			}
		}

		public void RefreshPond(string jsonPath)
		{
			if (!File.Exists(jsonPath))
			{
				throw new InvalidOperationException(string.Format("BiteEditor.Ponds: Can't find json file at path {0}", jsonPath));
			}
			JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
			jsonSerializerSettings.Converters.Add(new DataConverter());
			Pond pond = JsonConvert.DeserializeObject<Pond>(File.ReadAllText(jsonPath), jsonSerializerSettings);
			pond.FinishInitialization();
			this._ponds[pond.Name] = pond;
		}

		public Pond FindPond(string shortName)
		{
			string text = string.Format("{0}_settings", shortName);
			if (this._ponds.ContainsKey(text))
			{
				return this._ponds[text];
			}
			return null;
		}

		public Pond FindPondByFileName(string name)
		{
			if (this._ponds.ContainsKey(name))
			{
				return this._ponds[name];
			}
			return null;
		}

		public bool WasSettingChanged(FileInfo pondFile)
		{
			FileInfo fileInfo = this._pondsSettings.FirstOrDefault((FileInfo f) => f.FullName == pondFile.FullName);
			return fileInfo != null && fileInfo.LastWriteTimeUtc < pondFile.LastWriteTimeUtc;
		}

		public Pond GetAnyPond()
		{
			return (this._ponds.Count <= 0) ? null : this._ponds.First<KeyValuePair<string, Pond>>().Value;
		}

		private Dictionary<string, Pond> _ponds = new Dictionary<string, Pond>();

		private readonly List<FileInfo> _pondsSettings = new List<FileInfo>();
	}
}
