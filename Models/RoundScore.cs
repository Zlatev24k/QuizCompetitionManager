using System.ComponentModel.DataAnnotations;

namespace QuizCompetitionManager.Models
{
    public class RoundScore
    {
        public int Id { get; set; }

        [Required]
        public int CompetitionRegistrationId { get; set; }

        [Range(1, 50)]
        public int RoundNumber { get; set; }

        public int Points { get; set; }

        public CompetitionRegistration? CompetitionRegistration { get; set; }
    }
}
