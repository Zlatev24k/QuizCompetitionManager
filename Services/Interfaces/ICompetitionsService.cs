using QuizCompetitionManager.Models;
using QuizCompetitionManager.Models.ViewModels;

namespace QuizCompetitionManager.Services.Interfaces
{
    public interface ICompetitionsService
    {
        Task<List<Competition>> GetAllAsync();
        Task<Competition> GetCreateModelAsync();
        Task<Competition?> GetByIdAsync(int id);
        Task<AdminCompetitionDetailsVM?> GetDetailsAsync(int id);
        Task CreateAsync(Competition model);
        Task UpdateAsync(Competition model);
        Task DeleteAsync(int id);

        Task<CompetitionManageVM?> GetManageDataAsync(int id);
        Task SaveAllScoresAsync(CompetitionManageVM vm);

        Task<ServiceResult> StartCompetitionAsync(int id);
        Task<ServiceResult> FinishCompetitionAsync(int id);
        Task<ServiceResult> RemoveTeamAsync(int competitionId, int registrationId);
    }
}