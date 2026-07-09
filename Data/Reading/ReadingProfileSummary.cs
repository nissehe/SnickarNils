namespace Data.Reading
{
    public class ReadingProfileSummary
    {
        public string Name { get; set; }
        public int GoalMinutes { get; set; }
        public int TotalMinutesRead { get; set; }
        public int RemainingMinutes { get; set; }
        public int DailyTargetMinutes { get; set; }
    }
}
