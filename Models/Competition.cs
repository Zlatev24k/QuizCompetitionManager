using System.ComponentModel.DataAnnotations;

namespace QuizCompetitionManager.Models
{
    public class Competition
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Моля, въведете име на състезанието.")]
        [StringLength(120, ErrorMessage = "Името може да бъде до 120 символа.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Моля, въведете дата и час.")]
        public DateTime StartDateTime { get; set; }

        [Range(1, 20, ErrorMessage = "Броят кръгове трябва да бъде между 1 и 20.")]
        public int RoundsCount { get; set; } = 4;

        public CompetitionStatus Status { get; set; } = CompetitionStatus.Planned;

        public ICollection<CompetitionRegistration> Registrations { get; set; } = new List<CompetitionRegistration>();
    }
}