﻿using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium.Chrome;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    public class SPAErrorReportingTests : AppSeleniumTest
    {
        public SPAErrorReportingTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPAErrorReporting_default))]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPAErrorReporting_test))]
        [SkipBrowser("firefox:dev", "Cannot simulate offline mode.")]
        [SkipBrowser("firefox:fast", "Cannot simulate offline mode.")]
        [SkipBrowser("ie:dev", "Cannot simulate offline mode.")]
        [SkipBrowser("firefox:fast", "Cannot simulate offline mode.")]
        public void Complex_SPAErrorReporting_NavigationAndBackButtons()
        {
            RunInAllBrowsers(browser => {

                void SetOfflineMode(bool offline)
                {
                    ((ChromeDriver)browser.Driver).NetworkConditions = new ChromeNetworkConditions() {
                        IsOffline = offline,
                        Latency = TimeSpan.FromMilliseconds(5),
                        DownloadThroughput = 500 * 1024,
                        UploadThroughput = 500 * 1024
                    };
                }

                try
                {
                    browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_SPAErrorReporting_default);

                    // go to Test page and verify the success
                    browser.ElementAt("a", 1).Click();
                    AssertUI.TextEquals(browser.Single("h2"), "Test");

                    SetOfflineMode(true);

                    // try to submit command in offline mode
                    browser.Single("input[type=text]").SendKeys("aaa");
                    browser.Single("input[type=button]").Click();
                    browser.Single("#debugWindow button").Click();
                    AssertUI.TextEquals(browser.Single("#numberOfErrors"), "1");

                    // try to go back in offline mode
                    browser.ElementAt("a", 0).Click();
                    AssertUI.TextEquals(browser.Single("h2"), "Test");
                    browser.Single("#debugWindow button").Click();
                    AssertUI.TextEquals(browser.Single("#numberOfErrors"), "2");

                    SetOfflineMode(false);

                    // go back to online mode and retry
                    browser.Single("input[type=button]").Click();
                    AssertUI.TextEquals(browser.Single("#numberOfErrors"), "2");
                    AssertUI.TextEquals(browser.Single("*[data-ui=sample-text]"), "Sample Text");

                    browser.ElementAt("a", 0).Click();
                    AssertUI.TextEquals(browser.Single("h2"), "Default");
                    AssertUI.TextEquals(browser.Single("#numberOfErrors"), "2");
                }
                finally
                {
                    SetOfflineMode(false);
                }
            });
        }
    }
}
