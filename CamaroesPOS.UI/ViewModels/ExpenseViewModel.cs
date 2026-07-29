using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using CamaroesPOS.Data;
using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.UI.ViewModels
{
    public class ExpenseViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Expense> Expenses { get; set; }
        public ICommand CreateCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand ResetFilterCommand { get; }

        private DateTime? _startDate;
        public DateTime? StartDate { get => _startDate; set { _startDate = value; OnPropertyChanged(); } }

        private DateTime? _endDate;
        public DateTime? EndDate { get => _endDate; set { _endDate = value; OnPropertyChanged(); } }

        private Expense _selectedExpense;
        public Expense SelectedExpense
        {
            get => _selectedExpense;
            set { _selectedExpense = value; OnPropertyChanged(); }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }

        private bool _isNewExpense;
        public bool IsNewExpense
        {
            get => _isNewExpense;
            set { _isNewExpense = value; OnPropertyChanged(); }
        }

        private double _totalExpenses;
        public double TotalExpenses
        {
            get => _totalExpenses;
            set { _totalExpenses = value; OnPropertyChanged(); }
        }

        public bool IsAdmin => System.Windows.Application.Current.Properties["CurrentUserRole"]?.ToString() == "Admin";

        public ObservableCollection<string> Categories { get; set; }

        public ExpenseViewModel()
        {
            Expenses = new ObservableCollection<Expense>();
            Categories = new ObservableCollection<string> { "Électricité", "Loyer", "Salaires", "Transport", "Marketing", "Équipement", "Autre" };

            CreateCommand = new RelayCommand<object>(o => {
                SelectedExpense = new Expense { Date = DateTime.Now };
                IsEditing = true;
                IsNewExpense = true;
            });

            EditCommand = new RelayCommand<Expense>(e => {
                if (e != null)
                {
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
                }
            });

            DeleteCommand = new RelayCommand<Expense>(DeleteExpense);
            SaveCommand = new RelayCommand<object>(SaveExpense, CanSave);
            CancelCommand = new RelayCommand<object>(o => { IsEditing = false; SelectedExpense = null; });
            
            FilterCommand = new RelayCommand<object>(o => LoadExpenses());
            ResetFilterCommand = new RelayCommand<object>(o => {
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
            using (var context = new PosDbContext())
            {
                var query = context.Expenses.AsQueryable();

                if (StartDate.HasValue)
                    query = query.Where(e => e.Date >= StartDate.Value.Date);
                
                if (EndDate.HasValue)
                {
                    var end = EndDate.Value.Date.AddDays(1);
                    query = query.Where(e => e.Date < end);
                }

                var list = query.OrderByDescending(e => e.Date).ToList();
                Expenses.Clear();
                foreach(var item in list)
                    Expenses.Add(item);

                TotalExpenses = list.Sum(e => e.Amount);
            }
        }

        private bool CanSave(object obj)
        {
            return SelectedExpense != null && !string.IsNullOrWhiteSpace(SelectedExpense.Category) && SelectedExpense.Amount > 0;
        }

        private void SaveExpense(object obj)
        {
            if (!IsAdmin)
            {
                MessageBox.Show("Seul l'administrateur peut modifier ou ajouter des dépenses.", "Accès Refusé", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new PosDbContext())
            {
                if (IsNewExpense)
                {
                    SelectedExpense.AddedBy = System.Windows.Application.Current.Properties["CurrentUserName"]?.ToString() ?? "Admin";
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

        private void DeleteExpense(Expense expense)
        {
            if (!IsAdmin)
            {
                MessageBox.Show("Seul l'administrateur peut supprimer une dépense.", "Accès Refusé", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (expense == null) return;

            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer la dépense de {expense.Amount} FCFA ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
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
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
