using System.ComponentModel.DataAnnotations;

namespace ShopHub.API.DTOs
{
    public class CheckoutDto
    {
        [Required]
        [MinLength(5)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}