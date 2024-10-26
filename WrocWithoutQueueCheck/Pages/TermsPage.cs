using OpenQA.Selenium;
using WrocWithoutQueueCheck.Helpers;

namespace WrocWithoutQueueCheck.Pages
{
    public class TermsPage : Page
    {
        private static readonly By HeaderLocator = By.XPath("//span[contains(text(), 'Regulamin')]");
        private static readonly By AcceptButtonLocator = By.XPath("//button/span[contains(text(), 'Akceptuj')]");

        public TermsPage(IWebDriver driver) : base(driver) { }

        public static bool IsTermsPagePresent(IWebDriver driver)
        {
            return driver.Url.EndsWith("/rules");
            //return driver.WaitForOptionalElement(HeaderLocator) != null ? true : false;
        }

        public PlaceSelectionPage AcceptTerms()
        {
            Driver.WaitForElement(AcceptButtonLocator).Click();
            return new PlaceSelectionPage(Driver);
        }
    }
}
