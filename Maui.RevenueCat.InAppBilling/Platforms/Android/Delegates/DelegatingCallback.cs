using Com.Revenuecat.Purchases.Interfaces;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Delegates;

public class DelegatingCallback<TResult> : DelegatingListenerBase<TResult>, ICallback
{
    public DelegatingCallback(CancellationToken cancellationToken) : base(cancellationToken)
    {
    }

    public void OnReceived(Java.Lang.Object resultObject)
    {
        if (resultObject is TResult result)
        {
            ReportSuccess(result);
        }
        else
        {
            ReportException(new Exception($"{resultObject?.GetType().Name} is not a {typeof(TResult).Name}"));
        }
    }
}