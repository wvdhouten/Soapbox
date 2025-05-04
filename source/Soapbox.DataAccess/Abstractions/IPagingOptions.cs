namespace Soapbox.DataAccess.Abstractions;

public interface IPagingOptions
{
    public int Page { get; set; }

    public int? PageSize { get; set; }
}
