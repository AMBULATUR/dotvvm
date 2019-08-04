﻿using System;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class RedirectTests : AppSeleniumTest
    {
        public RedirectTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_Redirect_Redirect()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Redirect_Redirect);

                var currentUrl = new Uri(browser.CurrentUrl);
                Assert.Matches(@"time=\d+", currentUrl.Query);

                browser.First("[data-ui=object-redirect-button]").Click();
                currentUrl = new Uri(browser.CurrentUrl);
                Assert.Matches(@"^\?(param=temp1&time=\d+|time=\d+&param=temp1)$", currentUrl.Query);
                Assert.Equal("#test1", currentUrl.Fragment);

                browser.First("[data-ui=dictionary-redirect-button]").Click();
                currentUrl = new Uri(browser.CurrentUrl);
                Assert.Matches(@"^\?(time=\d+&param=temp2|param=temp2&time=\d+)$", currentUrl.Query);
                Assert.Equal("#test2", currentUrl.Fragment);
            });
        }
    }
}
