using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageMagick;
using Tesseract;

namespace NM_PDFHilite_Test.app
{
	public class OcrTest : PdfReader
	{
		// ces | eng
		private const string language = "ces";
		private const string imageFolder = "OutputImages/";
		private const bool hocr = true;

		private string imageExtenion = ".png";

		public OcrTest(PdfDocumentInfo doc) : base(doc)
		{
			if (Settings.USE_TIF_FORMAT)
				imageExtenion = ".tif";
		}

		public override void Process()
		{
			try
			{
				TesseractEngine engine = new TesseractEngine(@"../../tessdata", language, EngineMode.Default);

				foreach (string file in Directory.GetFiles(imageFolder))
				{
					if (file.StartsWith(imageFolder + "" + CurrentDocumentInfo.FileName.Split('.')[0]) && file.EndsWith(imageExtenion))
					{
						try
						{
							int pageNum = 0;

							using (Pix pix = Pix.LoadFromFile(file))
							{
								using (Page page = engine.Process(pix))
								{
									string text;

									if (hocr)
										text = page.GetHOCRText(pageNum);
									else
										text = page.GetText();

									Output += "===========";
									Output += "Page " + (++pageNum);
									Output += ("Mean confidence: " + page.GetMeanConfidence());
									Output += "===========";
									Output += ("\n" + text);
								}
							}
						}
						catch (Exception e)
						{
							Trace.TraceError(e.ToString());
							MessageBox.Show("Error while OCR of file OutputImages/" + file + "\n" + e);
						}
					}
				}
			}
			catch (Exception e)
			{
				Trace.TraceError(e.ToString());
				MessageBox.Show("Error while trying to OCR: " + e);
			}
		}
	}
}
