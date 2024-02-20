using System;
using UnityEngine;

namespace frame8.Logic.Misc.Other.Extensions
{
	public static class WWWExtensions
	{
		public static long GetContentLengthFromHeader(this WWW www)
		{
			string text;
			long num;
			if (www.responseHeaders.TryGetValue("Content-Length", out text) && long.TryParse(text, out num))
			{
				return num;
			}
			return -1L;
		}
	}
}
