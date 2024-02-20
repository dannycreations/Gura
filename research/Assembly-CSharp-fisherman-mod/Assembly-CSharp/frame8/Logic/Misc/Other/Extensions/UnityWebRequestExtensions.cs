using System;
using UnityEngine.Networking;

namespace frame8.Logic.Misc.Other.Extensions
{
	public static class UnityWebRequestExtensions
	{
		public static long GetContentLengthFromHeader(this UnityWebRequest www)
		{
			string responseHeader;
			long num;
			if ((responseHeader = www.GetResponseHeader("Content-Length")) != null && long.TryParse(responseHeader, out num))
			{
				return num;
			}
			return -1L;
		}
	}
}
