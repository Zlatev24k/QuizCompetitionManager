namespace QuizCompetitionManager.Models.ViewModels
{
    public class PublicCompetitionsVM
    {
        public List<Competition> Competitions { get; set; } = new();
        public HashSet<int> JoinedCompetitionIds { get; set; } = new();
        public bool CanJoinOrUnjoin { get; set; }
    }
}
