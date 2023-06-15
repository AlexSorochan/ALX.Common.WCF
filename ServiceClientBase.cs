using System;
using System.ServiceModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.ServiceModel.Security;
using static ALX.Common.WCF.Enums;
using System.Security.Cryptography.X509Certificates;

namespace ALX.Common.WCF
{
    /// <summary>
    /// Базовый класс клиента WCF-сервиса
    /// </summary>
    /// <typeparam name="TContract">Контракт WCF-сервиса</typeparam>
    /// <typeparam name="TError">Класс ошибки WCF-сервиса</typeparam>
    public abstract class ServiceClientBase<TContract, TError> where TContract : class where TError : class
    {
        private const string TimeoutExceptionMessage = "Тайм-аут канала запроса во время ожидания ответа.";
        private const string EndpointNotFoundExceptionMessage = "Невозможно подключиться к удаленному серверу.";

        /// <summary>
        /// Имя пользователя
        /// </summary>
        protected string UserName;

        /// <summary>
        /// Пароль
        /// </summary>
        protected string Password;

        /// <summary>
        /// Сертификат пользователя
        /// </summary>
        protected X509Certificate2 ClientCertificate;

        /// <summary>
        /// Сертификат сервиса
        /// </summary>
        protected X509Certificate2 ServiceCertificate;

        /// <summary>
        /// Способ аутентификации
        /// </summary>
        protected LoginMethods LoginMethod;

        #region ABSTRACT

        /// <summary>
        /// Вызов WCF сервиса
        /// </summary>
        /// <typeparam name="TResult">Тип возвращаемого значения</typeparam>
        /// <param name="invocation">Лямбда выражение вызова</param>
        /// <returns></returns>
        public abstract TResult CallService<TResult>(Func<TContract, TResult> invocation);

        public abstract void CallService(Action<TContract> invocation);

        /// <summary>
        /// Чтение параметров типизированого исключения WCF-сервиса
        /// </summary>
        /// <param name="error">Типизированое исключение</param>
        /// <returns></returns>
        protected abstract string ParsServiceFaultException(TError error);

        /// <summary>
        /// Привязка без аутентификации
        /// </summary>
        protected abstract string NonAuthBindingName { get; }

        /// <summary>
        /// Привязка с аутентификацией по имени пользователя
        /// </summary>
        protected abstract string AuthByUserNameBindingName { get; }

        /// <summary>
        /// Привязка с аутентификацией по сертификату
        /// </summary>
        protected abstract string AuthByCertificateBindingName { get; }

        #endregion

        #region PROTECTED

        /// <summary>
        /// Вызов WCF сервиса
        /// </summary>
        /// <typeparam name="TResult">Возвращаемый тип</typeparam>
        /// <param name="invocation">Лямбда выражение вызова WCF сервиса</param>
        /// <param name="outMsgHeaders">Заголовки исходящего сообщения</param>
        /// <returns></returns>
        protected TResult InvokeWcf<TResult>(Func<TContract, TResult> invocation, List<SimpleOutMessageHeader> outMsgHeaders = null)
        {
            if (invocation == null)
                throw new ArgumentNullException(nameof(invocation));

            // создать киент подключения к WCF-службе
            using (ServiceClientWrapper<TContract> wrapper = CreateServiceClient())
            {
                // открыть контектс текущего вызова
                using (new OperationContextScope(wrapper.InnerChannel))
                {
                    try
                    {
                        wrapper.SetOutGoingMessageHeaders(outMsgHeaders);
                        return invocation(wrapper.Client);
                    }
                    catch (Exception exception)
                    {
                        wrapper.Abort();
                        string message = ServiceExceptionFilter(serviceException: exception);
                        if (!string.IsNullOrEmpty(message))
                            throw new WarningException(message);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Вызов WCF сервиса
        /// </summary>
        /// <param name="invocation">Лямбда выражение вызова WCF сервиса</param>
        /// <param name="outMsgHeaders">Заголовки исходящего сообщения</param>
        /// <returns></returns>
        protected void InvokeWcf(Action<TContract> invocation, List<SimpleOutMessageHeader> outMsgHeaders = null)
        {
            if (invocation == null)
                throw new ArgumentNullException(nameof(invocation));

            // создать киент подключения к WCF-службе
            using (ServiceClientWrapper<TContract> wrapper = CreateServiceClient())
            {
                // открыть контектс текущего вызова
                using (new OperationContextScope(wrapper.InnerChannel))
                {
                    try
                    {
                        wrapper.SetOutGoingMessageHeaders(outMsgHeaders);
                        invocation(wrapper.Client);
                    }
                    catch (Exception exception)
                    {
                        wrapper.Abort();
                        string message = ServiceExceptionFilter(serviceException: exception);
                        if (!string.IsNullOrEmpty(message))
                            throw new WarningException(message);
                        throw;
                    }
                }
            }
        }

        #endregion

        #region PRIVATE

        /// <summary>
        /// Создать клиент WCF сервиса
        /// </summary>
        /// <returns></returns>
        private ServiceClientWrapper<TContract> CreateServiceClient()
        {
            ServiceClientWrapper<TContract> client;

            #region СОЗДАНИЕ КЛИЕНТА WCF-СЕРВИСА

            switch (LoginMethod)
            {
                case LoginMethods.NoAuth:
                {
                    if (string.IsNullOrEmpty(NonAuthBindingName))
                        throw new WarningException("Привязка без аутентификации не указана.");

                    client = new ServiceClientWrapper<TContract>(NonAuthBindingName);
                    break;
                }
                case LoginMethods.Certificate:
                {
                    if (ClientCertificate == null)
                        throw new WarningException("Сертификат не указаны.");

                    if (string.IsNullOrEmpty(AuthByCertificateBindingName))
                        throw new WarningException("Привязка для аутентификации по сертификату не указана.");

                    client = new ServiceClientWrapper<TContract>(AuthByCertificateBindingName);
                    client.ClientCredentials.ClientCertificate.Certificate = ClientCertificate;
                    client.SetServiceCertificate(ServiceCertificate);

                    break;
                }
                case LoginMethods.UserName:
                {
                    if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
                        throw new WarningException("Имя пользователя / Пароль не указаны.");

                    if (string.IsNullOrEmpty(AuthByUserNameBindingName))
                        throw new WarningException("Привязка для аутентификации по имени пользователя не указана.");

                    client = new ServiceClientWrapper<TContract>(AuthByUserNameBindingName);
                    client.ClientCredentials.UserName.UserName = UserName;
                    client.ClientCredentials.UserName.Password = Password;
                    client.SetServiceCertificate(ServiceCertificate);

                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(paramName: nameof(LoginMethods),
                        actualValue: LoginMethod, message: @"Способ аутентификации указан некорректно");
                }
            }

            #endregion

            return client;
        }

        /// <summary>
        /// Фильтр известных ошибок сервиса данных (string.Empty => неизвестная ошибка, не будет показана пользователю)
        /// </summary>
        /// <param name="serviceException">Ошибка сервиса данных</param>
        /// <returns></returns>
        private string ServiceExceptionFilter(Exception serviceException)
        {
            // Локальная функция получения сообщений вложенных ошибок (рекурсивный вызов)
            string GetLastInnerExceptionMessage(Exception exception, string message = "")
            {
                if (exception == null) return message;
                if (string.IsNullOrEmpty(message)) message = exception.Message;
                if (exception.InnerException != null) message += $"{Environment.NewLine}InnerException: {GetLastInnerExceptionMessage(exception.InnerException)}";
                return message;
            }

            switch (serviceException)
            {
                case TimeoutException _:
                {
                    return TimeoutExceptionMessage;
                }
                case EndpointNotFoundException _:
                {
                    return EndpointNotFoundExceptionMessage;
                }
                case MessageSecurityException securityException:
                {
                    return GetLastInnerExceptionMessage(serviceException.InnerException);
                }
                case AggregateException aggregateException:
                {
                    return GetLastInnerExceptionMessage(aggregateException.InnerException, aggregateException.Message);
                }
                case FaultException<WarningException> faultWarning:
                {
                    return $"{faultWarning.Message}{Environment.NewLine}" +
                            $"{GetLastInnerExceptionMessage(faultWarning.Detail.InnerException, faultWarning.Detail.Message)}";
                }
                case FaultException<TError> faultSvcError:
                {
                    return $"{faultSvcError.Message}{Environment.NewLine}{ParsServiceFaultException(faultSvcError.Detail)}";
                }
                default:
                {
                    return string.Empty;
                }
            }
        }

        #endregion
    }
}
