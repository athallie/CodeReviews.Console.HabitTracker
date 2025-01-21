using Microsoft.Data.Sqlite;

//DB Setup
const string dbPath = "habit.db";
await using var connection = new SqliteConnection($"Data Source={dbPath}");
await connection.OpenAsync();

await using var command = new SqliteCommand("""
    CREATE TABLE IF NOT EXISTS HABITS(
        ID INTEGER PRIMARY KEY,
        DATE TEXT NOT NULL,
        AMOUNT REAL NOT NULL
    )
    """, connection);
await using var reader = await command.ExecuteReaderAsync();

await connection.CloseAsync();

//Query Templates
Func<string, string> readAllRows = (table) => $"SELECT * FROM {table};";
Func<string, string, double, string> insert = (table, date, amount) =>
{
    return $"""
        INSERT INTO {table}(DATE, AMOUNT)
        VALUES("{date}", {amount});
    """;
};
Func<string, int, string, double, string> updateRow = (table, id, date, amount) =>
{
    return $"""
        UPDATE {table}
        SET DATE = "{date}",
            AMOUNT = {amount}
        WHERE ID = {id};
    """;
};
Func<string, int, string> deleteRow = (table, id) => $"DELETE FROM {table} WHERE ID = {id};";
Func<string, int, string> selectRow = (table, id) => $"SELECT * FROM {table} WHERE id = {id};";

//Main Program Loop
Console.WriteLine("[Water Logger]");
while (true)
{
    //Menu
    Console.WriteLine("""

    What would you like to do?

    Type 0 to Close Application.
    Type 1 to View All Records.
    Type 2 to Insert Record.
    Type 3 to Delete Record.
    Type 4 to Update Record.

    """);

    Console.Write("Your choice: ");
    var userChoice = Console.ReadLine();
    Console.WriteLine();

    Func<string, bool> checkDateTime = (date) =>
    {
        DateTime dateTime;
        if (DateTime.TryParse(date, out dateTime)) { return true; }
        return false;
    };
    Func<string, bool> checkAmount = (amount) =>
    {
        double am;
        if (Double.TryParse(amount, out am)) { return true; }
        return false;
    };
    Func<string, bool> checkId = (id) =>
    {
        int idOut;
        if (int.TryParse(id, out idOut)) { return true; }
        return false;
    };

    //Input Processing
    switch (userChoice)
    {
        case "0": System.Environment.Exit(0); break;
        case "1": await executeQuery(readAllRows("HABITS"), "READALL"); break;
        case "2":
            string date = getUserInput(
                "Date: ",
                "Invalid date. Please use the format MM/DD/YYYY.",
                checkDateTime
            );

            string amount = getUserInput(
                "Amount: ",
                "Invalid amount. Please use valid numbers and dot '.' as separator if necessary.",
                checkAmount
            );
            await executeQuery(insert("HABITS", date, Double.Parse(amount)), "INSERT");
            break;
        case "3":
            string id = getUserInput(
                "ID of Record: ",
                "Id invalid. Please input a valid number.",
                checkId
            );
            await executeQuery(deleteRow("HABITS", Int32.Parse(id)), "DELETE");
            break;
        case "4":
            while(true)
            {
                id = getUserInput(
                    "ID of Record: ",
                    "Id invalid. Please input a valid number.",
                    checkId
                );
                var rowExist = await executeQuery(selectRow("HABITS", Int32.Parse(id)), "CHECK");
                if (!rowExist)
                {
                    Console.WriteLine($"Row with ID {id} not found. Please input an existing ID.");
                    continue;
                }
                break;
            }
            date = getUserInput(
                "Date: ",
                "Invalid date. Please use the format MM/DD/YYYY.",
                checkDateTime
            );
            amount = getUserInput(
                "Amount: ",
                "Invalid amount. Please use valid numbers and dot '.' as separator if necessary.",
                checkAmount
            );
            await executeQuery(updateRow("HABITS", Int32.Parse(id), date, Double.Parse(amount)), "UPDATE");
            break;
        default: Console.WriteLine("Invalid input. Please only type one of the numbers in the menu.\n"); continue;
    }
}


string getUserInput(string prompt, string errorPrompt, Func<string, bool> validation)
{
    while (true)
    {
        Console.Write(prompt);
        string input = Console.ReadLine();
        bool isValid = validation(input);
        if (isValid)
        {
            return input;
        }
        else
        {
            Console.WriteLine($"\n{errorPrompt}\n");
        }
    }
}

async Task<bool> executeQuery(string query, string queryType)
{
    await connection.OpenAsync();

    //TODO: Check query validity

    //Run Query
    await using var command = new SqliteCommand(query, connection);

    try
    {
        await using var reader = await command.ExecuteReaderAsync();
        Console.WriteLine();
        switch (queryType.ToLower())
        {
            case "check":
                if (!reader.HasRows)
                {
                    return false;
                }
                break;
            case "update": Console.WriteLine("Record updated."); break;
            case "delete":
                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    Console.WriteLine("ID not found. No records deleted.");
                } else
                {
                    Console.WriteLine("Record deleted.");
                }
                break;
            case "insert": Console.WriteLine("Record added."); break;
            case "readall":
                if (reader.HasRows)
                {
                    Console.WriteLine("".PadLeft(50, '-'));
                    Console.WriteLine(
                        $"ID".PadLeft(15) +
                        "|" +
                        $"DATE".PadLeft(10) +
                        "|" +
                        $"AMOUNT"
                    );
                    while (await reader.ReadAsync())
                    {
                        Console.WriteLine(
                            $"{reader["ID"]}".PadLeft(15) +
                            "|" +
                            $"{reader["DATE"]}".PadLeft(10) +
                            "|" +
                            $"{reader["AMOUNT"]}"
                        );
                    }
                    Console.WriteLine("".PadLeft(50, '-'));
                }
                else
                {
                    Console.WriteLine("No records found.");
                }

                break;
        }
    }
    catch (SqliteException ex) {
        int errorCode = ex.SqliteErrorCode;
        string message = ex.Message;
        Console.WriteLine(message);
    } 
    finally
    {
        await connection.CloseAsync();
    }
    return true;
}

