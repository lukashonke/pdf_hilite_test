using System;
using System.Collections.Generic;
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

namespace NM_PDFHilite_Test
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private PdfDocumentInfo currentFile;
		private ParserType selectedType;

		public MainWindow()
		{
			InitializeComponent();
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
				string name = dlg.FileName;

				if (File.Exists(name))
				{
					PdfDocumentInfo doc = new PdfDocumentInfo();
					doc.FileName = name;
					currentFile = doc;

					StartProcessing();
					
					this.DataContext = currentFile;
				}
			}
		}

		private void StartProcessing()
		{
			if (currentFile == null)
			{
				MessageBox.Show("No file selected");
				return;
			}

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
	}

	public enum ParserType
	{
		PDFClown,
		PDFBox,
		Tesseract,
	}
}
