using System;

namespace GameDevWare.Dynamic.Expressions.CSharp
{
	public struct Token : ILineInfo
	{
		public Token(TokenType type, string value, int line, int col, int len)
		{
			this.Type = type;
			this.Value = value;
			this.LineNumber = line;
			this.ColumnNumber = col;
			this.TokenLength = len;
		}

		public bool IsValid
		{
			get
			{
				return this.Type != TokenType.None;
			}
		}

		public string Position
		{
			get
			{
				return string.Format("{0}:{1}+{2}", this.LineNumber.ToString(), this.ColumnNumber.ToString(), this.TokenLength.ToString());
			}
		}

		int ILineInfo.GetLineNumber()
		{
			return this.LineNumber;
		}

		int ILineInfo.GetColumnNumber()
		{
			return this.ColumnNumber;
		}

		int ILineInfo.GetTokenLength()
		{
			return this.TokenLength;
		}

		public override string ToString()
		{
			return this.Type + ((this.Type != TokenType.Number && this.Type != TokenType.Identifier && this.Type != TokenType.Literal) ? string.Empty : ("(" + this.Value + ")"));
		}

		public readonly TokenType Type;

		public readonly string Value;

		public readonly int LineNumber;

		public readonly int ColumnNumber;

		public readonly int TokenLength;
	}
}
