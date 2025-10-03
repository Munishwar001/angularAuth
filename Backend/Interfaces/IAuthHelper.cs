using Backend.Model.AuthModel;
using Backend.Model.ReturnModels;
using Backend.Model.UserModel;

namespace Backend.Interfaces
{
    public interface IAuthHelper
    {
        Users emailExist(string email);

        bool signUpHelper(SignUpDto data);

        AuthReturn loginHelper(LoginDto data);
    }
}