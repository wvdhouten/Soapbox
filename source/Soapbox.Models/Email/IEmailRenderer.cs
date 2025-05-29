namespace Soapbox.Domain.Email;

using System.Threading.Tasks;

public interface IEmailRenderer
{
    public Task<string> Render<TModel>(string templateName, TModel model);
}
