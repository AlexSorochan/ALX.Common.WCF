using System.ServiceModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Security.Cryptography.X509Certificates;

namespace ALX.Common.WCF
{
    public static class Extensions
    {
        /// <summary>
        /// Указать сертификат сервиса
        /// </summary>
        /// <typeparam name="TService">Контракт WCF-сервиса</typeparam>
        /// <param name="client">Клиент WCF-сервиса</param>
        /// <param name="certificate">Сертификат WCF-сервиса</param>
        internal static void SetServiceCertificate<TService>(this ServiceClientWrapper<TService> client, X509Certificate2 certificate) where TService : class
        {
            if (certificate == null)
            {
                return;
            }

            EndpointIdentity clientEndpointIdentity = new X509CertificateEndpointIdentity(certificate);
            client.Endpoint.Address = new EndpointAddress(uri: client.Endpoint.Address.Uri, identity: clientEndpointIdentity);
        }

        /// <summary>
        /// Указать заголовки исходящих сообщений
        /// </summary>
        /// <typeparam name="TService">Контракт WCF-сервиса</typeparam>
        /// <param name="client">Клиент WCF-сервиса</param>
        /// <param name="headers">Заголовки исходящих сообщений</param>
        internal static void SetOutGoingMessageHeaders<TService>(this ServiceClientWrapper<TService> client, List<SimpleOutMessageHeader> headers) where TService : class
        {
            if (headers == null || headers.Count <= default(int))
            {
                return;
            }

            MessageHeaders messageHeadersElement = OperationContext.Current?.OutgoingMessageHeaders ?? throw new WarningException("Контекст вызова не объявлен!");

            headers.ForEach(x =>
            {
                messageHeadersElement.Add(MessageHeader.CreateHeader(x.Name, x.NameSpace, x.Value));
            });
        }
    }
}
