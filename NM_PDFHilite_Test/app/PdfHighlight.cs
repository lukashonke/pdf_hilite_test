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
using System.Windows.Documents;
using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.files;
using org.pdfclown.tools;
using org.pdfclown.util.math;
using org.pdfclown.util.math.geom;
using org.pdfclown.util.metadata;

namespace NM_PDFHilite_Test.app
{
	/// <summary>
	/// highlighting using PDFClown library
	/// bugs: 
	/// - no undecodable text highlighting (only readable text)
	/// </summary>
	public class PdfHighlight_Clown : PdfReader
	{
		private List<string> words;

		private IDictionary<RectangleF?, IList<ITextString>> textStrings;

		public PdfHighlight_Clown(PdfDocumentInfo doc, List<string> words)
			: base(doc)
		{
			this.words = words;
		}

		public PdfHighlight_Clown(PdfDocumentInfo doc, params string[] w)
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
					//textStrings = extractor.Extract(page); // alternative method, buggy
					textStrings = ProcessMethod3(page);
				}
				catch (Exception e)
				{
					MessageBox.Show("chyba extrakce textu - " + e);
					throw;
				}

				foreach (string word in words)
				{
					// ignorovat pomlcky
					string result = "";
					for (int i = 0; i < word.Length; i++)
					{
						if (i + 1 < word.Length)
							result += word[i] + @"\-*";
						else
							result += word[i];
					}

					Debug.Write(result);

					Regex pattern = new Regex(result, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);
					MatchCollection matches;

					bool dehyphenated = false;
					//TODO buggy: characters are shifted due to dehyphenation thus highlighting is shifted too
					// found a better way using regexes propably, needs testing
					if (dehyphenated) 
					{
						List<SearchableString> searchables = ConvertToSearchable(textStrings);
						string rawTextToSearch = ToString(searchables);

						matches = pattern.Matches(Utils.CleanUpString(rawTextToSearch));

						// debug output
						Output += "=========== \n\n text to search for highlighting \n\n";
						Output += rawTextToSearch;
						// end debug output

						FilterAndHighlight(searchables, new Filter(matches.GetEnumerator(), page)); // my method
					}
					else
					{
						string rawTextToSearch = TextExtractor.ToString(textStrings);

						matches = pattern.Matches(Utils.CleanUpString(rawTextToSearch));

						// debug output
						Output += "=========== \n\n text to search for highlighting \n\n";
						Output += rawTextToSearch;
						// end debug output

						extractor.Filter(textStrings, new Filter(matches.GetEnumerator(), page)); // PDFClown's method
					}
				}
			}

			pdfFile.Save(pdfFile.Path + "highlighted.pdf", SerializationModeEnum.Incremental);
		}

		private void FilterAndHighlight(List<SearchableString> textStrings, Filter filter)
		{
			IEnumerator<SearchableString> textStringsIterator = textStrings.GetEnumerator();
			if (!textStringsIterator.MoveNext())
				return;

			IList<TextChar> textChars = textStringsIterator.Current.OriginalText.TextChars;
			string searchableString = textStringsIterator.Current.SearchableText;

			// prohledavat pomoci searchableText
			// v pripade shody zavolat process na ActualText

			// kazdymu slovu priradit jeho SearchableTvar - ten porovnavat pri vyhledavani, ale pouzivat jinak boxy puvodniho slova

			int baseTextCharIndex = 0;
			int textCharIndex = 0;

			while (filter.MoveNext())
			{
				Interval<int> interval = filter.Current; // 1642, 1637
				TextString match = new TextString();

				int matchStartIndex = interval.Low;
				int matchEndIndex = interval.High;

				while (matchStartIndex > baseTextCharIndex + textChars.Count)
				{
					baseTextCharIndex += textChars.Count;

					if (!textStringsIterator.MoveNext())
					{
					}

					textChars = textStringsIterator.Current.OriginalText.TextChars;
				}

				textCharIndex = matchStartIndex - baseTextCharIndex;

				while (baseTextCharIndex + textCharIndex < matchEndIndex)
				{
					if (textCharIndex == textChars.Count)
					{
						baseTextCharIndex += textChars.Count;

						if (!textStringsIterator.MoveNext())
						{
						}

						textChars = textStringsIterator.Current.OriginalText.TextChars;
						textCharIndex = 0;
					}

					match.TextChars.Add(textChars[textCharIndex++]);
				}

				filter.Process(interval, match);
			}
		}

		private string ToString(List<SearchableString> strings)
		{
			StringBuilder textBuilder = new StringBuilder();
			foreach (SearchableString str in strings)
			{
				textBuilder.Append(str.SearchableText.TrimStart());

				// TODO: has bugs with headlines
				if (str.SearchableText.EndsWith(".") || str.SearchableText.EndsWith("!") || str.SearchableText.EndsWith("?") || str.SearchableText.EndsWith("\""))
					textBuilder.Append(" ");
			}
			return textBuilder.ToString();
		}

		private List<SearchableString> ConvertToSearchable(IDictionary<RectangleF?, IList<ITextString>> strings)
		{
			List<SearchableString> searchables = new List<SearchableString>();

			foreach (KeyValuePair<RectangleF?, IList<ITextString>> e in strings)
			{
				foreach (ITextString origString in e.Value)
				{
					string searchableString = origString.Text;
					searchableString = Utils.DehyphenateLine(searchableString);

					SearchableString newString = new SearchableString(searchableString, origString);
					searchables.Add(newString);
				}
			}

			return searchables;
		}

		public class Filter : TextExtractor.IIntervalFilter
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

		private IDictionary<RectangleF?, IList<ITextString>> ProcessMethod3(Page page)
		{
			IDictionary<RectangleF?, IList<ITextString>> extractedTextStrings;
			List<ITextString> textStrings = new List<ITextString>();

			List<ContentScanner.TextStringWrapper> rawTextStrings = new List<ContentScanner.TextStringWrapper>();

			Extract(new ContentScanner(page), rawTextStrings);

			foreach (ContentScanner.TextStringWrapper rawTextString in rawTextStrings)
			{
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

	public class SearchableString
	{
		private string searchableText;
		private ITextString actualText;

		public SearchableString(string searchableText, ITextString actualText)
		{
			this.searchableText = searchableText;
			this.actualText = actualText;
		}

		public string SearchableText
		{
			get { return searchableText; }
		}

		public ITextString OriginalText
		{
			get { return actualText; }
		}
	}

	public class TextString : ITextString
	{
		private List<TextChar> textChars = new List<TextChar>();

		public RectangleF? Box
		{
			get
			{
				RectangleF? box = null;
				foreach (TextChar textChar in textChars)
				{
					if (!box.HasValue)
					{ box = (RectangleF?)textChar.Box; }
					else
					{ box = RectangleF.Union(box.Value, textChar.Box); }
				}
				return box;
			}
		}

		public string Text
		{
			get
			{
				StringBuilder textBuilder = new StringBuilder();
				foreach (TextChar textChar in textChars)
				{ textBuilder.Append(textChar); }
				return textBuilder.ToString();
			}
		}

		public List<TextChar> TextChars
		{
			get
			{ return textChars; }
		}
	}
}
