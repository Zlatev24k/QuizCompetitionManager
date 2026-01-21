namespace QuizCompetitionManager.Models
{
    public class CompetitionRegistration
    {
        public int Id { get; set; }

        public int CompetitionId { get; set; }
        public int TeamId { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public Competition? Competition { get; set; }
        public Team? Team { get; set; }

        public ICollection<RoundScore> RoundScores { get; set; } = new List<RoundScore>();
    }
}
