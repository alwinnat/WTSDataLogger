using CsvHelper;
using System.Globalization;

namespace WTSDataLogger.Core
{
    public sealed class CsvWeightWriter : IWeightWriter
    {
        private readonly FileStream _fileStream;
        private readonly StreamWriter _streamWriter;
        private readonly CsvWriter _csvWriter;

        public CsvWeightWriter(string? csvFileName)
        {
            ArgumentNullException.ThrowIfNull(csvFileName, nameof(csvFileName));

            bool isFileExists = File.Exists(csvFileName);
            string directory = Path.GetDirectoryName(csvFileName)!;

            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            _fileStream = new FileStream(csvFileName, FileMode.Append);
            _streamWriter = new StreamWriter(_fileStream);
            _csvWriter = new CsvWriter(_streamWriter, new(CultureInfo.InvariantCulture) { Delimiter = ";" });
            _csvWriter.Context.RegisterClassMap<WeightItem.CsvMap>();

            if (!isFileExists)
            {
                _csvWriter.WriteHeader<WeightItem>();
                _csvWriter.NextRecord();
            }
        }

        ~CsvWeightWriter()
        {
            Close();
        }

        #region IWeightWriter Members
        public void Write(WeightItem item)
        {
            _csvWriter.WriteRecord(item);
            _csvWriter.NextRecord();

            _fileStream.Flush();
            _streamWriter.Flush();
        }

        public void Close() => _fileStream.Close();
        #endregion
    }
}
