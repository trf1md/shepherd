using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ShepherdEplan.Models;
using ShepherdEplan.Services.Merge;

namespace ShepherdEplan.ViewModels
{
    public sealed class MaterialsViewModel : INotifyPropertyChanged
    {
        private readonly DataMergeService _dataMerge;

        private int _buttonPressCount;
        public int ButtonPressCount
        {
            get => _buttonPressCount;
            set => SetProperty(ref _buttonPressCount, value);
        }

        public ObservableCollection<MaterialModel> Materials { get; }
            = new ObservableCollection<MaterialModel>();

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

        public ICommand LoadCommand { get; }

        public MaterialsViewModel(DataMergeService dataMerge)
        {
            _dataMerge = dataMerge;

            LoadCommand = new RelayCommand(async () =>
            {
                ButtonPressCount++;
                Debug.WriteLine($"[DEBUG] BOTÓN PULSADO → total: {ButtonPressCount}");
                await LoadMaterialsAsync();
            });

            // Auto-load on startup - properly marshalled to UI thread
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

                Debug.WriteLine("[DEBUG] Iniciando carga de datos...");

                string eplanFile = @"C:\temp\EPLAN-SAP.txt";
                string excelFile = @"\\md02fs05.emea.bosch.com\ATMO2Storage$\00_Public\37_HW_Eplan\DB\Material_STD.xlsm";
                string apiBaseUrl = "https://md0vm00162.emea.bosch.com/materials/api/";

                Debug.WriteLine("[DEBUG] Ejecutando _dataMerge.BuildMaterialListAsync()...");

                var list = await _dataMerge.BuildMaterialListAsync(eplanFile, excelFile, apiBaseUrl);

                Debug.WriteLine($"[DEBUG] Materiales obtenidos: {list.Count}");

                // Ensure we're on the UI thread when updating ObservableCollection
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var item in list)
                        Materials.Add(item);

                    Debug.WriteLine("[DEBUG] Materiales añadidos al ObservableCollection.");
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
                Debug.WriteLine("[DEBUG] IsBusy = false");
            }
        }

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
}