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
	public class TesseractOCRReader : PdfReader
	{
		// ces | eng
		private const string language = "ces";
		private const string imageFolder = "OutputImages/";
		private const bool hocr = true;

		private string imageExtenion = ".png";

		private string ocrOutput;
		private string hocrOutput;

		public TesseractOCRReader(PdfDocumentInfo doc) : base(doc)
		{
			if (Settings.USE_TIF_FORMAT)
				imageExtenion = ".tif";
		}

		public string OcrOutput
		{
			get { return ocrOutput; }
		}

		public string HocrOutput
		{
			get { return hocrOutput; }
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
									ocrOutput += "===========";
									ocrOutput += "Page " + (++pageNum);
									ocrOutput += ("Mean confidence: " + page.GetMeanConfidence());
									ocrOutput += "===========";

									hocrOutput = page.GetHOCRText(pageNum);
									ocrOutput += page.GetText();
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

				/*foreach (string file in Directory.GetFiles(imageFolder + "" + CurrentDocumentInfo.FileName.Split('.')[0]))
				{
					if (file.EndsWith(imageExtenion))
					{
						try
						{
							int pageNum = 0;

							using (Pix pix = Pix.LoadFromFile(file))
							{
								using (Page page = engine.Process(pix))
								{
									ocrOutput += "===========";
									ocrOutput += "Page " + (++pageNum);
									ocrOutput += ("Mean confidence: " + page.GetMeanConfidence());
									ocrOutput += "===========";

									hocrOutput = page.GetHOCRText(pageNum);
									ocrOutput += page.GetText();
								}
							}
						}
						catch (Exception e)
						{
							Trace.TraceError(e.ToString());
							MessageBox.Show("Error while OCR of file OutputImages/" + file + "\n" + e);
						}
					}
				}*/
			}
			catch (Exception e)
			{
				Trace.TraceError(e.ToString());
				MessageBox.Show("Error while trying to OCR: " + e);
			}
		}
	}
}
