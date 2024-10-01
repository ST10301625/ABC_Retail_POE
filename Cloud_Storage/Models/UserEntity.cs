using Azure;
using Azure.Data.Tables;

namespace Cloud_Storage.Models
{
    public class UserEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        // Timestamp and ETag are required for Table entities
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Constructor
        public UserEntity(string username, string email, string password)
        {
            PartitionKey = "Users";
            RowKey = Guid.NewGuid().ToString(); // Unique identifier for each user
            Username = username;
            Email = email;
            Password = password;
        }

        public UserEntity() { } // Parameterless constructor is required for deserialization
    }
}
