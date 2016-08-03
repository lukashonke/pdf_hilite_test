using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM_PDFHilite_Test
{
	public class ProductModel : ObservableObject
	{
		private int productId;
		private string productName;
		private decimal unitPrice;
 
		#region Properties
 
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
 
		public string ProductName
		{
			get { return productName; }
			set
			{
				if (value != productName)
				{
					productName = value;
					OnPropertyChanged("ProductName");
				}
			}
		}
 
		public decimal UnitPrice
		{
			get { return unitPrice; }
			set
			{
				if (value != unitPrice)
				{
					unitPrice = value;
					OnPropertyChanged("UnitPrice");
				}
			}
		}
 
		#endregion // Properties
	}
}
