using System.ComponentModel.DataAnnotations;

namespace QuizCompetitionManager.Models
{
    public class Competition
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime StartDateTime { get; set; }

        [Range(1, 20)]
        public int RoundsCount { get; set; } = 4;

        public CompetitionStatus Status { get; set; } = CompetitionStatus.Planned;

        public ICollection<CompetitionRegistration> Registrations { get; set; } = new List<CompetitionRegistration>();
    }
}
}
