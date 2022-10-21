using CommandLine;
using WTSDataLogger.Core;

namespace WTSDataSimulator
{
    internal sealed class Program
    {
        static readonly CancellationTokenSource _cancelToken = new();

        static async Task Main(string[] args)
        {
            try
            {
                Console.CancelKeyPress += Console_CancelKeyPress;
                await Parser.Default.ParseArguments<CommandlineOptions>(args).WithParsedAsync(RunAsync);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
            }
        }

        static async Task RunAsync(CommandlineOptions options)
        {
            await Console.Out.WriteLineAsync("Press Ctrl+C or Ctrl+Break to quit.");

            var port = new SerialPort(options);
            var weightTransmitter = new WeightTransmitter(port, new MockWeightWriter());
            var random = new Random();

            port.Open();

            while (!_cancelToken.IsCancellationRequested)
            {
                int count = random.Next(1, 30);
                int grossWeight = random.Next(0, 9999);
                int netWeight = random.Next(0, 9999);

                for (int i = 0; i < count; ++i)
                {
                    weightTransmitter.Send(grossWeight, netWeight);
                    await Task.Delay(options.WriteInterval, _cancelToken.Token);

                    if (_cancelToken.IsCancellationRequested)
                        break;
                }
            }
            port.Close();
        }

        #region Event-Handling Methods
        static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            _cancelToken.Cancel();
            e.Cancel = true;
        }
        #endregion
    }
}