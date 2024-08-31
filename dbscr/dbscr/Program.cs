using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Dapper;
using Microsoft.Data.SqlClient;

namespace dbscr
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var command = args[0];

            if (command.Equals("exec", StringComparison.OrdinalIgnoreCase))
            {
                command = "EXEC";
            }

            if (command.Equals("query", StringComparison.OrdinalIgnoreCase))
            {
                command = "QUERY";
            }

            if (command != "EXEC" && command != "QUERY")
            {
                Console.WriteLine("Invalid command.");
                return;
            }

            var commandText = File.ReadAllText(args[1].Trim('"'));
            var connectionString = args[2].Trim('"');

            using (var db = new SqlConnection(connectionString))
            {
                try
                {
                    db.Open();

                    if (command == "QUERY")
                    {
                        var jsonSerializerOptions = new JsonSerializerOptions { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };

                        var result = db.Query(commandText);

                        foreach (var row in result)
                        {
                            Console.WriteLine(JsonSerializer.Serialize(row, jsonSerializerOptions));
                        }

                        if (args.Length > 3)
                        {
                            var output = args[3].Trim('"');

                            File.WriteAllText(output, JsonSerializer.Serialize(result, jsonSerializerOptions));
                        }
                    }
                    else
                    {
                        db.Execute(commandText);
                    }

                    Console.WriteLine("Success.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error occurred: " + ex.Message);
                }
            }
        }
    }
}