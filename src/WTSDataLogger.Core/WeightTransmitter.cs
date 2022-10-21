using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace WTSDataLogger.Core
{
    public sealed class WeightTransmitter : IDisposable
    {
        private readonly ISerialPort _serialPort;
        private readonly IWeightWriter _writer;
        private readonly StringBuilder _data = new();
        private readonly Spooler<string> _receivedDataBuffer = new();
        private readonly Spooler<WeightItem> _writeDataBuffer = new();
        private readonly Regex _regex = new(NET_WEIGHT_MARKER + "(?<netWeight>.+)" + GROSS_WEIGHT_MARKER + "(?<grossWeight>.+)\\\\(?<checksum>.{2})");

        private bool _isTransmitting;
        private int _counter;

        private const char START_SEQUENCE = '&';
        private const char NET_WEIGHT_MARKER = 'N';
        private const char GROSS_WEIGHT_MARKER = 'L';
        private const char STOP_SEQUENCE = '\r';

        public WeightTransmitter(ISerialPort serialPort, IWeightWriter writer)
        {
            _serialPort = serialPort ?? throw new ArgumentNullException(nameof(serialPort));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));

            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        ~WeightTransmitter()
        {
            Dispose();
        }

        #region Public API
        public event EventHandler<DataReceivedEventArgs>? DataReceived;
        public event EventHandler<DataErrorEventArgs>? DataError;
        public event EventHandler<WriteErrorEventArgs>? WriteError;

        public void BeginRecording()
        {
            _receivedDataBuffer.BeginStart(Parse);
            _writeDataBuffer.BeginStart(Write);
        }

        public void StopRecoding()
        {
            _receivedDataBuffer.Stop();
            _writeDataBuffer.Stop();
        }

        public void Send(int grossWeight, int netWeight)
        {
            var output = $"{NET_WEIGHT_MARKER}{grossWeight:000000}{GROSS_WEIGHT_MARKER}{netWeight:000000}";
            var checksum = ComputeChecksum(output);

            _serialPort.Write($"{START_SEQUENCE}{output}\\{checksum:X2}{STOP_SEQUENCE}\n");
        }
        #endregion

        #region IDisposable Member
        public void Dispose()
        {
            _serialPort.DataReceived -= SerialPort_DataReceived;
        }
        #endregion

        #region Event-Handling Methods
        private void SerialPort_DataReceived(object? sender, EventArgs e) => _receivedDataBuffer.Add(_serialPort.ReadExisting());
        #endregion

        private void Parse(string data)
        {
            foreach (char c in data)
            {
                if (c == START_SEQUENCE)
                {
                    _isTransmitting = true;
                    continue;
                }
                if (_isTransmitting)
                {
                    if (c == STOP_SEQUENCE)
                    {
                        var match = _regex.Match(_data.ToString());

                        if (match.Success)
                        {
                            var strNetWeight = match.Groups["netWeight"].Value;
                            var strGrossWeight = match.Groups["grossWeight"].Value;
                            var checksum = ComputeChecksum($"{NET_WEIGHT_MARKER}{strNetWeight}{GROSS_WEIGHT_MARKER}{strGrossWeight}");

                            var item = new WeightItem
                            {
                                CreateDate = DateTime.Now,
                                Net = Convert.ToDouble(strNetWeight, CultureInfo.InvariantCulture),
                                Gross = Convert.ToDouble(strGrossWeight, CultureInfo.InvariantCulture),
                                IsChecksumValid = checksum.ToString("X2").Equals(match.Groups["checksum"].Value)
                            };
                            _writeDataBuffer.Add(item);

                            DataReceived?.Invoke(this, new DataReceivedEventArgs(item, _counter++));
                        }
                        else
                        {
                            DataError?.Invoke(this, new DataErrorEventArgs(_data.ToString()));
                        }
                        _data.Clear();
                        _isTransmitting = false;
                        continue;
                    }
                    _data.Append(c);
                }
            }
        }

        private void Write(WeightItem item)
        {
            try
            {
                _writer.Write(item);
            }
            catch (Exception ex)
            {
                WriteError?.Invoke(this, new WriteErrorEventArgs(ex));
            }
        }

        private int ComputeChecksum(string input)
        {
            var inputBytes = Encoding.Default.GetBytes(input);
            var checksum = inputBytes[0];

            for (int i = 1; i < inputBytes.Length; ++i)
                checksum ^= inputBytes[i];

            return checksum;
        }
    }
}
