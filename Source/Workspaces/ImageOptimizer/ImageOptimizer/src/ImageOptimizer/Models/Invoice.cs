using System;
using System.ComponentModel.DataAnnotations;

namespace ImageOptimizer.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public string Item { get; set; }
        //public string Status { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; } = DateTime.Now;

        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal GrossAmount { get; set; }

        public string TransactionId { get; set; }
        //public string PaymentMethod { get; set; }

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}