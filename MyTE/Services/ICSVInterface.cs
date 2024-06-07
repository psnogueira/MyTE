namespace MyTE.Services
{
    public interface ICSVInterface
    {
        public IEnumerable<T> ReadCSV<T>(Stream file);
        public byte[] WriteCSV<T>(List<T> records, List<string> columnNames);
    }
}
