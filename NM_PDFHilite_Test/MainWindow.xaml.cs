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

		public bool showMetadata;
		public bool convertToImg;

		private string status;

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

			status = "Creating image from PDF";
			worker.ReportProgress(0);

			if (convertToImg || selectedType == ParserType.Tesseract)
			{
				ConvertPdfToImage conv = new ConvertPdfToImage(currentFile);
				conv.Process();
			}

			if (showMetadata)
			{
				status = "Creating metadata";
				worker.ReportProgress(25);

				MetadataReader meta = new MetadataReader(currentFile);
				meta.Process();

				currentFile.Parameters = meta.Output;
			}

			status = "Running PDF processor";
			worker.ReportProgress(50);

			PdfReader reader = null;

			switch (selectedType)
			{
				case ParserType.PDFClown:

					reader = new PdfClownReader(currentFile);
					reader.Process();

					break;
				case ParserType.PDFBox:

					//TODO ?? 

					break;
				case ParserType.Tesseract:

					reader = new OcrTest(currentFile);
					reader.Process();

					break;
			}

			string text = reader.Output;
			currentFile.Text = text;

			worker.ReportProgress(100);
		}

		private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			// update info in the app
			ProgramOutput.Text = currentFile.Text;
			Metadata.Text = currentFile.Parameters;
		}

		private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			ProgramOutput.Text = e.ProgressPercentage + "% done\n";
			ProgramOutput.Text += "(" + status + ")";
		}

		private void StartProcessing()
		{
			if (currentFile == null)
			{
				MessageBox.Show("No file selected");
				return;
			}

			worker.RunWorkerAsync();
		}

		private void PDFClown_Click(object sender, RoutedEventArgs e)
		{
			selectedType = ParserType.PDFClown;

			Parser1.IsChecked = true;
			Parser2.IsChecked = false;
			Tesseract.IsChecked = false;
		}

		private void PDFBox_Click(object sender, RoutedEventArgs e)
		{
			selectedType = ParserType.PDFBox;

			Parser1.IsChecked = false;
			Parser2.IsChecked = true;
			Tesseract.IsChecked = false;
		}

		private void Tesseract_Click(object sender, RoutedEventArgs e)
		{
			selectedType = ParserType.Tesseract;

			Parser1.IsChecked = false;
			Parser2.IsChecked = false;
			Tesseract.IsChecked = true;
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
		PDFClown,
		PDFBox,
		Tesseract,
	}
}
