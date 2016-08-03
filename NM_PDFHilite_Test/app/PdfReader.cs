using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM_PDFHilite_Test.app
{
	public abstract class PdfReader
	{
		protected PdfDocumentInfo currentDocumentInfo;

		public PdfReader(PdfDocumentInfo doc)
		{
			currentDocumentInfo = doc;
		}

		public string Output { get; protected set; }

		public abstract void Process();
	}
}
