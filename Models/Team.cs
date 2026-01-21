using System.ComponentModel.DataAnnotations;

namespace QuizCompetitionManager.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string OwnerUserId { get; set; } = string.Empty;

        public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
        public ICollection<CompetitionRegistration> Registrations { get; set; } = new List<CompetitionRegistration>();
    }
}
