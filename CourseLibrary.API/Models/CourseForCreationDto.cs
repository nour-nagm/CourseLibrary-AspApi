using CourseLibrary.API.ValidationAttributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models
{
    [CourseTitleMustBeDifferentFromDescription (
        ErrorMessage = "Title must be different from description")]
    public class CourseForCreationDto //: IValidatableObject
    {
        [Required(ErrorMessage = "You should fill out a title")]
        [MaxLength(100, ErrorMessage = "The title should not have more than 100 chararcters")]
        public string Title { get; set; }
        
        [MaxLength(1500, ErrorMessage = "The title should not have more than 1500 chararcters")]
        public string Description { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if(Title == Description)
        //    {
        //        yield return new ValidationResult(
        //            "The provided description should be different from the the title.",
        //            new[] { "CourseForCreationDto" });
        //    }
        //}
    }
}
