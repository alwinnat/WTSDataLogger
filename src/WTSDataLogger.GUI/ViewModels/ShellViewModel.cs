using Caliburn.Micro;
using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WTSDataLogger.Core;
using WTSDataLogger.GUI.Properties;

namespace WTSDataLogger.GUI.ViewModels
{
    internal sealed class ShellViewModel : Screen
    {
        private readonly ISerialPort _port;
        private readonly StringBuilder _errors;
        private CsvWeightWriter? _writer;
        private WeightTransmitter? _weightTransmitter;
        private BindableCollection<string> _availablePorts;
        private double? _netWeight, _grossWeight;
        private int? _parsedCount;
        private bool _isRecording;

        public ShellViewModel(ISerialPort port)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
            _errors = new();
            _availablePorts = new(port.GetPortNames());
        }

        public string Title
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version ?? new(1, 0, 0);
                return $"WTS Data Logger {version.Major}.{version.Minor}.{version.Build}";
            }
        }

        public string? Errors
        {
            get => _errors.ToString();
            set
            {
                if (string.IsNullOrEmpty(value))
                    _errors.Clear();
                else
                    _errors.AppendLine(value);

                NotifyOfPropertyChange(() => Errors);
            }
        }

        public BindableCollection<string> AvailablePorts
        {
            get => _availablePorts;
            set
            {
                _availablePorts = value;
                NotifyOfPropertyChange(() => AvailablePorts);
            }
        }

        public string? SelectedPort
        {
            get => Settings.Default.PortName;
            set
            {
                Settings.Default.PortName = value;
                NotifyOfPropertyChange(() => SelectedPort);
            }
        }

        public BindableCollection<int> AvailableDataBits => new(new[] { 5, 6, 7, 8 });

        public int BaudRate
        {
            get => Settings.Default.BaudRate;
            set
            {
                Settings.Default.BaudRate = value;
                NotifyOfPropertyChange(() => BaudRate);
            }
        }

        public int SelectedDataBits
        {
            get => Settings.Default.DataBits;
            set
            {
                Settings.Default.DataBits = value;
                NotifyOfPropertyChange(() => SelectedDataBits);
            }
        }

        public Parity SelectedParity
        {
            get => Settings.Default.Parity;
            set
            {
                Settings.Default.Parity = value;
                NotifyOfPropertyChange(() => SelectedParity);
            }
        }

        public StopBits SelectedStopBits
        {
            get => Settings.Default.StopBits;
            set
            {
                Settings.Default.StopBits = value;
                NotifyOfPropertyChange(() => SelectedStopBits);
            }
        }

        public Handshake SelectedHandshake
        {
            get => Settings.Default.Handshake;
            set
            {
                Settings.Default.Handshake = value;
                NotifyOfPropertyChange(() => SelectedHandshake);
            }
        }

        public int ReadTimeout
        {
            get => Settings.Default.ReadTimeout;
            set
            {
                Settings.Default.ReadTimeout = value;
                NotifyOfPropertyChange(() => ReadTimeout);
            }
        }

        public string? CsvFileName
        {
            get => Settings.Default.CsvFileName;
            set
            {
                Settings.Default.CsvFileName = value;
                NotifyOfPropertyChange(() => CsvFileName);
            }
        }

        public double? NetWeight
        {
            get => _netWeight;
            set
            {
                _netWeight = value;
                NotifyOfPropertyChange(() => NetWeight);
            }
        }

        public double? GrossWeight
        {
            get => _grossWeight;
            set
            {
                _grossWeight = value;
                NotifyOfPropertyChange(() => GrossWeight);
            }
        }

        public int? ParsedCount
        {
            get => _parsedCount;
            set
            {
                _parsedCount = value;
                NotifyOfPropertyChange(() => ParsedCount);
            }
        }

        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                _isRecording = value;

                if (!value)
                {
                    NetWeight = null;
                    GrossWeight = null;
                }
                else
                {
                    Errors = null;
                    ParsedCount = null;
                }
                NotifyOfPropertyChange(() => IsRecording);
            }
        }

        public void RefreshAvailablePorts() => AvailablePorts = new(_port.GetPortNames());

        public void BrowseCsvFileName()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    InitialDirectory = GetPath(),
                    Filter = "Text Files (*.csv)|*.csv",
                    FilterIndex = 0,
                    AddExtension = true
                };
                if (saveFileDialog.ShowDialog() ?? false)
                    CsvFileName = saveFileDialog.FileName;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        public void StartOrStop()
        {
            try
            {
                ThrowExceptionIfInvalidInputs();

                if (IsRecording)
                {
                    if (_weightTransmitter != null)
                    {
                        _weightTransmitter.DataReceived -= WeightTransmitter_DataReceived;
                        _weightTransmitter.DataError -= WeightTransmitter_DataError;
                        _weightTransmitter.WriteError -= WeightTransmitter_WriteError;
                        _weightTransmitter.StopRecoding();
                        _weightTransmitter.Dispose();
                    }
                    _writer?.Close();
                    _port.Close();

                    IsRecording = false;
                    return;
                }
                _port.Initialize(Settings.Default);
                _port.Open();

                _writer = new(CsvFileName);
                _weightTransmitter = new(_port, _writer);

                _weightTransmitter.DataReceived += WeightTransmitter_DataReceived;
                _weightTransmitter.DataError += WeightTransmitter_DataError;
                _weightTransmitter.WriteError += WeightTransmitter_WriteError;
                _weightTransmitter.BeginRecording();

                IsRecording = true;
            }
            catch (Exception ex)
            {
                _writer?.Close();
                _port.Close();

                ShowError(ex.Message);
            }
        }

        #region Event-Handling Methods
        private void WeightTransmitter_DataReceived(object? sender, DataReceivedEventArgs e)
        {
            NetWeight = e.Data.Net;
            GrossWeight = e.Data.Gross;
            ParsedCount = e.ParsedDataCount;
        }

        private void WeightTransmitter_DataError(object? sender, DataErrorEventArgs e) => Errors = $"'{e.InvalidData}' could not be parsed.";
        private void WeightTransmitter_WriteError(object? sender, WriteErrorEventArgs e) => Errors = e.Error.Message;
        #endregion

        #region Overrides
        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            Settings.Default.Save();
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion

        private void ShowError(string text) => MessageBox.Show(text, "Error...", MessageBoxButton.OK, MessageBoxImage.Error);

        private string? GetPath()
        {
            if (!string.IsNullOrEmpty(CsvFileName))
            {
                var directory = Path.GetDirectoryName(CsvFileName);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                return directory;
            }
            return null;
        }

        private void ThrowExceptionIfInvalidInputs()
        {
            if (string.IsNullOrWhiteSpace(CsvFileName))
                throw new NullReferenceException("Filename (*.csv) is empty.");

            if (string.IsNullOrWhiteSpace(SelectedPort))
                throw new NullReferenceException("Port is empty.");

            if (BaudRate <= 0)
                throw new ArgumentException("Baud Rate must be greater than 0.");
        }
    }
}
