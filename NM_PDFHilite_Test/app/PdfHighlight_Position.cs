using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using org.pdfclown.documents;
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.files;
using org.pdfclown.util.math.geom;

namespace NM_PDFHilite_Test.app
{
	/// <summary>
	/// zvyrazneni na danem miste v dokumentu
	/// TODO: pridat podporu pro zvyrazneni slovosledu
	/// </summary>
	public class PdfHighlight_Position : PdfReader
	{
		private List<OCRWordData> wordsData;
		private List<string> wordsToHighlight; 

		/// <summary>
		/// 
		/// </summary>
		/// <param name="wordsToHighlight">null to highlight all words that were recognized by OCR</param>
		public PdfHighlight_Position(PdfDocumentInfo doc, List<OCRWordData> wordsData, List<string> wordsToHighlight) : base(doc)
		{
			this.wordsData = wordsData;
			this.wordsToHighlight = wordsToHighlight;
		}

		public override void Process()
		{
			File pdfFile = new File(CurrentDocumentInfo.Path);

			foreach (Page page in pdfFile.Document.Pages)
			{
				IList<Quad> quads = new List<Quad>();

				int documentWidth;
				int documentHeight;

				try
				{
					documentWidth = (int)page.Size.Width;
					documentHeight = (int)page.Size.Height;

					//documentWidth = pdfFile.Document.PageSize.Value.Width;
					//documentHeight = pdfFile.Document.PageSize.Value.Height;
				}
				catch (Exception e)
				{
					MessageBox.Show("cannot determine size of the PDF document!\n" + e);
					return;
				}

				// 1929 1819 2353 1918
				// [x:66,y:436,w:500,h:29]
				// 300 DPI

				foreach (OCRWordData wordData in wordsData)
				{
					if (wordsToHighlight == null || wordsToHighlight.Contains(wordData.Word))
					{
						RectangleF rect = Utils.RecalculatePosition(documentWidth, documentHeight, wordData.WordPosition.SourceWidth, wordData.WordPosition.SourceHeight, wordData.WordPosition.X1,
						wordData.WordPosition.Y1, wordData.WordPosition.X2, wordData.WordPosition.Y2);

						Quad quad = Quad.Get(rect);

						quads.Add(quad);

						new TextMarkup(page, quads, null, TextMarkup.MarkupTypeEnum.Highlight);
					}
				}
			}

			pdfFile.Save(pdfFile.Path + "_pos_highlighted.pdf", SerializationModeEnum.Incremental);
		}
	}

	public struct WordPos
	{
		private int x1, y1, x2, y2;
		private int sourceWidth, sourceHeight;

		public WordPos(int x1, int y1, int x2, int y2, int sourceWidth, int sourceHeight)
		{
			this.x1 = x1;
			this.y1 = y1;
			this.x2 = x2;
			this.y2 = y2;
			this.sourceWidth = sourceWidth;
			this.sourceHeight = sourceHeight;
		}

		public int X1
		{
			get { return x1; }
		}

		public int Y1
		{
			get { return y1; }
		}

		public int X2
		{
			get { return x2; }
		}

		public int Y2
		{
			get { return y2; }
		}

		public int SourceWidth
		{
			get { return sourceWidth; }
		}

		public int SourceHeight
		{
			get { return sourceHeight; }
		}
	}
}
