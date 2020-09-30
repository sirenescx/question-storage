using QuestionStorage.Models.Users;

namespace QuestionStorage.Models.Courses
{
    public class UsersCourses
    {
        public int UserId { get; set; }
        public int CourseId { get; set; }

        public virtual CoursesInfo Course { get; set; }
        public virtual User User { get; set; }
    }
}
