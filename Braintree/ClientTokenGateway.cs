#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using Braintree.Exceptions;

namespace Braintree
{
    /// <summary>
    /// Generates client tokens, which are used to authenticate requests clients make directly
    ///   on behalf of merchants
    /// </summary>
    public class ClientTokenGateway : IClientTokenGateway
    {
        private readonly BraintreeService service;

        protected internal ClientTokenGateway(BraintreeGateway gateway)
        {
            gateway.Configuration.AssertHasAccessTokenOrKeys();
            service = new BraintreeService(gateway.Configuration);
        }

        public virtual string Generate(ClientTokenRequest request = null)
        {
            if (request == null) request = new ClientTokenRequest();
            VerifyOptions(request);
            XmlNode response = service.Post(service.MerchantPath() + "/client_token", request);

            if (response.Name.Equals("client-token")) {
                return Regex.Unescape(response.InnerText);
            } else {
                throw new ArgumentException(response.SelectSingleNode("message").InnerText);
            }
        }

        private void VerifyOptions(ClientTokenRequest request)
        {
            if (request.Options != null && request.CustomerId == null) {
                var invalidOptions = new List<string>{};

                if (request.Options.VerifyCard != null) {
                    invalidOptions.Add("VerifyCard");
                }
                if (request.Options.MakeDefault != null) {
                    invalidOptions.Add("MakeDefault");
                }
                if (request.Options.FailOnDuplicatePaymentMethod != null) {
                    invalidOptions.Add("FailOnDuplicatePaymentMethod");
                }

                if (invalidOptions.Count != 0) {
                    var message = "Following arguments are invalid without customerId: ";
                    foreach (var invalidOption in invalidOptions) {
                        message += " " + invalidOption;
                    }
                    throw new ArgumentException(message);
                }
            }
        }
    }
}
