using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace QuestionStorage.Utils
{
    public static class Validator
    {
        private static void ValidateField(string field, ModelStateDictionary modelState,
            (string key, string errorMessage) modelError)
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                modelState.AddModelError(modelError.key, modelError.errorMessage);
            }
        }

        internal static void ValidateTestCreation(IFormCollection collection, ModelStateDictionary modelState)
        {
            if (collection["QuestionId"].Count(string.IsNullOrWhiteSpace) >= 3)
            {
                modelState.AddModelError("QuestionId", "Test should contain at least 3 questions.");
            }

            ValidateField(collection["Date"], modelState, ("Date", "Date is required."));
            ValidateField(collection["Name"], modelState, ("Name", "Name is required."));
        }

        internal static void ValidateUserCreation(IFormCollection collection, HashSet<string> currentUsers,
            ModelStateDictionary modelState)
        {
            var email = collection["Email"][0].ToLower();
            var regex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            if (!regex.Match(email).Success)
            {
                modelState.AddModelError("Email", "Invalid email format (should be jsmith@example.com).");
                return;
            }

            if (currentUsers.Contains(email))
            {
                modelState.AddModelError("Email", "User with such email already exists.");
            }
        }
        
        internal static void ValidatePasswordChange(string modelOldPassword, string userPassword,
            ModelStateDictionary modelState)
        {
            ValidateField(modelOldPassword, modelState, ("OldPassword", "Old password is required"));
            if (StorageUtils.GetPasswordHash(modelOldPassword) != userPassword)
            {
                modelState.AddModelError("OldPassword", "Old password is incorrect");
            }
        }

    }
}