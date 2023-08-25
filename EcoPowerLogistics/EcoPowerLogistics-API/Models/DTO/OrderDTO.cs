﻿namespace EcoPowerLogistics_API.Models.DTO
{
    public class OrderDTO
    {
        public short OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public short CustomerId { get; set; }
        public string? DeliveryAddress { get; set; }
    }
}
