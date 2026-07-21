using System;
using System.Collections.Generic;
using System.Text;

namespace BubbleShop.Domain.Entities
{
    public sealed class WhatsAppSettings
    {
        public string AccessToken { get; set; } = string.Empty;

        public string PhoneNumberId { get; set; } = string.Empty;

        public string VerifyToken { get; set; } = string.Empty;

        public string ApiVersion { get; set; } = "v23.0";
    }
}
