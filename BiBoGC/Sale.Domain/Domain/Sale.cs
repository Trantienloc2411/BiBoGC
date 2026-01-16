using Sale.Domain.Enum;
using Shared.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sale.Domain.Domain
{
    public class Sale : BaseEntity
    {
        public string InvoiceNumber { get; private set; }
        public Guid? CustomerId { get; private set; }
        public DateTime SaleDate { get; private set; }
        public decimal SubTotal { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public decimal TaxAmount { get; private set; }
        public decimal TotalAmount { get; private set; }
        public decimal AmountPaid { get; private set; }
        public decimal ChangeAmount => AmountPaid - TotalAmount;
        public PaymentMethod PaymentMethod { get; private set; }
        public SaleStatus Status { get; private set; }
        public string? Notes { get; private set; }

        // Navigation
        public Customer? Customer { get; private set; }
        private readonly List<SaleItem> _items = new();
        public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

        private Sale() { }

        public Sale(
            string invoiceNumber,
            PaymentMethod paymentMethod,
            Guid? customerId = null,
            string? notes = null)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
                throw new ArgumentException("Số hoá đơn không được để trống.");

            InvoiceNumber = invoiceNumber;
            CustomerId = customerId;
            PaymentMethod = paymentMethod;
            Notes = notes?.Trim();
            SaleDate = DateTime.UtcNow;
            Status = SaleStatus.Pending;
        }

        public void AddItem(
            Guid productId,
            Guid? productBatchId,
            string productName,
            string sku,
            int quantity,
            decimal unitPrice,
            decimal discountPercent = 0)
        {
            if (Status != SaleStatus.Pending)
                throw new InvalidOperationException("Không thể thêm sản phẩm vào đơn hàng đã hoàn thành hoặc hủy.");

            var existingItem = _items.FirstOrDefault(i => i.ProductId == productId && i.ProductBatchId == productBatchId);
            if (existingItem != null)
            {
                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            }
            else
            {
                var item = new SaleItem(Id, productId, productBatchId, productName, sku, quantity, unitPrice);
                _items.Add(item);
            }

            RecalculateTotals();
        }

        public void RemoveItem(Guid itemId)
        {
            if (Status != SaleStatus.Pending)
                throw new InvalidOperationException("Không thể xóa sản phẩm khỏi đơn hàng đã hoàn thành hoặc hủy.");

            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                _items.Remove(item);
                RecalculateTotals();
            }
        }

        public void Complete(decimal amountPaid)
        {
            if (Status != SaleStatus.Pending)
                throw new InvalidOperationException("Đơn hàng đã được xử lý.");

            if (!_items.Any())
                throw new InvalidOperationException("Không thể hoàn thành đơn hàng trống.");

            if (amountPaid < TotalAmount)
                throw new InvalidOperationException($"Số tiền thanh toán ({amountPaid:N0}) không đủ. Cần thanh toán {TotalAmount:N0}.");

            AmountPaid = amountPaid;
            Status = SaleStatus.Completed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel(string? reason = null)
        {
            if (Status == SaleStatus.Cancelled)
                throw new InvalidOperationException("Đơn hàng đã bị hủy.");

            Status = SaleStatus.Cancelled;
            Notes = string.IsNullOrEmpty(Notes) ? reason : $"{Notes} | Lý do hủy: {reason}";
            UpdatedAt = DateTime.UtcNow;
        }

        private void RecalculateTotals()
        {
            SubTotal = _items.Sum(i => i.LineTotal);
            TotalAmount = SubTotal - DiscountAmount + TaxAmount;
            UpdatedAt = DateTime.UtcNow;
        }

    }
}
