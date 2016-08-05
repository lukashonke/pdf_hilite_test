using System;
using System.Collections.Generic;
using System.Drawing;
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
		public static string DehyphenateLine(string parameter)
		{
			string temp = parameter.TrimEnd();
			string txt;

			if (temp.EndsWith("-") || temp.EndsWith("–") || temp.EndsWith("-") || temp.EndsWith("—"))
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

		public static RectangleF RecalculatePosition(int documentWidth, int documentHeight, int ocrImageWidth, int ocrImageHeight, int wordPositionX1, int wordPositionY1, int wordPositionX2, int wordPositionY2)
		{
			// document { Width = 645 Height = 858}
			// Zeman OCR cords: 1929 1819 2353 1918
			// OCR image size: 2691x3577

			/*documentWidth = 645;
			documentHeight = 858;

			ocrImageWidth = 2691;
			ocrImageHeight = 3577;*/

			int wordPositionX = wordPositionX1;
			int wordPositionY = wordPositionY1;
			int wordHeight = wordPositionY2 - wordPositionY1;
			int wordWidth = wordPositionX2 - wordPositionX1;

			RectangleF rect = new RectangleF(wordPositionX/(float)ocrImageWidth * documentWidth, wordPositionY/(float)ocrImageHeight * documentHeight, wordWidth /(float)ocrImageWidth * documentWidth, wordHeight / (float) ocrImageHeight * documentHeight);
			return rect;
		}
	}
}
