using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM_PDFHilite_Test.app
{
	public class PdfDocumentInfo
	{
		private string fileName;
		private string text;

		public string FileName
		{
			get { return fileName; }
			set { fileName = value; }
		}

		public string Text
		{
			get { return text; }
			set { text = value; }
		}
	}
}
