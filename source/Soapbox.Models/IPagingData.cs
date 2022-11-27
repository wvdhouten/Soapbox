namespace Soapbox.Models
{
    public interface IPagingData
    {
        int Page { get; set; }
        int PageCount { get; }
        int PageSize { get; set; }
        int Total { get; set; }
    }
}
