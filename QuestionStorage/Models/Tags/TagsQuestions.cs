using QuestionStorage.Models.Questions;

namespace QuestionStorage.Models.Tags
{
    public class TagsQuestions
    {
        public int TagId { get; set; }
        public int QuestionId { get; set; }

        public virtual Question Question { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
