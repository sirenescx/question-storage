using System;
using Fare;

namespace QuestionStorage.Utils
{
    internal class QRandom : Random
    {
        internal string StringFromRegex(string pattern)
        {
            var xeger = new Xeger(pattern);
            
            return xeger.Generate();
        }
    }
}