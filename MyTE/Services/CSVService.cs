using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;

namespace MyTE.Services
{
    public class CSVService : ICSVInterface
    {
        /// <summary>
        /// Lê um arquivo CSV e retorna uma lista de registros.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="file"></param>
        /// <returns></returns>
        public IEnumerable<T> ReadCSV<T>(Stream file)
        {
            var reader = new StreamReader(file);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<T>();
            return records;
        }


        /// <summary>
        /// Retorna um arquivo CSV com formatação UTF-8, com base em uma Lista de registros e uma Lista dos nomes do cabeçalho.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records">Lista dos elementos do arquivo csv</param>
        /// <param name="columnNames">Lista dos nomes do cabeçalho</param>
        public byte[] WriteCSV<T>(List<T> records, List<string> columnNames)
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {

                // Escrever o cabeçalho do arquivo CSV.
                if (columnNames != null && columnNames.Count > 0)
                {
                    var header = new List<string>();
                    foreach (var columnName in columnNames)
                    {
                        header.Add(columnName);
                    }
                    csv.WriteField(header);

                    csv.NextRecord();
                }
                // Escrever os registros no arquivo CSV, menos o cabeçalho.
                foreach (var record in records)
                {
                    csv.WriteRecord(record);
                    csv.NextRecord();
                }
                
                // Limpar o buffer de gravação e posicionar o ponteiro no início do arquivo.
                csv.Flush();
                memoryStream.Position = 0;

                return memoryStream.ToArray();
            }
        }
    }
}
