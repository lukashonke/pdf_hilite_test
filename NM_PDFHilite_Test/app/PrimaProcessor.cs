using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM_PDFHilite_Test.app
{
	public class PrimaProcessor : PdfReader
	{
		private const string PATH_TO_PRIMA_TOOLS = @"C:/NewtonMedia/PRIMA/";

		private const string language = "ces";
		private const string imageFolder = "OutputImages/";
		private string imageExtension = ".png";

		public PrimaProcessor(PdfDocumentInfo doc) : base(doc)
		{
		}

		public override void Process()
		{
			// convert pdf to image
			// analyze image via Prima to layouts
			// extract layouts to different files
			// run OCR on these files
			// combine the text

			string filePath = CurrentDocumentInfo.Path;
			string fileName = CurrentDocumentInfo.FileName;

			foreach (string imgFile in Directory.GetFiles(imageFolder))
			{
				if (imgFile.StartsWith(imageFolder + "" + CurrentDocumentInfo.FileName.Split('.')[0]) &&
				    imgFile.EndsWith(imageExtension))
				{
					// convert to PAGE format
					string inputImage = imgFile;
					string outputImage = imageFolder + "out" + imageExtension;
					string outputXml = imageFolder + "out.xml";
					string recMode = "ocr-regions";

					Process p = new Process();
					p.StartInfo.FileName = PATH_TO_PRIMA_TOOLS + "tess-to-page/bin/prima.exe";
					p.StartInfo.Arguments = "-inp-img \"" + inputImage + "\" -out-img \"" + outputImage + "\" -out-xml \"" + outputXml + "\" -rec-mode " + recMode + " -lang " + language;
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.RedirectStandardOutput = true;
					p.Start();

					string output = p.StandardOutput.ReadToEnd();
					p.WaitForExit();

					p.Dispose();

					outputImage = "OutputImages\\out.png";
					outputXml = "OutputImages\\out.xml";
					string outputFolder = "" + imgFile.Split('.')[0];

					if (!Directory.Exists(outputFolder))
						Directory.CreateDirectory(outputFolder);

					// extract layouts to files
					p = new Process();
					p.StartInfo.FileName = PATH_TO_PRIMA_TOOLS + "tess-exporter/extractor.exe";
					p.StartInfo.Arguments = "-extract imageSnippets -image ./" + outputImage + " -page-content ./" + outputXml +
					                        " -filter-by type -filter text -output-folder ./" + outputFolder;
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.RedirectStandardOutput = true;
					p.Start();

					output = p.StandardOutput.ReadToEnd();
					p.WaitForExit();

					p = new Process();
					p.StartInfo.FileName = PATH_TO_PRIMA_TOOLS + "tess-exporter/extractor.exe";
					p.StartInfo.Arguments = "-export text -page-content ./" + outputXml + " -output-folder ./" + outputFolder;
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.RedirectStandardOutput = true;
					p.Start();

					output = p.StandardOutput.ReadToEnd();
					p.WaitForExit();

					string rawText = System.IO.File.ReadAllText(outputFolder + "/out.txt");
					Output = rawText;
				}
			}
		}

	}
}
