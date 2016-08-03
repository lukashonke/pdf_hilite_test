using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM_PDFHilite_Test
{
	public abstract class ObservableObject : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

		protected virtual void OnPropertyChanged(string propName)
		{
			this.VerifyPropertyName(propName);

			if (this.PropertyChanged != null)
			{
				PropertyChangedEventArgs e = new PropertyChangedEventArgs(propName);
				this.PropertyChanged(this, e);
			}
		}

		public virtual void VerifyPropertyName(string propertyName)
		{
			if (TypeDescriptor.GetProperties(this)[propertyName] == null)
			{
				string msg = "Invalid property name! " + propertyName;

				if (this.ThrowOnInvalidPropertyName)
					throw new Exception(msg);
				else
					Debug.Fail(msg);
			}
		}
	}
}
