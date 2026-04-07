using QuizCompetitionManager.Models;

namespace QuizCompetitionManager.Services.Interfaces
{
    public interface ITeamService
    {
        Task<Team?> GetUserTeamAsync(string userId);
        Task<ServiceResult> CreateTeamAsync(string userId, string name);
        Task<ServiceResult> AddMemberAsync(string userId, string fullName);
        Task<ServiceResult> RemoveMemberAsync(string userId, int memberId);
        Task<ServiceResult<Team>> GetTeamForEditAsync(string userId);
        Task<ServiceResult> EditTeamNameAsync(string userId, int teamId, string name);
    }
}