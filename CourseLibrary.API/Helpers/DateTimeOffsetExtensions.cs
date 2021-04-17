using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseLibrary.API.Helpers
{
    public static class DateTimeOffsetExtensions
    {
        public static int GetCurrentAge(this DateTimeOffset dateOfBirth)
        {
            var currentDate = DateTime.UtcNow;
            int age = currentDate.Year - dateOfBirth.Year;

            if (currentDate < dateOfBirth.AddYears(age))
                age--;

            return age;
        }
    }
}
