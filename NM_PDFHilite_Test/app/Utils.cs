using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM_PDFHilite_Test.app
{
	public static class Utils
	{
		public static string CleanUpString(string parameter)
		{
			parameter = parameter.Replace((char)0xA0, ' ');

			return parameter;
		}

		public static string Dehyphenate(string parameter)
		{
			/*string temp = parameter.Text.TrimEnd();
			string txt;

			if (temp.EndsWith("-") || temp.EndsWith("–") || temp.EndsWith("-"))
			{
				txt = temp.Substring(0, temp.Length - 1); // cut off the last letter
			}
			else
			{
				txt = textString.Text;
			}

			rawText += txt + "";*/
			return parameter;
			//TODO
		}
	}
}
