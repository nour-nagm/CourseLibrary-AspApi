using System;

namespace CourseLibrary.API.Models
{
    public class AuthorForCreationDtoWithDateOfDeath : AuthorForCreationDto
    {
        public DateTimeOffset? DateOfDeath { get; set; }
    }
}
