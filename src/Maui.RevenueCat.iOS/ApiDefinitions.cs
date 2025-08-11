using Foundation;
using ObjCRuntime;
using StoreKit;
using System;
using System.ComponentModel;

namespace Maui.RevenueCat.iOS;

delegate void ReadyForPromotedProductCallbackHandler(RCStoreTransaction transaction, RCCustomerInfo customerInfo, NSError error, bool userCancelled);
delegate void StartPurchaseHandler([BlockCallback] ReadyForPromotedProductCallbackHandler defermentBlock);

// @interface RCAttribution : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCAttribution
{
	// -(void)enableAdServicesAttributionTokenCollection;
	[Export ("enableAdServicesAttributionTokenCollection")]
	void EnableAdServicesAttributionTokenCollection ();

	// -(void)collectDeviceIdentifiers;
	[Export ("collectDeviceIdentifiers")]
	void CollectDeviceIdentifiers ();

	// -(void)setAttributes:(NSDictionary<NSString *,NSString *> * _Nonnull)attributes;
	[Export ("setAttributes:")]
	void SetAttributes (NSDictionary<NSString, NSString> attributes);

	// -(void)setEmail:(NSString * _Nullable)email;
	[Export ("setEmail:")]
	void SetEmail ([NullAllowed] string email);

	// -(void)setPhoneNumber:(NSString * _Nullable)phoneNumber;
	[Export ("setPhoneNumber:")]
	void SetPhoneNumber ([NullAllowed] string phoneNumber);

	// -(void)setDisplayName:(NSString * _Nullable)displayName;
	[Export ("setDisplayName:")]
	void SetDisplayName ([NullAllowed] string displayName);

	// -(void)setPushToken:(NSData * _Nullable)pushToken;
	[Export ("setPushToken:")]
	void SetPushToken ([NullAllowed] NSData pushToken);

	// -(void)setPushTokenString:(NSString * _Nullable)pushToken;
	[Export ("setPushTokenString:")]
	void SetPushTokenString ([NullAllowed] string pushToken);

	// -(void)setAdjustID:(NSString * _Nullable)adjustID;
	[Export ("setAdjustID:")]
	void SetAdjustID ([NullAllowed] string adjustID);

	// -(void)setAppsflyerID:(NSString * _Nullable)appsflyerID;
	[Export ("setAppsflyerID:")]
	void SetAppsflyerID ([NullAllowed] string appsflyerID);

	// -(void)setFBAnonymousID:(NSString * _Nullable)fbAnonymousID;
	[Export ("setFBAnonymousID:")]
	void SetFBAnonymousID ([NullAllowed] string fbAnonymousID);

	// -(void)setMparticleID:(NSString * _Nullable)mparticleID;
	[Export ("setMparticleID:")]
	void SetMparticleID ([NullAllowed] string mparticleID);

	// -(void)setOnesignalID:(NSString * _Nullable)onesignalID;
	[Export ("setOnesignalID:")]
	void SetOnesignalID ([NullAllowed] string onesignalID);

	// -(void)setOnesignalUserID:(NSString * _Nullable)onesignalUserID;
	[Export ("setOnesignalUserID:")]
	void SetOnesignalUserID ([NullAllowed] string onesignalUserID);

	// -(void)setAirshipChannelID:(NSString * _Nullable)airshipChannelID;
	[Export ("setAirshipChannelID:")]
	void SetAirshipChannelID ([NullAllowed] string airshipChannelID);

	// -(void)setCleverTapID:(NSString * _Nullable)cleverTapID;
	[Export ("setCleverTapID:")]
	void SetCleverTapID ([NullAllowed] string cleverTapID);

	// -(void)setKochavaDeviceID:(NSString * _Nullable)kochavaDeviceID;
	[Export ("setKochavaDeviceID:")]
	void SetKochavaDeviceID ([NullAllowed] string kochavaDeviceID);

	// -(void)setMixpanelDistinctID:(NSString * _Nullable)mixpanelDistinctID;
	[Export ("setMixpanelDistinctID:")]
	void SetMixpanelDistinctID ([NullAllowed] string mixpanelDistinctID);

	// -(void)setFirebaseAppInstanceID:(NSString * _Nullable)firebaseAppInstanceID;
	[Export ("setFirebaseAppInstanceID:")]
	void SetFirebaseAppInstanceID ([NullAllowed] string firebaseAppInstanceID);

	// -(void)setTenjinAnalyticsInstallationID:(NSString * _Nullable)tenjinAnalyticsInstallationID;
	[Export ("setTenjinAnalyticsInstallationID:")]
	void SetTenjinAnalyticsInstallationID ([NullAllowed] string tenjinAnalyticsInstallationID);

	// -(void)setPostHogUserID:(NSString * _Nullable)postHogUserID;
	[Export ("setPostHogUserID:")]
	void SetPostHogUserID ([NullAllowed] string postHogUserID);

	// -(void)setAmplitudeUserID:(NSString * _Nullable)amplitudeUserID;
	[Export ("setAmplitudeUserID:")]
	void SetAmplitudeUserID ([NullAllowed] string amplitudeUserID);

	// -(void)setAmplitudeDeviceID:(NSString * _Nullable)amplitudeDeviceID;
	[Export ("setAmplitudeDeviceID:")]
	void SetAmplitudeDeviceID ([NullAllowed] string amplitudeDeviceID);

	// -(void)setMediaSource:(NSString * _Nullable)mediaSource;
	[Export ("setMediaSource:")]
	void SetMediaSource ([NullAllowed] string mediaSource);

	// -(void)setCampaign:(NSString * _Nullable)campaign;
	[Export ("setCampaign:")]
	void SetCampaign ([NullAllowed] string campaign);

	// -(void)setAdGroup:(NSString * _Nullable)adGroup;
	[Export ("setAdGroup:")]
	void SetAdGroup ([NullAllowed] string adGroup);

	// -(void)setAd:(NSString * _Nullable)installAd;
	[Export ("setAd:")]
	void SetAd ([NullAllowed] string installAd);

	// -(void)setKeyword:(NSString * _Nullable)keyword;
	[Export ("setKeyword:")]
	void SetKeyword ([NullAllowed] string keyword);

	// -(void)setCreative:(NSString * _Nullable)creative;
	[Export ("setCreative:")]
	void SetCreative ([NullAllowed] string creative);
}

// @interface RCConfigurationBuilder : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCConfigurationBuilder
{
	// -(instancetype _Nonnull)initWithAPIKey:(NSString * _Nonnull)apiKey __attribute__((objc_designated_initializer));
	[Export ("initWithAPIKey:")]
	[DesignatedInitializer]
	NativeHandle Constructor (string apiKey);

	// -(RCConfigurationBuilder * _Nonnull)withApiKey:(NSString * _Nonnull)apiKey __attribute__((warn_unused_result("")));
	[Export ("withApiKey:")]
	RCConfigurationBuilder WithApiKey (string apiKey);

	// -(RCConfigurationBuilder * _Nonnull)withAppUserID:(NSString * _Nullable)appUserID __attribute__((warn_unused_result("")));
	[Export ("withAppUserID:")]
	RCConfigurationBuilder WithAppUserID ([NullAllowed] string appUserID);

	// -(RCConfigurationBuilder * _Nonnull)withPurchasesAreCompletedBy:(enum RCPurchasesAreCompletedBy)purchasesAreCompletedBy storeKitVersion:(enum RCStoreKitVersion)storeKitVersion __attribute__((warn_unused_result("")));
	[Export ("withPurchasesAreCompletedBy:storeKitVersion:")]
	RCConfigurationBuilder WithPurchasesAreCompletedBy (RCPurchasesAreCompletedBy purchasesAreCompletedBy, RCStoreKitVersion storeKitVersion);

	// -(RCConfigurationBuilder * _Nonnull)withUserDefaults:(NSUserDefaults * _Nonnull)userDefaults __attribute__((warn_unused_result("")));
	[Export ("withUserDefaults:")]
	RCConfigurationBuilder WithUserDefaults (NSUserDefaults userDefaults);

	// -(RCConfigurationBuilder * _Nonnull)withDangerousSettings:(RCDangerousSettings * _Nonnull)dangerousSettings __attribute__((warn_unused_result("")));
	[Export ("withDangerousSettings:")]
	RCConfigurationBuilder WithDangerousSettings (RCDangerousSettings dangerousSettings);

	// -(RCConfigurationBuilder * _Nonnull)withNetworkTimeout:(NSTimeInterval)networkTimeout __attribute__((warn_unused_result("")));
	[Export ("withNetworkTimeout:")]
	RCConfigurationBuilder WithNetworkTimeout (double networkTimeout);

	// -(RCConfigurationBuilder * _Nonnull)withStoreKit1Timeout:(NSTimeInterval)storeKit1Timeout __attribute__((warn_unused_result("")));
	[Export ("withStoreKit1Timeout:")]
	RCConfigurationBuilder WithStoreKit1Timeout (double storeKit1Timeout);

	// -(RCConfigurationBuilder * _Nonnull)withPlatformInfo:(RCPlatformInfo * _Nonnull)platformInfo __attribute__((warn_unused_result("")));
	[Export ("withPlatformInfo:")]
	RCConfigurationBuilder WithPlatformInfo (RCPlatformInfo platformInfo);

	// -(RCConfigurationBuilder * _Nonnull)withShowStoreMessagesAutomatically:(BOOL)showStoreMessagesAutomatically __attribute__((warn_unused_result("")));
	[Export ("withShowStoreMessagesAutomatically:")]
	RCConfigurationBuilder WithShowStoreMessagesAutomatically (bool showStoreMessagesAutomatically);

	// -(RCConfigurationBuilder * _Nonnull)withEntitlementVerificationMode:(enum RCEntitlementVerificationMode)mode __attribute__((warn_unused_result("")));
	[Export ("withEntitlementVerificationMode:")]
	RCConfigurationBuilder WithEntitlementVerificationMode (RCEntitlementVerificationMode mode);

	// -(RCConfigurationBuilder * _Nonnull)withDiagnosticsEnabled:(BOOL)diagnosticsEnabled __attribute__((warn_unused_result(""))) __attribute__((availability(watchos, introduced=8.0))) __attribute__((availability(macos, introduced=12.0))) __attribute__((availability(tvos, introduced=15.0))) __attribute__((availability(ios, introduced=15.0)));
	[Export ("withDiagnosticsEnabled:")]
	RCConfigurationBuilder WithDiagnosticsEnabled (bool diagnosticsEnabled);

	// -(RCConfigurationBuilder * _Nonnull)withStoreKitVersion:(enum RCStoreKitVersion)version __attribute__((warn_unused_result("")));
	[Export ("withStoreKitVersion:")]
	RCConfigurationBuilder WithStoreKitVersion (RCStoreKitVersion version);

	// -(RCConfiguration * _Nonnull)build __attribute__((warn_unused_result("")));
	[Export ("build")]
	RCConfiguration Build { get; }

	// -(RCConfigurationBuilder * _Nonnull)withObserverMode:(BOOL)observerMode __attribute__((warn_unused_result(""))) __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'with' has been renamed to 'withPurchasesAreCompletedBy:storeKitVersion:': Observer Mode is now named PurchasesAreCompletedBy.")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'with' has been renamed to 'withPurchasesAreCompletedBy:storeKitVersion:': Observer Mode is now named PurchasesAreCompletedBy.")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'with' has been renamed to 'withPurchasesAreCompletedBy:storeKitVersion:': Observer Mode is now named PurchasesAreCompletedBy.")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'with' has been renamed to 'withPurchasesAreCompletedBy:storeKitVersion:': Observer Mode is now named PurchasesAreCompletedBy.")]
	[Export ("withObserverMode:")]
	RCConfigurationBuilder WithObserverMode (bool observerMode);

	// -(RCConfigurationBuilder * _Nonnull)withUsesStoreKit2IfAvailable:(BOOL)usesStoreKit2IfAvailable __attribute__((warn_unused_result(""))) __attribute__((deprecated("Use .with(storeKitVersion:) to enable StoreKit 2")));
	[Export ("withUsesStoreKit2IfAvailable:")]
	RCConfigurationBuilder WithUsesStoreKit2IfAvailable (bool usesStoreKit2IfAvailable);
}

// @interface RCConfiguration : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCConfiguration
{
	// +(RCConfigurationBuilder * _Nonnull)builderWithAPIKey:(NSString * _Nonnull)apiKey __attribute__((warn_unused_result("")));
	[Static]
	[Export ("builderWithAPIKey:")]
	RCConfigurationBuilder BuilderWithAPIKey (string apiKey);
}

// @interface RCCustomerInfo : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCCustomerInfo
{
	// @property (readonly, nonatomic, strong) RCEntitlementInfos * _Nonnull entitlements;
	[Export ("entitlements", ArgumentSemantic.Strong)]
	RCEntitlementInfos Entitlements { get; }

	// @property (readonly, copy, nonatomic) NSSet<NSString *> * _Nonnull activeSubscriptions;
	[Export ("activeSubscriptions", ArgumentSemantic.Copy)]
	NSSet<NSString> ActiveSubscriptions { get; }

	// @property (readonly, copy, nonatomic) NSSet<NSString *> * _Nonnull allPurchasedProductIdentifiers;
	[Export ("allPurchasedProductIdentifiers", ArgumentSemantic.Copy)]
	NSSet<NSString> AllPurchasedProductIdentifiers { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nullable latestExpirationDate;
	[NullAllowed, Export ("latestExpirationDate", ArgumentSemantic.Copy)]
	NSDate LatestExpirationDate { get; }

	// @property (readonly, copy, nonatomic) NSArray<RCNonSubscriptionTransaction *> * _Nonnull nonSubscriptions;
	[Export ("nonSubscriptions", ArgumentSemantic.Copy)]
	RCNonSubscriptionTransaction[] NonSubscriptions { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nonnull requestDate;
	[Export ("requestDate", ArgumentSemantic.Copy)]
	NSDate RequestDate { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nonnull firstSeen;
	[Export ("firstSeen", ArgumentSemantic.Copy)]
	NSDate FirstSeen { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull originalAppUserId;
	[Export ("originalAppUserId")]
	string OriginalAppUserId { get; }

	// @property (readonly, copy, nonatomic) NSURL * _Nullable managementURL;
	[NullAllowed, Export ("managementURL", ArgumentSemantic.Copy)]
	NSUrl ManagementURL { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nullable originalPurchaseDate;
	[NullAllowed, Export ("originalPurchaseDate", ArgumentSemantic.Copy)]
	NSDate OriginalPurchaseDate { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nullable originalApplicationVersion;
	[NullAllowed, Export ("originalApplicationVersion")]
	string OriginalApplicationVersion { get; }

	// @property (readonly, copy, nonatomic) NSDictionary<NSString *,RCSubscriptionInfo *> * _Nonnull subscriptionsByProductIdentifier;
	[Export ("subscriptionsByProductIdentifier", ArgumentSemantic.Copy)]
	NSDictionary<NSString, RCSubscriptionInfo> SubscriptionsByProductIdentifier { get; }

	// -(NSDate * _Nullable)expirationDateForProductIdentifier:(NSString * _Nonnull)productIdentifier __attribute__((warn_unused_result("")));
	[Export ("expirationDateForProductIdentifier:")]
	[return: NullAllowed]
	NSDate ExpirationDateForProductIdentifier (string productIdentifier);

	// -(NSDate * _Nullable)purchaseDateForProductIdentifier:(NSString * _Nonnull)productIdentifier __attribute__((warn_unused_result("")));
	[Export ("purchaseDateForProductIdentifier:")]
	[return: NullAllowed]
	NSDate PurchaseDateForProductIdentifier (string productIdentifier);

	// -(NSDate * _Nullable)expirationDateForEntitlement:(NSString * _Nonnull)entitlementIdentifier __attribute__((warn_unused_result("")));
	[Export ("expirationDateForEntitlement:")]
	[return: NullAllowed]
	NSDate ExpirationDateForEntitlement (string entitlementIdentifier);

	// -(NSDate * _Nullable)purchaseDateForEntitlement:(NSString * _Nonnull)entitlementIdentifier __attribute__((warn_unused_result("")));
	[Export ("purchaseDateForEntitlement:")]
	[return: NullAllowed]
	NSDate PurchaseDateForEntitlement (string entitlementIdentifier);

	// @property (readonly, nonatomic) NSUInteger hash;
	[Export ("hash")]
	nuint Hash { get; }

	// @property (readonly, copy, nonatomic) NSDictionary<NSString *,id> * _Nonnull rawData;
	[Export ("rawData", ArgumentSemantic.Copy)]
	NSDictionary<NSString, NSObject> RawData { get; }

	// @property (readonly, copy, nonatomic) SWIFT_DEPRECATED_MSG("use nonSubscriptionTransactions") NSSet<NSString *> * nonConsumablePurchases __attribute__((deprecated("use nonSubscriptionTransactions")));
	[Export ("nonConsumablePurchases", ArgumentSemantic.Copy)]
	NSSet<NSString> NonConsumablePurchases { get; }

	// @property (readonly, copy, nonatomic) SWIFT_DEPRECATED_MSG("", "nonSubscriptions") NSArray<RCStoreTransaction *> * nonSubscriptionTransactions __attribute__((deprecated("", "nonSubscriptions")));
	[Export ("nonSubscriptionTransactions", ArgumentSemantic.Copy)]
	RCStoreTransaction[] NonSubscriptionTransactions { get; }
}

// @interface RCDangerousSettings : NSObject
[BaseType (typeof(NSObject))]
interface RCDangerousSettings
{
	// @property (readonly, nonatomic) BOOL autoSyncPurchases;
	[Export ("autoSyncPurchases")]
	bool AutoSyncPurchases { get; }

	// @property (readonly, nonatomic) BOOL customEntitlementComputation;
	[Export ("customEntitlementComputation")]
	bool CustomEntitlementComputation { get; }

	// -(instancetype _Nonnull)initWithAutoSyncPurchases:(BOOL)autoSyncPurchases;
	[Export ("initWithAutoSyncPurchases:")]
	NativeHandle Constructor (bool autoSyncPurchases);

	// -(instancetype _Nonnull)initWithAutoSyncPurchases:(BOOL)autoSyncPurchases customEntitlementComputation:(BOOL)customEntitlementComputation;
	[Export ("initWithAutoSyncPurchases:customEntitlementComputation:")]
	NativeHandle Constructor (bool autoSyncPurchases, bool customEntitlementComputation);
}

// @interface RCEntitlementInfo : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCEntitlementInfo : INativeObject
{
	// @property (readonly, copy, nonatomic) NSString * _Nonnull identifier;
	[Export ("identifier")]
	string Identifier { get; }

	// @property (readonly, nonatomic) BOOL isActive;
	[Export ("isActive")]
	bool IsActive { get; }

	// @property (readonly, nonatomic) BOOL willRenew;
	[Export ("willRenew")]
	bool WillRenew { get; }

	// @property (readonly, nonatomic) enum RCPeriodType periodType;
	[Export ("periodType")]
	RCPeriodType PeriodType { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nullable latestPurchaseDate;
	[NullAllowed, Export ("latestPurchaseDate", ArgumentSemantic.Copy)]
	NSDate LatestPurchaseDate { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nullable originalPurchaseDate;
	[NullAllowed, Export ("originalPurchaseDate", ArgumentSemantic.Copy)]
	NSDate OriginalPurchaseDate { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nullable expirationDate;
	[NullAllowed, Export ("expirationDate", ArgumentSemantic.Copy)]
	NSDate ExpirationDate { get; }

	// @property (readonly, nonatomic) enum RCStore store;
	[Export ("store")]
	RCStore Store { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull productIdentifier;
	[Export ("productIdentifier")]
	string ProductIdentifier { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nullable productPlanIdentifier;
	[NullAllowed, Export ("productPlanIdentifier")]
	string ProductPlanIdentifier { get; }

	// @property (readonly, nonatomic) BOOL isSandbox;
	[Export ("isSandbox")]
	bool IsSandbox { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nullable unsubscribeDetectedAt;
	[NullAllowed, Export ("unsubscribeDetectedAt", ArgumentSemantic.Copy)]
	NSDate UnsubscribeDetectedAt { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nullable billingIssueDetectedAt;
	[NullAllowed, Export ("billingIssueDetectedAt", ArgumentSemantic.Copy)]
	NSDate BillingIssueDetectedAt { get; }

	// @property (readonly, nonatomic) enum RCPurchaseOwnershipType ownershipType;
	[Export ("ownershipType")]
	RCPurchaseOwnershipType OwnershipType { get; }

	// @property (readonly, nonatomic) enum RCVerificationResult verification;
	[Export ("verification")]
	RCVerificationResult Verification { get; }

	// @property (readonly, copy, nonatomic) NSDictionary<NSString *,id> * _Nonnull rawData;
	[Export ("rawData", ArgumentSemantic.Copy)]
	NSDictionary<NSString, NSObject> RawData { get; }

	// @property (readonly, nonatomic) NSUInteger hash;
	[Export ("hash")]
	nuint Hash { get; }

	// @property (readonly, nonatomic) BOOL isActiveInCurrentEnvironment;
	[Export ("isActiveInCurrentEnvironment")]
	bool IsActiveInCurrentEnvironment { get; }

	// @property (readonly, nonatomic) BOOL isActiveInAnyEnvironment;
	[Export ("isActiveInAnyEnvironment")]
	bool IsActiveInAnyEnvironment { get; }
}

// @interface RCEntitlementInfos : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCEntitlementInfos
{
	// @property (readonly, copy, nonatomic) NSDictionary<NSString *,RCEntitlementInfo *> * _Nonnull all;
	[Export ("all", ArgumentSemantic.Copy)]
	NSDictionary<NSString, RCEntitlementInfo> All { get; }

	// -(RCEntitlementInfo * _Nullable)objectForKeyedSubscript:(NSString * _Nonnull)key __attribute__((warn_unused_result("")));
	[Export ("objectForKeyedSubscript:")]
	[return: NullAllowed]
	RCEntitlementInfo ObjectForKeyedSubscript (string key);

	// @property (readonly, nonatomic) enum RCVerificationResult verification;
	[Export ("verification")]
	RCVerificationResult Verification { get; }

	// @property (readonly, copy, nonatomic) NSDictionary<NSString *,RCEntitlementInfo *> * _Nonnull active;
	[Export ("active", ArgumentSemantic.Copy)]
	NSDictionary<NSString, RCEntitlementInfo> Active { get; }

	// @property (readonly, copy, nonatomic) NSDictionary<NSString *,RCEntitlementInfo *> * _Nonnull activeInCurrentEnvironment;
	[Export ("activeInCurrentEnvironment", ArgumentSemantic.Copy)]
	NSDictionary<NSString, RCEntitlementInfo> ActiveInCurrentEnvironment { get; }

	// @property (readonly, copy, nonatomic) NSDictionary<NSString *,RCEntitlementInfo *> * _Nonnull activeInAnyEnvironment;
	[Export ("activeInAnyEnvironment", ArgumentSemantic.Copy)]
	NSDictionary<NSString, RCEntitlementInfo> ActiveInAnyEnvironment { get; }
}

// @interface RCIntroEligibility : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCIntroEligibility : INativeObject
{
	// @property (readonly, nonatomic) enum RCIntroEligibilityStatus status;
	[Export ("status")]
	RCIntroEligibilityStatus Status { get; }

	// @property (readonly, nonatomic) NSUInteger hash;
	[Export ("hash")]
	nuint Hash { get; }
}

// @interface RCNonSubscriptionTransaction : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCNonSubscriptionTransaction
{
	// @property (readonly, copy, nonatomic) NSString * _Nonnull productIdentifier;
	[Export ("productIdentifier")]
	string ProductIdentifier { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nonnull purchaseDate;
	[Export ("purchaseDate", ArgumentSemantic.Copy)]
	NSDate PurchaseDate { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull transactionIdentifier;
	[Export ("transactionIdentifier")]
	string TransactionIdentifier { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull storeTransactionIdentifier;
	[Export ("storeTransactionIdentifier")]
	string StoreTransactionIdentifier { get; }

	// @property (readonly, nonatomic) enum RCStore store;
	[Export ("store")]
	RCStore Store { get; }

	// @property (readonly, nonatomic, strong) RCProductPaidPrice * _Nullable price;
	[NullAllowed, Export ("price", ArgumentSemantic.Strong)]
	RCProductPaidPrice Price { get; }

	// @property (readonly, nonatomic) BOOL isSandbox;
	[Export ("isSandbox")]
	bool IsSandbox { get; }
}

// @interface RCOffering : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCOffering : INativeObject
{
	// @property (readonly, copy, nonatomic) NSString * _Nonnull identifier;
	[Export ("identifier")]
	string Identifier { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull serverDescription;
	[Export ("serverDescription")]
	string ServerDescription { get; }

	// @property (readonly, copy, nonatomic) NSDictionary<NSString *,id> * _Nonnull metadata;
	[Export ("metadata", ArgumentSemantic.Copy)]
	NSDictionary<NSString, NSObject> Metadata { get; }

	// @property (readonly, copy, nonatomic) NSArray<RCPackage *> * _Nonnull availablePackages;
	[Export ("availablePackages", ArgumentSemantic.Copy)]
	RCPackage[] AvailablePackages { get; }

	// @property (readonly, nonatomic, strong) RCPackage * _Nullable lifetime;
	[NullAllowed, Export ("lifetime", ArgumentSemantic.Strong)]
	RCPackage Lifetime { get; }

	// @property (readonly, nonatomic, strong) RCPackage * _Nullable annual;
	[NullAllowed, Export ("annual", ArgumentSemantic.Strong)]
	RCPackage Annual { get; }

	// @property (readonly, nonatomic, strong) RCPackage * _Nullable sixMonth;
	[NullAllowed, Export ("sixMonth", ArgumentSemantic.Strong)]
	RCPackage SixMonth { get; }

	// @property (readonly, nonatomic, strong) RCPackage * _Nullable threeMonth;
	[NullAllowed, Export ("threeMonth", ArgumentSemantic.Strong)]
	RCPackage ThreeMonth { get; }

	// @property (readonly, nonatomic, strong) RCPackage * _Nullable twoMonth;
	[NullAllowed, Export ("twoMonth", ArgumentSemantic.Strong)]
	RCPackage TwoMonth { get; }

	// @property (readonly, nonatomic, strong) RCPackage * _Nullable monthly;
	[NullAllowed, Export ("monthly", ArgumentSemantic.Strong)]
	RCPackage Monthly { get; }

	// @property (readonly, nonatomic, strong) RCPackage * _Nullable weekly;
	[NullAllowed, Export ("weekly", ArgumentSemantic.Strong)]
	RCPackage Weekly { get; }

	// @property (readonly, copy, nonatomic) NSURL * _Nullable webCheckoutUrl;
	[NullAllowed, Export ("webCheckoutUrl", ArgumentSemantic.Copy)]
	NSUrl WebCheckoutUrl { get; }

	// -(RCPackage * _Nullable)packageWithIdentifier:(NSString * _Nullable)identifier __attribute__((warn_unused_result("")));
	[Export ("packageWithIdentifier:")]
	[return: NullAllowed]
	RCPackage PackageWithIdentifier ([NullAllowed] string identifier);

	// -(RCPackage * _Nullable)objectForKeyedSubscript:(NSString * _Nonnull)key __attribute__((warn_unused_result("")));
	[Export ("objectForKeyedSubscript:")]
	[return: NullAllowed]
	RCPackage ObjectForKeyedSubscript (string key);

	// -(instancetype _Nonnull)initWithIdentifier:(NSString * _Nonnull)identifier serverDescription:(NSString * _Nonnull)serverDescription metadata:(NSDictionary<NSString *,id> * _Nonnull)metadata availablePackages:(NSArray<RCPackage *> * _Nonnull)availablePackages webCheckoutUrl:(NSURL * _Nullable)webCheckoutUrl;
	[Export ("initWithIdentifier:serverDescription:metadata:availablePackages:webCheckoutUrl:")]
	NativeHandle Constructor (string identifier, string serverDescription, NSDictionary<NSString, NSObject> metadata, RCPackage[] availablePackages, [NullAllowed] NSUrl webCheckoutUrl);
}

// @interface RCOfferings : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCOfferings
{
	// @property (readonly, copy, nonatomic) NSDictionary<NSString *,RCOffering *> * _Nonnull all;
	[Export ("all", ArgumentSemantic.Copy)]
	NSDictionary<NSString, RCOffering> All { get; }

	// @property (readonly, nonatomic, strong) RCOffering * _Nullable current;
	[NullAllowed, Export ("current", ArgumentSemantic.Strong)]
	RCOffering Current { get; }

	// -(RCOffering * _Nullable)offeringWithIdentifier:(NSString * _Nullable)identifier __attribute__((warn_unused_result("")));
	[Export ("offeringWithIdentifier:")]
	[return: NullAllowed]
	RCOffering OfferingWithIdentifier ([NullAllowed] string identifier);

	// -(RCOffering * _Nullable)objectForKeyedSubscript:(NSString * _Nonnull)key __attribute__((warn_unused_result("")));
	[Export ("objectForKeyedSubscript:")]
	[return: NullAllowed]
	RCOffering ObjectForKeyedSubscript (string key);

	// -(RCOffering * _Nullable)currentOfferingForPlacement:(NSString * _Nonnull)placementIdentifier __attribute__((warn_unused_result("")));
	[Export ("currentOfferingForPlacement:")]
	[return: NullAllowed]
	RCOffering CurrentOfferingForPlacement (string placementIdentifier);
}

// @interface RCPackage : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCPackage
{
	// @property (readonly, copy, nonatomic) NSString * _Nonnull identifier;
	[Export ("identifier")]
	string Identifier { get; }

	// @property (readonly, nonatomic) enum RCPackageType packageType;
	[Export ("packageType")]
	RCPackageType PackageType { get; }

	// @property (readonly, nonatomic, strong) RCStoreProduct * _Nonnull storeProduct;
	[Export ("storeProduct", ArgumentSemantic.Strong)]
	RCStoreProduct StoreProduct { get; }

	// @property (readonly, nonatomic, strong) RCPresentedOfferingContext * _Nonnull presentedOfferingContext;
	[Export ("presentedOfferingContext", ArgumentSemantic.Strong)]
	RCPresentedOfferingContext PresentedOfferingContext { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull localizedPriceString;
	[Export ("localizedPriceString")]
	string LocalizedPriceString { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nullable localizedIntroductoryPriceString;
	[NullAllowed, Export ("localizedIntroductoryPriceString")]
	string LocalizedIntroductoryPriceString { get; }

	// @property (readonly, copy, nonatomic) NSURL * _Nullable webCheckoutUrl;
	[NullAllowed, Export ("webCheckoutUrl", ArgumentSemantic.Copy)]
	NSUrl WebCheckoutUrl { get; }

	// -(instancetype _Nonnull)initWithIdentifier:(NSString * _Nonnull)identifier packageType:(enum RCPackageType)packageType storeProduct:(RCStoreProduct * _Nonnull)storeProduct offeringIdentifier:(NSString * _Nonnull)offeringIdentifier webCheckoutUrl:(NSURL * _Nullable)webCheckoutUrl;
	[Export ("initWithIdentifier:packageType:storeProduct:offeringIdentifier:webCheckoutUrl:")]
	NativeHandle Constructor (string identifier, RCPackageType packageType, RCStoreProduct storeProduct, string offeringIdentifier, [NullAllowed] NSUrl webCheckoutUrl);

	// -(instancetype _Nonnull)initWithIdentifier:(NSString * _Nonnull)identifier packageType:(enum RCPackageType)packageType storeProduct:(RCStoreProduct * _Nonnull)storeProduct presentedOfferingContext:(RCPresentedOfferingContext * _Nonnull)presentedOfferingContext webCheckoutUrl:(NSURL * _Nullable)webCheckoutUrl __attribute__((objc_designated_initializer));
	[Export ("initWithIdentifier:packageType:storeProduct:presentedOfferingContext:webCheckoutUrl:")]
	[DesignatedInitializer]
	NativeHandle Constructor (string identifier, RCPackageType packageType, RCStoreProduct storeProduct, RCPresentedOfferingContext presentedOfferingContext, [NullAllowed] NSUrl webCheckoutUrl);


	// @property (readonly, nonatomic) NSUInteger hash;
	[Export ("hash")]
	nuint Hash { get; }

	// @property (readonly, nonatomic, strong) SWIFT_AVAILABILITY(maccatalyst,obsoleted=1,message="'product' has been renamed to 'storeProduct': Use StoreProduct instead") SKProduct * product __attribute__((availability(maccatalyst, obsoleted=1))) __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacCatalyst, 1, 0, message: "'product' has been renamed to 'storeProduct': Use StoreProduct instead")]
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'product' has been renamed to 'storeProduct': Use StoreProduct instead")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'product' has been renamed to 'storeProduct': Use StoreProduct instead")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'product' has been renamed to 'storeProduct': Use StoreProduct instead")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'product' has been renamed to 'storeProduct': Use StoreProduct instead")]
	[Export ("product", ArgumentSemantic.Strong)]
	SKProduct Product { get; }

	// +(NSString * _Nullable)stringFrom:(enum RCPackageType)packageType __attribute__((warn_unused_result("")));
	[Static]
	[Export ("stringFrom:")]
	[return: NullAllowed]
	string StringFrom (RCPackageType packageType);

	// +(enum RCPackageType)packageTypeFrom:(NSString * _Nonnull)string __attribute__((warn_unused_result("")));
	[Static]
	[Export ("packageTypeFrom:")]
	RCPackageType PackageTypeFrom (string @string);

	// @property (readonly, copy, nonatomic) NSString * _Nonnull offeringIdentifier;
	[Export ("offeringIdentifier")]
	string OfferingIdentifier { get; }
}

// @interface RCPresentedOfferingContext : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCPresentedOfferingContext
{
	// @property (readonly, copy, nonatomic) NSString * _Nonnull offeringIdentifier;
	[Export ("offeringIdentifier")]
	string OfferingIdentifier { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nullable placementIdentifier;
	[NullAllowed, Export ("placementIdentifier")]
	string PlacementIdentifier { get; }

	// @property (readonly, nonatomic, strong) RCTargetingContext * _Nullable targetingContext;
	[NullAllowed, Export ("targetingContext", ArgumentSemantic.Strong)]
	RCTargetingContext TargetingContext { get; }

	// -(instancetype _Nonnull)initWithOfferingIdentifier:(NSString * _Nonnull)offeringIdentifier placementIdentifier:(NSString * _Nullable)placementIdentifier targetingContext:(RCTargetingContext * _Nullable)targetingContext __attribute__((objc_designated_initializer));
	[Export ("initWithOfferingIdentifier:placementIdentifier:targetingContext:")]
	[DesignatedInitializer]
	NativeHandle Constructor (string offeringIdentifier, [NullAllowed] string placementIdentifier, [NullAllowed] RCTargetingContext targetingContext);

	// -(instancetype _Nonnull)initWithOfferingIdentifier:(NSString * _Nonnull)offeringIdentifier;
	[Export ("initWithOfferingIdentifier:")]
	NativeHandle Constructor (string offeringIdentifier);

	// @property (readonly, nonatomic) NSUInteger hash;
	[Export ("hash")]
	nuint Hash { get; }
}

// @interface RCTargetingContext : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCTargetingContext
{
	// @property (readonly, nonatomic) NSInteger revision;
	[Export ("revision")]
	nint Revision { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull ruleId;
	[Export ("ruleId")]
	string RuleId { get; }

	// -(instancetype _Nonnull)initWithRevision:(NSInteger)revision ruleId:(NSString * _Nonnull)ruleId __attribute__((objc_designated_initializer));
	[Export ("initWithRevision:ruleId:")]
	[DesignatedInitializer]
	NativeHandle Constructor (nint revision, string ruleId);
}

// @interface RCProductPaidPrice : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCProductPaidPrice
{
	// @property (readonly, copy, nonatomic) NSString * _Nonnull currency;
	[Export ("currency")]
	string Currency { get; }

	// @property (readonly, nonatomic) double amount;
	[Export ("amount")]
	double Amount { get; }
}

// @interface RCPromotionalOffer : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCPromotionalOffer : INativeObject
{
	// @property (readonly, nonatomic, strong) RCStoreProductDiscount * _Nonnull discount;
	[Export ("discount", ArgumentSemantic.Strong)]
	RCStoreProductDiscount Discount { get; }

	// @property (readonly, nonatomic, strong) RCPromotionalOfferSignedData * _Nonnull signedData;
	[Export ("signedData", ArgumentSemantic.Strong)]
	RCPromotionalOfferSignedData SignedData { get; }
}

// @interface RCPromotionalOfferSignedData : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCPromotionalOfferSignedData
{
	// @property (readonly, copy, nonatomic) NSString * _Nonnull identifier;
	[Export ("identifier")]
	string Identifier { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull keyIdentifier;
	[Export ("keyIdentifier")]
	string KeyIdentifier { get; }

	// @property (readonly, copy, nonatomic) NSUUID * _Nonnull nonce;
	[Export ("nonce", ArgumentSemantic.Copy)]
	NSUuid Nonce { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull signature;
	[Export ("signature")]
	string Signature { get; }

	// @property (readonly, nonatomic) NSInteger timestamp;
	[Export ("timestamp")]
	nint Timestamp { get; }
}

// @interface RCPurchaseParams : NSObject
[BaseType(typeof(NSObject))]
[DisableDefaultCtor]
interface RCPurchaseParams
{
}

// @interface RCPurchaseParamsBuilder : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCPurchaseParamsBuilder
{
	// -(instancetype _Nonnull)initWithPackage:(RCPackage * _Nonnull)package __attribute__((objc_designated_initializer));
	[Export ("initWithPackage:")]
	[DesignatedInitializer]
	NativeHandle Constructor (RCPackage package);

	// -(instancetype _Nonnull)initWithProduct:(RCStoreProduct * _Nonnull)product __attribute__((objc_designated_initializer));
	[Export ("initWithProduct:")]
	[DesignatedInitializer]
	NativeHandle Constructor (RCStoreProduct product);

	// -(instancetype _Nonnull)withPromotionalOffer:(RCPromotionalOffer * _Nonnull)promotionalOffer __attribute__((warn_unused_result("")));
	[Export ("withPromotionalOffer:")]
	RCPurchaseParamsBuilder WithPromotionalOffer (RCPromotionalOffer promotionalOffer);

	// -(instancetype _Nonnull)withWinBackOffer:(RCWinBackOffer * _Nonnull)winBackOffer __attribute__((warn_unused_result(""))) __attribute__((availability(visionos, introduced=2.0))) __attribute__((availability(watchos, introduced=11.0))) __attribute__((availability(tvos, introduced=18.0))) __attribute__((availability(macos, introduced=15.0))) __attribute__((availability(ios, introduced=18.0)));
	[Export ("withWinBackOffer:")]
	RCPurchaseParamsBuilder WithWinBackOffer (RCWinBackOffer winBackOffer);

	// -(RCPurchaseParams * _Nonnull)build __attribute__((warn_unused_result("")));
	[Export ("build")]
	RCPurchaseParams Build { get; }
}

// @interface RCPlatformInfo : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCPlatformInfo
{
	// -(instancetype _Nonnull)initWithFlavor:(NSString * _Nonnull)flavor version:(NSString * _Nonnull)version __attribute__((objc_designated_initializer));
	[Export ("initWithFlavor:version:")]
	[DesignatedInitializer]
	NativeHandle Constructor (string flavor, string version);
}

// @interface RCPurchases : NSObject <RCPurchasesType>
[BaseType(typeof(NSObject))]
[DisableDefaultCtor]
interface RCPurchases
{
    // @property (readonly, nonatomic, strong, class) RCPurchases * _Nonnull sharedPurchases;
    [Static]
    [Export("sharedPurchases", ArgumentSemantic.Strong)]
    RCPurchases SharedPurchases { get; }

    // @property (readonly, nonatomic, class) BOOL isConfigured;
    [Static]
    [Export("isConfigured")]
    bool IsConfigured { get; }

    [Wrap("WeakDelegate")]
    [NullAllowed]
    RCPurchasesDelegate Delegate { get; set; }

    // @property (nonatomic, strong) id<RCPurchasesDelegate> _Nullable delegate;
    [NullAllowed, Export("delegate", ArgumentSemantic.Strong)]
    NSObject WeakDelegate { get; set; }

    // @property (nonatomic, class) enum RCLogLevel logLevel;
    [Static]
    [Export("logLevel", ArgumentSemantic.Assign)]
    RCLogLevel LogLevel { get; set; }

    // @property (copy, nonatomic, class) NSURL * _Nullable proxyURL;
    [Static]
    [NullAllowed, Export("proxyURL", ArgumentSemantic.Copy)]
    NSUrl ProxyURL { get; set; }

    // @property (nonatomic, class) BOOL forceUniversalAppStore;
    [Static]
    [Export("forceUniversalAppStore")]
    bool ForceUniversalAppStore { get; set; }

    // @property (nonatomic, class) BOOL simulatesAskToBuyInSandbox __attribute__((availability(maccatalyst, introduced=13.0))) __attribute__((availability(watchos, introduced=6.2))) __attribute__((availability(macos, introduced=10.14))) __attribute__((availability(ios, introduced=8.0)));
    [Static]
    [Export("simulatesAskToBuyInSandbox")]
    bool SimulatesAskToBuyInSandbox { get; set; }

    // +(BOOL)canMakePayments __attribute__((warn_unused_result("")));
    [Static]
    [Export("canMakePayments")]
    bool CanMakePayments { get; }

    // @property (copy, nonatomic, class) void (^ _Nonnull)(enum RCLogLevel, NSString * _Nonnull) logHandler;
    [Static]
    [Export("logHandler", ArgumentSemantic.Copy)]
    Action<RCLogLevel, NSString> LogHandler { get; set; }

    // @property (copy, nonatomic, class) void (^ _Nonnull)(enum RCLogLevel, NSString * _Nonnull, NSString * _Nullable, NSString * _Nullable, NSUInteger) verboseLogHandler;
    [Static]
    [Export("verboseLogHandler", ArgumentSemantic.Copy)]
    Action<RCLogLevel, NSString, NSString, NSString, nuint> VerboseLogHandler { get; set; }

    // @property (nonatomic, class) BOOL verboseLogs;
    [Static]
    [Export("verboseLogs")]
    bool VerboseLogs { get; set; }

    // @property (readonly, copy, nonatomic, class) NSString * _Nonnull frameworkVersion;
    [Static]
    [Export("frameworkVersion")]
    string FrameworkVersion { get; }

    // @property (readonly, nonatomic, strong) RCAttribution * _Nonnull attribution;
    [Export("attribution", ArgumentSemantic.Strong)]
    RCAttribution Attribution { get; }

    // @property (nonatomic) enum RCPurchasesAreCompletedBy purchasesAreCompletedBy;
    [Export("purchasesAreCompletedBy", ArgumentSemantic.Assign)]
    RCPurchasesAreCompletedBy PurchasesAreCompletedBy { get; set; }

    // @property (readonly, copy, nonatomic) NSString * _Nullable storeFrontCountryCode;
    [NullAllowed, Export("storeFrontCountryCode")]
    string StoreFrontCountryCode { get; }

    // @property (nonatomic, strong, class) RCPlatformInfo * _Nullable platformInfo;
    [Static]
    [NullAllowed, Export("platformInfo", ArgumentSemantic.Strong)]
    RCPlatformInfo PlatformInfo { get; set; }

    // -(void)eligibleWinBackOffersForProduct:(RCStoreProduct * _Nonnull)product completion:(void (^ _Nonnull)(NSArray<RCWinBackOffer *> * _Nullable, NSError * _Nullable))completion;
    [Export("eligibleWinBackOffersForProduct:completion:")]
    void EligibleWinBackOffersForProduct(RCStoreProduct product, Action<NSArray<RCWinBackOffer>, NSError> completion);

    // -(void)eligibleWinBackOffersForPackage:(RCPackage * _Nonnull)package completion:(void (^ _Nonnull)(NSArray<RCWinBackOffer *> * _Nullable, NSError * _Nullable))completion __attribute__((availability(visionos, introduced=2.0))) __attribute__((availability(watchos, introduced=11.0))) __attribute__((availability(tvos, introduced=18.0))) __attribute__((availability(macos, introduced=15.0))) __attribute__((availability(ios, introduced=18.0)));
    [Export("eligibleWinBackOffersForPackage:completion:")]
    void EligibleWinBackOffersForPackage(RCPackage package, Action<NSArray<RCWinBackOffer>, NSError> completion);
    // -(void)readyForPromotedProduct:(RCStoreProduct * _Nonnull)product purchase:(void (^ _Nonnull)(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, NSError * _Nullable, BOOL)))startPurchase;
    [Export ("readyForPromotedProduct:purchase:")]
	void ReadyForPromotedProduct (RCStoreProduct product, StartPurchaseHandler startPurchase);

	// @property (readonly, nonatomic) BOOL shouldShowPriceConsent __attribute__((availability(maccatalyst, introduced=13.4))) __attribute__((availability(ios, introduced=13.4)));
	[Export ("shouldShowPriceConsent")]
	bool ShouldShowPriceConsent { get; }

	// -(void)getVirtualCurrenciesWithCompletion:(void (^ _Nonnull)(RCVirtualCurrencies * _Nullable, NSError * _Nullable))completion;
	[Export ("getVirtualCurrenciesWithCompletion:")]
	void GetVirtualCurrenciesWithCompletion (Action<RCVirtualCurrencies, NSError> completion);

	// @property (readonly, nonatomic, strong) RCVirtualCurrencies * _Nullable cachedVirtualCurrencies;
	[NullAllowed, Export ("cachedVirtualCurrencies", ArgumentSemantic.Strong)]
	RCVirtualCurrencies CachedVirtualCurrencies { get; }

	// -(void)invalidateVirtualCurrenciesCache;
	[Export ("invalidateVirtualCurrenciesCache")]
	void InvalidateVirtualCurrenciesCache ();

	// @property (nonatomic, class) BOOL debugLogsEnabled __attribute__((deprecated("use Purchases.logLevel instead")));
	[Static]
	[Export ("debugLogsEnabled")]
	bool DebugLogsEnabled { get; set; }

	// @property (nonatomic) BOOL allowSharingAppStoreAccount __attribute__((deprecated("
    //Configure behavior through the RevenueCat dashboard instead. If you have configured the "Legacy" restore
    //behavior in the [RevenueCat Dashboard](app.revenuecat.com) and are currently setting this to `true`, keep
    //this setting active.
    //")));
	[Export ("allowSharingAppStoreAccount")]
	bool AllowSharingAppStoreAccount { get; set; }

	// @property (nonatomic) BOOL finishTransactions __attribute__((deprecated("Use ``purchasesAreCompletedBy`` instead.")));
	[Export ("finishTransactions")]
	bool FinishTransactions { get; set; }

	// +(void)addAttributionData:(NSDictionary<NSString *,id> * _Nonnull)data fromNetwork:(enum RCAttributionNetwork)network __attribute__((deprecated("Use the set<NetworkId> functions instead")));
	[Static]
	[Export ("addAttributionData:fromNetwork:")]
	void AddAttributionData (NSDictionary<NSString, NSObject> data, RCAttributionNetwork network);

	// +(void)addAttributionData:(NSDictionary<NSString *,id> * _Nonnull)data fromNetwork:(enum RCAttributionNetwork)network forNetworkUserId:(NSString * _Nullable)networkUserId __attribute__((deprecated("Use the set<NetworkId> functions instead")));
	[Static]
	[Export ("addAttributionData:fromNetwork:forNetworkUserId:")]
	void AddAttributionData (NSDictionary<NSString, NSObject> data, RCAttributionNetwork network, [NullAllowed] string networkUserId);

	// +(RCPurchases * _Nonnull)configureWithConfiguration:(RCConfiguration * _Nonnull)configuration;
	[Static]
	[Export ("configureWithConfiguration:")]
	RCPurchases ConfigureWithConfiguration (RCConfiguration configuration);

	// +(RCPurchases * _Nonnull)configureWithConfigurationBuilder:(RCConfigurationBuilder * _Nonnull)builder;
	[Static]
	[Export ("configureWithConfigurationBuilder:")]
	RCPurchases ConfigureWithConfigurationBuilder (RCConfigurationBuilder builder);

	// +(RCPurchases * _Nonnull)configureWithAPIKey:(NSString * _Nonnull)apiKey;
	[Static]
	[Export ("configureWithAPIKey:")]
	RCPurchases ConfigureWithAPIKey (string apiKey);

	// +(RCPurchases * _Nonnull)configureWithAPIKey:(NSString * _Nonnull)apiKey appUserID:(NSString * _Nullable)appUserID;
	[Static]
	[Export ("configureWithAPIKey:appUserID:")]
	RCPurchases ConfigureWithAPIKey (string apiKey, [NullAllowed] string appUserID);

	// +(RCPurchases * _Nonnull)configureWithAPIKey:(NSString * _Nonnull)apiKey appUserID:(NSString * _Nullable)appUserID purchasesAreCompletedBy:(enum RCPurchasesAreCompletedBy)purchasesAreCompletedBy storeKitVersion:(enum RCStoreKitVersion)storeKitVersion;
	[Static]
	[Export ("configureWithAPIKey:appUserID:purchasesAreCompletedBy:storeKitVersion:")]
	RCPurchases ConfigureWithAPIKey (string apiKey, [NullAllowed] string appUserID, RCPurchasesAreCompletedBy purchasesAreCompletedBy, RCStoreKitVersion storeKitVersion);

	// -(void)logIn:(NSString * _Nonnull)appUserID completion:(void (^ _Nonnull)(RCCustomerInfo * _Nullable, BOOL, NSError * _Nullable))completion;
	[Export ("logIn:completion:")]
	void LogIn (string appUserID, Action<RCCustomerInfo, bool, NSError> completion);

	// -(void)logIn:(NSString * _Nonnull)appUserID completionHandler:(void (^ _Nonnull)(RCCustomerInfo * _Nullable, BOOL, NSError * _Nullable))completionHandler;
	[Export ("logIn:completionHandler:")]
	void LogInCompletionHandler(string appUserID, Action<RCCustomerInfo, bool, NSError> completionHandler);

	// -(void)logOutWithCompletion:(void (^ _Nullable)(RCCustomerInfo * _Nullable, NSError * _Nullable))completion;
	[Export ("logOutWithCompletion:")]
	void LogOutWithCompletion ([NullAllowed] Action<RCCustomerInfo, NSError> completion);

	// -(void)logOutWithCompletionHandler:(void (^ _Nonnull)(RCCustomerInfo * _Nullable, NSError * _Nullable))completionHandler;
	[Export ("logOutWithCompletionHandler:")]
	void LogOutWithCompletionHandler (Action<RCCustomerInfo, NSError> completionHandler);

	// -(void)syncAttributesAndOfferingsIfNeededWithCompletion:(void (^ _Nonnull)(RCOfferings * _Nullable, NSError * _Nullable))completion;
	[Export ("syncAttributesAndOfferingsIfNeededWithCompletion:")]
	void SyncAttributesAndOfferingsIfNeededWithCompletion (Action<RCOfferings, NSError> completion);

	// -(void)syncAttributesAndOfferingsIfNeededWithCompletionHandler:(void (^ _Nonnull)(RCOfferings * _Nullable, NSError * _Nullable))completionHandler __attribute__((availability(watchos, introduced=6.2))) __attribute__((availability(tvos, introduced=13.0))) __attribute__((availability(macos, introduced=10.15))) __attribute__((availability(ios, introduced=13.0)));
	[Export ("syncAttributesAndOfferingsIfNeededWithCompletionHandler:")]
	void SyncAttributesAndOfferingsIfNeededWithCompletionHandler (Action<RCOfferings, NSError> completionHandler);

	// -(void)getStorefrontWithCompletion:(void (^ _Nonnull)(RCStorefront * _Nullable))completion;
	[Export ("getStorefrontWithCompletion:")]
	void GetStorefrontWithCompletion (Action<RCStorefront> completion);

	// -(void)getStorefrontWithCompletionHandler:(void (^ _Nonnull)(RCStorefront * _Nullable))completionHandler;
	[Export ("getStorefrontWithCompletionHandler:")]
	void GetStorefrontWithCompletionHandler (Action<RCStorefront> completionHandler);

    // +(RCWebPurchaseRedemption * _Nullable)parseAsWebPurchaseRedemption:(NSURL * _Nonnull)url __attribute__((warn_unused_result("")));
    [Static]
	[Export ("parseAsWebPurchaseRedemption:")]
	[return: NullAllowed]
	RCWebPurchaseRedemption ParseAsWebPurchaseRedemption (NSUrl url);

	// @property (readonly, copy, nonatomic) NSString * _Nonnull appUserID;
	[Export ("appUserID")]
	string AppUserID { get; }

	// @property (readonly, nonatomic) BOOL isAnonymous;
	[Export ("isAnonymous")]
	bool IsAnonymous { get; }

	// @property (readonly, nonatomic) BOOL isSandbox;
	[Export ("isSandbox")]
	bool IsSandbox { get; }

	// -(void)getOfferingsWithCompletion:(void (^ _Nonnull)(RCOfferings * _Nullable, NSError * _Nullable))completion;
	[Export ("getOfferingsWithCompletion:")]
	void GetOfferingsWithCompletion (Action<RCOfferings, NSError> completion);

	// -(void)offeringsWithCompletionHandler:(void (^ _Nonnull)(RCOfferings * _Nullable, NSError * _Nullable))completionHandler;
	[Export ("offeringsWithCompletionHandler:")]
	void OfferingsWithCompletionHandler (Action<RCOfferings, NSError> completionHandler);

	// @property (readonly, nonatomic, strong) RCOfferings * _Nullable cachedOfferings;
	[NullAllowed, Export ("cachedOfferings", ArgumentSemantic.Strong)]
	RCOfferings CachedOfferings { get; }

	// -(void)collectDeviceIdentifiers __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'collectDeviceIdentifiers' has been renamed to 'attribution.collectDeviceIdentifiers()'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'collectDeviceIdentifiers' has been renamed to 'attribution.collectDeviceIdentifiers()'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'collectDeviceIdentifiers' has been renamed to 'attribution.collectDeviceIdentifiers()'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'collectDeviceIdentifiers' has been renamed to 'attribution.collectDeviceIdentifiers()'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'collectDeviceIdentifiers' has been renamed to 'attribution.collectDeviceIdentifiers()'")]
	[Export ("collectDeviceIdentifiers")]
	void CollectDeviceIdentifiers ();

	// -(void)setAttributes:(NSDictionary<NSString *,NSString *> * _Nonnull)attributes __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setAttributes' has been renamed to 'attribution.setAttributes(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setAttributes' has been renamed to 'attribution.setAttributes(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setAttributes' has been renamed to 'attribution.setAttributes(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setAttributes' has been renamed to 'attribution.setAttributes(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setAttributes' has been renamed to 'attribution.setAttributes(_:)'")]
	[Export ("setAttributes:")]
	void SetAttributes (NSDictionary<NSString, NSString> attributes);

	// -(void)setEmail:(NSString * _Nullable)email __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setEmail' has been renamed to 'attribution.setEmail(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setEmail' has been renamed to 'attribution.setEmail(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setEmail' has been renamed to 'attribution.setEmail(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setEmail' has been renamed to 'attribution.setEmail(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setEmail' has been renamed to 'attribution.setEmail(_:)'")]
	[Export ("setEmail:")]
	void SetEmail ([NullAllowed] string email);

	// -(void)setPhoneNumber:(NSString * _Nullable)phoneNumber __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setPhoneNumber' has been renamed to 'attribution.setPhoneNumber(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setPhoneNumber' has been renamed to 'attribution.setPhoneNumber(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setPhoneNumber' has been renamed to 'attribution.setPhoneNumber(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setPhoneNumber' has been renamed to 'attribution.setPhoneNumber(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setPhoneNumber' has been renamed to 'attribution.setPhoneNumber(_:)'")]
	[Export ("setPhoneNumber:")]
	void SetPhoneNumber ([NullAllowed] string phoneNumber);

	// -(void)setDisplayName:(NSString * _Nullable)displayName __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setDisplayName' has been renamed to 'attribution.setDisplayName(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setDisplayName' has been renamed to 'attribution.setDisplayName(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setDisplayName' has been renamed to 'attribution.setDisplayName(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setDisplayName' has been renamed to 'attribution.setDisplayName(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setDisplayName' has been renamed to 'attribution.setDisplayName(_:)'")]
	[Export ("setDisplayName:")]
	void SetDisplayName ([NullAllowed] string displayName);

	// -(void)setPushToken:(NSData * _Nullable)pushToken __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setPushToken' has been renamed to 'attribution.setPushToken(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setPushToken' has been renamed to 'attribution.setPushToken(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setPushToken' has been renamed to 'attribution.setPushToken(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setPushToken' has been renamed to 'attribution.setPushToken(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setPushToken' has been renamed to 'attribution.setPushToken(_:)'")]
	[Export ("setPushToken:")]
	void SetPushToken ([NullAllowed] NSData pushToken);

	// -(void)setPushTokenString:(NSString * _Nullable)pushToken __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setPushTokenString' has been renamed to 'attribution.setPushTokenString(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setPushTokenString' has been renamed to 'attribution.setPushTokenString(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setPushTokenString' has been renamed to 'attribution.setPushTokenString(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setPushTokenString' has been renamed to 'attribution.setPushTokenString(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setPushTokenString' has been renamed to 'attribution.setPushTokenString(_:)'")]
	[Export ("setPushTokenString:")]
	void SetPushTokenString ([NullAllowed] string pushToken);

	// -(void)setAdjustID:(NSString * _Nullable)adjustID __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setAdjustID' has been renamed to 'attribution.setAdjustID(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setAdjustID' has been renamed to 'attribution.setAdjustID(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setAdjustID' has been renamed to 'attribution.setAdjustID(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setAdjustID' has been renamed to 'attribution.setAdjustID(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setAdjustID' has been renamed to 'attribution.setAdjustID(_:)'")]
	[Export ("setAdjustID:")]
	void SetAdjustID ([NullAllowed] string adjustID);

	// -(void)setAppsflyerID:(NSString * _Nullable)appsflyerID __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setAppsflyerID' has been renamed to 'attribution.setAppsflyerID(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setAppsflyerID' has been renamed to 'attribution.setAppsflyerID(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setAppsflyerID' has been renamed to 'attribution.setAppsflyerID(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setAppsflyerID' has been renamed to 'attribution.setAppsflyerID(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setAppsflyerID' has been renamed to 'attribution.setAppsflyerID(_:)'")]
	[Export ("setAppsflyerID:")]
	void SetAppsflyerID ([NullAllowed] string appsflyerID);

	// -(void)setFBAnonymousID:(NSString * _Nullable)fbAnonymousID __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setFBAnonymousID' has been renamed to 'attribution.setFBAnonymousID(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setFBAnonymousID' has been renamed to 'attribution.setFBAnonymousID(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setFBAnonymousID' has been renamed to 'attribution.setFBAnonymousID(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setFBAnonymousID' has been renamed to 'attribution.setFBAnonymousID(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setFBAnonymousID' has been renamed to 'attribution.setFBAnonymousID(_:)'")]
	[Export ("setFBAnonymousID:")]
	void SetFBAnonymousID ([NullAllowed] string fbAnonymousID);

	// -(void)setMparticleID:(NSString * _Nullable)mparticleID __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setMparticleID' has been renamed to 'attribution.setMparticleID(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setMparticleID' has been renamed to 'attribution.setMparticleID(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setMparticleID' has been renamed to 'attribution.setMparticleID(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setMparticleID' has been renamed to 'attribution.setMparticleID(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setMparticleID' has been renamed to 'attribution.setMparticleID(_:)'")]
	[Export ("setMparticleID:")]
	void SetMparticleID ([NullAllowed] string mparticleID);

	// -(void)setOnesignalID:(NSString * _Nullable)onesignalID __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setOnesignalID' has been renamed to 'attribution.setOnesignalID(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setOnesignalID' has been renamed to 'attribution.setOnesignalID(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setOnesignalID' has been renamed to 'attribution.setOnesignalID(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setOnesignalID' has been renamed to 'attribution.setOnesignalID(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setOnesignalID' has been renamed to 'attribution.setOnesignalID(_:)'")]
	[Export ("setOnesignalID:")]
	void SetOnesignalID ([NullAllowed] string onesignalID);

	// -(void)setAirshipChannelID:(NSString * _Nullable)airshipChannelID __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setAirshipChannelID' has been renamed to 'attribution.setAirshipChannelID(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setAirshipChannelID' has been renamed to 'attribution.setAirshipChannelID(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setAirshipChannelID' has been renamed to 'attribution.setAirshipChannelID(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setAirshipChannelID' has been renamed to 'attribution.setAirshipChannelID(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setAirshipChannelID' has been renamed to 'attribution.setAirshipChannelID(_:)'")]
	[Export ("setAirshipChannelID:")]
	void SetAirshipChannelID ([NullAllowed] string airshipChannelID);

	// -(void)setCleverTapID:(NSString * _Nullable)cleverTapID __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setCleverTapID' has been renamed to 'attribution.setCleverTapID(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setCleverTapID' has been renamed to 'attribution.setCleverTapID(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setCleverTapID' has been renamed to 'attribution.setCleverTapID(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setCleverTapID' has been renamed to 'attribution.setCleverTapID(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setCleverTapID' has been renamed to 'attribution.setCleverTapID(_:)'")]
	[Export ("setCleverTapID:")]
	void SetCleverTapID ([NullAllowed] string cleverTapID);

	// -(void)setMixpanelDistinctID:(NSString * _Nullable)mixpanelDistinctID __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setMixpanelDistinctID' has been renamed to 'attribution.setMixpanelDistinctID(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setMixpanelDistinctID' has been renamed to 'attribution.setMixpanelDistinctID(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setMixpanelDistinctID' has been renamed to 'attribution.setMixpanelDistinctID(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setMixpanelDistinctID' has been renamed to 'attribution.setMixpanelDistinctID(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setMixpanelDistinctID' has been renamed to 'attribution.setMixpanelDistinctID(_:)'")]
	[Export ("setMixpanelDistinctID:")]
	void SetMixpanelDistinctID ([NullAllowed] string mixpanelDistinctID);

	// -(void)setFirebaseAppInstanceID:(NSString * _Nullable)firebaseAppInstanceID __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setFirebaseAppInstanceID' has been renamed to 'attribution.setFirebaseAppInstanceID(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setFirebaseAppInstanceID' has been renamed to 'attribution.setFirebaseAppInstanceID(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setFirebaseAppInstanceID' has been renamed to 'attribution.setFirebaseAppInstanceID(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setFirebaseAppInstanceID' has been renamed to 'attribution.setFirebaseAppInstanceID(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setFirebaseAppInstanceID' has been renamed to 'attribution.setFirebaseAppInstanceID(_:)'")]
	[Export ("setFirebaseAppInstanceID:")]
	void SetFirebaseAppInstanceID ([NullAllowed] string firebaseAppInstanceID);

	// -(void)setMediaSource:(NSString * _Nullable)mediaSource __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setMediaSource' has been renamed to 'attribution.setMediaSource(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setMediaSource' has been renamed to 'attribution.setMediaSource(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setMediaSource' has been renamed to 'attribution.setMediaSource(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setMediaSource' has been renamed to 'attribution.setMediaSource(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setMediaSource' has been renamed to 'attribution.setMediaSource(_:)'")]
	[Export ("setMediaSource:")]
	void SetMediaSource ([NullAllowed] string mediaSource);

	// -(void)setCampaign:(NSString * _Nullable)campaign __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setCampaign' has been renamed to 'attribution.setCampaign(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setCampaign' has been renamed to 'attribution.setCampaign(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setCampaign' has been renamed to 'attribution.setCampaign(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setCampaign' has been renamed to 'attribution.setCampaign(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setCampaign' has been renamed to 'attribution.setCampaign(_:)'")]
	[Export ("setCampaign:")]
	void SetCampaign ([NullAllowed] string campaign);

	// -(void)setAdGroup:(NSString * _Nullable)adGroup __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setAdGroup' has been renamed to 'attribution.setAdGroup(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setAdGroup' has been renamed to 'attribution.setAdGroup(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setAdGroup' has been renamed to 'attribution.setAdGroup(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setAdGroup' has been renamed to 'attribution.setAdGroup(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setAdGroup' has been renamed to 'attribution.setAdGroup(_:)'")]
	[Export ("setAdGroup:")]
	void SetAdGroup ([NullAllowed] string adGroup);

	// -(void)setAd:(NSString * _Nullable)installAd __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setAd' has been renamed to 'attribution.setAd(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setAd' has been renamed to 'attribution.setAd(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setAd' has been renamed to 'attribution.setAd(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setAd' has been renamed to 'attribution.setAd(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setAd' has been renamed to 'attribution.setAd(_:)'")]
	[Export ("setAd:")]
	void SetAd ([NullAllowed] string installAd);

	// -(void)setKeyword:(NSString * _Nullable)keyword __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setKeyword' has been renamed to 'attribution.setKeyword(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setKeyword' has been renamed to 'attribution.setKeyword(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setKeyword' has been renamed to 'attribution.setKeyword(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setKeyword' has been renamed to 'attribution.setKeyword(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setKeyword' has been renamed to 'attribution.setKeyword(_:)'")]
	[Export ("setKeyword:")]
	void SetKeyword ([NullAllowed] string keyword);

	// -(void)setCreative:(NSString * _Nullable)creative __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'setCreative' has been renamed to 'attribution.setCreative(_:)'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'setCreative' has been renamed to 'attribution.setCreative(_:)'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'setCreative' has been renamed to 'attribution.setCreative(_:)'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'setCreative' has been renamed to 'attribution.setCreative(_:)'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'setCreative' has been renamed to 'attribution.setCreative(_:)'")]
	[Export ("setCreative:")]
	void SetCreative ([NullAllowed] string creative);

	// -(void)params:(RCPurchaseParams * _Nonnull)params withCompletion:(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, NSError * _Nullable, BOOL))completion __attribute__((availability(maccatalyst, deprecated=0.0.1))) __attribute__((availability(macos, deprecated=0.0.1))) __attribute__((availability(watchos, deprecated=0.0.1))) __attribute__((availability(tvos, deprecated=0.0.1))) __attribute__((availability(ios, deprecated=0.0.1)));
	[Deprecated (PlatformName.MacCatalyst, 0, 0, 1, message: "'purchaseWithParams' has been renamed to 'purchaseWithParams:completion:'")]
	[Deprecated (PlatformName.MacOSX, 0, 0, 1, message: "'purchaseWithParams' has been renamed to 'purchaseWithParams:completion:'")]
	[Deprecated (PlatformName.WatchOS, 0, 0, 1, message: "'purchaseWithParams' has been renamed to 'purchaseWithParams:completion:'")]
	[Deprecated (PlatformName.TvOS, 0, 0, 1, message: "'purchaseWithParams' has been renamed to 'purchaseWithParams:completion:'")]
	[Deprecated (PlatformName.iOS, 0, 0, 1, message: "'purchaseWithParams' has been renamed to 'purchaseWithParams:completion:'")]
	[Export ("params:withCompletion:")]
	void Params (RCPurchaseParams @params, Action<RCStoreTransaction, RCCustomerInfo, NSError, bool> completion);

	// -(void)restoreTransactionsWithCompletionBlock:(void (^ _Nullable)(RCCustomerInfo * _Nullable, NSError * _Nullable))completion __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'restoreTransactions' has been renamed to 'restorePurchasesWithCompletion:'")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'restoreTransactions' has been renamed to 'restorePurchasesWithCompletion:'")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'restoreTransactions' has been renamed to 'restorePurchasesWithCompletion:'")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'restoreTransactions' has been renamed to 'restorePurchasesWithCompletion:'")]
	[Export ("restoreTransactionsWithCompletionBlock:")]
	void RestoreTransactionsWithCompletionBlock ([NullAllowed] Action<RCCustomerInfo, NSError> completion);

	// -(void)customerInfoWithCompletion:(void (^ _Nonnull)(RCCustomerInfo * _Nullable, NSError * _Nullable))completion __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'customerInfo' has been renamed to 'getCustomerInfoWithCompletion:'")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'customerInfo' has been renamed to 'getCustomerInfoWithCompletion:'")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'customerInfo' has been renamed to 'getCustomerInfoWithCompletion:'")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'customerInfo' has been renamed to 'getCustomerInfoWithCompletion:'")]
	[Export ("customerInfoWithCompletion:")]
	void CustomerInfoWithCompletion (Action<RCCustomerInfo, NSError> completion);

	// -(void)purchaserInfoWithCompletionBlock:(void (^ _Nonnull)(RCCustomerInfo * _Nullable, NSError * _Nullable))completion __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'purchaserInfo' has been renamed to 'getCustomerInfoWithCompletion:'")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'purchaserInfo' has been renamed to 'getCustomerInfoWithCompletion:'")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'purchaserInfo' has been renamed to 'getCustomerInfoWithCompletion:'")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'purchaserInfo' has been renamed to 'getCustomerInfoWithCompletion:'")]
	[Export ("purchaserInfoWithCompletionBlock:")]
	void PurchaserInfoWithCompletionBlock (Action<RCCustomerInfo, NSError> completion);

	// -(void)productsWithIdentifiers:(NSArray<NSString *> * _Nonnull)productIdentifiers completionBlock:(void (^ _Nonnull)(NSArray<SKProduct *> * _Nonnull))completion __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'products' has been renamed to 'getProductsWithIdentifiers:completion:'")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'products' has been renamed to 'getProductsWithIdentifiers:completion:'")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'products' has been renamed to 'getProductsWithIdentifiers:completion:'")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'products' has been renamed to 'getProductsWithIdentifiers:completion:'")]
	[Export ("productsWithIdentifiers:completionBlock:")]
	void ProductsWithIdentifiers (string[] productIdentifiers, Action<NSArray<SKProduct>> completion);

	// -(void)offeringsWithCompletionBlock:(void (^ _Nonnull)(RCOfferings * _Nullable, NSError * _Nullable))completion __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'offerings' has been renamed to 'getOfferingsWithCompletion:'")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'offerings' has been renamed to 'getOfferingsWithCompletion:'")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'offerings' has been renamed to 'getOfferingsWithCompletion:'")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'offerings' has been renamed to 'getOfferingsWithCompletion:'")]
	[Export ("offeringsWithCompletionBlock:")]
	void OfferingsWithCompletionBlock (Action<RCOfferings, NSError> completion);

	// -(void)purchaseProduct:(SKProduct * _Nonnull)product withCompletionBlock:(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, NSError * _Nullable, BOOL))completion __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'purchaseProduct' has been renamed to 'purchase(product:_:)'")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'purchaseProduct' has been renamed to 'purchase(product:_:)'")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'purchaseProduct' has been renamed to 'purchase(product:_:)'")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'purchaseProduct' has been renamed to 'purchase(product:_:)'")]
	[Export ("purchaseProduct:withCompletionBlock:")]
	void PurchaseProduct (SKProduct product, Action<RCStoreTransaction, RCCustomerInfo, NSError, bool> completion);

	// -(void)purchaseProduct:(SKProduct * _Nonnull)product withDiscount:(SKPaymentDiscount * _Nonnull)discount completionBlock:(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, NSError * _Nullable, BOOL))completion __attribute__((availability(maccatalyst, unavailable))) __attribute__((availability(macos, unavailable))) __attribute__((availability(watchos, unavailable))) __attribute__((availability(tvos, unavailable))) __attribute__((availability(ios, unavailable)));
	[Unavailable (PlatformName.MacCatalyst)]
	[Unavailable (PlatformName.MacOSX)]
	[Unavailable (PlatformName.WatchOS)]
	[Unavailable (PlatformName.TvOS)]
	[Unavailable (PlatformName.iOS)]
	[Export ("purchaseProduct:withDiscount:completionBlock:")]
	void PurchaseProduct (SKProduct product, SKPaymentDiscount discount, Action<RCStoreTransaction, RCCustomerInfo, NSError, bool> completion);

	// -(void)invalidatePurchaserInfoCache __attribute__((availability(maccatalyst, obsoleted=1))) __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacCatalyst, 1, 0, message: "'invalidatePurchaserInfoCache' has been renamed to 'invalidateCustomerInfoCache'")]
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'invalidatePurchaserInfoCache' has been renamed to 'invalidateCustomerInfoCache'")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'invalidatePurchaserInfoCache' has been renamed to 'invalidateCustomerInfoCache'")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'invalidatePurchaserInfoCache' has been renamed to 'invalidateCustomerInfoCache'")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'invalidatePurchaserInfoCache' has been renamed to 'invalidateCustomerInfoCache'")]
	[Export ("invalidatePurchaserInfoCache")]
	void InvalidatePurchaserInfoCache ();

	// -(void)checkTrialOrIntroductoryPriceEligibility:(NSArray<NSString *> * _Nonnull)productIdentifiers completion:(void (^ _Nonnull)(NSDictionary<NSString *,RCIntroEligibility *> * _Nonnull))completion __attribute__((availability(maccatalyst, obsoleted=1))) __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacCatalyst, 1, 0, message: "'checkTrialOrIntroductoryPriceEligibility' has been renamed to 'checkTrialOrIntroDiscountEligibility(_:completion:)'")]
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'checkTrialOrIntroductoryPriceEligibility' has been renamed to 'checkTrialOrIntroDiscountEligibility(_:completion:)'")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'checkTrialOrIntroductoryPriceEligibility' has been renamed to 'checkTrialOrIntroDiscountEligibility(_:completion:)'")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'checkTrialOrIntroductoryPriceEligibility' has been renamed to 'checkTrialOrIntroDiscountEligibility(_:completion:)'")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'checkTrialOrIntroductoryPriceEligibility' has been renamed to 'checkTrialOrIntroDiscountEligibility(_:completion:)'")]
	[Export ("checkTrialOrIntroductoryPriceEligibility:completion:")]
	void CheckTrialOrIntroductoryPriceEligibility (string[] productIdentifiers, Action<NSDictionary<NSString, RCIntroEligibility>> completion);

	// -(void)paymentDiscountForProductDiscount:(SKProductDiscount * _Nonnull)discount product:(SKProduct * _Nonnull)product completion:(void (^ _Nonnull)(SKPaymentDiscount * _Nullable, NSError * _Nullable))completion __attribute__((availability(maccatalyst, unavailable))) __attribute__((availability(macos, unavailable))) __attribute__((availability(watchos, unavailable))) __attribute__((availability(tvos, unavailable))) __attribute__((availability(ios, unavailable)));
	[Unavailable (PlatformName.MacCatalyst)]
	[Unavailable (PlatformName.MacOSX)]
	[Unavailable (PlatformName.WatchOS)]
	[Unavailable (PlatformName.TvOS)]
	[Unavailable (PlatformName.iOS)]
	[Export ("paymentDiscountForProductDiscount:product:completion:")]
	void PaymentDiscountForProductDiscount (SKProductDiscount discount, SKProduct product, Action<SKPaymentDiscount, NSError> completion);

	// -(void)shouldPurchasePromoProduct:(RCStoreProduct * _Nonnull)product defermentBlock:(void (^ _Nonnull)(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, NSError * _Nullable, BOOL)))defermentBlock __attribute__((availability(maccatalyst, obsoleted=1))) __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacCatalyst, 1, 0, message: "This was never meant to be public. Use `PurchasesDelegate.purchases(_:readyForPromotedProduct:purchase:)`")]
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "This was never meant to be public. Use `PurchasesDelegate.purchases(_:readyForPromotedProduct:purchase:)`")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "This was never meant to be public. Use `PurchasesDelegate.purchases(_:readyForPromotedProduct:purchase:)`")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "This was never meant to be public. Use `PurchasesDelegate.purchases(_:readyForPromotedProduct:purchase:)`")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "This was never meant to be public. Use `PurchasesDelegate.purchases(_:readyForPromotedProduct:purchase:)`")]
	[Export ("shouldPurchasePromoProduct:defermentBlock:")]
	void ShouldPurchasePromoProduct (RCStoreProduct product, StartPurchaseHandler defermentBlock);

	// -(void)createAlias:(NSString * _Nonnull)alias completionBlock:(void (^ _Nullable)(RCCustomerInfo * _Nullable, NSError * _Nullable))completion __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'createAlias' has been renamed to 'logIn:completionHandler:'")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'createAlias' has been renamed to 'logIn:completionHandler:'")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'createAlias' has been renamed to 'logIn:completionHandler:'")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'createAlias' has been renamed to 'logIn:completionHandler:'")]
	[Export ("createAlias:completionBlock:")]
	void CreateAlias (string alias, [NullAllowed] Action<RCCustomerInfo, NSError> completion);

	// -(void)identify:(NSString * _Nonnull)appUserID completionBlock:(void (^ _Nullable)(RCCustomerInfo * _Nullable, NSError * _Nullable))completion __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'identify' has been renamed to 'logIn:completionHandler:'")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'identify' has been renamed to 'logIn:completionHandler:'")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'identify' has been renamed to 'logIn:completionHandler:'")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'identify' has been renamed to 'logIn:completionHandler:'")]
	[Export ("identify:completionBlock:")]
	void Identify (string appUserID, [NullAllowed] Action<RCCustomerInfo, NSError> completion);

	// -(void)resetWithCompletionBlock:(void (^ _Nullable)(RCCustomerInfo * _Nullable, NSError * _Nullable))completion __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'reset' has been renamed to 'logOutWithCompletionHandler:'")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'reset' has been renamed to 'logOutWithCompletionHandler:'")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'reset' has been renamed to 'logOutWithCompletionHandler:'")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'reset' has been renamed to 'logOutWithCompletionHandler:'")]
	[Export ("resetWithCompletionBlock:")]
	void ResetWithCompletionBlock ([NullAllowed] Action<RCCustomerInfo, NSError> completion);

	// +(RCPurchases * _Nonnull)configureWithAPIKey:(NSString * _Nonnull)apiKey appUserID:(NSString * _Nullable)appUserID observerMode:(BOOL)observerMode __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'configure' has been renamed to 'configure(withAPIKey:appUserID:purchasesAreCompletedBy:storeKitVersion:)':Explicitly setting the StoreKit version is now required when setting purchasesAreCompletedBy.")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'configure' has been renamed to 'configure(withAPIKey:appUserID:purchasesAreCompletedBy:storeKitVersion:)':Explicitly setting the StoreKit version is now required when setting purchasesAreCompletedBy.")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'configure' has been renamed to 'configure(withAPIKey:appUserID:purchasesAreCompletedBy:storeKitVersion:)':Explicitly setting the StoreKit version is now required when setting purchasesAreCompletedBy.")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'configure' has been renamed to 'configure(withAPIKey:appUserID:purchasesAreCompletedBy:storeKitVersion:)':Explicitly setting the StoreKit version is now required when setting purchasesAreCompletedBy.")]
	[Static]
	[Export ("configureWithAPIKey:appUserID:observerMode:")]
	RCPurchases ConfigureWithAPIKey (string apiKey, [NullAllowed] string appUserID, bool observerMode);

	// +(RCPurchases * _Nonnull)configureWithAPIKey:(NSString * _Nonnull)apiKey appUserID:(NSString * _Nullable)appUserID observerMode:(BOOL)observerMode userDefaults:(NSUserDefaults * _Nullable)userDefaults __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'configure' has been renamed to 'configure(withAPIKey:appUserID:purchasesAreCompletedBy:storeKitVersion:)':Explicitly setting the StoreKit version is now required when setting purchasesAreCompletedBy. Please use the Configuration.Builder class to configure the SDK with custom UserDefaults.")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'configure' has been renamed to 'configure(withAPIKey:appUserID:purchasesAreCompletedBy:storeKitVersion:)':Explicitly setting the StoreKit version is now required when setting purchasesAreCompletedBy. Please use the Configuration.Builder class to configure the SDK with custom UserDefaults.")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'configure' has been renamed to 'configure(withAPIKey:appUserID:purchasesAreCompletedBy:storeKitVersion:)':Explicitly setting the StoreKit version is now required when setting purchasesAreCompletedBy. Please use the Configuration.Builder class to configure the SDK with custom UserDefaults.")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'configure' has been renamed to 'configure(withAPIKey:appUserID:purchasesAreCompletedBy:storeKitVersion:)':Explicitly setting the StoreKit version is now required when setting purchasesAreCompletedBy. Please use the Configuration.Builder class to configure the SDK with custom UserDefaults.")]
	[Static]
	[Export ("configureWithAPIKey:appUserID:observerMode:userDefaults:")]
	RCPurchases ConfigureWithAPIKey (string apiKey, [NullAllowed] string appUserID, bool observerMode, [NullAllowed] NSUserDefaults userDefaults);

	// +(RCPurchases * _Nonnull)configureWithAPIKey:(NSString * _Nonnull)apiKey appUserID:(NSString * _Nullable)appUserID observerMode:(BOOL)observerMode userDefaults:(NSUserDefaults * _Nullable)userDefaults useStoreKit2IfAvailable:(BOOL)useStoreKit2IfAvailable __attribute__((availability(maccatalyst, obsoleted=1))) __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacCatalyst, 1, 0, message: "'configure' has been renamed to 'configure(with:)'")]
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'configure' has been renamed to 'configure(with:)'")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'configure' has been renamed to 'configure(with:)'")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'configure' has been renamed to 'configure(with:)'")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'configure' has been renamed to 'configure(with:)'")]
	[Static]
	[Export ("configureWithAPIKey:appUserID:observerMode:userDefaults:useStoreKit2IfAvailable:")]
	RCPurchases ConfigureWithAPIKey (string apiKey, [NullAllowed] string appUserID, bool observerMode, [NullAllowed] NSUserDefaults userDefaults, bool useStoreKit2IfAvailable);

	// +(RCPurchases * _Nonnull)configureWithAPIKey:(NSString * _Nonnull)apiKey appUserID:(NSString * _Nullable)appUserID observerMode:(BOOL)observerMode userDefaults:(NSUserDefaults * _Nullable)userDefaults useStoreKit2IfAvailable:(BOOL)useStoreKit2IfAvailable dangerousSettings:(RCDangerousSettings * _Nullable)dangerousSettings __attribute__((availability(maccatalyst, obsoleted=1))) __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacCatalyst, 1, 0, message: "'configure' has been renamed to 'configure(with:)'")]
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'configure' has been renamed to 'configure(with:)'")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'configure' has been renamed to 'configure(with:)'")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'configure' has been renamed to 'configure(with:)'")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'configure' has been renamed to 'configure(with:)'")]
	[Static]
	[Export ("configureWithAPIKey:appUserID:observerMode:userDefaults:useStoreKit2IfAvailable:dangerousSettings:")]
	RCPurchases ConfigureWithAPIKey (string apiKey, [NullAllowed] string appUserID, bool observerMode, [NullAllowed] NSUserDefaults userDefaults, bool useStoreKit2IfAvailable, [NullAllowed] RCDangerousSettings dangerousSettings);

	// -(void)getCustomerInfoWithCompletion:(void (^ _Nonnull)(RCCustomerInfo * _Nullable, NSError * _Nullable))completion;
	[Export ("getCustomerInfoWithCompletion:")]
	void GetCustomerInfoWithCompletion (Action<RCCustomerInfo, NSError> completion);

	// -(void)getCustomerInfoWithFetchPolicy:(enum RCCacheFetchPolicy)fetchPolicy completion:(void (^ _Nonnull)(RCCustomerInfo * _Nullable, NSError * _Nullable))completion;
	[Export ("getCustomerInfoWithFetchPolicy:completion:")]
	void GetCustomerInfoWithFetchPolicy (RCCacheFetchPolicy fetchPolicy, Action<RCCustomerInfo, NSError> completion);

	// -(void)customerInfoWithCompletionHandler:(void (^ _Nonnull)(RCCustomerInfo * _Nullable, NSError * _Nullable))completionHandler;
	[Export ("customerInfoWithCompletionHandler:")]
	void CustomerInfoWithCompletionHandler (Action<RCCustomerInfo, NSError> completionHandler);

	// -(void)customerInfoWithFetchPolicy:(enum RCCacheFetchPolicy)fetchPolicy completionHandler:(void (^ _Nonnull)(RCCustomerInfo * _Nullable, NSError * _Nullable))completionHandler;
	[Export ("customerInfoWithFetchPolicy:completionHandler:")]
	void CustomerInfoWithFetchPolicy (RCCacheFetchPolicy fetchPolicy, Action<RCCustomerInfo, NSError> completionHandler);

	// @property (readonly, nonatomic, strong) RCCustomerInfo * _Nullable cachedCustomerInfo;
	[NullAllowed, Export ("cachedCustomerInfo", ArgumentSemantic.Strong)]
	RCCustomerInfo CachedCustomerInfo { get; }

	// -(void)getProductsWithIdentifiers:(NSArray<NSString *> * _Nonnull)productIdentifiers completion:(void (^ _Nonnull)(NSArray<RCStoreProduct *> * _Nonnull))completion;
	[Export ("getProductsWithIdentifiers:completion:")]
	void GetProductsWithIdentifiers (string[] productIdentifiers, Action<NSArray<RCStoreProduct>> completion);

	// -(void)products:(NSArray<NSString *> * _Nonnull)productIdentifiers completionHandler:(void (^ _Nonnull)(NSArray<RCStoreProduct *> * _Nonnull))completionHandler;
	[Export ("products:completionHandler:")]
	void Products (string[] productIdentifiers, Action<NSArray<RCStoreProduct>> completionHandler);

	// -(void)purchaseProduct:(RCStoreProduct * _Nonnull)product withCompletion:(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, NSError * _Nullable, BOOL))completion;
	[Export ("purchaseProduct:withCompletion:")]
	void PurchaseProduct (RCStoreProduct product, Action<RCStoreTransaction, RCCustomerInfo, NSError, bool> completion);

	// -(void)purchaseWithProduct:(RCStoreProduct * _Nonnull)product completionHandler:(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, BOOL, NSError * _Nullable))completionHandler;
	[Export ("purchaseWithProduct:completionHandler:")]
	void PurchaseWithProduct (RCStoreProduct product, Action<RCStoreTransaction, RCCustomerInfo, bool, NSError> completionHandler);

	// -(void)purchasePackage:(RCPackage * _Nonnull)package withCompletion:(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, NSError * _Nullable, BOOL))completion;
	[Export ("purchasePackage:withCompletion:")]
	void PurchasePackage (RCPackage package, Action<RCStoreTransaction, RCCustomerInfo, NSError, bool> completion);

	// -(void)purchaseWithPackage:(RCPackage * _Nonnull)package completionHandler:(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, BOOL, NSError * _Nullable))completionHandler;
	[Export ("purchaseWithPackage:completionHandler:")]
	void PurchaseWithPackage (RCPackage package, Action<RCStoreTransaction, RCCustomerInfo, bool, NSError> completionHandler);

	// -(void)restorePurchasesWithCompletion:(void (^ _Nullable)(RCCustomerInfo * _Nullable, NSError * _Nullable))completion;
	[Export ("restorePurchasesWithCompletion:")]
	void RestorePurchasesWithCompletion ([NullAllowed] Action<RCCustomerInfo, NSError> completion);

	// -(void)restorePurchasesWithCompletionHandler:(void (^ _Nonnull)(RCCustomerInfo * _Nullable, NSError * _Nullable))completionHandler;
	[Export ("restorePurchasesWithCompletionHandler:")]
	void RestorePurchasesWithCompletionHandler (Action<RCCustomerInfo, NSError> completionHandler);

	// -(void)purchaseWithParams:(RCPurchaseParams * _Nonnull)params completion:(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, NSError * _Nullable, BOOL))completion;
	[Export ("purchaseWithParams:completion:")]
	void PurchaseWithParams (RCPurchaseParams @params, Action<RCStoreTransaction, RCCustomerInfo, NSError, bool> completion);

	// -(void)purchase:(RCPurchaseParams * _Nonnull)params completionHandler:(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, BOOL, NSError * _Nullable))completionHandler;
	[Export ("purchase:completionHandler:")]
	void Purchase (RCPurchaseParams @params, Action<RCStoreTransaction, RCCustomerInfo, bool, NSError> completionHandler);

	// -(void)purchaseProduct:(RCStoreProduct * _Nonnull)product withPromotionalOffer:(RCPromotionalOffer * _Nonnull)promotionalOffer completion:(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, NSError * _Nullable, BOOL))completion;
	[Export ("purchaseProduct:withPromotionalOffer:completion:")]
	void PurchaseProduct (RCStoreProduct product, RCPromotionalOffer promotionalOffer, Action<RCStoreTransaction, RCCustomerInfo, NSError, bool> completion);

	// -(void)purchaseWithProduct:(RCStoreProduct * _Nonnull)product promotionalOffer:(RCPromotionalOffer * _Nonnull)promotionalOffer completionHandler:(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, BOOL, NSError * _Nullable))completionHandler;
	[Export ("purchaseWithProduct:promotionalOffer:completionHandler:")]
	void PurchaseWithProduct (RCStoreProduct product, RCPromotionalOffer promotionalOffer, Action<RCStoreTransaction, RCCustomerInfo, bool, NSError> completionHandler);

	// -(void)purchasePackage:(RCPackage * _Nonnull)package withPromotionalOffer:(RCPromotionalOffer * _Nonnull)promotionalOffer completion:(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, NSError * _Nullable, BOOL))completion;
	[Export ("purchasePackage:withPromotionalOffer:completion:")]
	void PurchasePackage (RCPackage package, RCPromotionalOffer promotionalOffer, Action<RCStoreTransaction, RCCustomerInfo, NSError, bool> completion);

	// -(void)purchaseWithPackage:(RCPackage * _Nonnull)package promotionalOffer:(RCPromotionalOffer * _Nonnull)promotionalOffer completionHandler:(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, BOOL, NSError * _Nullable))completionHandler;
	[Export ("purchaseWithPackage:promotionalOffer:completionHandler:")]
	void PurchaseWithPackage (RCPackage package, RCPromotionalOffer promotionalOffer, Action<RCStoreTransaction, RCCustomerInfo, bool, NSError> completionHandler);

	// -(void)invalidateCustomerInfoCache;
	[Export ("invalidateCustomerInfoCache")]
	void InvalidateCustomerInfoCache ();

	// -(void)syncPurchasesWithCompletion:(void (^ _Nullable)(RCCustomerInfo * _Nullable, NSError * _Nullable))completion;
	[Export ("syncPurchasesWithCompletion:")]
	void SyncPurchasesWithCompletion ([NullAllowed] Action<RCCustomerInfo, NSError> completion);

	// -(void)syncPurchasesWithCompletionHandler:(void (^ _Nonnull)(RCCustomerInfo * _Nullable, NSError * _Nullable))completionHandler;
	[Export ("syncPurchasesWithCompletionHandler:")]
	void SyncPurchasesWithCompletionHandler (Action<RCCustomerInfo, NSError> completionHandler);

	// -(void)checkTrialOrIntroDiscountEligibility:(NSArray<NSString *> * _Nonnull)productIdentifiers completion:(void (^ _Nonnull)(NSDictionary<NSString *,RCIntroEligibility *> * _Nonnull))completion;
	[Export ("checkTrialOrIntroDiscountEligibility:completion:")]
	void CheckTrialOrIntroDiscountEligibility (string[] productIdentifiers, Action<NSDictionary<NSString, RCIntroEligibility>> completion);

	// -(void)checkTrialOrIntroDiscountEligibilityWithProductIdentifiers:(NSArray<NSString *> * _Nonnull)productIdentifiers completionHandler:(void (^ _Nonnull)(NSDictionary<NSString *,RCIntroEligibility *> * _Nonnull))completionHandler;
	[Export ("checkTrialOrIntroDiscountEligibilityWithProductIdentifiers:completionHandler:")]
	void CheckTrialOrIntroDiscountEligibilityWithProductIdentifiers (string[] productIdentifiers, Action<NSDictionary<NSString, RCIntroEligibility>> completionHandler);

	// -(void)checkTrialOrIntroDiscountEligibilityForProduct:(RCStoreProduct * _Nonnull)product completion:(void (^ _Nonnull)(enum RCIntroEligibilityStatus))completion;
	[Export ("checkTrialOrIntroDiscountEligibilityForProduct:completion:")]
	void CheckTrialOrIntroDiscountEligibilityForProduct (RCStoreProduct product, Action<RCIntroEligibilityStatus> completion);

	// -(void)checkTrialOrIntroDiscountEligibilityWithProduct:(RCStoreProduct * _Nonnull)product completionHandler:(void (^ _Nonnull)(enum RCIntroEligibilityStatus))completionHandler;
	[Export ("checkTrialOrIntroDiscountEligibilityWithProduct:completionHandler:")]
	void CheckTrialOrIntroDiscountEligibilityWithProduct (RCStoreProduct product, Action<RCIntroEligibilityStatus> completionHandler);

	// -(void)showPriceConsentIfNeeded __attribute__((availability(maccatalyst, introduced=13.4))) __attribute__((availability(ios, introduced=13.4)));
	[Export ("showPriceConsentIfNeeded")]
	void ShowPriceConsentIfNeeded ();

	// -(void)presentCodeRedemptionSheet __attribute__((availability(maccatalyst, unavailable))) __attribute__((availability(macos, unavailable))) __attribute__((availability(tvos, unavailable))) __attribute__((availability(watchos, unavailable))) __attribute__((availability(ios, introduced=14.0)));
	[Export ("presentCodeRedemptionSheet")]
	void PresentCodeRedemptionSheet ();

	// -(void)getPromotionalOfferForProductDiscount:(RCStoreProductDiscount * _Nonnull)discount withProduct:(RCStoreProduct * _Nonnull)product withCompletion:(void (^ _Nonnull)(RCPromotionalOffer * _Nullable, NSError * _Nullable))completion;
	[Export ("getPromotionalOfferForProductDiscount:withProduct:withCompletion:")]
	void GetPromotionalOfferForProductDiscount (RCStoreProductDiscount discount, RCStoreProduct product, Action<RCPromotionalOffer, NSError> completion);

	// -(void)promotionalOfferForProductDiscount:(RCStoreProductDiscount * _Nonnull)discount product:(RCStoreProduct * _Nonnull)product completionHandler:(void (^ _Nonnull)(RCPromotionalOffer * _Nullable, NSError * _Nullable))completionHandler;
	[Export ("promotionalOfferForProductDiscount:product:completionHandler:")]
	void PromotionalOfferForProductDiscount (RCStoreProductDiscount discount, RCStoreProduct product, Action<RCPromotionalOffer, NSError> completionHandler);

	// -(void)eligiblePromotionalOffersForProduct:(RCStoreProduct * _Nonnull)product completionHandler:(void (^ _Nonnull)(NSArray<RCPromotionalOffer *> * _Nonnull))completionHandler;
	[Export ("eligiblePromotionalOffersForProduct:completionHandler:")]
	void EligiblePromotionalOffersForProduct (RCStoreProduct product, Action<NSArray<RCPromotionalOffer>> completionHandler);

	// -(void)showManageSubscriptionsWithCompletion:(void (^ _Nonnull)(NSError * _Nullable))completion __attribute__((availability(macos, introduced=10.15))) __attribute__((availability(ios, introduced=13.0))) __attribute__((availability(tvos, unavailable))) __attribute__((availability(watchos, unavailable)));
	[Export ("showManageSubscriptionsWithCompletion:")]
	void ShowManageSubscriptionsWithCompletion (Action<NSError> completion);

	// -(void)showManageSubscriptionsWithCompletionHandler:(void (^ _Nonnull)(NSError * _Nullable))completionHandler __attribute__((availability(macos, introduced=10.15))) __attribute__((availability(ios, introduced=13.0))) __attribute__((availability(tvos, unavailable))) __attribute__((availability(watchos, unavailable)));
	[Export ("showManageSubscriptionsWithCompletionHandler:")]
	void ShowManageSubscriptionsWithCompletionHandler (Action<NSError> completionHandler);

	// -(void)beginRefundRequestForProduct:(NSString * _Nonnull)productID completion:(void (^ _Nonnull)(enum RCRefundRequestStatus, NSError * _Nullable))completionHandler __attribute__((availability(tvos, unavailable))) __attribute__((availability(watchos, unavailable))) __attribute__((availability(macos, unavailable))) __attribute__((availability(ios, introduced=15.0)));
	[Export ("beginRefundRequestForProduct:completion:")]
	void BeginRefundRequestForProduct (string productID, Action<RCRefundRequestStatus, NSError> completionHandler);

	// -(void)beginRefundRequestForEntitlement:(NSString * _Nonnull)entitlementID completion:(void (^ _Nonnull)(enum RCRefundRequestStatus, NSError * _Nullable))completionHandler __attribute__((availability(tvos, unavailable))) __attribute__((availability(watchos, unavailable))) __attribute__((availability(macos, unavailable))) __attribute__((availability(ios, introduced=15.0)));
	[Export ("beginRefundRequestForEntitlement:completion:")]
	void BeginRefundRequestForEntitlement (string entitlementID, Action<RCRefundRequestStatus, NSError> completionHandler);

	// -(void)beginRefundRequestForActiveEntitlementWithCompletion:(void (^ _Nonnull)(enum RCRefundRequestStatus, NSError * _Nullable))completionHandler __attribute__((availability(tvos, unavailable))) __attribute__((availability(watchos, unavailable))) __attribute__((availability(macos, unavailable))) __attribute__((availability(ios, introduced=15.0)));
	[Export ("beginRefundRequestForActiveEntitlementWithCompletion:")]
	void BeginRefundRequestForActiveEntitlementWithCompletion (Action<RCRefundRequestStatus, NSError> completionHandler);

	// -(void)redeemWebPurchaseWithWebPurchaseRedemption:(RCWebPurchaseRedemption * _Nonnull)webPurchaseRedemption completion:(void (^ _Nonnull)(RCCustomerInfo * _Nullable, NSError * _Nullable))completion;
	[Export ("redeemWebPurchaseWithWebPurchaseRedemption:completion:")]
	void RedeemWebPurchaseWithWebPurchaseRedemption (RCWebPurchaseRedemption webPurchaseRedemption, Action<RCCustomerInfo, NSError> completion);
}

// @protocol RCPurchasesDelegate <NSObject>
[Protocol, Model]
[BaseType (typeof(NSObject))]
interface RCPurchasesDelegate
{
	// @optional -(void)purchases:(RCPurchases * _Nonnull)purchases didReceiveUpdatedPurchaserInfo:(RCCustomerInfo * _Nonnull)purchaserInfo __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.WatchOS, 1, 0)]
	[Obsoleted (PlatformName.TvOS, 1, 0)]
	[Obsoleted (PlatformName.MacOSX, 1, 0)]
	[Obsoleted (PlatformName.iOS, 1, 0)]
	[Export ("purchases:didReceiveUpdatedPurchaserInfo:")]
	void DidReceiveUpdatedPurchaserInfo (RCPurchases purchases, RCCustomerInfo purchaserInfo);

	// @optional -(void)purchases:(RCPurchases * _Nonnull)purchases receivedUpdatedCustomerInfo:(RCCustomerInfo * _Nonnull)customerInfo;
	[Export ("purchases:receivedUpdatedCustomerInfo:")]
	void ReceivedUpdatedCustomerInfo (RCPurchases purchases, RCCustomerInfo customerInfo);

	// @optional -(void)purchases:(RCPurchases * _Nonnull)purchases readyForPromotedProduct:(RCStoreProduct * _Nonnull)product purchase:(void (^ _Nonnull)(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, NSError * _Nullable, BOOL)))startPurchase;
	[Export ("purchases:readyForPromotedProduct:purchase:")]
	void ReadyForPromotedProduct (RCPurchases purchases, RCStoreProduct product, StartPurchaseHandler startPurchase);

	// @optional -(void)purchases:(RCPurchases * _Nonnull)purchases shouldPurchasePromoProduct:(RCStoreProduct * _Nonnull)product defermentBlock:(void (^ _Nonnull)(void (^ _Nonnull)(RCStoreTransaction * _Nullable, RCCustomerInfo * _Nullable, NSError * _Nullable, BOOL)))makeDeferredPurchase __attribute__((availability(maccatalyst, obsoleted=1))) __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacCatalyst, 1, 0, message: "'purchases' has been renamed to 'purchases:readyForPromotedProduct:purchase:'")]
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'purchases' has been renamed to 'purchases:readyForPromotedProduct:purchase:'")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'purchases' has been renamed to 'purchases:readyForPromotedProduct:purchase:'")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'purchases' has been renamed to 'purchases:readyForPromotedProduct:purchase:'")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'purchases' has been renamed to 'purchases:readyForPromotedProduct:purchase:'")]
	[Export ("purchases:shouldPurchasePromoProduct:defermentBlock:")]
	void ShouldPurchasePromoProduct (RCPurchases purchases, RCStoreProduct product, StartPurchaseHandler makeDeferredPurchase);

	// @optional @property (readonly, nonatomic) BOOL shouldShowPriceConsent __attribute__((availability(watchos, unavailable))) __attribute__((availability(tvos, unavailable))) __attribute__((availability(macos, unavailable))) __attribute__((availability(maccatalyst, introduced=13.4))) __attribute__((availability(ios, introduced=13.4)));
	[Export ("shouldShowPriceConsent")]
	bool ShouldShowPriceConsent { get; }
}

// @interface RCPurchasesDiagnostics : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCPurchasesDiagnostics
{
	// @property (readonly, getter = default, nonatomic, strong, class) RCPurchasesDiagnostics * _Nonnull default_;
	[Static]
	[Export ("default_", ArgumentSemantic.Strong)]
	RCPurchasesDiagnostics Default_ { [Bind ("default")] get; }
	// -(void)testSDKHealthWithCompletion:(void (^ _Nonnull)(NSError * _Nullable))completionHandler __attribute__((deprecated("
    //Use the `PurchasesDiagnostics.shared.checkSDKHealth()` method instead.
    //")));
	[Export ("testSDKHealthWithCompletion:")]
	void TestSDKHealthWithCompletion (Action<NSError> completionHandler);
}

// @interface RCStoreProduct : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCStoreProduct : INativeObject
{
	// @property (readonly, nonatomic) NSUInteger hash;
	[Export ("hash")]
	nuint Hash { get; }

	// @property (readonly, nonatomic) enum RCStoreProductType productType;
	[Export ("productType")]
	RCStoreProductType ProductType { get; }

	// @property (readonly, nonatomic) enum RCStoreProductCategory productCategory;
	[Export ("productCategory")]
	RCStoreProductCategory ProductCategory { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull localizedDescription;
	[Export ("localizedDescription")]
	string LocalizedDescription { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull localizedTitle;
	[Export ("localizedTitle")]
	string LocalizedTitle { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nullable currencyCode;
	[NullAllowed, Export ("currencyCode")]
	string CurrencyCode { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull localizedPriceString;
	[Export ("localizedPriceString")]
	string LocalizedPriceString { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull productIdentifier;
	[Export ("productIdentifier")]
	string ProductIdentifier { get; }

	// @property (readonly, nonatomic) BOOL isFamilyShareable __attribute__((availability(watchos, introduced=7.0))) __attribute__((availability(tvos, introduced=14.0))) __attribute__((availability(macos, introduced=11.0))) __attribute__((availability(ios, introduced=14.0)));
	[Export ("isFamilyShareable")]
	bool IsFamilyShareable { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nullable subscriptionGroupIdentifier;
	[NullAllowed, Export ("subscriptionGroupIdentifier")]
	string SubscriptionGroupIdentifier { get; }

	// @property (readonly, nonatomic, strong) NSNumberFormatter * _Nullable priceFormatter;
	[NullAllowed, Export ("priceFormatter", ArgumentSemantic.Strong)]
	NSNumberFormatter PriceFormatter { get; }

	// @property (readonly, nonatomic, strong) RCSubscriptionPeriod * _Nullable subscriptionPeriod;
	[NullAllowed, Export ("subscriptionPeriod", ArgumentSemantic.Strong)]
	RCSubscriptionPeriod SubscriptionPeriod { get; }

	// @property (readonly, nonatomic, strong) RCStoreProductDiscount * _Nullable introductoryDiscount;
	[NullAllowed, Export ("introductoryDiscount", ArgumentSemantic.Strong)]
	RCStoreProductDiscount IntroductoryDiscount { get; }

	// @property (readonly, copy, nonatomic) NSArray<RCStoreProductDiscount *> * _Nonnull discounts;
	[Export ("discounts", ArgumentSemantic.Copy)]
	RCStoreProductDiscount[] Discounts { get; }

	// @property (readonly, nonatomic, strong) SWIFT_AVAILABILITY(macos,unavailable,message="'introductoryPrice' has been renamed to 'introductoryDiscount': Use StoreProductDiscount instead") SKProductDiscount * introductoryPrice __attribute__((availability(macos, unavailable))) __attribute__((availability(watchos, unavailable))) __attribute__((availability(tvos, unavailable))) __attribute__((availability(ios, unavailable)));
	[Unavailable (PlatformName.MacOSX)]
	[Unavailable (PlatformName.WatchOS)]
	[Unavailable (PlatformName.TvOS)]
	[Unavailable (PlatformName.iOS)]
	[Export ("introductoryPrice", ArgumentSemantic.Strong)]
	SKProductDiscount IntroductoryPrice { get; }

	// @property (readonly, copy, nonatomic) SWIFT_AVAILABILITY(macos,unavailable,message="Use localizedPriceString instead") NSLocale * priceLocale __attribute__((availability(macos, unavailable))) __attribute__((availability(watchos, unavailable))) __attribute__((availability(tvos, unavailable))) __attribute__((availability(ios, unavailable)));
	[Unavailable (PlatformName.MacOSX)]
	[Unavailable (PlatformName.WatchOS)]
	[Unavailable (PlatformName.TvOS)]
	[Unavailable (PlatformName.iOS)]
	[Export ("priceLocale", ArgumentSemantic.Copy)]
	NSLocale PriceLocale { get; }

	// -(instancetype _Nonnull)initWithSk1Product:(SKProduct * _Nonnull)sk1Product;
	[Export ("initWithSk1Product:")]
	NativeHandle Constructor (SKProduct sk1Product);

	// @property (readonly, nonatomic, strong) SKProduct * _Nullable sk1Product;
	[NullAllowed, Export ("sk1Product", ArgumentSemantic.Strong)]
	SKProduct Sk1Product { get; }

	// @property (readonly, nonatomic, strong) NSDecimalNumber * _Nonnull price;
	[Export ("price", ArgumentSemantic.Strong)]
	NSDecimalNumber Price { get; }

	// @property (readonly, nonatomic, strong) SWIFT_AVAILABILITY(watchos,introduced=6.2) NSDecimalNumber * pricePerDay __attribute__((availability(watchos, introduced=6.2))) __attribute__((availability(tvos, introduced=11.2))) __attribute__((availability(macos, introduced=10.13.2))) __attribute__((availability(ios, introduced=11.2)));
	[Export ("pricePerDay", ArgumentSemantic.Strong)]
	NSDecimalNumber PricePerDay { get; }

	// @property (readonly, nonatomic, strong) SWIFT_AVAILABILITY(watchos,introduced=6.2) NSDecimalNumber * pricePerWeek __attribute__((availability(watchos, introduced=6.2))) __attribute__((availability(tvos, introduced=11.2))) __attribute__((availability(macos, introduced=10.13.2))) __attribute__((availability(ios, introduced=11.2)));
	[Export ("pricePerWeek", ArgumentSemantic.Strong)]
	NSDecimalNumber PricePerWeek { get; }

	// @property (readonly, nonatomic, strong) SWIFT_AVAILABILITY(watchos,introduced=6.2) NSDecimalNumber * pricePerMonth __attribute__((availability(watchos, introduced=6.2))) __attribute__((availability(tvos, introduced=11.2))) __attribute__((availability(macos, introduced=10.13.2))) __attribute__((availability(ios, introduced=11.2)));
	[Export ("pricePerMonth", ArgumentSemantic.Strong)]
	NSDecimalNumber PricePerMonth { get; }

	// @property (readonly, nonatomic, strong) SWIFT_AVAILABILITY(watchos,introduced=6.2) NSDecimalNumber * pricePerYear __attribute__((availability(watchos, introduced=6.2))) __attribute__((availability(tvos, introduced=11.2))) __attribute__((availability(macos, introduced=10.13.2))) __attribute__((availability(ios, introduced=11.2)));
	[Export ("pricePerYear", ArgumentSemantic.Strong)]
	NSDecimalNumber PricePerYear { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nullable localizedIntroductoryPriceString;
	[NullAllowed, Export ("localizedIntroductoryPriceString")]
	string LocalizedIntroductoryPriceString { get; }

	// @property (readonly, copy, nonatomic) SWIFT_AVAILABILITY(watchos,introduced=6.2) NSString * localizedPricePerDay __attribute__((availability(watchos, introduced=6.2))) __attribute__((availability(tvos, introduced=11.2))) __attribute__((availability(macos, introduced=10.13.2))) __attribute__((availability(ios, introduced=11.2)));
	[Export ("localizedPricePerDay")]
	string LocalizedPricePerDay { get; }

	// @property (readonly, copy, nonatomic) SWIFT_AVAILABILITY(watchos,introduced=6.2) NSString * localizedPricePerWeek __attribute__((availability(watchos, introduced=6.2))) __attribute__((availability(tvos, introduced=11.2))) __attribute__((availability(macos, introduced=10.13.2))) __attribute__((availability(ios, introduced=11.2)));
	[Export ("localizedPricePerWeek")]
	string LocalizedPricePerWeek { get; }

	// @property (readonly, copy, nonatomic) SWIFT_AVAILABILITY(watchos,introduced=6.2) NSString * localizedPricePerMonth __attribute__((availability(watchos, introduced=6.2))) __attribute__((availability(tvos, introduced=11.2))) __attribute__((availability(macos, introduced=10.13.2))) __attribute__((availability(ios, introduced=11.2)));
	[Export ("localizedPricePerMonth")]
	string LocalizedPricePerMonth { get; }

	// @property (readonly, copy, nonatomic) SWIFT_AVAILABILITY(watchos,introduced=6.2) NSString * localizedPricePerYear __attribute__((availability(watchos, introduced=6.2))) __attribute__((availability(tvos, introduced=11.2))) __attribute__((availability(macos, introduced=10.13.2))) __attribute__((availability(ios, introduced=11.2)));
	[Export ("localizedPricePerYear")]
	string LocalizedPricePerYear { get; }
}

// @interface RCStoreProductDiscount : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCStoreProductDiscount
{
	// @property (readonly, copy, nonatomic) NSString * _Nullable offerIdentifier;
	[NullAllowed, Export ("offerIdentifier")]
	string OfferIdentifier { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nullable currencyCode;
	[NullAllowed, Export ("currencyCode")]
	string CurrencyCode { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull localizedPriceString;
	[Export ("localizedPriceString")]
	string LocalizedPriceString { get; }

	// @property (readonly, nonatomic) enum RCPaymentMode paymentMode;
	[Export ("paymentMode")]
	RCPaymentMode PaymentMode { get; }

	// @property (readonly, nonatomic, strong) RCSubscriptionPeriod * _Nonnull subscriptionPeriod;
	[Export ("subscriptionPeriod", ArgumentSemantic.Strong)]
	RCSubscriptionPeriod SubscriptionPeriod { get; }

	// @property (readonly, nonatomic) NSInteger numberOfPeriods;
	[Export ("numberOfPeriods")]
	nint NumberOfPeriods { get; }

	// @property (readonly, nonatomic) enum RCDiscountType type;
	[Export ("type")]
	RCDiscountType Type { get; }

	// @property (readonly, nonatomic) NSUInteger hash;
	[Export ("hash")]
	nuint Hash { get; }

	// @property (readonly, nonatomic, strong) NSDecimalNumber * _Nonnull price;
	[Export ("price", ArgumentSemantic.Strong)]
	NSDecimalNumber Price { get; }

	// @property (readonly, nonatomic, strong) SKProductDiscount * _Nullable sk1Discount;
	[NullAllowed, Export ("sk1Discount", ArgumentSemantic.Strong)]
	SKProductDiscount Sk1Discount { get; }

	// @property (readonly, nonatomic, strong) SWIFT_AVAILABILITY(watchos,introduced=6.2) NSDecimalNumber * pricePerDay __attribute__((availability(watchos, introduced=6.2))) __attribute__((availability(tvos, introduced=11.2))) __attribute__((availability(macos, introduced=10.13.2))) __attribute__((availability(ios, introduced=11.2)));
	[Export ("pricePerDay", ArgumentSemantic.Strong)]
	NSDecimalNumber PricePerDay { get; }

	// @property (readonly, nonatomic, strong) SWIFT_AVAILABILITY(watchos,introduced=6.2) NSDecimalNumber * pricePerWeek __attribute__((availability(watchos, introduced=6.2))) __attribute__((availability(tvos, introduced=11.2))) __attribute__((availability(macos, introduced=10.13.2))) __attribute__((availability(ios, introduced=11.2)));
	[Export ("pricePerWeek", ArgumentSemantic.Strong)]
	NSDecimalNumber PricePerWeek { get; }

	// @property (readonly, nonatomic, strong) SWIFT_AVAILABILITY(watchos,introduced=6.2) NSDecimalNumber * pricePerMonth __attribute__((availability(watchos, introduced=6.2))) __attribute__((availability(tvos, introduced=11.2))) __attribute__((availability(macos, introduced=10.13.2))) __attribute__((availability(ios, introduced=11.2)));
	[Export ("pricePerMonth", ArgumentSemantic.Strong)]
	NSDecimalNumber PricePerMonth { get; }

	// @property (readonly, nonatomic, strong) SWIFT_AVAILABILITY(watchos,introduced=6.2) NSDecimalNumber * pricePerYear __attribute__((availability(watchos, introduced=6.2))) __attribute__((availability(tvos, introduced=11.2))) __attribute__((availability(macos, introduced=10.13.2))) __attribute__((availability(ios, introduced=11.2)));
	[Export ("pricePerYear", ArgumentSemantic.Strong)]
	NSDecimalNumber PricePerYear { get; }
}

// @interface RCStoreTransaction : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCStoreTransaction
{
	// @property (readonly, copy, nonatomic) NSString * _Nonnull productIdentifier;
	[Export ("productIdentifier")]
	string ProductIdentifier { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nonnull purchaseDate;
	[Export ("purchaseDate", ArgumentSemantic.Copy)]
	NSDate PurchaseDate { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull transactionIdentifier;
	[Export ("transactionIdentifier")]
	string TransactionIdentifier { get; }

	// @property (readonly, nonatomic) NSInteger quantity;
	[Export ("quantity")]
	nint Quantity { get; }

	// @property (readonly, nonatomic, strong) RCStorefront * _Nullable storefront;
	[NullAllowed, Export ("storefront", ArgumentSemantic.Strong)]
	RCStorefront Storefront { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nullable jwsRepresentation;
	[NullAllowed, Export ("jwsRepresentation")]
	string JwsRepresentation { get; }

	// @property (readonly, nonatomic) NSUInteger hash;
	[Export ("hash")]
	nuint Hash { get; }

	// @property (readonly, copy, nonatomic) SWIFT_AVAILABILITY(macos,obsoleted=1,message="'productId' has been renamed to 'productIdentifier'") NSString * productId __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'productId' has been renamed to 'productIdentifier'")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'productId' has been renamed to 'productIdentifier'")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'productId' has been renamed to 'productIdentifier'")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'productId' has been renamed to 'productIdentifier'")]
	[Export ("productId")]
	string ProductId { get; }

	// @property (readonly, copy, nonatomic) SWIFT_AVAILABILITY(macos,obsoleted=1,message="'revenueCatId' has been renamed to 'transactionIdentifier'") NSString * revenueCatId __attribute__((availability(macos, obsoleted=1))) __attribute__((availability(watchos, obsoleted=1))) __attribute__((availability(tvos, obsoleted=1))) __attribute__((availability(ios, obsoleted=1)));
	[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'revenueCatId' has been renamed to 'transactionIdentifier'")]
	[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'revenueCatId' has been renamed to 'transactionIdentifier'")]
	[Obsoleted (PlatformName.TvOS, 1, 0, message: "'revenueCatId' has been renamed to 'transactionIdentifier'")]
	[Obsoleted (PlatformName.iOS, 1, 0, message: "'revenueCatId' has been renamed to 'transactionIdentifier'")]
	[Export ("revenueCatId")]
	string RevenueCatId { get; }

	// @property (readonly, nonatomic, strong) SKPaymentTransaction * _Nullable sk1Transaction;
	[NullAllowed, Export ("sk1Transaction", ArgumentSemantic.Strong)]
	SKPaymentTransaction Sk1Transaction { get; }
}

// @interface RCStorefront : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCStorefront
{
	// @property (readonly, copy, nonatomic) NSString * _Nonnull countryCode;
	[Export ("countryCode")]
	string CountryCode { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull identifier;
	[Export ("identifier")]
	string Identifier { get; }

	// @property (readonly, nonatomic) NSUInteger hash;
	[Export ("hash")]
	nuint Hash { get; }

	// @property (readonly, nonatomic, strong) SWIFT_AVAILABILITY(maccatalyst,introduced=13.1) SKStorefront * sk1Storefront __attribute__((availability(maccatalyst, introduced=13.1))) __attribute__((availability(watchos, introduced=6.2))) __attribute__((availability(tvos, introduced=13.0))) __attribute__((availability(macos, introduced=10.15))) __attribute__((availability(ios, introduced=13.0)));
	[Export ("sk1Storefront", ArgumentSemantic.Strong)]
	SKStorefront Sk1Storefront { get; }

	// @property (readonly, nonatomic, strong, class) RCStorefront * _Nullable sk1CurrentStorefront __attribute__((availability(maccatalyst, introduced=13.1))) __attribute__((availability(watchos, introduced=6.2))) __attribute__((availability(tvos, introduced=13.0))) __attribute__((availability(macos, introduced=10.15))) __attribute__((availability(ios, introduced=13.0)));
	[Static]
	[NullAllowed, Export ("sk1CurrentStorefront", ArgumentSemantic.Strong)]
	RCStorefront Sk1CurrentStorefront { get; }
}

// @interface RCSubscriptionInfo : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCSubscriptionInfo : INativeObject
{
	// @property (readonly, copy, nonatomic) NSString * _Nonnull productIdentifier;
	[Export ("productIdentifier")]
	string ProductIdentifier { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nonnull purchaseDate;
	[Export ("purchaseDate", ArgumentSemantic.Copy)]
	NSDate PurchaseDate { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nullable originalPurchaseDate;
	[NullAllowed, Export ("originalPurchaseDate", ArgumentSemantic.Copy)]
	NSDate OriginalPurchaseDate { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nullable expiresDate;
	[NullAllowed, Export ("expiresDate", ArgumentSemantic.Copy)]
	NSDate ExpiresDate { get; }

	// @property (readonly, nonatomic) enum RCStore store;
	[Export ("store")]
	RCStore Store { get; }

	// @property (readonly, nonatomic) BOOL isSandbox;
	[Export ("isSandbox")]
	bool IsSandbox { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nullable unsubscribeDetectedAt;
	[NullAllowed, Export ("unsubscribeDetectedAt", ArgumentSemantic.Copy)]
	NSDate UnsubscribeDetectedAt { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nullable billingIssuesDetectedAt;
	[NullAllowed, Export ("billingIssuesDetectedAt", ArgumentSemantic.Copy)]
	NSDate BillingIssuesDetectedAt { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nullable gracePeriodExpiresDate;
	[NullAllowed, Export ("gracePeriodExpiresDate", ArgumentSemantic.Copy)]
	NSDate GracePeriodExpiresDate { get; }

	// @property (readonly, nonatomic) enum RCPurchaseOwnershipType ownershipType;
	[Export ("ownershipType")]
	RCPurchaseOwnershipType OwnershipType { get; }

	// @property (readonly, nonatomic) enum RCPeriodType periodType;
	[Export ("periodType")]
	RCPeriodType PeriodType { get; }

	// @property (readonly, copy, nonatomic) NSDate * _Nullable refundedAt;
	[NullAllowed, Export ("refundedAt", ArgumentSemantic.Copy)]
	NSDate RefundedAt { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nullable storeTransactionId;
	[NullAllowed, Export ("storeTransactionId")]
	string StoreTransactionId { get; }

	// @property (readonly, nonatomic) BOOL isActive;
	[Export ("isActive")]
	bool IsActive { get; }

	// @property (readonly, nonatomic) BOOL willRenew;
	[Export ("willRenew")]
	bool WillRenew { get; }

	// @property (readonly, nonatomic, strong) RCProductPaidPrice * _Nullable price;
	[NullAllowed, Export ("price", ArgumentSemantic.Strong)]
	RCProductPaidPrice Price { get; }
}

// @interface RCSubscriptionPeriod : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCSubscriptionPeriod
{
	// @property (readonly, nonatomic) NSInteger value;
	[Export ("value")]
	nint Value { get; }

	// @property (readonly, nonatomic) enum RCSubscriptionPeriodUnit unit;
	[Export ("unit")]
	RCSubscriptionPeriodUnit Unit { get; }

	// @property (readonly, nonatomic) NSUInteger hash;
	[Export ("hash")]
	nuint Hash { get; }

	// @property (readonly, nonatomic) NSInteger numberOfUnits __attribute__((availability(macos, unavailable))) __attribute__((availability(watchos, unavailable))) __attribute__((availability(tvos, unavailable))) __attribute__((availability(ios, unavailable)));
	[Unavailable (PlatformName.MacOSX)]
	[Unavailable (PlatformName.WatchOS)]
	[Unavailable (PlatformName.TvOS)]
	[Unavailable (PlatformName.iOS)]
	[Export ("numberOfUnits")]
	nint NumberOfUnits { get; }
}

// @interface RCTransaction : NSObject
[Obsoleted (PlatformName.MacOSX, 1, 0, message: "'Transaction' has been renamed to 'RCStoreTransaction'")]
[Obsoleted (PlatformName.WatchOS, 1, 0, message: "'Transaction' has been renamed to 'RCStoreTransaction'")]
[Obsoleted (PlatformName.TvOS, 1, 0, message: "'Transaction' has been renamed to 'RCStoreTransaction'")]
[Obsoleted (PlatformName.iOS, 1, 0, message: "'Transaction' has been renamed to 'RCStoreTransaction'")]
[BaseType (typeof(NSObject))]
interface RCTransaction
{
}

// @interface RCVirtualCurrencies : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCVirtualCurrencies
{
	// @property (readonly, copy, nonatomic) NSDictionary<NSString *,RCVirtualCurrency *> * _Nonnull all;
	[Export ("all", ArgumentSemantic.Copy)]
	NSDictionary<NSString, RCVirtualCurrency> All { get; }

	// -(RCVirtualCurrency * _Nullable)objectForKeyedSubscript:(NSString * _Nonnull)key __attribute__((warn_unused_result("")));
	[Export ("objectForKeyedSubscript:")]
	[return: NullAllowed]
	RCVirtualCurrency ObjectForKeyedSubscript (string key);
}

// @interface RCVirtualCurrency : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCVirtualCurrency : INativeObject
{
	// @property (readonly, nonatomic) NSInteger balance;
	[Export ("balance")]
	nint Balance { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull name;
	[Export ("name")]
	string Name { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nonnull code;
	[Export ("code")]
	string Code { get; }

	// @property (readonly, copy, nonatomic) NSString * _Nullable serverDescription;
	[NullAllowed, Export ("serverDescription")]
	string ServerDescription { get; }
}

// @interface RCWebPurchaseRedemption : NSObject
[BaseType(typeof(NSObject))]
[DisableDefaultCtor]
interface RCWebPurchaseRedemption
{
}

// @interface RCWinBackOffer : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface RCWinBackOffer : INativeObject
{
	// @property (readonly, nonatomic, strong) RCStoreProductDiscount * _Nonnull discount;
	[Export ("discount", ArgumentSemantic.Strong)]
	RCStoreProductDiscount Discount { get; }
}
