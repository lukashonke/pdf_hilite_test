using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tesseract;

namespace NM_PDFHilite_Test.app
{
	public class OcrTest : PdfReader
	{
		public OcrTest(PdfDocumentInfo doc) : base(doc)
		{
			
		}

		public override void Process()
		{
			try
			{
				Debug.Write(Environment.CurrentDirectory);
				TesseractEngine engine = new TesseractEngine(@"../../tessdata", "ces", EngineMode.Default);

				Pix pix = Pix.LoadFromFile(@"../../ocr_test/test1.png");

				Page page = engine.Process(pix);

				string text = page.GetText();

				Output += ("Mean confidence: " + page.GetMeanConfidence());
				Output += "===========";
				Output += ("\nText:\n " + text);
			}
			catch (Exception e)
			{
				Trace.TraceError(e.ToString());
				MessageBox.Show("Error while OCR: " + e);
			}
		}
	}
}
