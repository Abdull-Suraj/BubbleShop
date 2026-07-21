using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Domain.Interfaces.Repositories;
using BubbleShop.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace BubbleShop.Infrastructure.Services;

public sealed class EmailService : IEmailService, IDisposable
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;
    private readonly SmtpClient _smtpClient;

    public EmailService(
        IOptions<EmailOptions> options,
        ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;

        _smtpClient = new SmtpClient(
            _options.SmtpHost,
            _options.SmtpPort)
        {
            EnableSsl = _options.EnableSsl,
            UseDefaultCredentials = _options.UseDefaultCredentials,
            Timeout = _options.Timeout
        };

        if (!string.IsNullOrWhiteSpace(_options.SmtpUsername))
        {
            _smtpClient.Credentials =
                new NetworkCredential(
                    _options.SmtpUsername,
                    _options.SmtpPassword);
        }
    }


    public async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        await SendEmailAsync(
            to,
            subject,
            body,
            false,
            cancellationToken);
    }


    public async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Sending email to {Email}. Subject: {Subject}",
                to,
                subject);


            using var mailMessage = new MailMessage
            {
                From = new MailAddress(
                    _options.FromEmail,
                    _options.FromName),

                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml,
                Priority = MailPriority.Normal
            };


            mailMessage.To.Add(to);


            await _smtpClient.SendMailAsync(
                mailMessage,
                cancellationToken);


            _logger.LogInformation(
                "Email sent successfully to {Email}",
                to);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(
                ex,
                "SMTP error sending email to {Email}",
                to);

            throw new Exception(
                $"Email sending failed: {ex.Message}",
                ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error sending email to {Email}",
                to);

            throw;
        }
    }



    public async Task SendOrderConfirmationAsync(
        string to,
        string orderNumber,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var subject =
            $"Order #{orderNumber} Confirmed 🎉";


        var body = $@"
<html>
<body style='font-family:Arial'>

<h2>🎉 Order Confirmed!</h2>

<p>Thank you for your order.</p>

<hr/>

<p>
<strong>Order Number:</strong> #{orderNumber}
</p>

<p>
<strong>Total Amount:</strong> {amount:C}
</p>

<p>
We will notify you when your order is ready.
</p>


<br/>

<p>
Regards,<br/>
<strong>BubbleShop Team</strong>
</p>

</body>
</html>
";


        await SendEmailAsync(
            to,
            subject,
            body,
            true,
            cancellationToken);
    }



    public async Task SendPaymentConfirmationAsync(
        string to,
        string transactionReference,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var subject =
            $"Payment Confirmed - {transactionReference}";


        var body = $@"
<html>
<body style='font-family:Arial'>

<h2>✅ Payment Successful</h2>


<p>Your payment has been confirmed.</p>


<p>
<strong>Transaction Reference:</strong>
{transactionReference}
</p>


<p>
<strong>Amount:</strong>
{amount:C}
</p>


<p>
Your order is now being processed.
</p>


<br/>

<p>
Thank you for shopping with BubbleShop.
</p>


<p>
<strong>BubbleShop Team</strong>
</p>


</body>
</html>
";


        await SendEmailAsync(
            to,
            subject,
            body,
            true,
            cancellationToken);
    }



    public async Task SendWelcomeEmailAsync(
        string to,
        string name,
        CancellationToken cancellationToken = default)
    {
        var subject =
            "Welcome to BubbleShop 🎉";


        var body = $@"
<html>
<body style='font-family:Arial'>


<h2>Welcome {name}! 🚀</h2>


<p>
We are happy to have you join BubbleShop.
</p>


<p>
You can now:
</p>


<ul>
<li>Browse products</li>
<li>Add items to cart</li>
<li>Create orders</li>
<li>Track deliveries</li>
</ul>


<br/>

<p>
<strong>BubbleShop Team</strong>
</p>


</body>
</html>
";


        await SendEmailAsync(
            to,
            subject,
            body,
            true,
            cancellationToken);
    }




    public async Task SendPasswordResetAsync(
        string to,
        string resetToken,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        var subject =
            "Password Reset Request";


        var link =
            $"{resetLink}?token={resetToken}";


        var body = $@"
<html>
<body style='font-family:Arial'>


<h2>Password Reset</h2>


<p>
We received a request to reset your password.
</p>


<p>
Click the link below:
</p>


<a href='{link}'>
Reset Password
</a>


<p>
This link expires in 1 hour.
</p>


<p>
If you did not request this, ignore this email.
</p>


<br/>

<p>
<strong>BubbleShop Team</strong>
</p>


</body>
</html>
";


        await SendEmailAsync(
            to,
            subject,
            body,
            true,
            cancellationToken);
    }




    public async Task SendDeliveryUpdateAsync(
        string to,
        string orderNumber,
        string status,
        string? trackingNumber = null,
        CancellationToken cancellationToken = default)
    {

        var emoji = status.ToLower() switch
        {
            "shipped" => "🚚",
            "out_for_delivery" => "📦",
            "delivered" => "✅",
            _ => "📋"
        };


        var subject =
            $"Delivery Update Order #{orderNumber}";


        var body = $@"
<html>
<body style='font-family:Arial'>


<h2>
{emoji} Delivery Update
</h2>


<p>
<strong>Order:</strong>
#{orderNumber}
</p>


<p>
<strong>Status:</strong>
{status}
</p>


<p>
<strong>Tracking:</strong>
{trackingNumber ?? "Not available"}
</p>


<br/>

<p>
Thank you for using BubbleShop.
</p>


<p>
<strong>BubbleShop Team</strong>
</p>


</body>
</html>
";


        await SendEmailAsync(
            to,
            subject,
            body,
            true,
            cancellationToken);
    }




    public void Dispose()
    {
        _smtpClient.Dispose();
    }
}