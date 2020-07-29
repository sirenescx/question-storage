using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models.QuizzesQuestionsModels
{
    [Table("QuizzesInfo_QuestionsInfo")]
    public class QuizzesInfoQuestionsInfo
    {
        [Key]
        [Column("QuizID")]
        public int QuizId { get; set; }
        [Key]
        [Column("QuestID")]
        public int QuestId { get; set; }
        [Column("CodeSort")]
        public string CodeSort { get; set; }
        [ForeignKey(nameof(QuizId))]
        [InverseProperty(nameof(QuestionsInfo.QuizzesInfoQuestionsInfo))]
        public QuestionsInfo Quest { get; set; }
        [ForeignKey(nameof(QuestId))]
        [InverseProperty(nameof(QuizzesInfo.QuizzesInfoQuestionsInfo))]
        public QuizzesInfo Quiz { get; set; }
    }
}