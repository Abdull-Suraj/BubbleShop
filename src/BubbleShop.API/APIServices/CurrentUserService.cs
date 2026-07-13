using BubbleShop.Application.Common.Interfaces;

using System.Security.Claims;

namespace BubbleShop.API.APIServices;



public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid BusinessId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(value))
                throw new UnauthorizedAccessException("Business is not authenticated.");

            return Guid.Parse(value);
        }
    }

    public string Email =>
        _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    public string BusinessName =>
        _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
}