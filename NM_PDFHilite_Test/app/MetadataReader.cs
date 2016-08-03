using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageMagick;
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
