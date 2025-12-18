using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Business.Managers.BankProviders.Infrastructure
{
    public static class WcfCloseExtensions
    {
        public static void SafeClose(this ICommunicationObject client)
        {
            try
            {
                if (client.State == CommunicationState.Faulted) client.Abort();
                else client.Close();
            }
            catch
            {
                client.Abort();
            }
        }
    }
}
