using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using NM_PDFHilite_Test.app;
using Path = System.IO.Path;

namespace NM_PDFHilite_Test
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private PdfDocumentInfo currentFile;
		private ParserType selectedType;

		private readonly BackgroundWorker worker;

		// whether to load metadata or not 
		public bool showMetadata;
		// convert to image (forced true when OCRing)
		public bool convertToImg;

		// whats going on atm
		private string status;

		private List<string> wordsToHighlight;
		private string mainOutput, ocrOutput, hocrOutput, rawTextOutput, primaOutput;

		public MainWindow()
		{
			InitializeComponent();

			worker = new BackgroundWorker();
			worker.DoWork += Worker_DoWork;
			worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
			worker.ProgressChanged += Worker_ProgressChanged;
			worker.WorkerReportsProgress = true;

			this.showMetadata = ShowMetadata.IsChecked;
			this.convertToImg = ConvertPDF.IsChecked;
			
			Settings.USE_TIF_FORMAT = TifFormat.IsChecked;

			selectedType = ParserType.PDFClown;
		}

		public ParserType SelectedType
		{
			get { return selectedType; }
		}

		private void OpenFileItem_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.DefaultExt = ".pdf";
			dlg.Filter = "PDF Files (.pdf)|*.pdf|All Files (*.*)|*.*";

			bool? result = dlg.ShowDialog();

			if (result == true)
			{
				string path = dlg.FileName;

				if (File.Exists(path))
				{
					PdfDocumentInfo doc = new PdfDocumentInfo();
					doc.Path = path;
					doc.FileName = Path.GetFileName(path);
					currentFile = doc;

					StartProcessing();

					this.DataContext = currentFile;
				}
			}
		}

		private void Worker_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = (BackgroundWorker) sender;

			if (convertToImg || selectedType == ParserType.Tesseract || selectedType == ParserType.Prima)
			{
				status = "Creating images from PDF";
				worker.ReportProgress(0);

				ConvertPdfToImage conv = new ConvertPdfToImage(currentFile);
				conv.Process();
			}

			if (showMetadata)
			{
				status = "Reading metadata";
				worker.ReportProgress(25);

				MetadataReader meta = new MetadataReader(currentFile);
				meta.Process();

				currentFile.Parameters = meta.Output;
			}

			status = "Running PDF processor";
			worker.ReportProgress(50);

			if (selectedType == ParserType.All)
			{
				foreach (ParserType typ in Enum.GetValues(typeof(ParserType)))
				{
					if(typ == ParserType.All) continue;

					RunParser(typ);
				}
			}
			else
			{
				RunParser(selectedType);
			}

			status = "Highlighting words...";
			worker.ReportProgress(75);

			if (wordsToHighlight.Count > 0)
			{
				PdfHighlight highlight = new PdfHighlight(currentFile, wordsToHighlight);
				highlight.Process();
			}

			status = "Done";
			worker.ReportProgress(100);
		}

		private void RunParser(ParserType type)
		{
			PdfReader reader = null;

			switch (type)
			{
				case ParserType.PDFClown:

					reader = new PdfClownReader(currentFile);
					reader.Process();

					mainOutput = reader.Output;
					rawTextOutput = ((PdfClownReader)reader).RawText;

					break;
				case ParserType.PDFBox:

					//TODO ?? results will be very similar to PDFClown

					break;
				case ParserType.Tesseract:

					reader = new TesseractOCRReader(currentFile);
					reader.Process();

					ocrOutput = ((TesseractOCRReader)reader).OcrOutput;
					hocrOutput = ((TesseractOCRReader)reader).HocrOutput;

					break;
				case ParserType.Prima:

					reader = new PrimaProcessor(currentFile);
					reader.Process();

					primaOutput = reader.Output;

					break;
			}
		}

		private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			// update info in the app
			MainOutput.Text = currentFile.Text;
			Metadata.Text = currentFile.Parameters;

			MainOutput.Text = this.mainOutput;
			OCROutput.Text = this.ocrOutput;
			HOCROutput.Text = this.hocrOutput;
			RawTextOutput.Text = this.rawTextOutput;
			PRIMAOutput.Text = this.primaOutput;
		}

		private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			MainOutput.Text = e.ProgressPercentage + "% done\n";
			MainOutput.Text += "(" + status + ")";
		}

		private void StartProcessing()
		{
			if (currentFile == null)
			{
				MessageBox.Show("No file selected");
				return;
			}

			wordsToHighlight = new List<string>();

			if (WordsToHighlight.Text.Length > 0)
			{
				foreach (string word in WordsToHighlight.Text.Split(new[] { '\r', '\n' }))
				{
					if(word.Length > 0)
						wordsToHighlight.Add(word);
				}
			}

			worker.RunWorkerAsync();
		}

		private void RunAll_Click(object sender, RoutedEventArgs e)
		{
			selectedType = ParserType.All;

			Parser1.IsChecked = true;
			Parser2.IsChecked = true;
			Tesseract.IsChecked = true;
			Prima.IsChecked = true;
		}

		private void PDFClown_Click(object sender, RoutedEventArgs e)
		{
			selectedType = ParserType.PDFClown;

			Parser1.IsChecked = true;
			Parser2.IsChecked = false;
			Tesseract.IsChecked = false;
			Prima.IsChecked = false;
		}

		private void PDFBox_Click(object sender, RoutedEventArgs e)
		{
			selectedType = ParserType.PDFBox;

			Parser1.IsChecked = false;
			Parser2.IsChecked = true;
			Tesseract.IsChecked = false;
			Prima.IsChecked = false;
		}

		private void Tesseract_Click(object sender, RoutedEventArgs e)
		{
			selectedType = ParserType.Tesseract;

			Parser1.IsChecked = false;
			Parser2.IsChecked = false;
			Tesseract.IsChecked = true;
			Prima.IsChecked = false;
		}

		private void Prima_Click(object sender, RoutedEventArgs e)
		{
			selectedType = ParserType.Prima;

			Parser1.IsChecked = false;
			Parser2.IsChecked = false;
			Tesseract.IsChecked = false;
			Prima.IsChecked = true;
		}

		private void ShowMetadata_Click(object sender, RoutedEventArgs e)
		{
			showMetadata = ShowMetadata.IsChecked;
		}

		private void ConvertPdf_Click(object sender, RoutedEventArgs e)
		{
			convertToImg = ConvertPDF.IsChecked;
		}

		private void TifFormat_Click(object sender, RoutedEventArgs e)
		{
			Settings.USE_TIF_FORMAT = TifFormat.IsChecked;
		}
	}

	public enum ParserType
	{
		All,
		PDFClown,
		PDFBox,
		Tesseract,
		Prima
	}
}
