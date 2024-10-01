using Azure.Storage.Queues;
using System;
using System.Threading.Tasks;

public class QueueService
{
    private readonly QueueClient _queueClient;

    public QueueService(string connectionString, string queueName)
    {
        // Initialize the QueueClient using the connection string and queue name
        _queueClient = new QueueClient(connectionString, queueName);

        // Ensure the queue exists; if not, create it
        _queueClient.CreateIfNotExists();
    }

    public async Task SendMessageAsync(string message)
    {
        try
        {
            // Check if the message is not null or empty
            if (!string.IsNullOrWhiteSpace(message))
            {
                // Send the message to the queue
                await _queueClient.SendMessageAsync(message);
            }
            else
            {
                Console.WriteLine("Message is null or empty. No message sent.");
            }
        }
        catch (Exception ex)
        {
            // Log the exception (can replace with actual logging mechanism)
            Console.WriteLine($"Error sending message to queue: {ex.Message}");
        }
    }
    //
}
