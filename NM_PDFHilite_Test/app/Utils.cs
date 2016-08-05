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

		/// <summary>
		/// odebere "-" z koncu radku
		/// pokud konci teckou nebo jinym znamenkem, prida mezeru.
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		public static string DehyphenateLine(string parameter)
		{
			string temp = parameter.TrimEnd();
			string txt;

			if (temp.EndsWith("-") || temp.EndsWith("–") || temp.EndsWith("-"))
			{
				//txt = temp.Substring(0, temp.Length - 1); // cut off the last letter
				txt = temp.Substring(0, temp.Length - 1) + "";
			}
			else
			{
				txt = parameter;
			}

			if (txt.EndsWith(".") || txt.EndsWith("!") || txt.EndsWith("?") || txt.EndsWith("\""))
				txt += " ";

			return txt;
		}
	}
}
