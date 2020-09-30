using System.Linq;
using System.Threading.Tasks;
using QuestionStorage.Utils;

namespace QuestionStorage.Models.Tags
{
    public static class TagsExtensions
    {
        internal static async Task<TagsInfo> CreateTag(StorageContext context, string name, int courseId, int? parentId = null) =>
            new TagsInfo
            {
                TagId = await DataStorage.GetIdAsync(context.TagsInfo,tagsInfo => tagsInfo.TagId),
                Name = name.Trim(),
                CourseId = courseId,
                ParentId = parentId
            };
        
        internal static bool IsValidTagId(string tagInfo) => tagInfo.ElementAt(0).Equals('ŧ');
    }
}