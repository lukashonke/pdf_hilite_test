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
		private string path;
		private string text;
		private string parameters;

		public string FileName
		{
			get { return fileName; }
			set { fileName = value; }
		}

		public string Path
		{
			get { return path; }
			set { path = value; }
		}

		public string Text
		{
			get { return text; }
			set { text = value; }
		}

		public string Parameters
		{
			get { return parameters; }
			set { parameters = value; }
		}
	}
}
