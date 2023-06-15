using System;
using System.ServiceModel;

namespace ALX.Common.WCF
{
    internal class ServiceClientWrapper<TChannel> : ClientBase<TChannel>, IDisposable where TChannel : class
    {
        public ServiceClientWrapper(string endpointConfigurationName) : base(endpointConfigurationName)
        {
        }

        public TChannel Client => Channel;

        public new void Close()
        {
            ((IDisposable)this).Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (State != CommunicationState.Closed)
                {
                    base.Close();
                }
            }
            catch (CommunicationException)
            {
                base.Abort();
            }
            catch (TimeoutException)
            {
                base.Abort();
            }
            catch
            {
                base.Abort();
                throw;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
