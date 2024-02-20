using System;

namespace Assets.Scripts.Common.Exceptions
{
	public class WrongParameterException : Exception
	{
		public WrongParameterException(string text)
			: base(text)
		{
		}
	}
}
