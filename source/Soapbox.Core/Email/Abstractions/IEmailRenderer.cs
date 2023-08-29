namespace Soapbox.Core.Email.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IEmailRenderer
    {
        Task<string> Render<TModel>(string templateName, TModel model);
    }
}
