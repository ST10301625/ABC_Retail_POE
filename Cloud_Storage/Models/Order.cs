using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using Azure;
using Azure.Data.Tables;

namespace Cloud_Storage.Models
{
    public class Order : ITableEntity
    {
        [Key]
        public int Order_Id { get; set; }

        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Introduce validation sample
        [Required(ErrorMessage = "Please select a customer.")]
        public int Customer_ID { get; set; } // FK to the Customer who made the order

        [Required(ErrorMessage = "Please select a product.")]
        public int Product_ID { get; set; } // FK to the Product being ordered

        [Required(ErrorMessage = "Please select the date.")]
        public DateTime Order_Date { get; set; }

        [Required(ErrorMessage = "Please enter the location.")]
        public string? Order_Location { get; set; }
    }
}
