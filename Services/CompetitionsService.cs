using Microsoft.EntityFrameworkCore;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Helpers;
using QuizCompetitionManager.Models;
using QuizCompetitionManager.Models.ViewModels;
using QuizCompetitionManager.Services.Interfaces;

namespace QuizCompetitionManager.Services
{
    public class CompetitionsService : ICompetitionsService
    {
        private readonly ApplicationDbContext _db;

        public CompetitionsService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Competition>> GetAllAsync()
        {
            return await _db.Competitions
                .OrderByDescending(c => c.StartDateTime)
                .ToListAsync();
        }

        public Task<Competition> GetCreateModelAsync()
        {
            return Task.FromResult(new Competition
            {
                StartDateTime = DateTime.Now,
                RoundsCount = 4
            });
        }

        public async Task<Competition?> GetByIdAsync(int id)
        {
            return await _db.Competitions.FindAsync(id);
        }

        public async Task<AdminCompetitionDetailsVM?> GetDetailsAsync(int id)
        {
            var comp = await _db.Competitions
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comp == null)
                return null;

            var vm = new AdminCompetitionDetailsVM
            {
                Competition = comp
            };

            if (comp.Status != CompetitionStatus.Planned)
            {
                var regs = await _db.CompetitionRegistrations
                    .Where(r => r.CompetitionId == id)
                    .Include(r => r.Team)
                    .Include(r => r.RoundScores)
                    .ToListAsync();

                var ranked = RankingHelper.BuildRanking(regs, comp.RoundsCount);

                vm.Ranking = ranked.Select(r => new AdminRankingRowVM
                {
                    TeamName = r.TeamName,
                    TotalPoints = r.TotalPoints
                }).ToList();
            }

            return vm;
        }

        public async Task CreateAsync(Competition model)
        {
            _db.Competitions.Add(model);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Competition model)
        {
            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var comp = await _db.Competitions.FindAsync(id);

            if (comp != null)
            {
                _db.Competitions.Remove(comp);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<ServiceResult> StartCompetitionAsync(int id)
        {
            var comp = await _db.Competitions.FindAsync(id);
            if (comp == null)
                return ServiceResult.Fail("Състезанието не е намерено.");

            if (comp.Status != CompetitionStatus.Planned)
                return ServiceResult.Fail("Само планирано състезание може да бъде стартирано.");

            var hasTeams = await _db.CompetitionRegistrations
                .AnyAsync(r => r.CompetitionId == id);

            if (!hasTeams)
                return ServiceResult.Fail("Не може да стартираш състезание без записани отбори.");

            comp.Status = CompetitionStatus.Active;
            await _db.SaveChangesAsync();

            return ServiceResult.Ok("Състезанието е стартирано успешно.");
        }

        public async Task<CompetitionManageVM?> GetManageDataAsync(int id)
        {
            var comp = await _db.Competitions
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comp == null)
                return null;

            var regs = await _db.CompetitionRegistrations
                .Where(r => r.CompetitionId == id)
                .Include(r => r.Team)
                    .ThenInclude(t => t.Members)
                .Include(r => r.RoundScores)
                .AsNoTracking()
                .ToListAsync();

            return new CompetitionManageVM
            {
                CompetitionId = comp.Id,
                CompetitionName = comp.Name,
                StartDateTime = comp.StartDateTime,
                Status = comp.Status,
                RoundsCount = comp.RoundsCount,
                Teams = regs
                    .OrderBy(r => r.Team!.Name)
                    .Select(r => new TeamScoreRowVM
                    {
                        RegistrationId = r.Id,
                        TeamName = r.Team!.Name,
                        Members = r.Team!.Members
                            .OrderBy(m => m.FullName)
                            .Select(m => m.FullName)
                            .ToList(),
                        RoundPoints = Enumerable.Range(1, comp.RoundsCount)
                            .Select(round =>
                            {
                                var existing = r.RoundScores.FirstOrDefault(s => s.RoundNumber == round);

                                return new RoundPointVM
                                {
                                    RoundNumber = round,
                                    Points = existing?.Points ?? 0
                                };
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }

        public async Task SaveAllScoresAsync(CompetitionManageVM vm)
        {
            var comp = await _db.Competitions
                .FirstOrDefaultAsync(c => c.Id == vm.CompetitionId);

            if (comp == null)
                return;

            var regIds = vm.Teams.Select(t => t.RegistrationId).ToList();

            var existingScores = await _db.RoundScores
                .Where(s => regIds.Contains(s.CompetitionRegistrationId))
                .ToListAsync();

            var scoreMap = existingScores.ToDictionary(
                s => (s.CompetitionRegistrationId, s.RoundNumber),
                s => s
            );

            foreach (var team in vm.Teams)
            {
                foreach (var rp in team.RoundPoints)
                {
                    if (rp.RoundNumber < 1 || rp.RoundNumber > comp.RoundsCount)
                        continue;

                    var points = rp.Points;

                    if (points < 0)
                        points = 0;

                    if (points > 20)
                        points = 20;

                    var key = (team.RegistrationId, rp.RoundNumber);

                    if (scoreMap.TryGetValue(key, out var score))
                    {
                        score.Points = points;
                    }
                    else
                    {
                        _db.RoundScores.Add(new RoundScore
                        {
                            CompetitionRegistrationId = team.RegistrationId,
                            RoundNumber = rp.RoundNumber,
                            Points = points
                        });
                    }
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task<ServiceResult> FinishCompetitionAsync(int id)
        {
            var comp = await _db.Competitions.FindAsync(id);
            if (comp == null)
                return ServiceResult.Fail("Състезанието не е намерено.");

            if (comp.Status != CompetitionStatus.Active)
                return ServiceResult.Fail("Само активно състезание може да бъде приключено.");

            comp.Status = CompetitionStatus.Finished;
            await _db.SaveChangesAsync();

            return ServiceResult.Ok("Състезанието е приключено и е преместено в Архив.");
        }

        public async Task<ServiceResult> RemoveTeamAsync(int competitionId, int registrationId)
        {
            var comp = await _db.Competitions.FindAsync(competitionId);
            if (comp == null)
                return ServiceResult.Fail("Състезанието не е намерено.");

            if (comp.Status == CompetitionStatus.Finished)
                return ServiceResult.Fail("Не може да премахваш отбори от приключило състезание.");

            var reg = await _db.CompetitionRegistrations
                .FirstOrDefaultAsync(r => r.Id == registrationId && r.CompetitionId == competitionId);

            if (reg == null)
                return ServiceResult.Fail("Регистрацията не е намерена.");

            _db.CompetitionRegistrations.Remove(reg);
            await _db.SaveChangesAsync();

            return ServiceResult.Ok("Отборът беше премахнат от състезанието.");
        }
    }
}