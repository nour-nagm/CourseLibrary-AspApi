using CourseLibrary.API.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models
{
    [CourseTitleMustBeDifferentFromDescription(
        ErrorMessage = "Title must be different from description")]
    public abstract class CourseForManipulationDto
    {
        [Required(ErrorMessage = "You should fill out a title")]
        [MaxLength(100, ErrorMessage = "The title should not have more than 100 chararcters")]
        public string Title { get; set; }

        [MaxLength(1500, ErrorMessage = "The title should not have more than 1500 chararcters")]
        public virtual string Description { get; set; }
    }
}
