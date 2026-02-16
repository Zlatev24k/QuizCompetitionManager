using QuizCompetitionManager.Models;

namespace QuizCompetitionManager.Models.ViewModels
{
    public class MyResultsVM
    {
        public string TeamName { get; set; } = string.Empty;
        public List<MyCompetitionResultVM> Competitions { get; set; } = new();
    }

    public class MyCompetitionResultVM
    {
        public int CompetitionId { get; set; }
        public string CompetitionName { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public CompetitionStatus Status { get; set; }
        public int RoundsCount { get; set; }

        public List<RoundResultVM> Rounds { get; set; } = new();
        public int TotalPoints { get; set; }

        public int? Position { get; set; } 
        public int TeamsCount { get; set; } 
    }

    public class RoundResultVM
    {
        public int RoundNumber { get; set; }
        public int Points { get; set; }
    }
}
