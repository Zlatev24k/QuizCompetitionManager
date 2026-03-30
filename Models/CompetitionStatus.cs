using System.ComponentModel.DataAnnotations;

namespace QuizCompetitionManager.Models
{
    public enum CompetitionStatus
    {
        [Display(Name = "Предстоящо")]
        Planned,

        [Display(Name = "Активно")]
        Active,

        [Display(Name = "Приключило")]
        Finished
    }
}
