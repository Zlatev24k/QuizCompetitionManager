using QuizCompetitionManager.Models;

namespace QuizCompetitionManager.Models.ViewModels
{
    public class CompetitionManageVM
    {
        public int CompetitionId { get; set; }
        public string CompetitionName { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public CompetitionStatus Status { get; set; }
        public int RoundsCount { get; set; }

        public List<TeamScoreRowVM> Teams { get; set; } = new();
    }

    public class TeamScoreRowVM
    {
        public int RegistrationId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public List<string> Members { get; set; } = new();

        public List<RoundPointVM> RoundPoints { get; set; } = new();
    }

    public class RoundPointVM
    {
        public int RoundNumber { get; set; }
        public int Points { get; set; }
    }
}

