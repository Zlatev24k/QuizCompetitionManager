using QuizCompetitionManager.Models;

namespace QuizCompetitionManager.Models.ViewModels
{
    public class AdminCompetitionDetailsVM
    {
        public Competition Competition { get; set; } = null!;
        public List<AdminRankingRowVM> Ranking { get; set; } = new();
    }

    public class AdminRankingRowVM
    {
        public string TeamName { get; set; } = string.Empty;
        public int TotalPoints { get; set; }
    }
}