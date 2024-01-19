using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DemoApp;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private readonly IRevenueCatBilling _revenueCatBilling;

    //RC data
    private List<OfferingDto> _loadedOfferings { get; set; } = new();
    private PackageDto _monthlySubscription = new();
    private PackageDto _yearlySubscription = new();
    private PackageDto _consumable1 = new();
    private PackageDto _consumable2 = new();

    private string _consumableOfferingIdentifier = "TrainingPlan";
    private string _consumablePackageIdentifierPrefix = "satisfit_trainingplan_";

    //UI data
    public event PropertyChangedEventHandler? PropertyChanged;
    public bool AreOfferingsLoaded => _loadedOfferings.Any();
    public string MonthlyButtonText => $"Monthly subscription for {_monthlySubscription.Product.Pricing.PriceLocalized}";
    public string YearlyButtonText => $"Yearly subscription for {_yearlySubscription.Product.Pricing.PriceLocalized}";
    public string Consumable1ButtonText => $"Item1 for {_consumable1.Product.Pricing.PriceLocalized}";
    public string Consumable2ButtonText => $"Item2 for {_consumable2.Product.Pricing.PriceLocalized}";

    public MainPage(IRevenueCatBilling revenueCatBilling)
    {
        InitializeComponent();
        _revenueCatBilling = revenueCatBilling;
        BindingContext = this;
    }

    private void LoadOfferings(object sender, EventArgs e)
    {
        //this is just to showcase functionality. For running async actions use Commands and for UI updating proper NotifyPropertyChanged flow
        Task.Run(async () =>
        {
            _loadedOfferings = await _revenueCatBilling.GetOfferings();

            _monthlySubscription = _loadedOfferings
                .SelectMany(x => x.AvailablePackages)
                .First(x => x.Identifier == DefaultPackageIdentifier.Monthly);

            _yearlySubscription = _loadedOfferings
                .SelectMany(x => x.AvailablePackages)
                .First(x => x.Identifier == DefaultPackageIdentifier.Annually);

            _consumable1 = _loadedOfferings
                .First(x => x.Identifier == _consumableOfferingIdentifier)
                .AvailablePackages.First(x => x.Identifier == $"{_consumablePackageIdentifierPrefix}3");

            _consumable2 = _loadedOfferings
                .First(x => x.Identifier == _consumableOfferingIdentifier)
                .AvailablePackages.First(x => x.Identifier == $"{_consumablePackageIdentifierPrefix}5");

            NotifyPropertyChanged(nameof(AreOfferingsLoaded));
            NotifyPropertyChanged(nameof(MonthlyButtonText));
            NotifyPropertyChanged(nameof(YearlyButtonText));
            NotifyPropertyChanged(nameof(Consumable1ButtonText));
            NotifyPropertyChanged(nameof(Consumable2ButtonText));
        });
    }

    private void BuyItem(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var originalText = button.Text;
        button.Text = "Purchasing...";
        button.IsEnabled = false;

        Task.Run(async () =>
        {
            if (button == BtnMonthly)
            {
                await _revenueCatBilling.PurchaseProduct(_monthlySubscription);
            }
            else if (button == BtnYearly)
            {
                await _revenueCatBilling.PurchaseProduct(_yearlySubscription);
            }
            else if (button == BtnConsumable1)
            {
                await _revenueCatBilling.PurchaseProduct(_consumable1);
            }
            else if (button == BtnConsumable2)
            {
                await _revenueCatBilling.PurchaseProduct(_consumable2);
            }

            button.Dispatcher.Dispatch(() =>
            {
                button.Text = originalText;
                button.IsEnabled = true;
            });
        });
    }

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

