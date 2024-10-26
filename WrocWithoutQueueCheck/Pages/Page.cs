using OpenQA.Selenium;

namespace WrocWithoutQueueCheck.Pages
{
    public abstract class Page
    {
        protected readonly IWebDriver Driver;

        public Page(IWebDriver driver)
        {
            Driver = driver;
        }
    }
}