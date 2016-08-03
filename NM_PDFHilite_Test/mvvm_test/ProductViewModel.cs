using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NM_PDFHilite_Test
{
	public class ProductViewModel : ObservableObject
	{
		private int productId;
		private ProductModel currentProduct;

		private ICommand getProductCommand;
		private ICommand saveProductCommand;

		public ProductModel CurrentProduct
		{
			get { return currentProduct; }

			set
			{
				if (value != currentProduct)
				{
					currentProduct = value;
					OnPropertyChanged("CurrentProduct");
				}
			}
		}

		public int ProductId
		{
			get { return productId; }

			set
			{
				if (value != productId)
				{
					productId = value;
					OnPropertyChanged("ProductId");
				}
			}
		}

		public ICommand GetProductCommand
		{
			get
			{
				if (getProductCommand == null)
				{
					//getProductCommand = new RelayCommand(Param)
				}
				return null;
			}
		}

		public ICommand SaveProductCommand
		{
			get
			{
				if (getProductCommand == null)
				{
					//getProductCommand = new RelayCommand(Param)
				}
				return null;
			}
		}

		private void GetProduct()
		{
			ProductModel p = new ProductModel();
			p.ProductId = ProductId;
			p.ProductName = "Test";
			p.UnitPrice = 10;

			CurrentProduct = p;
		}

		private void SaveProduct()
		{
			// implement save here
		}
	}
}
