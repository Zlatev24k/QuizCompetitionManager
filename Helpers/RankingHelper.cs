using QuizCompetitionManager.Models;

namespace QuizCompetitionManager.Helpers
{
    public static class RankingHelper
    {
        public class RankedRow
        {
            public int TeamId { get; set; }
            public string TeamName { get; set; } = string.Empty;
            public int TotalPoints { get; set; }
            public int[] RoundPoints { get; set; } = Array.Empty<int>();
        }

        public static List<RankedRow> BuildRanking(
            IEnumerable<CompetitionRegistration> regs,
            int roundsCount)
        {
            var rows = regs.Select(r =>
            {
                var rounds = Enumerable.Range(1, roundsCount)
                    .Select(n => r.RoundScores.FirstOrDefault(s => s.RoundNumber == n)?.Points ?? 0)
                    .ToArray();

                return new RankedRow
                {
                    TeamId = r.TeamId,
                    TeamName = r.Team!.Name,
                    TotalPoints = rounds.Sum(),
                    RoundPoints = rounds
                };
            }).ToList();


            IOrderedEnumerable<RankedRow> ordered = rows.OrderByDescending(x => x.TotalPoints);

            for (int i = roundsCount - 1; i >= 0; i--)
            {
                int idx = i;
                ordered = ordered.ThenByDescending(x => x.RoundPoints[idx]);
            }

            return ordered.ThenBy(x => x.TeamName).ToList();
        }
    }
}