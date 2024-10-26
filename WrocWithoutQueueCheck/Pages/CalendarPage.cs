using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using WrocWithoutQueueCheck.Helpers;

namespace WrocWithoutQueueCheck.Pages
{
    public class CalendarPage : Page
    {
        private static readonly By TargetPlaceHeaderLocator = By.XPath($"//div[contains(@class, 'reservation-info')]//span[contains(text(), '{Config.PlaceName}')]");
        private static readonly By TargetDocumentTypeHeaderLocator = By.XPath($"//div[contains(@class, 'reservation-info')]//span[contains(text(), '{Config.PlaceName}')]");
        private static readonly By CalendarHeaderLocator = By.CssSelector("div.v-date-picker-header");
        private static readonly By DayOnCalendarButtonsLocator = By.CssSelector("div.v-date-picker-table button");
        private static readonly By NextMonthButtonLocator = By.CssSelector("button[aria-label='Następny miesiąc']");

        public CalendarPage(IWebDriver driver) : base(driver)
        {
            Driver.WaitForElement(TargetPlaceHeaderLocator);
            Driver.WaitForElement(TargetDocumentTypeHeaderLocator);
        }

        public List<string> GetEnabledDaysForCurrentMonth()
        {
            return Driver
                .FindElements(DayOnCalendarButtonsLocator)
                .Where(e => e.Enabled)
                .Select(e => e.GetAttribute("aria-label"))
                .ToList();
        }

        public void WaitForCalendarToFullyLoadForCurrentMonth()
        {
            Driver.WaitForAndScrollElementIntoView(CalendarHeaderLocator);
            Driver.WaitForNumberOfElements(DayOnCalendarButtonsLocator, 28);

            // Let's give it couple more secs in case it has to sync days from somewhere else
            Thread.Sleep(5000);
        }

        public bool IsNextMonthButtonEnabled() => Driver.WaitForElement(NextMonthButtonLocator).Enabled;

        public void GoToNextMonth() => Driver.WaitForElement(NextMonthButtonLocator).Click();
    }
}
