using QuestionStorage.Models.Questions;

namespace QuestionStorage.Models.Tags
{
    public class TagsQuestions
    {
        public int TagId { get; set; }
        public int QuestId { get; set; }

        public virtual QuestionsInfo Quest { get; set; }
        public virtual TagsInfo Tag { get; set; }
    }
}
