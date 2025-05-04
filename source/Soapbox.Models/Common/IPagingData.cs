namespace Soapbox.Domain.Common;

public interface IPagingData
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int PageCount { get; }

    public int Total { get; set; }
}
