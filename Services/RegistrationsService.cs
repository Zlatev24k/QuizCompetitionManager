using Microsoft.EntityFrameworkCore;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Models;
using QuizCompetitionManager.Services.Interfaces;

namespace QuizCompetitionManager.Services
{
    public class RegistrationsService : IRegistrationsService
    {
        private readonly ApplicationDbContext _db;

        public RegistrationsService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ServiceResult> JoinCompetitionAsync(int competitionId, string userId)
        {
            var comp = await _db.Competitions
                .FirstOrDefaultAsync(c => c.Id == competitionId);

            if (comp == null)
                return ServiceResult.Fail("Състезанието не е намерено.", "CompetitionNotFound");

            if (comp.Status != CompetitionStatus.Planned)
                return ServiceResult.Fail("Записване е възможно само за планирани състезания.");

            var team = await _db.Teams
                .FirstOrDefaultAsync(t => t.OwnerUserId == userId);

            if (team == null)
                return ServiceResult.Fail(
                    "За да се запишеш за състезание, първо трябва да създадеш отбор.",
                    "MissingTeam");

            var already = await _db.CompetitionRegistrations
                .AnyAsync(r => r.CompetitionId == competitionId && r.TeamId == team.Id);

            if (already)
                return ServiceResult.Fail("Този отбор вече е записан за състезанието.");

            _db.CompetitionRegistrations.Add(new CompetitionRegistration
            {
                CompetitionId = competitionId,
                TeamId = team.Id
            });

            await _db.SaveChangesAsync();

            return ServiceResult.Ok("Успешно записване за състезанието!");
        }

        public async Task<ServiceResult> UnjoinCompetitionAsync(int competitionId, string userId)
        {
            var comp = await _db.Competitions
                .FirstOrDefaultAsync(c => c.Id == competitionId);

            if (comp == null)
                return ServiceResult.Fail("Състезанието не е намерено.", "CompetitionNotFound");

            if (comp.Status != CompetitionStatus.Planned)
                return ServiceResult.Fail("Отписване е възможно само за планирани състезания.");

            var team = await _db.Teams
                .FirstOrDefaultAsync(t => t.OwnerUserId == userId);

            if (team == null)
                return ServiceResult.Fail("Нямаш отбор.", "MissingTeam");

            var reg = await _db.CompetitionRegistrations
                .FirstOrDefaultAsync(r => r.CompetitionId == competitionId && r.TeamId == team.Id);

            if (reg == null)
                return ServiceResult.Fail("Отборът не е записан за това състезание.");

            _db.CompetitionRegistrations.Remove(reg);
            await _db.SaveChangesAsync();

            return ServiceResult.Ok("Успешно отписване от състезанието.");
        }
    }
}