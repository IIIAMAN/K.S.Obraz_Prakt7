using System;
using System.Configuration;
using System.Data.SqlClient;
using TaskManagement.TaskManagementDataSetTableAdapters;

namespace TaskManagement.Helpers
{
    public class UserActivityLogger
    {
        private readonly string _connectionString;

        public UserActivityLogger(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void LogUserActivity(int userId, string login, string action)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var checkUserCommand = new SqlCommand("SELECT COUNT(*) FROM Users WHERE ID = @UserID", connection);
                checkUserCommand.Parameters.AddWithValue("@UserID", userId);
                int userExists = (int)checkUserCommand.ExecuteScalar();

                if (userExists == 0)
                {
                    throw new ArgumentException("Пользователь с указанным ID не найден.");
                }

                var command = new SqlCommand("INSERT INTO UserActivityLog (UserID, Action, Timestamp) VALUES (@UserID, @Action, @Timestamp)", connection);
                command.Parameters.AddWithValue("@UserID", userId);
                command.Parameters.AddWithValue("@Action", $"{login} {action}");
                command.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                command.ExecuteNonQuery();
            }
        }
    }
}
