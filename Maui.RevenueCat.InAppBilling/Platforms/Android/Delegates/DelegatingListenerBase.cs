namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Delegates;

public abstract class DelegatingListenerBase<TResult> : Java.Lang.Object
{
    private readonly TaskCompletionSource<TResult> _taskCompletionSource;

    public DelegatingListenerBase(CancellationToken cancellationToken)
    {
        _taskCompletionSource = new TaskCompletionSource<TResult>();
        cancellationToken.Register(() => _taskCompletionSource.TrySetCanceled());
    }

    public Task<TResult> Task => _taskCompletionSource.Task;

    protected void ReportSuccess(TResult result)
    {
        _taskCompletionSource.TrySetResult(result);
    }

    protected void ReportException(Exception exception)
    {
        _taskCompletionSource.TrySetException(exception);
    }
}