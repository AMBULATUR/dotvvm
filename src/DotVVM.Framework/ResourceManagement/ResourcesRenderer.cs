﻿using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DotVVM.Framework.ResourceManagement
{
    public static class ResourcesRenderer
    {
        private static ConditionalWeakTable<IResource, string> renderedCache = new ConditionalWeakTable<IResource, string>();

        public static void RenderResourceCached(this NamedResource resource, IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.WriteUnencodedText(resource.GetRenderedTextCached(context));
        }

        static void WriteResourceInfo(NamedResource resource, IHtmlWriter writer, bool preload)
        {
            var comment = $"Resource {resource.Name} of type {resource.Resource.GetType().Name}.";
            if (resource.Resource is ILinkResource linkResource)
                comment += $" Pointing to {string.Join(", ", linkResource.GetLocations().Select(l => l.GetType().Name))}.";

            if (preload) comment = "[preload link] " + comment;

            writer.WriteUnencodedText("\n    <!-- ");
            writer.WriteText(comment);
            writer.WriteUnencodedText(" -->\n    ");
            //                               ^~~~ most likely this info will be written directly in the <body> or <head>, so it should be indented by one level.
            //                                    we don't have any better way to know how we should indent
        }

        public static void RenderResources(ResourceManager resourceManager, IHtmlWriter writer, IDotvvmRequestContext context, ResourceRenderPosition position)
        {
            var writeDebugInfo = context.Configuration.Debug;
            foreach (var resource in resourceManager.GetNamedResourcesInOrder())
            {
                if (resource.Resource.RenderPosition == position)
                {
                    if (writeDebugInfo) WriteResourceInfo(resource, writer, preload: false);
                    resource.RenderResourceCached(writer, context);
                }
                else if (position == ResourceRenderPosition.Head && resource.Resource.RenderPosition != ResourceRenderPosition.Head && resource.Resource is IPreloadResource preloadResource)
                {
                    if (writeDebugInfo) WriteResourceInfo(resource, writer, preload: true);
                    preloadResource.RenderPreloadLink(writer, context, resource.Name);
                }
            }
        }

        public static string GetRenderedTextCached(this NamedResource resource, IDotvvmRequestContext context) =>
            // dont use cache when debug, so the resource can be refreshed when file is changed
            context.Configuration.Debug ?
            RenderToString(resource, context) :
            renderedCache.GetValue(resource.Resource, _ => RenderToString(resource, context));

        private static string RenderToString(NamedResource resource, IDotvvmRequestContext context)
        {
            using (var text = new StringWriter())
            {
                resource.Resource.Render(new HtmlWriter(text, context), context, resource.Name);
                return text.ToString();
            }
        }
    }
}
