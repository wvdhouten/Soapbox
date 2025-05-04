namespace Soapbox.Application.Email;

public class SmtpSettings
{
    public string Host { get; set; }

    public int Port { get; set; }

    public bool EnableSsl { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }

    public string Sender { get; set; }
}
