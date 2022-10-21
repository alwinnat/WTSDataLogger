using CsvHelper.Configuration;

namespace WTSDataLogger.Core
{
    public sealed class WeightItem
    {
        public DateTime CreateDate { get; set; }
        public double Net { get; set; }
        public double Gross { get; set; }
        public bool IsChecksumValid { get; set; }

        #region Subclass Definition
        internal sealed class CsvMap : ClassMap<WeightItem>
        {
            public CsvMap()
            {
                Map(m => m.CreateDate).Name("Create Date");
                Map(m => m.Net).Name("Net Weight");
                Map(m => m.Gross).Name("Gross Weight");
                Map(m => m.IsChecksumValid).Name("Checksum Valid");
                Map(m => m.CreateDate).TypeConverterOption.Format("o");
                Map(m => m.IsChecksumValid).TypeConverterOption.BooleanValues(true, true, "1");
                Map(m => m.IsChecksumValid).TypeConverterOption.BooleanValues(false, true, "0");
            }
        }
        #endregion
    }
}
