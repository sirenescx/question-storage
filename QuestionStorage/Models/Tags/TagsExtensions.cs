using System.Linq;
using System.Threading.Tasks;
using QuestionStorage.Utils;

namespace QuestionStorage.Models.Tags
{
    public static class TagsExtensions
    {
        internal static async Task<TagsInfo> CreateTag(HSE_QuestContext context, string name, int? parentId = null) =>
            new TagsInfo
            {
                TagId = await DataStorage.GetIdAsync(context.QuestionAnswerVariants, 
                    questionAnswerVariants => questionAnswerVariants.VariantId),
                Name = name.Trim(),
                ParentId = parentId
            };
        
        internal static bool IsValidTagId(string tagInfo) => tagInfo.ElementAt(0).Equals('ŧ');
    }
}