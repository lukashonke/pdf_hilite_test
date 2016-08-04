using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.tools;
using org.pdfclown.util.math;
using org.pdfclown.util.math.geom;

namespace NM_PDFHilite_Test.app
{
	public class PdfHighlight : PdfReader
	{
		private Page selectedPage;
		private List<string> words; 

		public PdfHighlight(PdfDocumentInfo doc, List<string> words) : base(doc)
		{
			this.words = words;
		}

		public void SetPage(Page page)
		{
			this.selectedPage = page;
		}

		public override void Process()
		{
			Page page = null;

			// load PDF pages again if not loaded yet
			// for every page, try to highlight all the words

			if (selectedPage != null)
				page = selectedPage;
			else
			{
					
			}

			TextExtractor extractor = new TextExtractor(true, true);

			foreach (string word in words)
			{
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
		}

		private string CleanUpString(string parameter)
		{
			parameter = parameter.Replace((char)0xA0, ' ');

			return parameter;
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
