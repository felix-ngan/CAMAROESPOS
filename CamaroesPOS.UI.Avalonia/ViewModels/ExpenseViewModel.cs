using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CamaroesPOS.Data;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.UI.Avalonia.Services;

namespace CamaroesPOS.UI.Avalonia.ViewModels;

public class ExpenseViewModel : INotifyPropertyChanged
{
    public ObservableCollection<Expense> Expenses { get; } = new();
    public ObservableCollection<string> Categories { get; } = new() { "Électricité", "Loyer", "Salaires", "Transport", "Marketing", "Équipement", "Autre" };

    private DateTime? _startDate;
    public DateTime? StartDate { get => _startDate; set { _startDate = value; OnPropertyChanged(); } }

    private DateTime? _endDate;
    public DateTime? EndDate { get => _endDate; set { _endDate = value; OnPropertyChanged(); } }

    private Expense? _selectedExpense;
    public Expense? SelectedExpense { get => _selectedExpense; set { _selectedExpense = value; OnPropertyChanged(); } }

    private bool _isEditing;
    public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); } }

    private bool _isNewExpense;
    public bool IsNewExpense { get => _isNewExpense; set { _isNewExpense = value; OnPropertyChanged(); } }

    private decimal _totalExpenses;
    public decimal TotalExpenses { get => _totalExpenses; set { _totalExpenses = value; OnPropertyChanged(); } }

    public bool IsAdmin => SessionService.IsAdmin;

    public RelayCommand<object?> CreateCommand { get; }
    public RelayCommand<Expense> EditCommand { get; }
    public RelayCommand<Expense> DeleteCommand { get; }
    public RelayCommand<object?> SaveCommand { get; }
    public RelayCommand<object?> CancelCommand { get; }
    public RelayCommand<object?> FilterCommand { get; }
    public RelayCommand<object?> ResetFilterCommand { get; }

    public ExpenseViewModel()
    {
        CreateCommand = new RelayCommand<object?>(_ =>
        {
            if (!IsAdmin) return;
            SelectedExpense = new Expense { Date = DateTime.Now, Category = "Autre", Description = string.Empty, AddedBy = SessionService.CurrentUserName ?? "Admin" };
            IsEditing = true;
            IsNewExpense = true;
        });

        EditCommand = new RelayCommand<Expense>(e =>
        {
            if (!IsAdmin || e == null) return;
            SelectedExpense = new Expense
            {
                Id = e.Id,
                Date = e.Date,
                Category = e.Category,
                Description = e.Description,
                Amount = e.Amount,
                AddedBy = e.AddedBy
            };
            IsEditing = true;
            IsNewExpense = false;
        });

        DeleteCommand = new RelayCommand<Expense>(DeleteExpenseAsync);
        SaveCommand = new RelayCommand<object?>(_ => SaveExpenseAsync());
        CancelCommand = new RelayCommand<object?>(_ => { IsEditing = false; SelectedExpense = null; });
        FilterCommand = new RelayCommand<object?>(_ => LoadExpenses());
        ResetFilterCommand = new RelayCommand<object?>(_ =>
        {
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate = DateTime.Now.Date;
            LoadExpenses();
        });

        StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        EndDate = DateTime.Now.Date;
        LoadExpenses();
    }

    public void LoadExpenses()
    {
        using var context = new PosDbContext();
        var query = context.Expenses.AsQueryable();
        if (StartDate.HasValue) query = query.Where(e => e.Date >= StartDate.Value.Date);
        if (EndDate.HasValue)
        {
            var end = EndDate.Value.Date.AddDays(1);
            query = query.Where(e => e.Date < end);
        }

        var list = query.OrderByDescending(e => e.Date).ToList();
        Expenses.Clear();
        foreach (var item in list) Expenses.Add(item);
        TotalExpenses = list.Sum(e => e.Amount);
    }

    private async void SaveExpenseAsync()
    {
        if (!IsAdmin)
        {
            await DialogService.ShowErrorAsync("Seul l'administrateur peut modifier ou ajouter des dépenses.", "Accès Refusé");
            return;
        }
        if (SelectedExpense == null || string.IsNullOrWhiteSpace(SelectedExpense.Category) || SelectedExpense.Amount <= 0)
        {
            await DialogService.ShowErrorAsync("Veuillez renseigner une catégorie et un montant supérieur à zéro.", "Erreur");
            return;
        }

        SelectedExpense.Description ??= string.Empty;
        SelectedExpense.AddedBy ??= SessionService.CurrentUserName ?? "Admin";

        using (var context = new PosDbContext())
        {
            if (IsNewExpense)
            {
                context.Expenses.Add(SelectedExpense);
            }
            else
            {
                var existing = context.Expenses.FirstOrDefault(e => e.Id == SelectedExpense.Id);
                if (existing != null)
                {
                    existing.Date = SelectedExpense.Date;
                    existing.Category = SelectedExpense.Category;
                    existing.Description = SelectedExpense.Description;
                    existing.Amount = SelectedExpense.Amount;
                }
            }
            context.SaveChanges();
        }

        IsEditing = false;
        LoadExpenses();
    }

    private async void DeleteExpenseAsync(Expense? expense)
    {
        if (!IsAdmin)
        {
            await DialogService.ShowErrorAsync("Seul l'administrateur peut supprimer une dépense.", "Accès Refusé");
            return;
        }
        if (expense == null) return;
        if (!await DialogService.ConfirmAsync($"Êtes-vous sûr de vouloir supprimer la dépense de {expense.Amount:N0} FCFA ?", "Confirmation"))
            return;

        using (var context = new PosDbContext())
        {
            var e = context.Expenses.FirstOrDefault(x => x.Id == expense.Id);
            if (e != null)
            {
                context.Expenses.Remove(e);
                context.SaveChanges();
            }
        }
        LoadExpenses();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
