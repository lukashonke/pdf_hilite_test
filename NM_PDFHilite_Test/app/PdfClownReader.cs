using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.files;
using org.pdfclown.objects;
using org.pdfclown.tools;
using org.pdfclown.util.math;
using org.pdfclown.util.math.geom;

namespace NM_PDFHilite_Test.app
{
	public class PdfClownReader : PdfReader
	{
		private File pdfFile;
		private IDictionary<RectangleF?, IList<ITextString>> textStrings;

		private string rawText;

		public PdfClownReader(PdfDocumentInfo doc) : base(doc)
		{
		}

		public string RawText
		{
			get { return rawText; }
		}

		public override void Process()
		{
			pdfFile = new File(CurrentDocumentInfo.Path);

			foreach (Page page in pdfFile.Document.Pages)
			{
				ProcessPageUsingExtractor(page);
				ProcessPage(page);
			}
		}

		private void ProcessPageUsingExtractor(Page page)
		{
			TextExtractor extractor = new TextExtractor(true, true);
			textStrings = new Dictionary<RectangleF?, IList<ITextString>>();

			try
			{
				textStrings = extractor.Extract(page);
			}
			catch (Exception e)
			{
				MessageBox.Show("chyba extrakce textu - " + e);
				throw;
			}

			StringBuilder sb = new StringBuilder();
			foreach (IList<ITextString> list in textStrings.Values)
			{
				foreach (ITextString str in list)
					sb.Append(str.Text + "\n");

				sb.Append("====\n");
			}

			Output += sb.ToString();
		}

		private void ProcessPage(Page page)
		{
			ContentScanner level = new ContentScanner(page);

			Process(level);
		}

		private void Process(ContentScanner level)
		{
			if (level == null)
				return;

			while (level.MoveNext())
			{
				ContentObject content = level.Current;

				if (content is Text)
				{
					ContentScanner.TextWrapper text = (ContentScanner.TextWrapper)level.CurrentWrapper;

					foreach (ContentScanner.TextStringWrapper textString in text.TextStrings)
					{
						RectangleF textStringBox = textString.Box.Value;

						Output += "\nText ["
						              + "x:" + Math.Round(textStringBox.X) + ","
						              + "y:" + Math.Round(textStringBox.Y) + ","
						              + "w:" + Math.Round(textStringBox.Width) + ","
						              + "h:" + Math.Round(textStringBox.Height)
						              + "] [font size:" + Math.Round(textString.Style.FontSize) + "]: " + textString.Text;


						string temp = textString.Text.TrimEnd();
						string txt;

						if (temp.EndsWith("-") || temp.EndsWith("–") || temp.EndsWith("-"))
						{
							txt = temp.Substring(0, temp.Length - 1); // cut off the last letter
						}
						else
						{
							txt = textString.Text;
						}

						rawText += txt + "";

						//PdfDataObject encoding = textString.Style.Font.BaseDataObject.Resolve(PdfName.Encoding);

						//PdfDictionary encodingDict = (PdfDictionary) encoding;

						rawText = Utils.CleanUpString(rawText);
					}
				}
				else if (content is ContainerObject)
				{
					Process(level.ChildLevel);
				}
			}
		}
	}
}