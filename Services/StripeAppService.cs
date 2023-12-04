using Azure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using webapi.Dtos.Stripe;
using webapi.Models.Stripe;

namespace webapi.Services
{
    public class StripeAppService : IStripeAppService
    {
        private readonly ChargeService _chargeService;
        private readonly CustomerService _customerService;
        private readonly TokenService _tokenService;
        private readonly SessionService _sessionService;
        private readonly ProductService _productService;
        private readonly PriceService _priceService;

        public StripeAppService(
            ChargeService chargeService,
            CustomerService customerService,
            TokenService tokenService,
            SessionService sessionService,
            ProductService productService,
            PriceService priceService)
        {
            _chargeService = chargeService;
            _customerService = customerService;
            _tokenService = tokenService;
            _sessionService = sessionService;
            _productService = productService;
            _priceService = priceService;
        }

        public async Task<StripeCustomer> AddStripeCustomerAsync(AddStripeCustomer customer, CancellationToken ct)
        {
            // Set Stripe Token options based on customer data
            /*TokenCreateOptions tokenOptions = new TokenCreateOptions
            {
                Card = new TokenCardOptions
                {
                    Name = customer.Name,
                    Number = customer.CreditCard.CardNumber,
                    ExpYear = customer.CreditCard.ExpirationYear,
                    ExpMonth = customer.CreditCard.ExpirationMonth,
                    Cvc = customer.CreditCard.Cvc,
                }
            };*/

            // Create new Stripe Token
            //Token stripeToken = await _tokenService.CreateAsync(tokenOptions, null, ct);

            // Set Customer options using
            CustomerCreateOptions customerOptions = new CustomerCreateOptions
            {
                Name = customer.Name,
                Email = customer.Email,
                //Source = stripeToken.Id
            };

            // *
            var newCustomer = _customerService.ListAsync(null, null, ct).Result.FirstOrDefault(c => c.Email == customer.Email);
            if(newCustomer != null) {
                return new StripeCustomer(null!, null!, null!);
            }

            // Create customer at Stripe
            Customer createdCustomer = await _customerService.CreateAsync(customerOptions, null, ct);

           
            // Return the created customer at stripe
            return new StripeCustomer(createdCustomer.Name, createdCustomer.Email, createdCustomer.Id);
        }


        public async Task<StripePayment> AddStripePaymentAsync(AddStripePayment payment, CancellationToken ct)
        {
            // Set the options for the payment we would like to create at Stripe
            ChargeCreateOptions paymentOptions = new ChargeCreateOptions
            {
                Customer = payment.CustomerId,
                ReceiptEmail = payment.ReceiptEmail,
                Description = payment.Description,
                Currency = payment.Currency,
                Amount = payment.Amount
            };

            // Create the payment
            var createdPayment = await _chargeService.CreateAsync(paymentOptions, null, ct);

            // Return the payment to requesting method
            return new StripePayment(
              createdPayment.CustomerId,
              createdPayment.ReceiptEmail,
              createdPayment.Description,
              createdPayment.Currency,
              createdPayment.Amount,
              createdPayment.Id);
        }


        public async Task<StripeSession> AddStripeSessionAsync(AddStripeSession session, CancellationToken ct)
        {
            List<SessionLineItemOptions> lineItems = new List<SessionLineItemOptions>();
            foreach (var item in session.Products)
            {
                lineItems.Add(new SessionLineItemOptions { Price = item.Item1, Quantity = item.Item2 });
            }

            SessionCreateOptions sessionOptions = new SessionCreateOptions
            {
                SuccessUrl = session.SuccessUrl, // + "{CHECKOUT_SESSION_ID}",  // *
                LineItems = lineItems,
                Mode = "payment",
                CustomerEmail = session.CustomerEmail,
                CancelUrl = session.CancelUrl,
                Customer = session.Customer,
                // *             
                PhoneNumberCollection = new SessionPhoneNumberCollectionOptions
                {
                    Enabled = true,
                },
                ShippingAddressCollection = new SessionShippingAddressCollectionOptions
                {
                    AllowedCountries = new List<string> { "US", "CA" },
                },
                BillingAddressCollection = "required",
            };

            var createdSession = await _sessionService.CreateAsync(sessionOptions, null, ct);

            return new StripeSession(createdSession.Id, createdSession.CustomerEmail, createdSession.CustomerId, createdSession.SuccessUrl, createdSession.CancelUrl);
        }

        public async Task<GetStripeSession> GetStripeSessionStatusAsync(string id, CancellationToken ct)    // StripeList<LineItem>
        {
            //var lineItems = await _sessionService.ListLineItemsAsync(id);
            var options = new SessionGetOptions();
            //options.AddExpand("line_items");
            //options.AddExpand("customer");
            var session = await _sessionService.GetAsync(id, options, null, ct);

            return new GetStripeSession(session.Status, session.ShippingDetails.Name);
        }

        public async Task<StripeProduct> AddStripeProductAsync(AddStripeProduct product, CancellationToken ct)
        {
            ProductCreateOptions productOptions = new ProductCreateOptions
            {
                Name = product.Name,
                Description = product.Description,

            };
            var createdProduct = await _productService.CreateAsync(productOptions, null, ct);

            return new StripeProduct(createdProduct.Id, createdProduct.Name, createdProduct.Description);
        }

        public async Task<StripePrice> AddStripePriceAsync(AddStripePrice price, CancellationToken ct)
        {
            PriceCreateOptions priceOptions = new PriceCreateOptions
            {
                UnitAmount = price.UnitAmount,
                Currency = price.Currency,
                Product = price.Product,
            };
            var createdPrice = await _priceService.CreateAsync(priceOptions, null, ct);
            return new StripePrice(createdPrice.Id, createdPrice.UnitAmount, createdPrice.Currency, createdPrice.ProductId);
        }
    }
}
