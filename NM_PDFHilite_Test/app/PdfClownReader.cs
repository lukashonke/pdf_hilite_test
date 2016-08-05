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
using Font = org.pdfclown.documents.contents.fonts.Font;

namespace NM_PDFHilite_Test.app
{
	/// <summary>
	/// extracts all readable text
	/// bugs:
	/// - no OCR (doesnt extract unreadable text)
	/// </summary>
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
				ProcessPageUsingExtractor(page, true);

				Output += "\n\n=========== \n\n Alternate method using sorting: \n\n";
				ProcessPageUsingExtractor(page, false);

				Output += "\n\n=========== \n\n Raw text cords:\n\n";
				ProcessPage(page);
			}
		}

		private void ProcessPageUsingExtractor(Page page, bool alternate)
		{
			TextExtractor extractor = new TextExtractor(true, true);
			textStrings = new Dictionary<RectangleF?, IList<ITextString>>();

			try
			{
				if (alternate)
					textStrings = ProcessMethod3(page);
				else 
					textStrings = extractor.Extract(page);
			}
			catch (Exception e)
			{
				MessageBox.Show("chyba extrakce textu - " + e);
				throw;
			}

			string rawText = TextExtractor.ToString(textStrings);
			Output += "======= raw text:\n\n" + rawText + "\n\n ========= multiline text: \n\n";

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

			rawText += "\n\n==========\n\n";
			rawText += "=== Alternative method (should be the same): \n\n";

			ProcessMethod2(new ContentScanner(page));
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


						string txt = Utils.DehyphenateLine(textString.Text);
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

		private void ProcessMethod2(ContentScanner level)
		{
			if (level == null)
				return;

			while (level.MoveNext())
			{
				ContentObject content = level.Current;
				if (content is ShowText)
				{
					Font font = level.State.Font;

					string text = font.Decode(((ShowText) content).Text);

					string txt = Utils.DehyphenateLine(text);
					rawText += txt + "";

					rawText = Utils.CleanUpString(rawText);
				}
				else if (content is Text || content is ContainerObject)
				{
					// Scan the inner level!
					ProcessMethod2(level.ChildLevel);
				}
			}
		}

		private IDictionary<RectangleF?, IList<ITextString>> ProcessMethod3(Page page)
		{
			// output with text positions
			IDictionary<RectangleF?, IList<ITextString>> extractedTextStrings;

			List<ContentScanner.TextStringWrapper> rawTextStrings = new List<ContentScanner.TextStringWrapper>();
			Extract(new ContentScanner(page), rawTextStrings);

			List<ITextString> textStrings = new List<ITextString>();

			foreach (ContentScanner.TextStringWrapper rawTextString in rawTextStrings)
			{
				/*string txt = rawTextString.Text;
				txt = Utils.DehyphenateLine(txt);
				rawTextString.Text = txt;*/

				textStrings.Add(rawTextString);
			}

			extractedTextStrings = new Dictionary<RectangleF?, IList<ITextString>>();
			extractedTextStrings[TextExtractor.DefaultArea] = textStrings;

			return extractedTextStrings;
		}

		private void Extract(ContentScanner level, IList<ContentScanner.TextStringWrapper> extractedTextStrings)
		{
			if (level == null)
				return;

			while (level.MoveNext())
			{
				ContentObject content = level.Current;
				if (content is Text)
				{
					// Collect the text strings!
					foreach (ContentScanner.TextStringWrapper textString in ((ContentScanner.TextWrapper)level.CurrentWrapper).TextStrings)
					{
						extractedTextStrings.Add(textString);
					}
				}
				else if (content is XObject) // external object
				{
					Extract(((XObject)content).GetScanner(level), extractedTextStrings);
				}
				else if (content is ContainerObject) // scan inner objects
				{
					Extract(level.ChildLevel, extractedTextStrings);
				}
			}
		}
	}
}