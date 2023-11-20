using FollowerParser.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Windows;
using System.Diagnostics;

namespace FollowerParser
{
    internal class InstagramBot
    {
        private int[] timeoutRange = new int[]{2500, 6500};
        private static readonly Random random = new Random();
        private IWebDriver _browser { get; set; }

        public List<Follower> FollowingUser(string targetUsername)
        {
            _browser.Navigate().GoToUrl($"https://www.instagram.com/{targetUsername}/followers/");
            Thread.Sleep(GetRandomTimeoutOutOfRange());

            try
            {
                WebDriverWait wait = new WebDriverWait(_browser, new TimeSpan(0, 0, 0, 180));

                wait.Until(ExpectedConditions.ElementIsVisible(
                    By.XPath("/html/body/div[6]/div[1]/div/div[2]/div/div/div/div/div[2]/div/div/div[3]")));

                IWebElement scrollBox =
                    _browser.FindElement(
                        By.XPath("/html/body/div[6]/div[1]/div/div[2]/div/div/div/div/div[2]/div/div/div[3]"));

                ScrollToBottom(scrollBox);

                var followers = GetFollowersInfo(scrollBox);
                CloseFollowerList();
                return followers;
            }
            catch (WebDriverTimeoutException ex)
            {
                MessageBox.Show("Invalid target username. Please check the username and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _browser.Quit();
                return new List<Follower>();
            }
        }

        private void ScrollToBottom(IWebElement scrollBox)
        {
            long lastHeight = 0;
            long currentHeight = 1;

            while (lastHeight != currentHeight)
            {
                lastHeight = currentHeight;
                Thread.Sleep(GetRandomTimeoutOutOfRange());
                currentHeight = (long)((IJavaScriptExecutor)_browser).ExecuteScript(
                    "arguments[0].scrollTo(0, arguments[0].scrollHeight); return arguments[0].scrollHeight;",
                    scrollBox);
            }
        }

        private List<Follower> GetFollowersInfo(IWebElement scrollBox)
        {
            Thread.Sleep(GetRandomTimeoutOutOfRange());
            List<IWebElement> links = scrollBox.FindElements(By.TagName("a")).ToList();
            List<IWebElement> spans = scrollBox.FindElements(By.TagName("span")).ToList();
            Thread.Sleep(GetRandomTimeoutOutOfRange());

            List<string> names = links.Select(name => name.Text).Where(text => !string.IsNullOrEmpty(text))
                .ToList();
            List<string> bios = spans.Select(bio => bio.Text)
                .ToList();

            for (int i = 0; i < bios.Count; i++)
            {
                if (names.Contains(bios[i]))
                {
                    bios.RemoveAt(i);
                }
            }

            List<Follower> followers = new List<Follower>();
            int b = 1;
            for (int i = 0; i < names.Count; i++)
            {
                followers.Add(new Follower() { UserName = names[i], Bio = bios[b] });
                b += 2;
            }
            return followers;
        }

        private void CloseFollowerList()
        {
            Thread.Sleep(GetRandomTimeoutOutOfRange());
            _browser.FindElement(By.XPath(
                    "/html/body/div[6]/div[1]/div/div[2]/div/div/div/div/div[2]/div/div/div[1]/div/div[3]/div/button"))
                .Click();
        }

        public void Quitting()
        {
            Thread.Sleep(GetRandomTimeoutOutOfRange());
            _browser.Close();
            _browser.Quit();
        }

        public void Login(string username, string password)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--lang=en-US");
            options.AddArgument("--window-size=1200,1000");

            _browser = new ChromeDriver(options);
            _browser.Navigate().GoToUrl("http://instagram.com");
            Thread.Sleep(GetRandomTimeoutOutOfRange());

            // Login
            _browser.FindElement(By.Name("username")).SendKeys(username);
            _browser.FindElement(By.Name("password")).SendKeys(password);
            _browser.FindElement(By.XPath("//button[@type='submit']")).Click();
            Thread.Sleep(GetRandomTimeoutOutOfRange());

            // Handle "Not Now" buttons
            try
            {
                WebDriverWait wait = new WebDriverWait(_browser, new TimeSpan(0, 0, 0, 180));

                wait.Until(ExpectedConditions.ElementIsVisible(
                    By.XPath("/html/body/div[2]/div/div/div[2]/div/div/div/div[1]/div[1]/div[2]/section/main/div/div/div/div/div")));
                _browser.FindElement(By.XPath(
                        "/html/body/div[2]/div/div/div[2]/div/div/div/div[1]/div[1]/div[2]/section/main/div/div/div/div/div"))
                    .Click();
                Thread.Sleep(GetRandomTimeoutOutOfRange());
                _browser.FindElement(By.XPath("//button[contains(text(),'Not Now')]")).Click();

                // Navigate to profile page
                _browser.FindElement(By.XPath($"//a[contains(@href, '/{username}')]")).Click();
                Thread.Sleep(GetRandomTimeoutOutOfRange());
            }
            catch (WebDriverTimeoutException ex)
            {

                MessageBox.Show("Invalid username or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                RestartApplication();
            }
        }

        public int GetRandomTimeoutOutOfRange()
        {
            return random.Next(timeoutRange[0], timeoutRange[1] + 1); ;
        }

        public void SetTimeoutRange(int min, int max)
        {
            timeoutRange[0] = min;
            timeoutRange[1] = max;
        }
        public void RestartApplication()
        {
            Process currentProcess = Process.GetCurrentProcess();

            Process.Start(currentProcess.MainModule.FileName);

            Application.Current.Shutdown();
        }
    }
}
