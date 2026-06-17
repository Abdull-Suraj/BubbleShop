
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Domain.Interfaces.Repositories;
using BubbleShop.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace BubbleShop.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;
    private readonly SmtpClient _smtpClient;

    public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;

        // Initialize SMTP client
        _smtpClient = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.EnableSsl,
            UseDefaultCredentials = _options.UseDefaultCredentials,
            Timeout = _options.Timeout
        };

        if (!string.IsNullOrEmpty(_options.SmtpUsername))
        {
            _smtpClient.Credentials = new NetworkCredential(
                _options.SmtpUsername,
                _options.SmtpPassword
            );
        }
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        await SendEmailAsync(to, subject, body, false, cancellationToken);
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending email to {To}: {Subject}", to, subject);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml,
                Priority = MailPriority.Normal
            };

            mailMessage.To.Add(to);

            await _smtpClient.SendMailAsync(mailMessage, cancellationToken);

            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "SMTP error sending email to {To}: {StatusCode}", to, smtpEx.StatusCode);
            throw new Exception($"Failed to send email: {smtpEx.Message}", smtpEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", to);
            throw;
        }
    }

    public async Task SendOrderConfirmationAsync(string to, string orderNumber, decimal amount, CancellationToken cancellationToken = default)
    {
        var subject = $"Order #{orderNumber} Confirmed! 🎉";
        var body = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: #4CAF50; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .order-details {{ background: #f5f5f5; padding: 15px; border-radius: 5px; }}
                    .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    .button {{ display: inline-block; padding: 10px 20px; background: #4CAF50; color: white; text-decoration: none; border-radius: 5px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>🎉 Order Confirmed!</h1>
                    </div>
                    <div class='content'>
                        <h2>Thank you for your order!</h2>
                        <div class='order-details'>
                            <p><strong>Order Number:</strong> #{orderNumber}</p>
                            <p><strong>Total Amount:</strong> {amount:C}</p>
                        </div>
                        <p>We'll notify you when your order is ready for delivery.</p>
                        <br/>
                        <p>Track your order status anytime in your account.</p>
                        <br/>
                        <p>Thank you for shopping with us! 🙏</p>
                        <p><strong>- BubbleShop Team</strong></p>
                    </div>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} BubbleShop. All rights reserved.</p>
                        <p>This is an automated message, please do not reply.</p>
                    </div>
                </div>
            </body>
            </html>
        ";

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendPaymentConfirmationAsync(string to, string transactionReference, decimal amount, CancellationToken cancellationToken = default)
    {
        var subject = $"Payment Confirmed - {transactionReference}";
        var body = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: #2196F3; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .payment-details {{ background: #f5f5f5; padding: 15px; border-radius: 5px; }}
                    .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Payment Successful!</h1>
                    </div>
                    <div class='content'>
                        <h2>Your payment has been confirmed</h2>
                        <div class='payment-details'>
                            <p><strong>Transaction Reference:</strong> {transactionReference}</p>
                            <p><strong>Amount:</strong> {amount:C}</p>
                        </div>
                        <p>Your order is now being processed.</p>
                        <br/>
                        <p>Thank you for your purchase!</p>
                        <p><strong>- BubbleShop Team</strong></p>
                    </div>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} BubbleShop. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>
        ";

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(string to, string name, CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to BubbleShop! 🎉";
        var body = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: #FF9800; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    .button {{ display: inline-block; padding: 10px 20px; background: #FF9800; color: white; text-decoration: none; border-radius: 5px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1> Welcome to BubbleShop!</h1>
                    </div>
                    <div class='content'>
                        <h2>Hello {name}!</h2>
                        <p>We're excited to have you on board! 🚀</p>
                        <p>You can now start shopping and enjoy our amazing products.</p>
                        <br/>
                        <p>Here's what you can do:</p>
                        <ul>
                            <li>🛍️ Browse our products</li>
                            <li>💰 Check out our deals</li>
                            <li>📦 Track your orders</li>
                        </ul>
                        <br/>
                        <p>Start exploring today!</p>
                        <p><strong>- BubbleShop Team</strong></p>
                    </div>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} BubbleShop. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>
        ";

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendPasswordResetAsync(string to, string resetToken, string resetLink, CancellationToken cancellationToken = default)
    {
        var subject = "Password Reset Request";
        var fullResetLink = $"{resetLink}?token={resetToken}";

        var body = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: #F44336; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .button {{ display: inline-block; padding: 12px 25px; background: #F44336; color: white; text-decoration: none; border-radius: 5px; }}
                    .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    .warning {{ background: #fff3cd; padding: 15px; border-radius: 5px; border-left: 4px solid #ffc107; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Password Reset Request</h1>
                    </div>
                    <div class='content'>
                        <p>We received a request to reset your password.</p>
                        <p>Click the button below to reset your password:</p>
                        <br/>
                        <a href='{fullResetLink}' class='button'>Reset Password</a>
                        <br/><br/>
                        <div class='warning'>
                            <p><strong>⚠️ This link will expire in 1 hour.</strong></p>
                        </div>
                        <br/>
                        <p>If you didn't request this, please ignore this email.</p>
                        <br/>
                        <p><strong>- BubbleShop Team</strong></p>
                    </div>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} BubbleShop. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>
        ";

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendDeliveryUpdateAsync(string to, string orderNumber, string status, string? trackingNumber = null, CancellationToken cancellationToken = default)
    {
        var statusEmoji = status.ToLower() switch
        {
            "shipped" => "🚚",
            "out_for_delivery" => "📦",
            "delivered" => "✅",
            _ => "📋"
        };

        var subject = $"Delivery Update - Order #{orderNumber}";
        var body = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: #9C27B0; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .status-details {{ background: #f5f5f5; padding: 15px; border-radius: 5px; }}
                    .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>{statusEmoji} Delivery Update</h1>
                    </div>
                    <div class='content'>
                        <h2>Order #{orderNumber}</h2>
                        <div class='status-details'>
                            <p><strong>Status:</strong> {status}</p>
                            {(trackingNumber != null ? $"<p><strong>Tracking Number:</strong> {trackingNumber}</p>" : "")}
                        </div>
                        <br/>
                        <p>Track your order anytime in your account.</p>
                        <br/>
                        <p><strong>- BubbleShop Team</strong></p>
                    </div>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} BubbleShop. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>
        ";

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    // Dispose SMTP client
    public void Dispose()
    {
        _smtpClient?.Dispose();
    }
}