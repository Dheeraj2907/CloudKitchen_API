using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Dto
{
    public class RiderCreateRequest
    {
        public string Name { get; set; } = null!;
        public string Mobile { get; set; } = null!;
        public string? VehicleNumber { get; set; }
        public string? PhotoUrl { get; set; }
    }

    public class RiderAdminDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Mobile { get; set; } = null!;
        public string? VehicleNumber { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsAvailable { get; set; }
    }
}
