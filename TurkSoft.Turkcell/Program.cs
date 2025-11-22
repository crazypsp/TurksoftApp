using System;
using System.Threading.Tasks;
using TurkSoft.Turkcell;

namespace TurkSoft.Turkcell
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Arka planda çalışan görünmez agent
            var worker = new Worker();
            await worker.StartAsync();
        }
    }
}
