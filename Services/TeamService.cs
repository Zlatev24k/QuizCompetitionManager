using Microsoft.EntityFrameworkCore;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Models;
using QuizCompetitionManager.Services.Interfaces;

namespace QuizCompetitionManager.Services
{
    public class TeamService : ITeamService
    {
        private readonly ApplicationDbContext _db;

        public TeamService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Team?> GetUserTeamAsync(string userId)
        {
            return await _db.Teams
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.OwnerUserId == userId);
        }

        public async Task<ServiceResult> CreateTeamAsync(string userId, string name)
        {
            name = (name ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
                return ServiceResult.Fail("Името на отбора е задължително.", "InvalidName");

            if (name.Length > 80)
                return ServiceResult.Fail("Името на отбора е твърде дълго (макс. 80 символа).", "InvalidName");

            var existing = await _db.Teams.AnyAsync(t => t.OwnerUserId == userId);
            if (existing)
                return ServiceResult.Fail("Потребителят вече има отбор.", "TeamAlreadyExistsForUser");

            var nameExists = await _db.Teams.AnyAsync(t => t.Name.ToLower() == name.ToLower());
            if (nameExists)
                return ServiceResult.Fail("Вече съществува отбор с това име.", "DuplicateTeamName");

            var team = new Team
            {
                Name = name,
                OwnerUserId = userId
            };

            _db.Teams.Add(team);
            await _db.SaveChangesAsync();

            return ServiceResult.Ok("Отборът беше създаден успешно. Вече можеш да се запишеш за състезание.");
        }

        public async Task<ServiceResult> AddMemberAsync(string userId, string fullName)
        {
            fullName = (fullName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(fullName))
                return ServiceResult.Fail("Името на члена е задължително.", "InvalidMemberName");

            if (fullName.Length > 80)
                return ServiceResult.Fail("Името е твърде дълго (макс. 80 символа).", "InvalidMemberName");

            var team = await _db.Teams.FirstOrDefaultAsync(t => t.OwnerUserId == userId);
            if (team == null)
                return ServiceResult.Fail("Потребителят няма отбор.", "TeamNotFound");

            _db.TeamMembers.Add(new TeamMember
            {
                TeamId = team.Id,
                FullName = fullName
            });

            await _db.SaveChangesAsync();

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> RemoveMemberAsync(string userId, int memberId)
        {
            var member = await _db.TeamMembers
                .Include(m => m.Team)
                .FirstOrDefaultAsync(m => m.Id == memberId);

            if (member == null)
                return ServiceResult.Fail("Членът не е намерен.", "MemberNotFound");

            if (member.Team == null || member.Team.OwnerUserId != userId)
                return ServiceResult.Fail("Нямаш право да изтриеш този член.", "Forbidden");

            _db.TeamMembers.Remove(member);
            await _db.SaveChangesAsync();

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult<Team>> GetTeamForEditAsync(string userId)
        {
            var team = await _db.Teams.FirstOrDefaultAsync(t => t.OwnerUserId == userId);

            if (team == null)
                return ServiceResult<Team>.Fail("Отборът не е намерен.", "TeamNotFound");

            return ServiceResult<Team>.Ok(team);
        }

        public async Task<ServiceResult> EditTeamNameAsync(string userId, int teamId, string name)
        {
            name = (name ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
                return ServiceResult.Fail("Името на отбора е задължително.", "InvalidName");

            if (name.Length > 80)
                return ServiceResult.Fail("Името на отбора е твърде дълго (макс. 80 символа).", "InvalidName");

            var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.OwnerUserId == userId);
            if (team == null)
                return ServiceResult.Fail("Отборът не е намерен.", "TeamNotFound");

            var nameExists = await _db.Teams.AnyAsync(t => t.Id != teamId && t.Name.ToLower() == name.ToLower());
            if (nameExists)
                return ServiceResult.Fail("Вече съществува отбор с това име.", "DuplicateTeamName");

            team.Name = name;
            await _db.SaveChangesAsync();

            return ServiceResult.Ok("Името на отбора беше обновено успешно.");
        }
    }
}