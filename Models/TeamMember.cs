using System.ComponentModel.DataAnnotations;

namespace QuizCompetitionManager.Models
{
    public class TeamMember
    {
        public int Id { get; set; }

        [Required]
        public int TeamId { get; set; }

        [Required, StringLength(80)]
        public string FullName { get; set; } = string.Empty;

        public Team? Team { get; set; }
    }
}
