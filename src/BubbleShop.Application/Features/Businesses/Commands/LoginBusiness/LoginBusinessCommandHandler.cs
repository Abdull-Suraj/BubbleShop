using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using BubbleShop.Domain.Entities;

namespace BubbleShop.Application.Features.Businesses.Commands.LoginBusiness;

public sealed class LoginBusinessCommandHandler : IRequestHandler<LoginBusinessCommand, Result<LoginResponseDto>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginBusinessCommandHandler> _logger;

    public LoginBusinessCommandHandler(
        IBusinessRepository businessRepository,
        IConfiguration configuration,
        ILogger<LoginBusinessCommandHandler> logger)
    {
        _businessRepository = businessRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginBusinessCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var business = await _businessRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (business is null)
                return Result<LoginResponseDto>.Failure("Invalid email or password", "Unauthorized");

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, business.PasswordHash))
            {
                _logger.LogWarning("Invalid password for email: {Email}", request.Email);

                return Result<LoginResponseDto>.Failure(
                    "Invalid email or password",
                    "Unauthorized");
            }


          
            // Check account status
            if (business.Status != BusinessStatus.Active &&
                business.Status != BusinessStatus.Pending)
            {
                return Result<LoginResponseDto>.Failure(
                    "Account is not active. Please contact support.",
                    "Unauthorized");
            }
            // Generate JWT token
            var token = GenerateJwtToken(business);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            _logger.LogInformation("Login successful for business: {BusinessId} - {BusinessName}", business.Id, business.BusinessName);

            return Result<LoginResponseDto>.Success(new LoginResponseDto
            {
                Token = token,
                BusinessId = business.Id,
                BusinessName = business.BusinessName,
                Email = business.Email,
                ExpiresAt = expiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return Result<LoginResponseDto>.Failure($"Login failed: {ex.Message}");
        }
    }

    private string GenerateJwtToken(Business business)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(24);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, business.Id.ToString()),
            new Claim(ClaimTypes.Name, business.BusinessName),
            new Claim(ClaimTypes.Email, business.Email),

            new Claim("whatsappNumber", business.WhatsAppNumber),

            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
};

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}