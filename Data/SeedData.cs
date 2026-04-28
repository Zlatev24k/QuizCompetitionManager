using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizCompetitionManager.Models;

namespace QuizCompetitionManager.Data
{
    public static class SeedData
    {
        public const string AdminRole = "Admin";
        public const string TeamRole = "Team";

        public static async Task EnsureSeededAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Roles
            if (!await roleManager.RoleExistsAsync(AdminRole))
                await roleManager.CreateAsync(new IdentityRole(AdminRole));

            if (!await roleManager.RoleExistsAsync(TeamRole))
                await roleManager.CreateAsync(new IdentityRole(TeamRole));

            // Admin
            var adminEmail = "admin@quiz.local";
            var adminPassword = "Admin!23456";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new Exception("Admin user creation failed: " + errors);
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
                await userManager.AddToRoleAsync(adminUser, AdminRole);

            // Seed demo teams, competitions, registrations and scores
            await SeedDemoDataAsync(userManager, db);
        }

        private static async Task SeedDemoDataAsync(
            UserManager<IdentityUser> userManager,
            ApplicationDbContext db)
        {
            if (await db.Competitions.AnyAsync(c =>
                c.Name == "Март 2026" ||
                c.Name == "Април 2026" ||
                c.Name == "Май 2026" ||
                c.Name == "Юни 2026" ||
                c.Name == "Юли 2026"))
            {
                return;
            }
            var teamSeedData = new[]
            {
                new
                {
                    Email = "team1@quiz.local",
                    Password = "Team1pass",
                    TeamName = "Отбор 1",
                    Members = new[] { "Иван Петров", "Мария Георгиева", "Георги Димитров" }
                },
                new
                {
                    Email = "team2@quiz.local",
                    Password = "Team2pass",
                    TeamName = "Отбор 2",
                    Members = new[] { "Николай Иванов", "Елена Стоянова", "Петър Колев" }
                },
                new
                {
                    Email = "team3@quiz.local",
                    Password = "Team3pass",
                    TeamName = "Отбор 3",
                    Members = new[] { "Виктория Тодорова", "Даниел Христов", "Анна Николова" }
                },
                new
                {
                    Email = "team4@quiz.local",
                    Password = "Team4pass",
                    TeamName = "Отбор 4",
                    Members = new[] { "Мартин Василев", "Симона Илиева", "Калоян Стефанов" }
                }
            };

            var teams = new List<Team>();

            foreach (var seedTeam in teamSeedData)
            {
                var user = await userManager.FindByEmailAsync(seedTeam.Email);

                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = seedTeam.Email,
                        Email = seedTeam.Email,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(user, seedTeam.Password);
                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                        throw new Exception($"Team user creation failed for {seedTeam.Email}: {errors}");
                    }
                }

                if (!await userManager.IsInRoleAsync(user, TeamRole))
                    await userManager.AddToRoleAsync(user, TeamRole);

                var team = await db.Teams
                    .Include(t => t.Members)
                    .FirstOrDefaultAsync(t => t.OwnerUserId == user.Id);

                if (team == null)
                {
                    team = await db.Teams
                        .Include(t => t.Members)
                        .FirstOrDefaultAsync(t => t.Name == seedTeam.TeamName);
                }

                if (team == null)
                {
                    team = new Team
                    {
                        Name = seedTeam.TeamName,
                        OwnerUserId = user.Id
                    };

                    db.Teams.Add(team);
                    await db.SaveChangesAsync();
                }
                else
                {
                    team.Name = seedTeam.TeamName;
                    team.OwnerUserId = user.Id;
                    await db.SaveChangesAsync();
                }

                foreach (var memberName in seedTeam.Members)
                {
                    if (!team.Members.Any(m => m.FullName == memberName))
                    {
                        team.Members.Add(new TeamMember
                        {
                            TeamId = team.Id,
                            FullName = memberName
                        });
                    }
                }

                teams.Add(team);
            }

            await db.SaveChangesAsync();

            var competitionSeedData = new[]
            {
                new
                {
                    Name = "Март 2026",
                    StartDateTime = new DateTime(2026, 3, 1, 19, 0, 0),
                    Status = CompetitionStatus.Finished
                },
                new
                {
                    Name = "Април 2026",
                    StartDateTime = new DateTime(2026, 4, 1, 19, 0, 0),
                    Status = CompetitionStatus.Finished
                },
                new
                {
                    Name = "Май 2026",
                    StartDateTime = new DateTime(2026, 5, 1, 19, 0, 0),
                    Status = CompetitionStatus.Active
                },
                new
                {
                    Name = "Юни 2026",
                    StartDateTime = new DateTime(2026, 6, 1, 19, 0, 0),
                    Status = CompetitionStatus.Planned
                },
                new
                {
                    Name = "Юли 2026",
                    StartDateTime = new DateTime(2026, 7, 1, 19, 0, 0),
                    Status = CompetitionStatus.Planned
                }
            };

            var competitions = new List<Competition>();

            foreach (var seedCompetition in competitionSeedData)
            {
                var competition = await db.Competitions
                    .FirstOrDefaultAsync(c => c.Name == seedCompetition.Name);

                if (competition == null)
                {
                    competition = new Competition
                    {
                        Name = seedCompetition.Name,
                        StartDateTime = seedCompetition.StartDateTime,
                        RoundsCount = 4,
                        Status = seedCompetition.Status
                    };

                    db.Competitions.Add(competition);
                }
                else
                {
                    competition.StartDateTime = seedCompetition.StartDateTime;
                    competition.RoundsCount = 4;
                    competition.Status = seedCompetition.Status;
                }

                competitions.Add(competition);
            }

            await db.SaveChangesAsync();

            foreach (var competition in competitions)
            {
                foreach (var team in teams)
                {
                    var registrationExists = await db.CompetitionRegistrations
                        .AnyAsync(r => r.CompetitionId == competition.Id && r.TeamId == team.Id);

                    if (!registrationExists)
                    {
                        db.CompetitionRegistrations.Add(new CompetitionRegistration
                        {
                            CompetitionId = competition.Id,
                            TeamId = team.Id,
                            RegisteredAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await db.SaveChangesAsync();

            await SeedScoresAsync(db, "Март 2026", new Dictionary<string, int[]>
            {
                ["Отбор 1"] = new[] { 15, 18, 12, 20 },
                ["Отбор 2"] = new[] { 16, 14, 17, 15 },
                ["Отбор 3"] = new[] { 12, 19, 16, 18 },
                ["Отбор 4"] = new[] { 10, 13, 15, 14 }
            });

            await SeedScoresAsync(db, "Април 2026", new Dictionary<string, int[]>
            {
                ["Отбор 1"] = new[] { 18, 15, 17, 16 },
                ["Отбор 2"] = new[] { 20, 16, 18, 19 },
                ["Отбор 3"] = new[] { 14, 15, 13, 17 },
                ["Отбор 4"] = new[] { 16, 18, 15, 14 }
            });

            await SeedScoresAsync(db, "Май 2026", new Dictionary<string, int[]>
            {
                ["Отбор 1"] = new[] { 17, 16 },
                ["Отбор 2"] = new[] { 14, 18 },
                ["Отбор 3"] = new[] { 20, 15 },
                ["Отбор 4"] = new[] { 12, 13 }
            });
        }

        private static async Task SeedScoresAsync(
            ApplicationDbContext db,
            string competitionName,
            Dictionary<string, int[]> scoresByTeam)
        {
            var competition = await db.Competitions
                .Include(c => c.Registrations)
                    .ThenInclude(r => r.Team)
                .Include(c => c.Registrations)
                    .ThenInclude(r => r.RoundScores)
                .FirstOrDefaultAsync(c => c.Name == competitionName);

            if (competition == null)
                return;

            foreach (var registration in competition.Registrations)
            {
                if (registration.Team == null)
                    continue;

                if (!scoresByTeam.TryGetValue(registration.Team.Name, out var scores))
                    continue;

                for (int i = 0; i < scores.Length; i++)
                {
                    var roundNumber = i + 1;
                    var points = scores[i];

                    var roundScore = registration.RoundScores
                        .FirstOrDefault(s => s.RoundNumber == roundNumber);

                    if (roundScore == null)
                    {
                        db.RoundScores.Add(new RoundScore
                        {
                            CompetitionRegistrationId = registration.Id,
                            RoundNumber = roundNumber,
                            Points = points
                        });
                    }
                    else
                    {
                        roundScore.Points = points;
                    }
                }
            }

            await db.SaveChangesAsync();
        }
    }
}