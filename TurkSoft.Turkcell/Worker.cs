using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TurkSoft.Turkcell
{
    public class Worker
    {
        private readonly HttpClient _http;
        private readonly string _clientId;

        public Worker()
        {
            _http = new HttpClient();
            _http.BaseAddress = new Uri("https://portal.noxmusavir.com/");
            _clientId = Environment.MachineName; // benzersiz ID
        }

        public async Task StartAsync()
        {
            while (true)
            {
                try
                {
                    await SendHeartbeat();
                    string cmd = await _http.GetStringAsync($"ClientCommand/Get?clientId={_clientId}");

                    if (!string.IsNullOrEmpty(cmd) && cmd != "NONE")
                    {
                        var obj = JsonSerializer.Deserialize<CommandModel>(cmd);

                        switch (obj.Command)
                        {
                            case "LOGIN_START":
                                await PlaywrightEngine.StartLogin(obj);
                                break;

                            case "SMS_CODE":
                                await PlaywrightEngine.EnterSmsCode(obj.SmsCode);
                                break;

                            case "FINAL_OPEN":
                                await PlaywrightEngine.OpenHomePage();
                                break;
                        }

                        await _http.GetAsync($"ClientCommand/Clear?clientId={_clientId}");
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText("client-log.txt", ex.Message + "\n");
                }

                await Task.Delay(1000);
            }
        }

        private async Task SendHeartbeat()
        {
            await _http.PostAsync($"ClientCommand/Heartbeat?clientId={_clientId}", null);
        }
    }
}
