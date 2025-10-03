using Backend.Model.AuthModel;
using Backend.Model.AuthModel;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Backend.Helper
{
    public class StringHelper
    {
        public static SignUpDto ToLowerCase(SignUpDto dto)
        {
            if (dto == null) return null;

            return new SignUpDto
            {
                fullName = dto.fullName?.ToLower(),
                email = dto.email?.ToLower(),
                password = dto.password ,
                confirmPassword = dto.confirmPassword
            };
        }


    }
}
