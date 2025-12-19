using ApiBilling.Helpers;
using ApiBilling.Models;
using Microsoft.Data.SqlClient;

namespace ApiBilling.BLL
{
    public class InvoiceBLL
    {
        private readonly SqlHelper _sql;

        public InvoiceBLL(SqlHelper sql)
        {
            _sql = sql;
        }

        public int InsertInvoice(InvoiceModel model)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@InvoiceNo", model.InvoiceNo),
                new SqlParameter("@InvoiceDate", model.InvoiceDate),
                new SqlParameter("@CustomerId", model.CustomerId),
                new SqlParameter("@TotalAmount", model.TotalAmount),
                new SqlParameter("@Discount", model.Discount),
                new SqlParameter("@TaxAmount", model.TaxAmount),
                new SqlParameter("@NetAmount", model.NetAmount),
                new SqlParameter("@PaidAmount", model.PaidAmount),
                new SqlParameter("@CreatedBy", model.CreatedBy)
            };

            int invoiceId = Convert.ToInt32(
                _sql.ExecuteScalar("sp_InsertInvoice", p)
            );

            foreach (var item in model.Items)
            {
                SqlParameter[] ip =
                {
                    new SqlParameter("@InvoiceId", invoiceId),
                    new SqlParameter("@ItemId", item.ItemId),
                    new SqlParameter("@Quantity", item.Quantity),
                    new SqlParameter("@Rate", item.Rate)
                };

                _sql.ExecuteNonQuery("sp_InsertInvoiceItem", ip);
            }

            return invoiceId;
        }
    }
}
