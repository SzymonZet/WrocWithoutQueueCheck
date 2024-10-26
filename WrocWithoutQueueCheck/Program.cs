using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Threading;
using WrocWithoutQueueCheck.Helpers;
using WrocWithoutQueueCheck.Pages;

namespace WrocWithoutQueueCheck
{
    internal class Program
    {
        private static readonly string AppName = $"{Assembly.GetCallingAssembly().GetName().Name}";
        private static readonly string AppVersion = $"{Assembly.GetCallingAssembly().GetName().Version}";
        private static readonly string LogsPath = "./Logs/";
        private static readonly string HitsPath = "./Hits/";
        private static readonly string TempPath = "./Temp/";
        private static List<string> _hitsFromPreviousSummary = [];
        private static int _runsWithHitsCurrentSummary = 0;
        private static int _errosCurrentSummary = 0;
        private static DateTime _lastNotificationHitTime = DateTime.MinValue;
        private static DateTime _lastSummaryTime = DateTime.Now;

        private static readonly Logger Log = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(
                path: $"{LogsPath}/{Assembly.GetCallingAssembly().GetName().Name}_.log",
                rollingInterval: RollingInterval.Day
            )
            .CreateLogger();

        private static void SendSummaryEmailAndResetTheData()
        {
            Log.Information($"Preparing attachment and sending summary email...");

            var zippedLogsDump = "./logs_dump.zip";
            if (File.Exists(zippedLogsDump)) File.Delete(zippedLogsDump);
            if (Directory.Exists(TempPath)) Directory.Delete(TempPath, true);

            Directory.CreateDirectory(TempPath);
            var currentLogFiles = new DirectoryInfo(LogsPath).GetFiles();
            foreach (var file in currentLogFiles)
            {
                File.Copy(file.FullName, $"{TempPath}/{file.Name}");
            }
            ZipFile.CreateFromDirectory(TempPath, zippedLogsDump);

            new SmtpHelper().SendEmail(
                $"{AppName} - {Config.PlaceName} - {Config.DocumentType} - summary; runs with hits: {_runsWithHitsCurrentSummary}, errors: {_errosCurrentSummary}",
                $"There were {_runsWithHitsCurrentSummary} runs with hits and {_errosCurrentSummary} errors since last summary (or initial run) from `{_lastSummaryTime}`. Logs dump attached below.\n\nNext report in ~{Config.TimeBetweenSummaryEmailHours} hour(s).",
                new List<Attachment> { new Attachment(zippedLogsDump) }
            );

            Log.Information($"Resetting summary data...");

            _errosCurrentSummary = 0;
            _runsWithHitsCurrentSummary = 0;
            _lastSummaryTime = DateTime.Now;

            if (File.Exists(zippedLogsDump)) File.Delete(zippedLogsDump);
            if (Directory.Exists(TempPath)) Directory.Delete(TempPath, true);
        }

        static void Main(string[] args)
        {
            Log.Information($"==============================");
            Log.Information($"{AppName} {AppVersion}");
            Log.Information($"==============================");

            Config.SetConfig();
            Directory.CreateDirectory(HitsPath);
            IWebDriver driver = null;

            for (int browserSession = 1; ; browserSession++)
            {
                try
                {
                    if (Config.NotificationEmails && _lastSummaryTime.AddHours(Config.TimeBetweenSummaryEmailHours) < DateTime.Now) SendSummaryEmailAndResetTheData();

                    Log.Information($"Creating browser session #{browserSession}...");
                    var firefoxOptions = new FirefoxOptions();
                    if (Config.BrowserHeadlessMode) firefoxOptions.AddArgument("--headless");
                    driver = new FirefoxDriver(firefoxOptions);
                    driver.Navigate().GoToUrl(Config.Url);
                    
                    var hits = new List<string>();
                    var attachments = new List<Attachment>();

                    for (int run = 1; run <= Config.MaxNumberOfRunsPerBrowserSession; run++)
                    {
                        if (Config.NotificationEmails && _lastSummaryTime.AddHours(Config.TimeBetweenSummaryEmailHours) < DateTime.Now) SendSummaryEmailAndResetTheData();

                        Log.Information($"Starting run #{browserSession}:{run}...");
                        driver.Navigate().Refresh();

                        PlaceSelectionPage placeSelectionPage;
                        if (TermsPage.IsTermsPagePresent(driver)) placeSelectionPage = new TermsPage(driver).AcceptTerms();
                        else placeSelectionPage = new PlaceSelectionPage(driver);

                        var calendarPage = placeSelectionPage
                            .SelectTargetPlace()
                            .SelectTargetDocumentType();

                        for (int month = 0; month <= Config.MaxNumberOfFutureMonthsToCheck; month++)
                        {
                            var currentMonthHits = new List<string>();
                            calendarPage.WaitForCalendarToFullyLoadForCurrentMonth();
                            currentMonthHits.AddRange(calendarPage.GetEnabledDaysForCurrentMonth());

                            if (currentMonthHits.Count > 0)
                            {
                                var currentMonthHitsString = string.Join("\n", currentMonthHits);
                                Log.Information($"Hit(s) found for current month + {month}:\n{currentMonthHitsString}");

                                Log.Information($"Capturing hit(s) data...");
                                var screenshotPath = $"{HitsPath}/{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}_session-{browserSession}_run-{run}_month-{month}.png";
                                var htmlPath = $"{HitsPath}/{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}_session-{browserSession}_run-{run}_month-{month}.html";
                                var txtPath = $"{HitsPath}/{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}_session-{browserSession}_run-{run}_month-{month}.txt";

                                driver.SaveScreenshot(screenshotPath);
                                driver.SavePageSource(htmlPath);
                                driver.SaveTextToFile(txtPath, currentMonthHitsString);

                                attachments.Add(new Attachment(screenshotPath));
                                attachments.Add(new Attachment(htmlPath));
                                attachments.Add(new Attachment(txtPath));

                                hits.AddRange(currentMonthHits);
                            }
                            else
                            {
                                Log.Information($"No hits found for current month + {month}!");
                            }

                            if (calendarPage.IsNextMonthButtonEnabled() == false) break;

                            if (month != Config.MaxNumberOfFutureMonthsToCheck) calendarPage.GoToNextMonth();
                        }

                        if(hits.Count() > 0)
                        {
                            _runsWithHitsCurrentSummary++;

                            if (
                                Config.NotificationEmails &&
                                (
                                _hitsFromPreviousSummary.Count == 0 ||
                                !_hitsFromPreviousSummary.SequenceEqual(hits) ||
                                    (
                                        _hitsFromPreviousSummary.SequenceEqual(hits) &&
                                        _lastNotificationHitTime.AddMinutes(Config.TimeBetweenNotificationEmailWithTheSameHitsMinutes) < DateTime.Now
                                    )
                                )
                            )
                            {
                                Log.Information($"Sending hit(s) notification email...");

                                new SmtpHelper().SendEmail(
                                    $"{AppName} - {Config.PlaceName} - {Config.DocumentType} - hits",
                                    $"Following hits found:\n\n{string.Join("\n", hits)}\n\nGo to {Config.Url} to reserve...",
                                    attachments
                                );

                                _lastNotificationHitTime = DateTime.Now;
                                _hitsFromPreviousSummary = hits;
                            }

                            if (Config.NotificationSoundOnHit)
                            {
                                Log.Information($"Playing notification sound...");
                                SoundHelper.PlaySuccess();
                            }
                        }

                        Log.Information($"Waiting for {Config.DelayBetweenRunsSeconds} before next run...");
                        Thread.Sleep((int)Config.DelayBetweenRunsSeconds.TotalMilliseconds);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning($"Unexpected exception occurred:");
                    Log.Warning($"{ex.Message}\n{ex.StackTrace}");
                    _errosCurrentSummary++;
                    Log.Warning($"Browser session will be closed, then re-created");
                    if (Config.NotificationSoundOnError) SoundHelper.PlayError();
                    continue;
                }
                finally
                {
                    Log.Information($"Closing current browser session...");
                    try
                    {
                        driver?.Close();
                        driver?.Quit();
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"Unexpected exception occurred when trying to close browser:");
                        _errosCurrentSummary++;
                        Log.Warning($"{ex.Message}\n{ex.StackTrace}");
                        if (Config.NotificationSoundOnError) SoundHelper.PlayError();
                    }
                }
            }
        }
    }
}
