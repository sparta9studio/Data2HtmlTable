using DataPreview.DataSource;

namespace DataPreview
{
    public class DataPreviewOptions
    {
        public string MapPath { get; set; }

        public IDataSource DataSource { get; set; }

        public int MaxRow { get; set; } = 50;

        public int MaxStringLength { get; set; } = 50;
    }
}