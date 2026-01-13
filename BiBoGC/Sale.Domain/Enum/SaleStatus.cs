using System;
using System.Collections.Generic;
using System.Text;

namespace Sale.Domain.Enum
{
    public enum SaleStatus
    {
        Pending = 1,      // Đang xử lý
        Completed = 2,    // Hoàn thành
        Cancelled = 3,    // Đã hủy
        Refunded = 4      // Đã hoàn tiền


    }
}
