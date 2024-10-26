using OpenQA.Selenium;
using WrocWithoutQueueCheck.Helpers;

namespace WrocWithoutQueueCheck.Pages
{
    public class DocumentTypeSelectionPage : Page
    {
        private static readonly By TargetPlaceHeaderLocator = By.XPath($"//div[contains(@class, 'reservation-info')]//span[contains(text(), '{Config.PlaceName}')]");
        private static readonly By DocumentTypeButtonsLocator = By.CssSelector("button.queue-button");
        private static readonly By NextButtonLocator = By.XPath("//button//div[contains(text(), 'DALEJ')]");
        private static readonly By TargetDocumentTypeButtonLocator = By.XPath($"//button[contains(@class, 'queue-button')]//div[contains(text(), '{Config.DocumentType}')]");

        public DocumentTypeSelectionPage(IWebDriver driver) : base(driver) {
            Driver.WaitForElement(TargetPlaceHeaderLocator);
            Driver.WaitForNumberOfElements(DocumentTypeButtonsLocator, 1);
        }

        public CalendarPage SelectTargetDocumentType()
        {
            Driver.WaitForElement(TargetDocumentTypeButtonLocator).Click();
            Driver.WaitForElement(NextButtonLocator).Click();
            return new CalendarPage(Driver);
        }
    }
}
