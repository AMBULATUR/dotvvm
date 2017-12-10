using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using System.Text.RegularExpressions;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Routing
{
    public class DotvvmRoute : RouteBase
    {
        private Func<IDotvvmPresenter> presenterFactory;

        private Regex routeRegex;
        private List<Func<Dictionary<string, object>, string>> urlBuilders;
        private List<KeyValuePair<string, Func<string, ParameterParseResult>>> parameters;

        /// <summary>
        /// Gets the names of the route parameters in the order in which they appear in the URL.
        /// </summary>
        public override IEnumerable<string> ParameterNames => parameters.Select(p => p.Key);


        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmRoute"/> class.
        /// </summary>
        public DotvvmRoute(string url, string virtualPath, object defaultValues, Func<IDotvvmPresenter> presenterFactory, DotvvmConfiguration configuration)
            : base(url, virtualPath, defaultValues)
        {
            this.presenterFactory = presenterFactory;

            ParseRouteUrl(configuration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmRoute"/> class.
        /// </summary>
        public DotvvmRoute(string url, string virtualPath, IDictionary<string, object> defaultValues, Func<DotvvmPresenter> presenterFactory, DotvvmConfiguration configuration)
            : base(url, virtualPath, defaultValues)
        {
            this.presenterFactory = presenterFactory;

            ParseRouteUrl(configuration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmRoute"/> class.
        /// </summary>
        public DotvvmRoute(string url, string virtualPath, string name, IDictionary<string, object> defaultValues, Func<DotvvmPresenter> presenterFactory, DotvvmConfiguration configuration)
            : base(url, virtualPath, name, defaultValues)
        {
            this.presenterFactory = presenterFactory;

            ParseRouteUrl(configuration);
        }


        /// <summary>
        /// Parses the route URL and extracts the components.
        /// </summary>
        private void ParseRouteUrl(DotvvmConfiguration configuration)
        {
            var parser = new DotvvmRouteParser(configuration.RouteConstraints);

            var result = parser.ParseRouteUrl(Url, DefaultValues);

            routeRegex = result.RouteRegex;
            urlBuilders = result.UrlBuilders;
            parameters = result.Parameters;
        }

        /// <summary>
        /// Determines whether the route matches to the specified URL and extracts the parameter values.
        /// </summary>
        public override bool IsMatch(string url, out IDictionary<string, object> values)
        {
            if (!url.StartsWith("/"))
                url = '/' + url;

            var match = routeRegex.Match(url);
            if (!match.Success)
            {
                values = null;
                return false;
            }

            values = new Dictionary<string, object>(DefaultValues, StringComparer.OrdinalIgnoreCase);

            foreach (var parameter in parameters)
            {
                var g = match.Groups["param" + parameter.Key];
                if (g.Success)
                {
                    if (parameter.Value != null)
                    {
                        var r = parameter.Value(g.Value);
                        if (!r.IsOK) return false;
                        values[parameter.Key] = r.Value;
                    }
                    else
                        values[parameter.Key] = g.Value;
                }
            }
            return true;
        }

        /// <summary>
        /// Builds the URL core from the parameters.
        /// </summary>
        protected override string BuildUrlCore(Dictionary<string, object> values)
        {
            try
            {
                var url = string.Concat(urlBuilders.Select(b => b(values)));

                if (url == "~")
                    return "~/";

                return url;
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not build URL for route '{ this.Url }' with values {{{ string.Join(", ", values.Select(kvp => kvp.Key + ": " + kvp.Value)) }}}", ex);
            }
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        public override IDotvvmPresenter GetPresenter() => presenterFactory();
    }
}