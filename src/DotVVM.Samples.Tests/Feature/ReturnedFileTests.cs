﻿using System.Net;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core.Abstractions;
using Xunit;
using Xunit.Abstractions;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace DotVVM.Samples.Tests.New.Feature
{
    public class ReturnedFileTests : AppSeleniumTest
    {
        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample))]
        public void Feature_ReturnedFile_ReturnedFileSample_Simple()
        {
            RunInAllBrowsers(browser => {
                ReturnedFileDownload(browser, "Hello DotVVM returned file sample!");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample))]
        public void Feature_ReturnedFile_ReturnedFileSample_Empty()
        {
            RunInAllBrowsers(browser => {
                ReturnedFileDownload(browser, "");
            });
        }

        private void ReturnedFileDownload(IBrowserWrapper browser, string fileContent)
        {
            browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample);
            var jsexec = browser.GetJavaScriptExecutor();
            jsexec.ExecuteScript("window.downloadURL = \"\";");
            jsexec.ExecuteScript("DotVVM.prototype.performRedirect = function(url){window.downloadURL = url};");

            browser.First("textarea").SendKeys(fileContent);
            browser.First("input").SendKeys(Keys.Enter);
            var downloadURL = (string)jsexec.ExecuteScript("return window.downloadURL;");
            Assert.IsNotNull(downloadURL);

            string returnedFile;
            using (var client = new WebClient())
            {
                returnedFile = client.DownloadString(browser.GetAbsoluteUrl(downloadURL));
            }
            Assert.AreEqual(fileContent, returnedFile);
        }

        public ReturnedFileTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
