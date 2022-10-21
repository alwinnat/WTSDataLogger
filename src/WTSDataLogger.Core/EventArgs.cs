namespace WTSDataLogger.Core
{
    public sealed class DataReceivedEventArgs : EventArgs
    {
        public DataReceivedEventArgs(WeightItem data, int parsedDataCount)
        {
            Data = data;
            ParsedDataCount = parsedDataCount;
        }

        public WeightItem Data { get; }
        public int ParsedDataCount { get; }
    }

    public sealed class DataErrorEventArgs : EventArgs
    {
        public DataErrorEventArgs(string invalidData)
        {
            InvalidData = invalidData;
        }

        public string InvalidData { get; }
    }

    public sealed class WriteErrorEventArgs : EventArgs
    {
        public WriteErrorEventArgs(Exception error)
        {
            Error = error;
        }

        public Exception Error { get; }
    }
}
