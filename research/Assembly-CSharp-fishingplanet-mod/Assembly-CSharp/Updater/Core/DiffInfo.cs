using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Updater.Core
{
	public class DiffInfo
	{
		public FileInfoEx Source { get; set; }

		public FileInfoEx Target { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public DiffType Type { get; set; }
	}
}
