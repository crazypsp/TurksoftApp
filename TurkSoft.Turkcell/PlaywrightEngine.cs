using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Turkcell
{
    public static class PlaywrightEngine
    {
        private static IPlaywright _pw;
        private static IBrowser _browser;
        private static IPage _page;

        public static async Task StartLogin(CommandModel cmd)
        {
            _pw = await Playwright.CreateAsync();

            _browser = await _pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false, // Kullanıcı ekranında tarayıcı açılacak
                Args = new[]
                {
                    $"--window-size={cmd.Width},{cmd.Height}",
                    "--start-fullscreen",
                    "--kiosk",
                    "--disable-infobars",
                    "--disable-blink-features=AutomationControlled"
                }
            });

            var ctx = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = cmd.Width, Height = cmd.Height }
            });

            _page = await ctx.NewPageAsync();

            await _page.GotoAsync("https://portal.turkcellesirket.com/auth/login");

            await _page.FillAsync("#txtUserName", cmd.Username);
            await _page.FillAsync("#txtPassword", cmd.Password);

            await _page.ClickAsync("button.primary-action");
        }

        public static async Task EnterSmsCode(string sms)
        {
            await _page.FillAsync("#txtSmsPassword", sms);
            await _page.ClickAsync("#btnConfirm");
        }

        public static async Task OpenHomePage()
        {
            await _page.GotoAsync("https://portal.turkcellesirket.com/home");
        }
    }
}
