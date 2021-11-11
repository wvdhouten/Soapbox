namespace Soapbox.DataAccess.Abstractions
{
    public interface IPagingOptions
    {
        int Page { get; set; }

        int? PageSize { get; set; }
    }
}
