using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageMagick;

namespace NM_PDFHilite_Test.app
{
	public class ConvertPdfToImage : PdfReader
	{
		public ConvertPdfToImage(PdfDocumentInfo doc) : base(doc)
		{
			
		}

		public override void Process()
		{
			SavePdfToImage();
		}

		private void SavePdfToImage()
		{
			try
			{
				if (!Directory.Exists("OutputImages"))
					Directory.CreateDirectory("OutputImages");

				int page = 1;

				MagickReadSettings settings = new MagickReadSettings();
				settings.Density = new Density(300, 300);
				//settings.FrameIndex = page - 1;
				//settings.FrameCount = 1;

				using (MagickImageCollection images = new MagickImageCollection())
				{
					images.Read(CurrentDocumentInfo.Path, settings);

					foreach (MagickImage image in images)
					{
						if (Settings.USE_TIF_FORMAT)
						{
							// Writing to a specific format works the same as for a single image
							image.Format = MagickFormat.Ptif;
							image.Write("OutputImages/" + CurrentDocumentInfo.FileName + ".Page" + page + ".tif");
						}
						else
							image.Write("OutputImages/" + CurrentDocumentInfo.FileName + ".Page" + page + ".png");

						page++;
					}
				}
			}
			catch (Exception e)
			{
				Trace.TraceError(e.ToString());
				MessageBox.Show("Error while saving pdf to img: " + e);
			}
		}
	}
}
