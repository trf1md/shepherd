using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ShepherdEplan.Models;
using ShepherdEplan.Services.Merge;

namespace ShepherdEplan.ViewModels
{
    public sealed class MaterialsViewModel : INotifyPropertyChanged
    {
        private readonly DataMergeService _dataMerge;
        private List<MaterialModel> _allMaterials = new();

        private int _buttonPressCount;
        public int ButtonPressCount
        {
            get => _buttonPressCount;
            set => SetProperty(ref _buttonPressCount, value);
        }

        public ObservableCollection<MaterialModel> Materials { get; }
            = new ObservableCollection<MaterialModel>();

        public ObservableCollection<GroupedMaterialCollection> GroupedMaterials { get; }
            = new ObservableCollection<GroupedMaterialCollection>();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Grouping
        private bool _isGrouped;
        public bool IsGrouped
        {
            get => _isGrouped;
            set => SetProperty(ref _isGrouped, value);
        }

        private string? _currentGroupBy;
        public string? CurrentGroupBy
        {
            get => _currentGroupBy;
            set => SetProperty(ref _currentGroupBy, value);
        }

        // 🔥 Nueva propiedad: control para permitir / bloquear agrupación
        private bool _canGroup = true;
        public bool CanGroup
        {
            get => _canGroup;
            set => SetProperty(ref _canGroup, value);
        }

        // Statistics
        private int _totalMaterials;
        public int TotalMaterials
        {
            get => _totalMaterials;
            set => SetProperty(ref _totalMaterials, value);
        }

        private int _standardCount;
        public int StandardCount
        {
            get => _standardCount;
            set => SetProperty(ref _standardCount, value);
        }

        private int _warningCount;
        public int WarningCount
        {
            get => _warningCount;
            set => SetProperty(ref _warningCount, value);
        }

        private int _forbiddenCount;
        public int ForbiddenCount
        {
            get => _forbiddenCount;
            set => SetProperty(ref _forbiddenCount, value);
        }

        private double _standardPercentage;
        public double StandardPercentage
        {
            get => _standardPercentage;
            set => SetProperty(ref _standardPercentage, value);
        }

        private double _warningPercentage;
        public double WarningPercentage
        {
            get => _warningPercentage;
            set => SetProperty(ref _warningPercentage, value);
        }

        private double _forbiddenPercentage;
        public double ForbiddenPercentage
        {
            get => _forbiddenPercentage;
            set => SetProperty(ref _forbiddenPercentage, value);
        }

        // Project Information
        private string? _projectNumber;
        public string? ProjectNumber
        {
            get => _projectNumber;
            set => SetProperty(ref _projectNumber, value);
        }

        private string? _extProject;
        public string? ExtProject
        {
            get => _extProject;
            set => SetProperty(ref _extProject, value);
        }

        private string? _projectSap;
        public string? ProjectSap
        {
            get => _projectSap;
            set => SetProperty(ref _projectSap, value);
        }

        // Search
        private string? _searchSap;
        public string? SearchSap
        {
            get => _searchSap;
            set => SetProperty(ref _searchSap, value);
        }

        private bool _isFiltered;
        public bool IsFiltered
        {
            get => _isFiltered;
            set => SetProperty(ref _isFiltered, value);
        }

        private string? _searchMessage;
        public string? SearchMessage
        {
            get => _searchMessage;
            set => SetProperty(ref _searchMessage, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand GroupByCommand { get; }
        public ICommand ClearGroupingCommand { get; }

        public MaterialsViewModel(DataMergeService dataMerge)
        {
            _dataMerge = dataMerge;

            LoadCommand = new RelayCommand(async () =>
            {
                ButtonPressCount++;
                Debug.WriteLine($"[DEBUG] BOTÓN PULSADO → total: {ButtonPressCount}");
                await LoadMaterialsAsync();
            });

            SearchCommand = new RelayCommand(() => PerformSearch());
            ClearSearchCommand = new RelayCommand(() => ClearSearch());
            GroupByCommand = new RelayCommand<string>(groupBy => GroupBy(groupBy));
            ClearGroupingCommand = new RelayCommand(() => ClearGrouping());

            Debug.WriteLine("[DEBUG] Carga automática al iniciar ViewModel...");
            _ = LoadMaterialsAsync();
        }

        private async Task LoadMaterialsAsync()
        {
            Debug.WriteLine("[DEBUG] LoadMaterialsAsync() llamado");

            if (IsBusy)
            {
                Debug.WriteLine("[DEBUG] Cancelado: IsBusy = true");
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = null;
                Materials.Clear();
                GroupedMaterials.Clear();

                Debug.WriteLine("[DEBUG] Iniciando carga de datos...");

                string eplanFile = @"C:\temp\EPLAN-SAP.txt";
                string excelFile = @"\\md02fs05.emea.bosch.com\ATMO2Storage$\00_Public\37_HW_Eplan\DB\Material_STD.xlsm";
                string apiBaseUrl = "https://md0vm00162.emea.bosch.com/materials/api/";

                Debug.WriteLine("[DEBUG] Ejecutando _dataMerge.BuildMaterialListAsync()...");

                var list = await _dataMerge.BuildMaterialListAsync(eplanFile, excelFile, apiBaseUrl);

                Debug.WriteLine($"[DEBUG] Materiales obtenidos: {list.Count}");

                _allMaterials = list;

                ExtractProjectInfo(eplanFile);
                CalculateStatistics();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var item in list)
                        Materials.Add(item);
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error cargando materiales: {ex.Message}";
                Debug.WriteLine($"[ERROR] {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ExtractProjectInfo(string eplanFile)
        {
            try
            {
                var firstLine = File.ReadLines(eplanFile).FirstOrDefault(l => !string.IsNullOrWhiteSpace(l));
                if (firstLine == null) return;

                var parts = firstLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return;

                string body = parts[2];

                var firstDot = body.IndexOf('.');
                var secondDot = body.IndexOf('.', firstDot + 1);
                if (firstDot > 0 && secondDot > firstDot)
                {
                    ProjectNumber = body.Substring(0, secondDot);
                    ExtProject = body.Substring(0, secondDot + 4);

                    string afterExt = body.Substring(secondDot + 4);
                    if (afterExt.Length >= 10)
                        ProjectSap = afterExt.Substring(0, 10);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Extracting project info: {ex.Message}");
            }
        }

        private void CalculateStatistics()
        {
            TotalMaterials = _allMaterials.Count;
            StandardCount = _allMaterials.Count(m => m.Status?.ToLower() == "standard");
            WarningCount = _allMaterials.Count(m => m.Status?.ToLower() == "warning");
            ForbiddenCount = _allMaterials.Count(m => m.Status?.ToLower() == "forbidden");

            if (TotalMaterials > 0)
            {
                StandardPercentage = (double)StandardCount / TotalMaterials;
                WarningPercentage = (double)WarningCount / TotalMaterials;
                ForbiddenPercentage = (double)ForbiddenCount / TotalMaterials;
            }
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchSap))
            {
                SearchMessage = "Por favor, introduce un número SAP.";
                return;
            }

            var found = _allMaterials.Where(m =>
                m.Sap?.Contains(SearchSap, StringComparison.OrdinalIgnoreCase) == true).ToList();

            if (!found.Any())
            {
                SearchMessage = $"No se encontró ningún material con SAP '{SearchSap}'.";
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Materials.Clear();
                foreach (var item in found)
                    Materials.Add(item);

                IsFiltered = true;
                IsGrouped = false;
                SearchMessage = $"Mostrando {found.Count} resultado(s) para '{SearchSap}'.";
            });
        }

        private void ClearSearch()
        {
            SearchSap = null;
            SearchMessage = null;
            IsFiltered = false;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Materials.Clear();
                foreach (var item in _allMaterials)
                    Materials.Add(item);

                if (IsGrouped)
                {
                    IsGrouped = false;
                    CurrentGroupBy = null;
                }
            });
        }

        private void GroupBy(string? propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return;

            // 🚫 Si ya está agrupado, no permitir volver a agrupar
            if (!CanGroup)
                return;

            Debug.WriteLine($"[GROUPING] Grouping by: {propertyName}");

            var materialsToGroup = IsFiltered && Materials.Any() ? Materials.ToList() : _allMaterials;

            var grouped = propertyName switch
            {
                "Location" => materialsToGroup.GroupBy(m => m.Location ?? "(Sin Location)"),
                "Group" => materialsToGroup.GroupBy(m => m.Group ?? "(Sin Group)"),
                "Sap" => materialsToGroup.GroupBy(m => m.Sap ?? "(Sin SAP)"),
                "Category" => materialsToGroup.GroupBy(m => m.Category ?? "(Sin Categoría)"),
                "Status" => materialsToGroup.GroupBy(m => m.Status ?? "(Sin Estado)"),
                "Stock" => materialsToGroup.GroupBy(m => m.Stock ?? "(Sin Stock)"),
                "Provider" => materialsToGroup.GroupBy(m => m.Provider ?? "(Sin Proveedor)"),
                _ => null
            };

            if (grouped == null)
                return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                GroupedMaterials.Clear();

                foreach (var group in grouped.OrderBy(g => g.Key))
                {
                    var groupCollection = new GroupedMaterialCollection(group.Key, new ObservableCollection<MaterialModel>(group));
                    GroupedMaterials.Add(groupCollection);
                }

                IsGrouped = true;
                CurrentGroupBy = propertyName;

                // 🔒 Bloquear agrupación hasta que se desagrupa
                CanGroup = false;

                Debug.WriteLine($"[GROUPING] Created {GroupedMaterials.Count} groups");
            });
        }

        private void ClearGrouping()
        {
            IsGrouped = false;
            CurrentGroupBy = null;
            GroupedMaterials.Clear();

            // 🔓 Habilitar de nuevo la agrupación
            CanGroup = true;

            Debug.WriteLine("[GROUPING] Grouping cleared");
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;
            OnPropertyChanged(name);
            return true;
        }
    }

    // Grouped collection class
    public class GroupedMaterialCollection : ObservableCollection<MaterialModel>
    {
        public string GroupName { get; }

        public GroupedMaterialCollection(string groupName, ObservableCollection<MaterialModel> items)
            : base(items)
        {
            GroupName = groupName;
        }
    }

    // RelayCommand with parameter support
    public sealed class RelayCommand : ICommand
    {
        private readonly Func<Task>? _asyncExecute;
        private readonly Action? _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public RelayCommand(Func<Task> asyncExecute, Func<bool>? canExecute = null)
        {
            _asyncExecute = asyncExecute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) =>
            _canExecute?.Invoke() ?? true;

        public async void Execute(object? parameter)
        {
            if (_asyncExecute != null)
                await _asyncExecute();
            else
                _execute?.Invoke();
        }

        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public sealed class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) =>
            _canExecute?.Invoke((T?)parameter) ?? true;

        public void Execute(object? parameter)
        {
            _execute((T?)parameter);
        }

        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
