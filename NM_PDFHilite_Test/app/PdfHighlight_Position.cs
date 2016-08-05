using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.pdfclown.documents;
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.files;
using org.pdfclown.util.math.geom;

namespace NM_PDFHilite_Test.app
{
	/// <summary>
	/// zvyrazneni na danem miste v dokumentu
	/// </summary>
	public class PdfHighlight_Position : PdfReader
	{
		public PdfHighlight_Position(PdfDocumentInfo doc) : base(doc)
		{
		}

		public override void Process()
		{
			File pdfFile = new File(CurrentDocumentInfo.Path);

			Page page = pdfFile.Document.Pages[0];

			IList<Quad> quads = new List<Quad>();

			RectangleF rect = new RectangleF(66, 770, 84, 8);
			Quad quad = Quad.Get(rect);

			quads.Add(quad);

			new TextMarkup(page, quads, null, TextMarkup.MarkupTypeEnum.Highlight);

			pdfFile.Save(pdfFile.Path + "pos_highlighted.pdf", SerializationModeEnum.Incremental);
		}
	}
}
