using System;

namespace global_ktable
{
    public class EnrichedOrder
	{
		private string _productName;
		private int _productId;
		private string _customerName;
		private int _customerId;
		private int _orderId;

        public EnrichedOrder(String productName, int productId, string customerName, int customerId,  int orderId)
        {
			_productName = productName;
			_productId = productId;
			_customerId = customerId;
			_customerName = customerName;
			_orderId = orderId;
        }

    	public string productName
		{
			get
			{
				return this._productName;
			}
			set
			{
				this._productName = value;
			}
		}
		public int productId
		{
			get
			{
				return this._productId;
			}
			set
			{
				this._productId = value;
			}
		}
		public string customerName
		{
			get
			{
				return this._customerName;
			}
			set
			{
				this._customerName = value;
			}
		}
		public int customerId
		{
			get
			{
				return this._customerId;
			}
			set
			{
				this._customerId = value;
			}
		}
		public int orderId
		{
			get
			{
				return this._orderId;
			}
			set
			{
				this._orderId = value;
			}
		}
	}
}
