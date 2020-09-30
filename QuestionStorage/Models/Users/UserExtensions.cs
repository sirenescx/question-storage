using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using QuestionStorage.Models.ViewModels;
using QuestionStorage.Utils;

namespace QuestionStorage.Models.Users
{
    public static class UserExtensions
    {
        internal static async Task<User> CreateUser(StorageContext context, string email, string password,
            StringValues roleInfo) =>
            new User
            {
                Id = await DataStorage.GetIdAsync(context.Users, user => user.Id),
                Email = email,
                Password = StorageUtils.GetPasswordHash(password),
                RoleId = StorageUtils.PreprocessCheckboxValues(roleInfo).First() ? 1 : 2
            };

        internal static void UpdatePassword(EditViewModel viewModel, User user) =>
            user.Password = StorageUtils.GetPasswordHash(viewModel.NewPassword);

        internal static void SetPassword(User user) =>
            user.Password = StorageUtils.GetPasswordHash("qstorage@#_pass");

        internal static void ChangeRole(User user) =>
            user.RoleId = user.RoleId == 1 ? 2 : 1;
    }
}