using Stripe;
using Stripe.Checkout;
using webapi.Services;

namespace webapi
{
    public static class StripeInfrastructure
    {
        public static IServiceCollection AddStripeInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            StripeConfiguration.ApiKey = configuration.GetValue<string>("StripeSettings:SecretKey");

            return services
                .AddScoped<CustomerService>()
                .AddScoped<ChargeService>()
                .AddScoped<TokenService>()
                .AddScoped<SessionService>()    // *
                .AddScoped<ProductService>()    // *
                .AddScoped<PriceService>()      //
                .AddScoped<IStripeAppService, StripeAppService>();
        }
    }
}
