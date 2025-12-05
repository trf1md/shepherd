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

        // ⬇⬇⬇ IMPORTANTÍSIMO: ahora son propiedades con SETTER
        private ObservableCollection<MaterialModel> _materials = new();
        public ObservableCollection<MaterialModel> Materials
        {
            get => _materials;
            set => SetProperty(ref _materials, value);
        }

        private ObservableCollection<GroupedMaterialCollection> _groupedMaterials = new();
        public ObservableCollection<GroupedMaterialCollection> GroupedMaterials
        {
            get => _groupedMaterials;
            set => SetProperty(ref _groupedMaterials, value);
        }
        // ⬆⬆⬆ Ahora se pueden reemplazar de golpe → carga ultra rápida


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

        // Grouping control
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

        private bool _canGroup = true;
        public bool CanGroup
        {
            get => _canGroup;
            set => SetProperty(ref _canGroup, value);
        }

        // Statistics
        private int _totalMaterials;
        public int TotalMaterials { get => _totalMaterials; set => SetProperty(ref _totalMaterials, value); }

        private int _standardCount;
        public int StandardCount { get => _standardCount; set => SetProperty(ref _standardCount, value); }

        private int _warningCount;
        public int WarningCount { get => _warningCount; set => SetProperty(ref _warningCount, value); }

        private int _forbiddenCount;
        public int ForbiddenCount { get => _forbiddenCount; set => SetProperty(ref _forbiddenCount, value); }

        private double _standardPercentage;
        public double StandardPercentage { get => _standardPercentage; set => SetProperty(ref _standardPercentage, value); }

        private double _warningPercentage;
        public double WarningPercentage { get => _warningPercentage; set => SetProperty(ref _warningPercentage, value); }

        private double _forbiddenPercentage;
        public double ForbiddenPercentage { get => _forbiddenPercentage; set => SetProperty(ref _forbiddenPercentage, value); }

        // Project information
        private string? _projectNumber;
        public string? ProjectNumber { get => _projectNumber; set => SetProperty(ref _projectNumber, value); }

        private string? _extProject;
        public string? ExtProject { get => _extProject; set => SetProperty(ref _extProject, value); }

        private string? _projectSap;
        public string? ProjectSap { get => _projectSap; set => SetProperty(ref _projectSap, value); }

        // Search
        private string? _searchSap;
        public string? SearchSap { get => _searchSap; set => SetProperty(ref _searchSap, value); }

        private bool _isFiltered;
        public bool IsFiltered { get => _isFiltered; set => SetProperty(ref _isFiltered, value); }

        private string? _searchMessage;
        public string? SearchMessage { get => _searchMessage; set => SetProperty(ref _searchMessage, value); }

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
            GroupByCommand = new RelayCommand<string>(group => GroupBy(group));
            ClearGroupingCommand = new RelayCommand(() => ClearGrouping());

            Debug.WriteLine("[DEBUG] Carga automática al iniciar ViewModel...");
            _ = LoadMaterialsAsync();
        }


        // ⚡⚡⚡ AQUI VIENE LA OPTIMIZACIÓN GORDA ⚡⚡⚡
        private async Task LoadMaterialsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                string eplanFile = @"C:\temp\EPLAN-SAP.txt";
                string excelFile = @"\\md02fs05.emea.bosch.com\ATMO2Storage$\00_Public\37_HW_Eplan\DB\Material_STD.xlsm";
                string apiBaseUrl = "https://md0vm00162.emea.bosch.com/materials/api/";

                var list = await _dataMerge.BuildMaterialListAsync(eplanFile, excelFile, apiBaseUrl);

                _allMaterials = list;

                ExtractProjectInfo(eplanFile);
                CalculateStatistics();

                // 🚀 AQUI LA MAGIA: UNA SOLA ASIGNACIÓN
                Materials = new ObservableCollection<MaterialModel>(list);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
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
                var line = File.ReadLines(eplanFile).FirstOrDefault();
                if (line == null) return;

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return;

                string body = parts[2];
                int d1 = body.IndexOf('.');
                int d2 = body.IndexOf('.', d1 + 1);

                if (d1 > 0 && d2 > d1)
                {
                    ProjectNumber = body[..d2];
                    ExtProject = body[..(d2 + 4)];

                    string tail = body[(d2 + 4)..];
                    if (tail.Length >= 10)
                        ProjectSap = tail[..10];
                }
            }
            catch { }
        }


        private void CalculateStatistics()
        {
            TotalMaterials = _allMaterials.Count;
            StandardCount = _allMaterials.Count(m => m.Status?.Equals("standard", StringComparison.OrdinalIgnoreCase) == true);
            WarningCount = _allMaterials.Count(m => m.Status?.Equals("warning", StringComparison.OrdinalIgnoreCase) == true);
            ForbiddenCount = _allMaterials.Count(m => m.Status?.Equals("forbidden", StringComparison.OrdinalIgnoreCase) == true);

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

            var result = _allMaterials
                .Where(m => m.Sap?.Contains(SearchSap, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            if (!result.Any())
            {
                SearchMessage = $"No se encontró ningún material con SAP '{SearchSap}'.";
                return;
            }

            Materials = new ObservableCollection<MaterialModel>(result);
            IsFiltered = true;
            IsGrouped = false;
            SearchMessage = $"Mostrando {result.Count} coincidencias.";
        }


        private void ClearSearch()
        {
            SearchSap = null;
            SearchMessage = null;
            IsFiltered = false;

            Materials = new ObservableCollection<MaterialModel>(_allMaterials);
            IsGrouped = false;
            CurrentGroupBy = null;
        }


        private void GroupBy(string? prop)
        {
            if (string.IsNullOrWhiteSpace(prop)) return;
            if (!CanGroup) return;

            var target = IsFiltered ? Materials.ToList() : _allMaterials;

            var grouped = prop switch
            {
                "Location" => target.GroupBy(m => m.Location ?? "(Sin Location)"),
                "Group" => target.GroupBy(m => m.Group ?? "(Sin Group)"),
                "Sap" => target.GroupBy(m => m.Sap ?? "(Sin SAP)"),
                "Category" => target.GroupBy(m => m.Category ?? "(Sin Categoría)"),
                "Status" => target.GroupBy(m => m.Status ?? "(Sin Estado)"),
                "Stock" => target.GroupBy(m => m.Stock ?? "(Sin Stock)"),
                "Provider" => target.GroupBy(m => m.Provider ?? "(Sin Proveedor)"),
                _ => null
            };

            if (grouped == null) return;

            GroupedMaterials = new ObservableCollection<GroupedMaterialCollection>(
                grouped.OrderBy(g => g.Key)
                       .Select(g => new GroupedMaterialCollection(
                           g.Key,
                           new ObservableCollection<MaterialModel>(g)))
            );

            IsGrouped = true;
            CurrentGroupBy = prop;
            CanGroup = false;
        }


        private void ClearGrouping()
        {
            GroupedMaterials = new ObservableCollection<GroupedMaterialCollection>();
            IsGrouped = false;
            CurrentGroupBy = null;

            CanGroup = true;
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? n = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(n);
            return true;
        }
    }

    public class GroupedMaterialCollection : ObservableCollection<MaterialModel>
    {
        public string GroupName { get; }

        public GroupedMaterialCollection(string groupName, ObservableCollection<MaterialModel> items)
            : base(items)
        {
            GroupName = groupName;
        }
    }


    // RelayCommand
    public sealed class RelayCommand : ICommand
    {
        private readonly Func<Task>? _async;
        private readonly Action? _sync;

        public RelayCommand(Action sync) => _sync = sync;
        public RelayCommand(Func<Task> async) => _async = async;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? p) => true;

        public async void Execute(object? p)
        {
            if (_async != null) await _async();
            else _sync?.Invoke();
        }
    }

    public sealed class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _exec;
        public RelayCommand(Action<T?> exec) => _exec = exec;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? p) => true;

        public void Execute(object? p) => _exec((T?)p);
    }
}
