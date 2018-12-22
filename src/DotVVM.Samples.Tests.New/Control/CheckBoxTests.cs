﻿using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.New.Control
{
    public class CheckBoxTests : AppSeleniumTest
    {
        [Fact]
        public void Control_CheckBox_CheckBox()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_CheckBox);

                var boxes = browser.FindElements("fieldset");

                // single check box
                boxes.ElementAt(0).First("input[type=checkbox]").Click();
                boxes.ElementAt(0).First("input[type=button]").Click();
                browser.Wait();

                AssertUI.InnerTextEquals(boxes.ElementAt(0).First("span.result")
                    , "True");

                // check box list
                boxes.ElementAt(1).ElementAt("input[type=checkbox]", 1).Click();
                boxes.ElementAt(1).ElementAt("input[type=checkbox]", 2).Click();
                boxes.ElementAt(1).First("input[type=button]").Click();
                browser.Wait();

                AssertUI.InnerTextEquals(boxes.ElementAt(1).First("span.result")
                    , "g, b");

                boxes.ElementAt(1).ElementAt("input[type=checkbox]", 2).Click();
                boxes.ElementAt(1).ElementAt("input[type=checkbox]", 0).Click();
                boxes.ElementAt(1).First("input[type=button]").Click();
                browser.Wait();

                AssertUI.InnerTextEquals(boxes.ElementAt(1).First("span.result")
                    , "g, r");

                // checked changed
                boxes.ElementAt(2).ElementAt("input[type=checkbox]", 0).Click();
                browser.Wait();

                AssertUI.InnerTextEquals(boxes.ElementAt(2).Last("span.result")
                    , "1");
                AssertUI.IsChecked(boxes.ElementAt(2).First("input[type=checkbox]"));

                boxes.ElementAt(2).ElementAt("input[type=checkbox]", 0).Click();
                browser.Wait();

                AssertUI.InnerTextEquals(boxes.ElementAt(2).Last("span.result")
                    , "2");
                AssertUI.IsNotChecked(boxes.ElementAt(2).First("input[type=checkbox]"));

                // checked visible
                var v = boxes.ElementAt(4);
                AssertUI.IsDisplayed(boxes.ElementAt(4).ElementAt("input[type=checkbox]", 0));
                AssertUI.IsNotDisplayed(boxes.ElementAt(4).ElementAt("input[type=checkbox]", 1));

                boxes.ElementAt(4).Single("input[data-ui=switch]").Click();

                AssertUI.IsNotDisplayed(boxes.ElementAt(4).ElementAt("input[type=checkbox]", 0));
                AssertUI.IsDisplayed(boxes.ElementAt(4).ElementAt("input[type=checkbox]", 1));

                // dataContext change
                boxes.ElementAt(5).First("input[type=checkbox]").Click();
                AssertUI.InnerTextEquals(boxes.ElementAt(5).First("span.result")
                    , "true");
            });
        }

        [Fact]
        public void Control_CheckBox_InRepeater()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_InRepeater);

                var repeater = browser.Single("div[data-ui='repeater']");
                var checkBoxes = browser.FindElements("label[data-ui='checkBox']");

                var checkBox = checkBoxes.ElementAt(0).Single("input");
                checkBox.Click();
                AssertUI.IsChecked(checkBox);
                AssertUI.InnerText(browser.Single("span[data-ui='selectedColors']"), s => s.Contains("orange"));

                checkBox = checkBoxes.ElementAt(1).Single("input");
                checkBox.Click();
                AssertUI.IsChecked(checkBox);
                AssertUI.InnerText(browser.Single("span[data-ui='selectedColors']"), s => s.Contains("orange") && s.Contains("red"));

                checkBox = checkBoxes.ElementAt(2).Single("input");
                checkBox.Click();
                AssertUI.IsChecked(checkBox);
                AssertUI.InnerText(browser.Single("span[data-ui='selectedColors']"), s => s.Contains("orange") && s.Contains("red") && s.Contains("black"));

                checkBoxes = browser.FindElements("label[data-ui='checkBox']");

                browser.First("[data-ui='set-server-values']").Click();
                AssertUI.IsChecked(checkBoxes.ElementAt(0).Single("input"));
                AssertUI.IsChecked(checkBoxes.ElementAt(2).Single("input"));
                AssertUI.InnerText(browser.Single("span[data-ui='selectedColors']"), s => s.Contains("orange") && s.Contains("black"));
            });
        }

        [Fact]
        public void Control_CheckBox_CheckedItemsNull()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_CheckedItemsNull);
            });
        }

        [Fact]
        public void Control_CheckBox_Indeterminate()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_Indeterminate);

                var checkBox = browser.First("input[type=checkbox]");
                var reset = browser.First("input[type=button]");
                var value = browser.First("span.value");

                AssertUI.InnerTextEquals(value, "Indeterminate");
                checkBox.Click();
                AssertUI.InnerTextEquals(value, "Other");
                reset.Click();
                AssertUI.InnerTextEquals(value, "Indeterminate");
            });
        }

        public CheckBoxTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
