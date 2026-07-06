using System.Collections.Generic;

namespace Data.Reading
{
    public class ReadingProfile : ReadingProfileSummary
    {
        public List<ReadingLogEntry> Entries { get; set; }
        public List<ReadingBook> Books { get; set; }

        public ReadingProfile()
        {
        }

        public ReadingProfile(ReadingProfileSummary summary, List<ReadingLogEntry> entries, List<ReadingBook> books)
        {
            Name = summary.Name;
            GoalMinutes = summary.GoalMinutes;
            TotalMinutesRead = summary.TotalMinutesRead;
            RemainingMinutes = summary.RemainingMinutes;
            Entries = entries;
            Books = books;
        }
    }
}
