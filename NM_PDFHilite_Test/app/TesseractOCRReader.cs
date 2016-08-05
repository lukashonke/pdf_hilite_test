using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.TextFormatting;
using ImageMagick;
using Tesseract;
using Rect = Tesseract.Rect;

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
		private List<OCRWordData> data;

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

		public List<OCRWordData> WordData
		{
			get { return data; }
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

									data = ExtractWordData(page, pix.Width, pix.Height);
									//TODO return back? 

									ocrOutput += "\n\n========= \n\n word positions: \n\n";

									foreach (OCRWordData word in data)
									{
										ocrOutput += word.Word + " [" + word.WordPosition.X1 + ";" + word.WordPosition.Y1 + ";" + word.WordPosition.X2 + ";" + word.WordPosition.Y2 + "]\n";
									}
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

		private List<OCRWordData> ExtractWordData(Page page, int sourceWidth, int sourceHeight)
		{
			List<OCRWordData> data = new List<OCRWordData>();

			using(ResultIterator iter = page.GetIterator())
			{
				iter.Begin();

				/*while (iter.Next(PageIteratorLevel.Block))
				{
					while (iter.Next(PageIteratorLevel.Para))
					{
						while (iter.Next(PageIteratorLevel.TextLine))
						{*/
							while (iter.Next(PageIteratorLevel.Word))
							{
								if (iter.IsAtBeginningOf(PageIteratorLevel.Word))
								{
									float confidence = iter.GetConfidence(PageIteratorLevel.Word) / 100;

									Rect bounds;
									if (iter.TryGetBoundingBox(PageIteratorLevel.Word, out bounds))
									{
										OCRWordData word = new OCRWordData(new WordPos(bounds.X1, bounds.Y1, bounds.X2, bounds.Y2, sourceWidth, sourceHeight), iter.GetText(PageIteratorLevel.Word));
										data.Add(word);
									}
									else
									{
										MessageBox.Show("cant find bounds for word " + iter.GetText(PageIteratorLevel.Word));
									}
								}
							}
						/*}
					}
				}*/
			}
			return data;
		}
	}

	public class OCRWordData
	{
		private string word;
		private WordPos wordPosition;

		public OCRWordData(WordPos wordPosition, string word)
		{
			this.wordPosition = wordPosition;
			this.word = word;
		}

		public WordPos WordPosition
		{
			get { return wordPosition; }
		}

		public string Word
		{
			get { return word; }
		}
	}
}
