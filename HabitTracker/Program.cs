using Microsoft.Data.Sqlite;
using System.Globalization;


class Program
{

    static readonly string connectionString = @"Data Source=habitTracker.db";


    static void Main()
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var tableCmd = connection.CreateCommand();

            tableCmd.CommandText =
                @"CREATE TABLE IF NOT EXISTS drinking_water (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Date TEXT,
            Quantity INTEGER
            )";

            tableCmd.ExecuteNonQuery();
        }
        GetUserInput();
    }
    static void GetUserInput()
    {
        Console.Clear();
        bool closeApp = false;
        while (closeApp == false)
        {
            Console.WriteLine("\n\nMAIN MENU");
            Console.WriteLine("\nWhat would you like to do?");
            Console.WriteLine("\nType 0 to close the application");
            Console.WriteLine("Type 1 to view all records");
            Console.WriteLine("Type 2 to insert record");
            Console.WriteLine("Type 3 to delete a record");
            Console.WriteLine("Type 4 to update a record");
            Console.WriteLine("------------------------------------------------\n");

            string? userInput = Console.ReadLine();

            int.TryParse(userInput, out int menuOption);

            switch (menuOption)
            {
                case 0:
                    Console.WriteLine("\nGoodbye!\n");
                    closeApp = true;
                    Environment.Exit(0);
                    break;
                case 1:
                    GetAllRecords();
                    break;
                case 2:
                    Insert();
                    break;
                case 3:
                    Delete();
                    break;
                case 4:
                    Update();
                    break;
                default:
                    Console.WriteLine("\nInvalid command. Please type a number from 0 to 4.\n");
                    break;
            }
        }
    }
    static void Insert()
    {
        string date = GetDateInput();
        int quantity = GetNumberInput("\n\nPlease insert the number of glasses or other measure of your choice (no decimals allowed).\n\n");

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText =
                $"INSERT INTO drinking_water(date, quantity) VALUES('{date}', {quantity})";

            tableCmd.ExecuteNonQuery();

            connection.Close();
        }
        GetUserInput();
    }

    static void GetAllRecords()
    {
        Console.Clear();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = "SELECT * FROM drinking_water";

            List<DrinkingWater> tableData = new();

            SqliteDataReader reader = tableCmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    tableData.Add(
                        new DrinkingWater
                        {
                            Id = reader.GetInt32(0),
                            Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                            Quantity = reader.GetInt32(2)

                        });

                }
                connection.Close();
            }

            else
            {
                Console.WriteLine("No entries found\n");
                connection.Close();
                GetUserInput();
            }

            Console.WriteLine("----------------------------------------------");
            foreach (var entry in tableData)
            {
                Console.WriteLine($"{entry.Id} - {entry.Date.ToString("dd-MM-yyy")} - Quantity: {entry.Quantity}");
            }
            Console.WriteLine("----------------------------------------------\n");
        }
    }

    static void Update()
    {
        Console.Clear();
        GetAllRecords();

        var recordId = GetNumberInput("\n\nPlease enter the Id of the record you want to update, or type 0 to return to the Main menu.");

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM drinking_water WHERE Id = {recordId})";
            int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (checkQuery == 0)
            {
                Console.WriteLine($"\n\nRecord with Id {recordId} does not exist.");
                connection.Close();
                Update();
            }

            string date = GetDateInput();
            int quantity = GetNumberInput("\n\nPlease insert the number of glasses or other measure of your choice (no decimals allowed).\n\n");

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = $"UPDATE drinking_water SET date = '{date}', quantity = {quantity} WHERE Id = {recordId}";

            tableCmd.ExecuteNonQuery();

            connection.Close();
        }
        Console.WriteLine($"\n\nRecord with Id {recordId} was updated. \n\n");
        GetUserInput();
    }
    static void Delete()
    {
        Console.Clear();
        GetAllRecords();

        var recordId = GetNumberInput("\n\n Please enter the Id of the record you want to delete, or type 0 to return to the Main menu.");

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();

            tableCmd.CommandText = $"DELETE from drinking_water WHERE Id = {recordId}";

            int rowCount = tableCmd.ExecuteNonQuery();

            if (rowCount == 0)
            {
                Console.WriteLine($"\n\nRecord with Id {recordId} does not exist. \n\n");
                Delete();
            }
            connection.Close();
        }
        Console.WriteLine($"\n\nRecord with Id {recordId} was deleted. \n\n");
        GetUserInput();
    }

    static string GetDateInput()
    {
        Console.WriteLine("\n\nPlease insert the date: (Format: dd-mm-yy). Type 0 to return to main menu.\n\n");

        string? userInput = Console.ReadLine();

        while (!DateTime.TryParseExact(userInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
        {
            Console.WriteLine("\n\nInvalid date. (Format: dd-mm-yy). Type 0 to return to main menu or try again: \n\n");
            userInput = Console.ReadLine();
        }
        return userInput;
    }

    static int GetNumberInput(string message)
    {
        Console.WriteLine(message);
        string? userInput = Console.ReadLine();

        if (userInput == "0") GetUserInput();

        while (!Int32.TryParse(userInput, out _) || Convert.ToInt32(userInput) < 0)
        {
            Console.WriteLine("\n\nInvalid number. Try again.\n\n");
            userInput = Console.ReadLine();
        }
        return Convert.ToInt32(userInput);
    }
}


public class DrinkingWater
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int Quantity { get; set; }
}