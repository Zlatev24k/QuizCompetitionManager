using QuizCompetitionManager.Models.ViewModels;

namespace QuizCompetitionManager.Services.Interfaces
{
    public interface IHomeService
    {
        Task<PublicCompetitionsVM> GetPublicCompetitionsAsync(bool canJoinOrUnjoin, string? userId);
        Task<CompetitionDetailsVM?> GetCompetitionDetailsAsync(int competitionId);
    }
}