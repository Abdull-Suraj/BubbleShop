# BubbleShop

WhatsApp Business Sales Agent backend built with ASP.NET Core 8 Web API and Clean Architecture.

## Prerequisites
- .NET SDK 8+
- SQL Server
- Docker & Docker Compose (optional)

## Solution layout
- `src/BubbleShop.Domain`
- `src/BubbleShop.Application`
- `src/BubbleShop.Infrastructure`
- `src/BubbleShop.API`
- `tests/*`

## Setup
1. Configure `src/BubbleShop.API/appsettings.json` values.
2. Restore packages:
   ```bash
   dotnet restore BubbleShop.slnx
   ```
3. Apply migrations:
   ```bash
   dotnet ef migrations add InitialCreate --project src/BubbleShop.Infrastructure --startup-project src/BubbleShop.API
   dotnet ef database update --project src/BubbleShop.Infrastructure --startup-project src/BubbleShop.API
   ```
4. Run API:
   ```bash
   dotnet run --project src/BubbleShop.API
   ```

## External services configuration
- `WhatsApp`: Graph API URL, phone number ID, access token, verify token, app secret.
- `AzureOpenAI`: endpoint, api key, deployment name, max tokens.
- `Stripe`: secret key, webhook secret, success and cancel URLs.
- `Jwt`: key, issuer, audience, expiry minutes.
- `Delivery`: provider api URL and api key.

## Environment variables
- `ConnectionStrings__DefaultConnection`
- `WhatsApp__ApiUrl`
- `WhatsApp__PhoneNumberId`
- `WhatsApp__AccessToken`
- `WhatsApp__VerifyToken`
- `WhatsApp__AppSecret`
- `AzureOpenAI__Endpoint`
- `AzureOpenAI__ApiKey`
- `AzureOpenAI__DeploymentName`
- `AzureOpenAI__MaxTokens`
- `Stripe__SecretKey`
- `Stripe__WebhookSecret`
- `Stripe__SuccessUrl`
- `Stripe__CancelUrl`
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__ExpiryMinutes`
- `Delivery__ProviderApiUrl`
- `Delivery__ApiKey`
