using System;

namespace GameDevWare.Dynamic.Expressions
{
	internal interface ILineInfo
	{
		int GetLineNumber();

		int GetColumnNumber();

		int GetTokenLength();
	}
}
