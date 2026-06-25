using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.Enums
{

  
    public enum Intent
    {
        CreateOrder,
        SearchProduct,
        GetProductPrice,
        CheckStock,
        TrackOrder,
        CancelOrder,
        GetHelp,
        ViewCart,
        AddToCart,
        RemoveFromCart,
        Checkout,
        ApplyCoupon,
        GetStoreHours,
        ContactSupport,
        ProvideFeedback,
        JustChatting,
        Unknown
    }
}
