namespace QuestionStorage.Models.Questions
{
    public class QuestionDto
    {
        public string Name { get; set; }
        
        public string Text { get; set; }
        
        public int AuthorId { get; set; }
        
        public int TypeId { get; set; }

        public bool IsTemplate { get; set; } = false;
        
        public int CourseId { get; set; }
        
        public string Xml { get; set; }
    }
}