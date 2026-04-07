using QuizCompetitionManager.Models.ViewModels;

namespace QuizCompetitionManager.Services.Interfaces
{
    public interface IResultsService
    {
        Task<ServiceResult<MyResultsVM>> GetMyResultsAsync(string userId);
    }
}