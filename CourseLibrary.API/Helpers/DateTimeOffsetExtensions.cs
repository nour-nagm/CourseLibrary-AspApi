using System;

namespace CourseLibrary.API.Helpers
{
    public static class DateTimeOffsetExtensions
    {
        public static int GetCurrentAge(this DateTimeOffset dateOfBirth,
            DateTimeOffset? dateOfDeath)
        {
            var dateToCalculateTo = dateOfDeath != null ?
                dateOfDeath.Value.UtcDateTime : DateTime.UtcNow;

            int age = dateToCalculateTo.Year - dateOfBirth.Year;

            if (dateToCalculateTo < dateOfBirth.AddYears(age))
                age--;

            return age;
        }
    }
}
