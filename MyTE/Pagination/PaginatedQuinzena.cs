using Microsoft.EntityFrameworkCore;

namespace MyTE.Pagination
{
    public class QuinzenaPaginatedList<T> : List<T>
    {
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public int TotalQuinzenas { get; private set; }

        public QuinzenaPaginatedList(List<T> items, DateTime startDate, DateTime endDate, int totalQuinzenas)
        {
            StartDate = startDate;
            EndDate = endDate;
            TotalQuinzenas = totalQuinzenas;

            this.AddRange(items);
        }

        public bool HasPreviousQuinzena => StartDate > DateTime.MinValue;
        public bool HasNextQuinzena => EndDate < DateTime.MaxValue;

        public static async Task<QuinzenaPaginatedList<T>> CreateAsync(IQueryable<T> source, DateTime initialDate, int quinzenaIndex)
        {
            var quinzenaSize = 15;
            var startDate = initialDate.AddDays(quinzenaIndex * quinzenaSize);
            var endDate = startDate.AddDays(quinzenaSize);

            var items = await source.Where(item => EF.Property<DateTime>(item, "Date") >= startDate && EF.Property<DateTime>(item, "Date") < endDate).ToListAsync();

            var totalDays = (await source.MaxAsync(item => EF.Property<DateTime>(item, "Date")) - initialDate).TotalDays;
            var totalQuinzenas = (int)Math.Ceiling(totalDays / quinzenaSize);

            return new QuinzenaPaginatedList<T>(items, startDate, endDate, totalQuinzenas);
        }
    }
}

