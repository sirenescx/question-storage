using System;
using System.Collections.Generic;

namespace QuestionStorage.Utils
{
    public static class Parser
    {
        public static HashSet<int> ParseIdentifiers(string query) =>
            new HashSet<int>(Array.ConvertAll(query.Split('&'), int.Parse));
    }
}