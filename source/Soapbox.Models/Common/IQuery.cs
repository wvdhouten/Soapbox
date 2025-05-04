namespace Soapbox.Domain.Common;

public interface IQuery<TRequest, TResult>
{
    public TResult Execute(TRequest request);
}
