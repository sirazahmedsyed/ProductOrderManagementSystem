﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProductOrderManagementSystem.Infrastructure.Entities
{
    public class Order
    {
        public Guid OrderId { get; set; }

        public DateTime OrderDate { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string? CustomerName { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }

        [NotMapped]
        public decimal TotalAmount => OrderDetails?.Sum(od => od.Quantity * od.Product.Price * (1 + od.Product.TaxPercentage / 100)) ?? 0;

        [NotMapped]
        public decimal DiscountedTotal => TotalAmount * (1 - DiscountPercentage / 100);
    }
}
