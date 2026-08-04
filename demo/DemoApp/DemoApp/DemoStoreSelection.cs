namespace DemoApp;

/// <summary>
/// Persisted choice of the RevenueCat backend the demo initializes against.
/// The Test Store key is platform-agnostic; production keys are the real store keys.
/// Changing the selection requires an app restart because the RevenueCat SDK is configured once per process.
/// </summary>
public static class DemoStoreSelection
{
    private const string PreferenceKey = "SelectedDemoStore";

    public const string TestStoreApiKey = "test_aHuTbnFAcfcLlozAnavrXKabAZJ";
    public const string ProductionAndroidApiKey = "goog_tHAVxtQQbsAWBRGLrEwezuavnmI";
    public const string ProductionIosApiKey = "appl_IbYjYDwWLoqUZXxFXrCCyJKLPZc";

    public static DemoStore Current
    {
        get => (DemoStore)Preferences.Get(PreferenceKey, (int)DemoStore.TestStore);
        set => Preferences.Set(PreferenceKey, (int)value);
    }

    public static string ActiveApiKey
    {
        get
        {
            if (Current == DemoStore.TestStore)
            {
                return TestStoreApiKey;
            }
#if __ANDROID__
			return ProductionAndroidApiKey;
#elif __IOS__
			return ProductionIosApiKey;
#else
            return string.Empty;
#endif
        }
    }
}

public enum DemoStore
{
    TestStore,
    Production,
}
