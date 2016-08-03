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

		public override void Process()
		{
			pdfFile = new File(CurrentDocumentInfo.Path);

			foreach (Page page in pdfFile.Document.Pages)
			{
				ProcessPageUsingExtractor(page);
				ProcessPage(page);

				Highlight(page, "Toplata");
				Highlight(page, "DOKUMENTARNA");
				Highlight(page, "že kdyby o tom rozhodoval");
				Highlight(page, "souhlasím s tím, že blokace musí");
			}

			pdfFile.Save(pdfFile.Path + "highlighted.pdf", SerializationModeEnum.Incremental);
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

						rawText += textString.Text + "";

						//PdfDataObject encoding = textString.Style.Font.BaseDataObject.Resolve(PdfName.Encoding);

						//PdfDictionary encodingDict = (PdfDictionary) encoding;

						rawText = CleanUpString(rawText);
					}
				}
				else if (content is ContainerObject)
				{
					Process(level.ChildLevel);
				}
			}
		}

		private void Highlight(Page page, string word, string source=null)
		{
			TextExtractor extractor = new TextExtractor(true, true);
			Regex pattern = new Regex(word, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);

			MatchCollection matches;

			if (source == null)
			{
				matches = pattern.Matches(CleanUpString(TextExtractor.ToString(textStrings)));
			}
			else
			{
				matches = pattern.Matches(source);
			}
			
			extractor.Filter(textStrings, new Filter(matches.GetEnumerator(), page));
		}

		private string CleanUpString(string parameter)
		{
			parameter = parameter.Replace((char)0xA0, ' ');

			return parameter;
		}
	}

	class Filter : TextExtractor.IIntervalFilter
	{
		private readonly IEnumerator matchEnumerator;
		private readonly Page page;

		public Filter(IEnumerator matchEnumerator, Page page)
		{
			this.matchEnumerator = matchEnumerator;
			this.page = page;
		}

		public bool MoveNext()
		{
			return matchEnumerator.MoveNext();
		}

		public Interval<int> Current
		{
			get
			{
				Match current = (Match)matchEnumerator.Current;
				return new Interval<int>(current.Index, current.Index + current.Length);
			}
		}

		public void Process(Interval<int> interval, ITextString match)
		{
			Debug.Write("highlighting word " + match.Text.ToString());

			IList<Quad> quads = new List<Quad>();

			RectangleF? textBox = null;
			foreach (TextChar textChar in match.TextChars)
			{
				RectangleF textCharBox = textChar.Box;
				if (!textBox.HasValue)
				{
					textBox = textCharBox;
				}
				else
				{
					if (textCharBox.Y > textBox.Value.Bottom)
					{
						quads.Add(Quad.Get(textBox.Value));
						textBox = textCharBox;
					}
					else
					{
						textBox = RectangleF.Union(textBox.Value, textCharBox);
					}
				}
			}

			quads.Add(Quad.Get(textBox.Value));

			new TextMarkup(page, quads, null, TextMarkup.MarkupTypeEnum.Highlight);
		}

		object IEnumerator.Current
		{
			get { return this.Current; }
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}
	}
}