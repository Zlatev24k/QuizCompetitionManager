namespace QuizCompetitionManager.Services.Interfaces
{
    public interface IRegistrationsService
    {
        Task<ServiceResult> JoinCompetitionAsync(int competitionId, string userId);
        Task<ServiceResult> UnjoinCompetitionAsync(int competitionId, string userId);
    }
}