using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageMagick;
using org.pdfclown.documents;
using org.pdfclown.files;

namespace NM_PDFHilite_Test.app
{
	public class MetadataReader : PdfReader
	{
		public MetadataReader(PdfDocumentInfo doc)
			: base(doc)
		{
			
		}

		public override void Process()
		{
			ReadMetadata();
		}

		private void ReadMetadata()
		{
			try
			{
				File pdfFile = new File(CurrentDocumentInfo.Path);

				Debug.Write(pdfFile.Document);

				if (pdfFile.Document.PageSize != null)
					Output += pdfFile.Document.PageSize.Value;

				foreach (Page page in pdfFile.Document.Pages)
					Output += "Page " + page.Index + " size " + page.Size.Height + "; " + page.Size.Width;

				Output += "\n\n";

				Output += pdfFile.Document.Metadata.Content.OuterXml;
			}
			catch (Exception e)
			{
				Trace.TraceError(e.ToString());
				MessageBox.Show("Error while reading pdf metadata " + e);
			}
		}
	}
}
