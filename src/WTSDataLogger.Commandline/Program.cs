using CommandLine;
using WTSDataLogger.Core;

namespace WTSDataLogger.Commandline
{
    internal sealed class Program
    {
        static readonly ManualResetEvent _quitEvent = new(false);

        static void Main(string[] args)
        {
            try
            {
                Console.CancelKeyPress += Console_CancelKeyPress;
                Parser.Default.ParseArguments<CommandlineOptions>(args).WithParsed(Run);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        static void Run(CommandlineOptions options)
        {
            Console.WriteLine("Press Ctrl+C or Ctrl+Break to quit.");

            var port = new SerialPort(options);
            var writer = new CsvWeightWriter(options.OutputFileName);
            var weightTransmitter = new WeightTransmitter(port, writer);

            weightTransmitter.DataReceived += WeightTransmitter_DataReceived;
            weightTransmitter.DataError += WeightTransmitter_DataError;
            weightTransmitter.WriteError += WeightTransmitter_WriteError;

            port.Open();
            weightTransmitter.BeginRecording();

            _quitEvent.WaitOne();

            weightTransmitter.StopRecoding();
            writer.Close();
            port.Close();
        }

        #region Event-Handling Methods
        static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            _quitEvent.Set();
            e.Cancel = true;
        }

        static void WeightTransmitter_DataReceived(object? sender, DataReceivedEventArgs e)
        {
            var text = $"\rCurrent Weight: [net] {e.Data.Net}  [gross] {e.Data.Gross}  ({e.ParsedDataCount} items received)";
            Console.Write(text.PadRight(Console.WindowWidth));
        }

        static void WeightTransmitter_DataError(object? sender, DataErrorEventArgs e)
        {
            Console.Error.WriteLine("ERROR: '{0}' could not be parsed.", e.InvalidData);
        }

        static void WeightTransmitter_WriteError(object? sender, WriteErrorEventArgs e)
        {
            Console.Error.WriteLine("ERROR: {0}", e.Error.Message);
        }
        #endregion
    }
}