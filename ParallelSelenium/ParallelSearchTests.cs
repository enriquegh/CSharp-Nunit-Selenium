using System;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using NUnit.Framework;
using ParallelSelenium.PageObjects.Bing;
using ParallelSelenium.Utils;
using System.Threading;
using System.Collections.Generic;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace ParallelSelenium
{
    [TestFixture("chrome", "66", "Windows 10")]
    //[TestFixture("internet explorer", "11", "Windows 7")]
    //[TestFixture("firefox", "41", "Windows 7")]
    //[TestFixture("chrome", "30", "Windows 7")]
    //[TestFixture("internet explorer", "9", "Windows 7")]
    //[TestFixture("firefox", "30", "Windows 7")]
    //[TestFixture("chrome", "38", "Windows 7")]
    //[TestFixture("internet explorer", "10", "Windows 7")]
    //[TestFixture("firefox", "35", "Windows 7")]
    [Parallelizable(ParallelScope.Children)]
    public class ParallelSearchTests
    {

		ThreadLocal<CustomRemoteWebDriver> driver;
        private String browser;
        private String version;
        private String os;

        public ParallelSearchTests(String browser, String version, String os)
        {
            this.browser = browser;
            this.version = version;
            this.os = os;
			driver = new ThreadLocal<CustomRemoteWebDriver>();
        }

        [SetUp]
        public void Init()
        {
            /* Web proxy setup to be used with the underlying Rest requests.
            **/
            /*
            WebProxy iProxy = new WebProxy("192.168.1.159:808", true);
            iProxy.UseDefaultCredentials = true;
            iProxy.Credentials = new NetworkCredential("test", "hello123");
            WebRequest.DefaultWebProxy = iProxy;
            */
            String seleniumUri = "http://{0}:{1}/wd/hub";


			Dictionary<string, object> sauceDict = new Dictionary<string, object>();
			sauceDict.Add("username", Constants.sauceUser);
			sauceDict.Add("accessKey", Constants.sauceKey);
			sauceDict.Add("seleniumVersion", "3.11.0");
			sauceDict.Add("name", TestContext.CurrentContext.Test.Name);

			DesiredCapabilities caps = new DesiredCapabilities();
			caps.SetCapability("browserVersion", "60");
			caps.SetCapability("browserName", "firefox");
			caps.SetCapability("platformName", "Windows 10");
			caps.SetCapability("sauce:options", sauceDict);


            seleniumUri = "https://ondemand.saucelabs.com:443/wd/hub";

			driver.Value = new CustomRemoteWebDriver(new Uri(seleniumUri), caps, TimeSpan.FromSeconds(600));
            //driver.Value.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));
        }

        [Test, Property("Description", "Searching for \"hello!\" on Bing.")]
        public void BingSearchHello()
        {
            //this code is repeated below intentionally.
            //Feel free to modify and experiment.
            string query = "hello!";
            driver.Value.Navigate().GoToUrl(BingSearchPage.URL);
            BingSearchPage searchPage = new BingSearchPage(driver.Value);
            BingResultPage resultPage = searchPage.search(query);
            Assert.IsTrue(true,
                String.Format("Title: {0} does not start with query: {1}!", resultPage.title, query));
        }

        [Test, Property("Description", "Searching for \"bye!\" on Bing.")]
        public void BingSearchBye()
        {
            string query = "bye!";
            driver.Value.Navigate().GoToUrl(BingSearchPage.URL);
            BingSearchPage searchPage = new BingSearchPage(driver.Value);
            BingResultPage resultPage = searchPage.search(query);
            Assert.IsTrue(resultPage.title.StartsWith(query),
                String.Format("Title: {0} does not start with query: {1}!", resultPage.title, query));
        }

        [TearDown]
        public void Cleanup()
        {
            bool passed = TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Passed;
            try
            {
                // Logs the result to Sauce Labs
                ((IJavaScriptExecutor)driver.Value).ExecuteScript("sauce:job-result=" + (passed ? "passed" : "failed"));
            }
            finally
            {
                Console.WriteLine(String.Format("SauceOnDemandSessionID={0} job-name={1}", ((CustomRemoteWebDriver)driver.Value).getSessionId(), TestContext.CurrentContext.Test.MethodName));
                // Terminates the remote webdriver session
                driver.Value.Quit();
            }
        }
    }
}
