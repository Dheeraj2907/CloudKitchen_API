using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Dto
{
    public class ApiException : Exception
    {
        public ApiException(string? message) : base(message)
        {
        }
    }
}
