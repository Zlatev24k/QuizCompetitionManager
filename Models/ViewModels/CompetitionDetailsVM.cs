using QuizCompetitionManager.Models;

namespace QuizCompetitionManager.Models.ViewModels
{
    public class CompetitionDetailsVM
    {
        public Competition Competition { get; set; } = null!;
        public List<RankingRowVM> Ranking { get; set; } = new();
    }

    public class RankingRowVM
    {
        public string TeamName { get; set; } = string.Empty;
        public int TotalPoints { get; set; }
    }
}