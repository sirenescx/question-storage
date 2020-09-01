using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using QuestionStorage.Models.Types;
using QuestionStorage.Utils;

namespace QuestionStorage.Models.Questions
{
    public static class QuestionExtensions
    {
        internal static async Task<QuestionsInfo> CreateQuestion(HSE_QuestContext context, string questionText,
            StringValues typeInfo, string questionName, StringValues checkTemplate, string xml = null)
        {
            var questionId = await DataStorage.GetIdAsync(context.QuestionsInfo,
                questionsInfo => questionsInfo.QuestId);

            var question = new QuestionsInfo
            {
                QuestId = questionId,
                QuestionName = questionName.Trim(),
                QuestionText = questionText.Trim(),
                TypeId = TypesExtensions.GetTypeId(typeInfo),
                IsTemplate = StorageUtils.PreprocessCheckboxValues(checkTemplate).First(),
                VersionId = 1,
                SourceQuestId = questionId
            };

            return question;
        }

        internal static async Task<QuestionsInfo> CreateQuestion(HSE_QuestContext context, StringValues questionName,
            StringValues questionText, StringValues typeInfo, int versionId, int sourceQuestionId) =>
            new QuestionsInfo
            {
                QuestId = await DataStorage.GetIdAsync(context.QuestionsInfo, questionsInfo => questionsInfo.QuestId),
                QuestionName = questionName,
                QuestionText = questionText,
                TypeId = TypesExtensions.GetTypeId(typeInfo),
                VersionId = versionId,
                SourceQuestId = sourceQuestionId
            };

        internal static async Task<int> GetSourceVersionId(HSE_QuestContext context, QuestionsInfo editableQuestion)
        {
            while (editableQuestion.SourceQuestId != editableQuestion.QuestId)
            {
                editableQuestion =
                    await context.QuestionsInfo.Where(
                        q => q.QuestId == editableQuestion.SourceQuestId).FirstOrDefaultAsync();
            }

            return editableQuestion.QuestId;
        }

        internal static async Task<QuestionAnswerVariants> CreateQuestionAnswerVariant(HSE_QuestContext context,
            int questionId, string text, bool isCorrect = true, int sortCode = 0) =>
            new QuestionAnswerVariants
            {
                VariantId = await DataStorage.GetIdAsync(context.QuestionAnswerVariants, 
                    questionAnswerVariants => questionAnswerVariants.VariantId),
                QuestId = questionId,
                Answer = text,
                IsCorrect = isCorrect,
                SortCode = sortCode
            };
        
        internal static async Task<bool> QuestionExists(HSE_QuestContext context, int id) =>
            await context.QuestionsInfo.AnyAsync(q => q.QuestId == id);
    }
}