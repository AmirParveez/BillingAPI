using System;
using System.Collections.Generic;

namespace ApiBilling.Models
{
    public class InvoiceModel
    {
        public string InvoiceNo { get; set; } = "";
        public DateTime InvoiceDate { get; set; }
        public int CustomerId { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }

        public int CreatedBy { get; set; }

        public List<InvoiceItemModel> Items { get; set; } = new();
    }
}
