using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM_PDFHilite_Test.app
{
	public abstract class PdfReader
	{
		private PdfDocumentInfo currentDocumentInfo;

		public PdfReader(PdfDocumentInfo doc)
		{
			currentDocumentInfo = doc;
		}

		public string Output { get; protected set; }

		public PdfDocumentInfo CurrentDocumentInfo
		{
			get { return currentDocumentInfo; }
		}

		public abstract void Process();
	}
}
