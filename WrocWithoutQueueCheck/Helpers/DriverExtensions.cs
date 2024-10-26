using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Linq;

namespace WrocWithoutQueueCheck.Helpers
{
    public static class DriverExtensions
    {
        public static IWebElement? WaitForOptionalElement(this IWebDriver driver, By by, TimeSpan? timeout = null)
        {
            timeout ??= Config.DefaultTimeoutSeconds;
            var wait = new WebDriverWait(driver, (TimeSpan)timeout);
            try
            {
                return wait.Until(d =>
                {
                    try
                    {
                        var element = d.FindElement(by);
                        return element.Displayed == true ? element : null;
                    }
                    catch (NoSuchElementException)
                    {
                        return null;
                    }
                    catch (StaleElementReferenceException)
                    {
                        return null;
                    }
                });
            }
            catch (WebDriverTimeoutException)
            {
                return null;
            }
        }

        public static IWebElement WaitForElement(this IWebDriver driver, By by, TimeSpan? timeout = null)
        {
            timeout ??= Config.DefaultTimeoutSeconds;
            var wait = new WebDriverWait(driver, (TimeSpan)timeout);
            return wait.Until(d =>
            {
                try
                {
                    var element = d.FindElement(by);
                    return element.Displayed == true ? element : null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });
        }

        public static void WaitForNumberOfElements(this IWebDriver driver, By by, int expectedMinNumberOfElements, TimeSpan? timeout = null)
        {
            timeout ??= Config.DefaultTimeoutSeconds;
            var wait = new WebDriverWait(driver, (TimeSpan)timeout);
            wait.Until(d =>
            {
                try
                {
                    var elements = d.FindElements(by);
                    return elements.Where(e => e.Displayed == true).Count() >= expectedMinNumberOfElements
                        ? true
                        : false;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        public static void WaitForAndScrollElementIntoView(this IWebDriver driver, By by, TimeSpan? timeout = null)
            => ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", WaitForElement(driver, by, timeout));

        public static void SaveScreenshot(this IWebDriver driver, string targetPath) => ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(targetPath);

        public static void SavePageSource(this IWebDriver driver, string targetPath) => File.WriteAllText(targetPath, driver.PageSource);

        public static void SaveTextToFile(this IWebDriver driver, string targetPath, string txt) => File.WriteAllText(targetPath, txt);
    }
}
