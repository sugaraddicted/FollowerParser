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

                var followers = GetFollowersUserame(scrollBox);
                CloseFollowerList();
                Thread.Sleep(GetRandomTimeoutOutOfRange());
                GetFollowersInfo(followers);
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

        private List<Follower> GetFollowersUserame(IWebElement scrollBox)
        {
            Thread.Sleep(GetRandomTimeoutOutOfRange());
            List<IWebElement> links = scrollBox.FindElements(By.TagName("a")).ToList();

            List<string> usernames = links.Select(name => name.Text).Where(text => !string.IsNullOrEmpty(text))
                .ToList();


            List<Follower> followers = new List<Follower>();
            for (int i = 0; i < usernames.Count; i++)
            {
                followers.Add(new Follower() { UserName = usernames[i]});
            }
            return followers;
        }

        private void GetFollowersInfo(List<Follower> followers)
        {
            foreach (var follower in followers)
            {
                _browser.Navigate().GoToUrl($"https://www.instagram.com/{follower.UserName}/"); 
                Thread.Sleep(GetRandomTimeoutOutOfRange());
                WebDriverWait wait = new WebDriverWait(_browser, new TimeSpan(0, 0, 0, 60));

                try
                {
                    wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h1")));
                    var bioElement = _browser.FindElement(By.TagName("h1"));
                    var bio = bioElement.Text;
                    follower.Bio = bio;
                }
                catch (NoSuchElementException ex)
                {

                }

                try
                {
                    var linkElement = _browser.FindElement(By.XPath(
                        "/html/body/div[2]/div/div/div[2]/div/div/div/div[1]/div[1]/div[2]/div[2]/section/main/div/header/section/div[3]/div[3]/a"));
                    var link = linkElement.GetAttribute("href");
                    follower.Link = link;
                }
                catch (NoSuchElementException ex)
                {

                }

                try
                {
                    var nameElement = _browser.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/div/div/div/div[1]/div[1]/div[2]/div[2]/section/main/div/header/section/div[3]/div[1]/span"));
                    var name = nameElement.Text;
                    follower.Name = name;
                }
                catch (NoSuchElementException ex)
                {

                }

                Thread.Sleep(GetRandomTimeoutOutOfRange());
            }
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
                try
                {
                    _browser.FindElement(By.XPath("//button[contains(text(),'Not Now')]")).Click();
                }
                catch (NoSuchElementException ex)
                {
                    return;
                }

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
