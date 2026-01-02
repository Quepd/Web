using Web.Models;

namespace Web.ViewModels
{
	public class ConfirmationViewModel
	{
		public List<CartItem> CartItems { get; set; }
		public decimal Total { get; set; }
		public List<string> PaymentMethods { get; set; }
		public List<string> ShippingAddresses { get; set; }

		// Các lựa chọn người dùng:
		public string SelectedPaymentMethod { get; set; }
		public string SelectedAddress { get; set; }
	}

}
