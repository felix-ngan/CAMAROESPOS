using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CamaroesPOS.Data;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.UI.Avalonia.Services;

namespace CamaroesPOS.UI.Avalonia.ViewModels;

public class WalletViewModel : INotifyPropertyChanged
{
    public ObservableCollection<CustomerWallet> Wallets { get; } = new();

    private CustomerWallet? _selectedWallet;
    public CustomerWallet? SelectedWallet { get => _selectedWallet; set { _selectedWallet = value; OnPropertyChanged(); } }

    private bool _isRecharging;
    public bool IsRecharging { get => _isRecharging; set { _isRecharging = value; OnPropertyChanged(); } }

    private bool _isNewWallet;
    public bool IsNewWallet { get => _isNewWallet; set { _isNewWallet = value; OnPropertyChanged(); } }

    private double _rechargeAmount;
    public double RechargeAmount { get => _rechargeAmount; set { _rechargeAmount = value; OnPropertyChanged(); } }

    public RelayCommand<object?> CreateWalletCommand { get; }
    public RelayCommand<CustomerWallet> RechargeCommand { get; }
    public RelayCommand<object?> SaveCommand { get; }
    public RelayCommand<object?> CancelCommand { get; }
    public RelayCommand<CustomerWallet> DeleteWalletCommand { get; }

    public WalletViewModel()
    {
        CreateWalletCommand = new RelayCommand<object?>(_ => OpenCreateForm());
        RechargeCommand = new RelayCommand<CustomerWallet>(OpenRechargeForm);
        SaveCommand = new RelayCommand<object?>(_ => SaveWalletActionAsync());
        CancelCommand = new RelayCommand<object?>(_ => { IsRecharging = false; SelectedWallet = null; });
        DeleteWalletCommand = new RelayCommand<CustomerWallet>(DeleteWalletAsync);

        LoadDataFromDatabase();
    }

    private void OpenCreateForm()
    {
        SelectedWallet = new CustomerWallet { Phone = string.Empty, CustomerName = string.Empty, Email = string.Empty };
        RechargeAmount = 0;
        IsNewWallet = true;
        IsRecharging = true;
    }

    private void OpenRechargeForm(CustomerWallet? wallet)
    {
        if (wallet == null) return;
        SelectedWallet = new CustomerWallet
        {
            Phone = wallet.Phone,
            CustomerName = wallet.CustomerName,
            Email = wallet.Email,
            Balance = wallet.Balance,
            LoyaltyPoints = wallet.LoyaltyPoints,
            TotalSpent = wallet.TotalSpent
        };
        RechargeAmount = 0;
        IsNewWallet = false;
        IsRecharging = true;
    }

    private async void SaveWalletActionAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedWallet?.Phone))
        {
            await DialogService.ShowErrorAsync("Le numéro de téléphone est obligatoire.", "Erreur");
            return;
        }
        if (RechargeAmount < 0)
        {
            await DialogService.ShowErrorAsync("Le montant ne peut pas être négatif.", "Erreur");
            return;
        }

        SelectedWallet.CustomerName ??= string.Empty;
        SelectedWallet.Email ??= string.Empty;

        using (var context = new PosDbContext())
        {
            if (IsNewWallet)
            {
                if (context.CustomerWallets.Any(w => w.Phone == SelectedWallet.Phone))
                {
                    await DialogService.ShowErrorAsync("Ce portefeuille existe déjà.", "Erreur");
                    return;
                }
                SelectedWallet.Balance = RechargeAmount;
                context.CustomerWallets.Add(SelectedWallet);
                if (RechargeAmount > 0)
                {
                    context.WalletTransactions.Add(new WalletTransaction
                    {
                        Phone = SelectedWallet.Phone,
                        TransNo = DateTime.Now.ToString("yyyyMMddHHmmss"),
                        Type = "Credit",
                        Amount = RechargeAmount,
                        CreatedAt = DateTime.Now
                    });
                }
                context.SaveChanges();
                await DialogService.ShowInfoAsync("Portefeuille créé avec succès.", "Succès");
            }
            else
            {
                if (RechargeAmount <= 0)
                {
                    await DialogService.ShowErrorAsync("Le montant de la recharge doit être supérieur à zéro.", "Erreur");
                    return;
                }
                var dbWallet = context.CustomerWallets.FirstOrDefault(w => w.Phone == SelectedWallet.Phone);
                if (dbWallet != null)
                {
                    dbWallet.Balance += RechargeAmount;
                    dbWallet.CustomerName = SelectedWallet.CustomerName;
                    dbWallet.Email = SelectedWallet.Email;
                    context.WalletTransactions.Add(new WalletTransaction
                    {
                        Phone = dbWallet.Phone,
                        TransNo = DateTime.Now.ToString("yyyyMMddHHmmss"),
                        Type = "Credit",
                        Amount = RechargeAmount,
                        CreatedAt = DateTime.Now
                    });
                    context.SaveChanges();
                    await DialogService.ShowInfoAsync($"Recharge de {RechargeAmount:N0} FCFA effectuée avec succès sur le compte {dbWallet.Phone}.", "Succès");
                }
            }
        }

        IsRecharging = false;
        LoadDataFromDatabase();
    }

    private async void DeleteWalletAsync(CustomerWallet? wallet)
    {
        if (wallet == null) return;
        if (!await DialogService.ConfirmAsync($"Êtes-vous sûr de vouloir supprimer définitivement le portefeuille {wallet.Phone} ?", "Confirmation"))
            return;

        using (var context = new PosDbContext())
        {
            var walletToDelete = context.CustomerWallets.FirstOrDefault(w => w.Phone == wallet.Phone);
            if (walletToDelete != null)
            {
                context.CustomerWallets.Remove(walletToDelete);
                context.WalletTransactions.RemoveRange(context.WalletTransactions.Where(t => t.Phone == wallet.Phone).ToList());
                context.SaveChanges();
            }
        }
        LoadDataFromDatabase();
        await DialogService.ShowInfoAsync("Portefeuille supprimé avec succès.", "Suppression");
    }

    public void LoadDataFromDatabase()
    {
        using var context = new PosDbContext();
        Wallets.Clear();
        foreach (var w in context.CustomerWallets.ToList()) Wallets.Add(w);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
