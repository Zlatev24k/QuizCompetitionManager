using Microsoft.EntityFrameworkCore;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Helpers;
using QuizCompetitionManager.Models;
using QuizCompetitionManager.Models.ViewModels;
using QuizCompetitionManager.Services.Interfaces;

namespace QuizCompetitionManager.Services
{
    public class HomeService : IHomeService
    {
        private readonly ApplicationDbContext _db;

        public HomeService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PublicCompetitionsVM> GetPublicCompetitionsAsync(bool canJoinOrUnjoin, string? userId)
        {
            var comps = await _db.Competitions
                .OrderBy(c => c.Status)
                .ThenByDescending(c => c.StartDateTime)
                .ToListAsync();

            var vm = new PublicCompetitionsVM
            {
                Competitions = comps,
                CanJoinOrUnjoin = canJoinOrUnjoin
            };

            if (canJoinOrUnjoin && !string.IsNullOrEmpty(userId))
            {
                var team = await _db.Teams
                    .FirstOrDefaultAsync(t => t.OwnerUserId == userId);

                if (team != null)
                {
                    var joined = await _db.CompetitionRegistrations
                        .Where(r => r.TeamId == team.Id)
                        .Select(r => r.CompetitionId)
                        .ToListAsync();

                    vm.JoinedCompetitionIds = joined.ToHashSet();
                }
            }

            return vm;
        }

        public async Task<CompetitionDetailsVM?> GetCompetitionDetailsAsync(int competitionId)
        {
            var comp = await _db.Competitions
                .FirstOrDefaultAsync(c => c.Id == competitionId);

            if (comp == null)
                return null;

            var vm = new CompetitionDetailsVM
            {
                Competition = comp
            };

            if (comp.Status != CompetitionStatus.Planned)
            {
                var regs = await _db.CompetitionRegistrations
                    .Where(r => r.CompetitionId == competitionId)
                    .Include(r => r.Team)
                    .Include(r => r.RoundScores)
                    .ToListAsync();

                var ranked = RankingHelper.BuildRanking(regs, comp.RoundsCount);

                vm.Ranking = ranked
                    .Select(r => new RankingRowVM
                    {
                        TeamName = r.TeamName,
                        TotalPoints = r.TotalPoints
                    })
                    .ToList();
            }

            return vm;
        }
    }
}