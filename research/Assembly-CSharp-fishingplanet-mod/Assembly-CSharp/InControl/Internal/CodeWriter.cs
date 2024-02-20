using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InControl.Internal
{
	internal class CodeWriter
	{
		public CodeWriter()
		{
			this.indent = 0;
			this.stringBuilder = new StringBuilder(4096);
		}

		public void IncreaseIndent()
		{
			this.indent++;
		}

		public void DecreaseIndent()
		{
			this.indent--;
		}

		public void Append(string code)
		{
			this.Append(false, code);
		}

		public void Append(bool trim, string code)
		{
			if (trim)
			{
				code = code.Trim();
			}
			string[] array = Regex.Split(code, "\\r?\\n|\\n");
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				string text = array[i];
				if (!text.All(new Func<char, bool>(char.IsWhiteSpace)))
				{
					this.stringBuilder.Append('\t', this.indent);
					this.stringBuilder.Append(text);
				}
				if (i < num - 1)
				{
					this.stringBuilder.Append('\n');
				}
			}
		}

		public void AppendLine(string code)
		{
			this.Append(code);
			this.stringBuilder.Append('\n');
		}

		public void AppendLine(int count)
		{
			this.stringBuilder.Append('\n', count);
		}

		public void AppendFormat(string format, params object[] args)
		{
			this.Append(string.Format(format, args));
		}

		public void AppendLineFormat(string format, params object[] args)
		{
			this.AppendLine(string.Format(format, args));
		}

		public override string ToString()
		{
			return this.stringBuilder.ToString();
		}

		private const char NewLine = '\n';

		private int indent;

		private StringBuilder stringBuilder;
	}
}
