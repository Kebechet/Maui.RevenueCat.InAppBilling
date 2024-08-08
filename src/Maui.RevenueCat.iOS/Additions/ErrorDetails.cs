using Foundation;

namespace Maui.RevenueCat.iOS.Additions;

/// <summary>
/// C# representation of https://github.com/RevenueCat/purchases-ios/blob/4.1.0/Purchases/Error%20Handling/ErrorDetails.swift
/// </summary>
public static class ErrorDetails
{
    public static readonly NSString FinishableKey = new("finishable");
    public static readonly NSString ReadableErrorCodeKey = new("readable_error_code");
    public static readonly NSString ExtraContextKey = new("extra_context");
    public static readonly NSString FileKey = new("source_file");
    public static readonly NSString FunctionKey = new("source_function");
}
