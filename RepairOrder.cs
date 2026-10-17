using System;

namespace Bai411_QLDKDVSC
{
    public class RepairOrder
    {
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string ErrorDescription { get; set; } = string.Empty;
        public DateTime ReceivedDate { get; set; } = DateTime.Now;
        public decimal EstimatedCost { get; set; }
        public string Status { get; set; } = "Mới tiếp nhận";
        public string ImagePath { get; set; } = string.Empty;
    }
}