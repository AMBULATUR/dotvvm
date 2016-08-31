﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using DotVVM.Framework.Hosting;
using Microsoft.Owin;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHttpContext : IHttpContext
    {
        public DotvvmHttpContext(IOwinContext originalContext, IAuthentication authentication)
        {
            OriginalContext = originalContext;
            Authentication = authentication;
        }

        public ClaimsPrincipal User { get; set; }
        public IHttpRequest Request { get; set; }
        public IHttpResponse Response { get; set; }
        public IAuthentication Authentication { get; }
        public IOwinContext OriginalContext { get; }

        public T GetItem<T>(string key) => OriginalContext.Get<T>(key);

        public void SetItem<T>(string key, T value) => OriginalContext.Set(key, value);

        public IEnumerable<Tuple<string, IEnumerable<KeyValuePair<string, object>>>> GetEnvironmentTabs()
        {
            yield return new Tuple<string, IEnumerable<KeyValuePair<string, object>>>("Environment", OriginalContext.Environment);
        }
    }
}