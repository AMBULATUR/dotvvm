using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Base class for all controls that decorates another control (e.g. adds attributes).
    /// </summary>
    public class Decorator : HtmlGenericControl 
    {

        public Decorator() : base("")
        {
        }

        public virtual Decorator Clone()
        {
            var decorator = (Decorator)Activator.CreateInstance(GetType());

            foreach (var prop in Properties)
            {
                var value = prop.Value;
                if (value is BindingExpression)
                {
                    value = ((BindingExpression)value).Clone();
                }

                decorator.Properties[prop.Key] = value;
            }

            foreach (var attr in Attributes)
            {
                decorator.Attributes[attr.Key] = attr.Value;
            }

            return decorator;
        }

        protected override void RenderBeginTag(IHtmlWriter writer, RenderContext context)
        {
            // do nothing
        }

        protected override void RenderEndTag(IHtmlWriter writer, RenderContext context)
        {
            // do nothing
        }
    }
}
