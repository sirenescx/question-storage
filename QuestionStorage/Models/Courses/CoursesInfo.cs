using System.Collections.Generic;

namespace QuestionStorage.Models.Courses
{
    public class CoursesInfo
    {
        public CoursesInfo()
        {
            UsersCourses = new HashSet<UsersCourses>();
        }

        public int CourseId { get; set; }
        public string CourseName { get; set; }

        public virtual ICollection<UsersCourses> UsersCourses { get; set; }
    }
}
