using System;
using Fare;

namespace QuestionStorage.Utils
{
    public class QRandom : Random
    {
        public string StringFromRegex(string pattern)
        {
            var xeger = new Xeger(pattern);
            
            return xeger.Generate();
        }
    }
}