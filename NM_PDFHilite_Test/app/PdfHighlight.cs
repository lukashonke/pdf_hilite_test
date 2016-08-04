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
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.files;
using org.pdfclown.tools;
using org.pdfclown.util.math;
using org.pdfclown.util.math.geom;

namespace NM_PDFHilite_Test.app
{
	public class PdfHighlight : PdfReader
	{
		private List<string> words;

		private IDictionary<RectangleF?, IList<ITextString>> textStrings;

		public PdfHighlight(PdfDocumentInfo doc, List<string> words)
			: base(doc)
		{
			this.words = words;
		}

		public PdfHighlight(PdfDocumentInfo doc, params string[] w)
			: base(doc)
		{
			words = new List<string>();

			foreach (string word in w)
			{
				words.Add(word);
			}
		}

		public override void Process()
		{
			File pdfFile = new File(CurrentDocumentInfo.Path);

			TextExtractor extractor = new TextExtractor(true, true);

			foreach (Page page in pdfFile.Document.Pages)
			{
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

				foreach (string word in words)
				{
					Regex pattern = new Regex(word, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);

					MatchCollection matches;

					matches = pattern.Matches(Utils.CleanUpString(TextExtractor.ToString(textStrings)));

					extractor.Filter(textStrings, new Filter(matches.GetEnumerator(), page));
				}
			}

			pdfFile.Save(pdfFile.Path + "highlighted.pdf", SerializationModeEnum.Incremental);
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
}
