﻿using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class SerializationTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_Serialization_Serialization()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Serialization_Serialization);

                // fill the values
                browser.ElementAt("input[type=text]", 0).SendKeys("1");
                browser.ElementAt("input[type=text]", 1).SendKeys("2");
                browser.Click("input[type=button]");

                // verify the results
                AssertUI.Attribute(browser.ElementAt("input[type=text]", 0), "value", s => s.Equals(""));
                AssertUI.Attribute(browser.ElementAt("input[type=text]", 1), "value", s => s.Equals("2"));
                AssertUI.InnerTextEquals(browser.Last("span"), ",2");
            });
        }

        [Fact]
        public void Feature_Serialization_ObservableCollectionShouldContainObservables()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Serialization_ObservableCollectionShouldContainObservables);
                browser.Wait();

                // verify that the values are selected
                browser.ElementAt("select", 0).Select(0);
                browser.ElementAt("select", 1).Select(1);
                browser.ElementAt("select", 2).Select(2);

                // click the button
                browser.Click("input[type=button]");

                // verify that the values are correct
                AssertUI.InnerTextEquals(browser.First("p.result"), "1,2,3");
                AssertUI.Attribute(browser.ElementAt("select", 0), "value", "1");
                AssertUI.Attribute(browser.ElementAt("select", 1), "value", "2");
                AssertUI.Attribute(browser.ElementAt("select", 2), "value", "3");
                browser.Wait();

                // change the values
                browser.ElementAt("select", 0).Select(1);
                browser.ElementAt("select", 1).Select(2);
                browser.ElementAt("select", 2).Select(1);

                // click the button
                browser.Click("input[type=button]");

                // verify that the values are correct
                AssertUI.InnerTextEquals(browser.First("p.result"), "2,3,2");
                AssertUI.Attribute(browser.ElementAt("select", 0), "value", "2");
                AssertUI.Attribute(browser.ElementAt("select", 1), "value", "3");
                AssertUI.Attribute(browser.ElementAt("select", 2), "value", "2");
            });
        }

        [Fact]
        public void Feature_Serialization_EnumSerializationWithJsonConverter()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Serialization_EnumSerializationWithJsonConverter);
                browser.Wait();

                // click on the button
                browser.Single("input[type=button]").Click().Wait();

                // make sure that deserialization worked correctly
                AssertUI.InnerTextEquals(browser.First("p.result"), "Success!");
            });
        }

        public SerializationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
