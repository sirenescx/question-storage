namespace QuestionStorage.Models.Types
{
    public static class TypesExtensions
    {
        internal static int GetTypeId(string typeInfo) =>
            typeInfo.Equals("sc") ? 1 : typeInfo.Equals("mc") ? 2 : typeInfo.Equals("oa") ? 3 : 4;
        
        internal static string GetTypeIdFromFullName(string typeInfo) =>
            typeInfo.Equals("multichoice") ? "sc" :
            typeInfo.Equals("multichoiceset") ? "mc" :
            typeInfo.Equals("shortanswer") ? "oa" : "o";
        
        internal static string GetShortTypeName(int typeId) =>
            typeId == 1 ? "sc" : typeId == 2 ? "mc" : typeId == 3 ? "oa" : "o";
        
        internal static int GetTypeId<T>(T typeInfo) =>
            typeInfo.Equals("sc") ? 1 : typeInfo.Equals("mc") ? 2 : typeInfo.Equals("oa") ? 3 : 4;
    }
}