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

    //Input Processing
    switch (userChoice)
    {
        case "0": System.Environment.Exit(0); break;
        case "1": await executeQuery(readAllRows("HABITS"), "READALL"); break;
        case "2":
            DateTime dateTime;
            while (true)
            {
                Console.Write("\nDate: ");
                string date = Console.ReadLine();
                if (DateTime.TryParse(date, out dateTime)) {
                    break;
                } else
                {
                    Console.WriteLine("\nInvalid date. Please use the format MM/DD/YYYY.");
                }
            }

            double am;
            while (true)
            {
                Console.Write("Amount: ");
                string amount = Console.ReadLine();
                if (Double.TryParse(amount, out am)) {
                    break;
                } else
                {
                    Console.WriteLine("""
                        \nInvalid amount. Please use valid numbers and dot '.' as separator if necessary.
                        """);
                }
            }
            await executeQuery(insert("HABITS", dateTime.ToShortDateString(), am), "INSERT");
            break;
        case "3":
            int id;
            while (true)
            {
                Console.Write("\nID of Record: ");
                string idIn = Console.ReadLine();
                if (Int32.TryParse(idIn, out id)) {
                    await executeQuery(deleteRow("HABITS", id), "DELETE");
                    break;
                } else
                {
                    Console.WriteLine("Id invalid. Please input a valid number.");
                }
            }
            break;
        case "4":
            while (true)
            {
                Console.Write("\nID of Record: ");
                string idIn = Console.ReadLine();
                if (Int32.TryParse(idIn, out id))
                {
                    var rowExist = await executeQuery(selectRow("HABITS", id), "CHECK");
                    if (!rowExist)
                    {
                        Console.WriteLine($"Row with ID {id} not found. Please input an existing ID.");
                        continue;
                    }
                    break;
                }
                else
                {
                    Console.WriteLine("Id invalid. Please input a valid number.");
                }
            }
            while (true)
            {
                Console.Write("Date: ");
                string date = Console.ReadLine();
                if (DateTime.TryParse(date, out dateTime))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("\nInvalid date. Please use the format MM/DD/YYYY.");
                }
            }
            while (true)
            {
                Console.Write("Amount: ");
                string amount = Console.ReadLine();
                if (Double.TryParse(amount, out am))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("""
                        \nInvalid amount. Please use valid numbers and dot '.' as separator if necessary.
                        """);
                }
            }
            await executeQuery(updateRow("HABITS", id, dateTime.ToShortDateString(), am), "UPDATE");
            break;
        default: Console.WriteLine("Invalid input. Please only type one of the numbers in the menu.\n"); continue;
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
        int rowsAffected = await command.ExecuteNonQueryAsync();
        await using var reader = await command.ExecuteReaderAsync();
        //Print prompt IF query was succesfully executed.
        //TODO: Check if query execution is a success.
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
                if (rowsAffected == 0)
                {
                    Console.WriteLine("ID not found. No records deleted.");
                }
                Console.WriteLine("Record deleted."); 
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
    } finally
    {
        await connection.CloseAsync();
    }
    return true;
}

