using Microsoft.EntityFrameworkCore;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Helpers;
using QuizCompetitionManager.Models;
using QuizCompetitionManager.Models.ViewModels;
using QuizCompetitionManager.Services.Interfaces;

namespace QuizCompetitionManager.Services
{
    public class ResultsService : IResultsService
    {
        private readonly ApplicationDbContext _db;

        public ResultsService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ServiceResult<MyResultsVM>> GetMyResultsAsync(string userId)
        {
            var team = await _db.Teams
                .FirstOrDefaultAsync(t => t.OwnerUserId == userId);

            if (team == null)
            {
                return ServiceResult<MyResultsVM>.Fail(
                    "Първо трябва да създадеш отбор.",
                    "MissingTeam");
            }

            var regs = await _db.CompetitionRegistrations
                .Where(r => r.TeamId == team.Id)
                .Include(r => r.Competition)
                .Include(r => r.RoundScores)
                .OrderByDescending(r => r.Competition!.StartDateTime)
                .ToListAsync();

            var vm = new MyResultsVM
            {
                TeamName = team.Name
            };

            foreach (var reg in regs)
            {
                var comp = reg.Competition!;

                var rounds = Enumerable.Range(1, comp.RoundsCount)
                    .Select(n =>
                    {
                        var score = reg.RoundScores.FirstOrDefault(s => s.RoundNumber == n);

                        return new RoundResultVM
                        {
                            RoundNumber = n,
                            Points = score?.Points ?? 0
                        };
                    })
                    .ToList();

                var total = reg.RoundScores.Sum(s => s.Points);

                var item = new MyCompetitionResultVM
                {
                    CompetitionId = comp.Id,
                    CompetitionName = comp.Name,
                    StartDateTime = comp.StartDateTime,
                    Status = comp.Status,
                    RoundsCount = comp.RoundsCount,
                    Rounds = rounds,
                    TotalPoints = total
                };

                if (comp.Status != CompetitionStatus.Planned)
                {
                    var allRegs = await _db.CompetitionRegistrations
                        .Where(r => r.CompetitionId == comp.Id)
                        .Include(r => r.RoundScores)
                        .Include(r => r.Team)
                        .ToListAsync();

                    item.TeamsCount = allRegs.Count;

                    var ranked = RankingHelper.BuildRanking(allRegs, comp.RoundsCount);

                    if (ranked.Any(x => x.TotalPoints > 0))
                    {
                        var index = ranked.FindIndex(x => x.TeamId == team.Id);
                        if (index >= 0)
                        {
                            item.Position = index + 1;
                        }
                    }
                }

                vm.Competitions.Add(item);
            }

            return ServiceResult<MyResultsVM>.Ok(vm);
        }
    }
}