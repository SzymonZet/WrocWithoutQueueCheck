using OpenQA.Selenium;
using WrocWithoutQueueCheck.Helpers;

namespace WrocWithoutQueueCheck.Pages
{
    public class PlaceSelectionPage : Page
    {
        private static readonly By HeaderLocator = By.XPath("//h2[contains(text(), 'Wybierz kolejkę')]");
        private static readonly By PlaceButtonsLocator = By.CssSelector("button.queue-button");
        private static readonly By NextButtonLocator = By.XPath("//button//div[contains(text(), 'DALEJ')]");
        private static readonly By TargetPlaceButtonLocator = By.XPath($"//button[contains(@class, 'queue-button')]//div[contains(text(), '{Config.PlaceName}')]");

        public PlaceSelectionPage(IWebDriver driver) : base(driver) {
            Driver.WaitForElement(HeaderLocator);
            Driver.WaitForNumberOfElements(PlaceButtonsLocator, 1);
        }

        public DocumentTypeSelectionPage SelectTargetPlace()
        {
            Driver.WaitForElement(TargetPlaceButtonLocator).Click();
            Driver.WaitForElement(NextButtonLocator).Click();
            return new DocumentTypeSelectionPage(Driver);
        }
    }
}
