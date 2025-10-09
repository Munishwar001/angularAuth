using Backend.Model.ReturnModels;
using Backend.Model.UserModel;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Backend.RepoHelper
{
    public class UserHelper
    {
        private readonly String connectionString; 
        public UserHelper(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            Console.WriteLine("Connection String is =>"+connectionString);
        }

        
        public List<Users> GetUsers()
        {
            try
            {
                var connection = new SqlConnection(connectionString);

                string query = @"
                SELECT u.Id, u.FullName, u.Email
                FROM AspNetUsers u
                INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
                WHERE r.Name = @RoleName and isDeleted = 0 ";

                return connection.Query<Users>(query, new { RoleName = "User" }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Exception while Getting the Users List" + ex);
            }
        }

        public bool UserExist(string userId, UpdateDto data)
        {
            var connection = new SqlConnection(connectionString);
            string checkQuery = @"SELECT COUNT(*) FROM AspNetUsers WHERE (FullName = @FullName OR Email = @Email)  AND Id <> @Id";

            int count = connection.ExecuteScalar<int>(checkQuery, new { FullName = data.FullName, Email = data.Email, Id = userId });

            return count > 0;
        }

        public AuthReturn UpdateData(string userId,UpdateDto data)
        {
            try
            {   
                if(UserExist(userId , data))
                {
                    return new AuthReturn { success = false, message = "Email or Name Already Taken Please take the another one ..."};
                }
                var connection = new SqlConnection(connectionString);

                    string query = @" UPDATE AspNetUsers SET FullName = @FullName, Email = @Email WHERE Id = @Id";

                    int rowsAffected = connection.Execute(query, new{ FullName = data.FullName, Email = data.Email, Id = userId });

                    if (rowsAffected > 0)
                    {
                        return new AuthReturn { success = true, message = "User updated successfully" };
                    }
                    else
                    {
                        return new AuthReturn { success = false,message = "User not found or nothing to update"};
                    }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception while Getting the Users List" + ex);
            }
        }

        public AuthReturn DeleteUser(string userId)
        {
            try
            {
                var connection = new SqlConnection(connectionString);

                string query = @" UPDATE AspNetUsers SET isDeleted = 1, DeletedAt = GetDate() WHERE Id = @Id";

                int rowsAffected = connection.Execute(query, new {Id = userId });

                if (rowsAffected > 0)
                {
                    return new AuthReturn { success = true, message = "User Deleted successfully" };
                }
                else
                {
                    return new AuthReturn { success = false, message = "Enable to delete User"};
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception while Deleting the User" + ex);
            }
        }

        public AuthReturn GetUser(string Email)
        {
            try
            {

                var connection = new SqlConnection(connectionString);

                string query = @" SELECT u.Id, u.FullName, u.Email FROM AspNetUsers u WHERE u.Email = @UserEmail and isDeleted = 0 ";

                var user = connection.QueryFirstOrDefault<Users>(query, new { UserEmail = Email });

                if (user == null)
                {
                    return new AuthReturn { success = false, message = "User not found" };
                }

                return new AuthReturn { success = true, message = "User fetched successfully", user = user };
            }
            catch (Exception ex)
            {
                throw new Exception("Exception while get the Users" + ex);
            }
        }
    }
}
