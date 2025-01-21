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

//Menu
Console.WriteLine("""
    [Water Logger]

    What would you like to do?

    Type 0 to Close Application.
    Type 1 to View All Records.
    Type 2 to Insert Record.
    Type 3 to Delete Record.
    Type 4 to Update Record.
    ------------------------------------

    """);

Console.Write("Your choice: ");
var userChoice = Console.ReadLine();
Console.WriteLine();

//Input Processing
switch(userChoice)
{
    case "0": System.Environment.Exit(0); break;
    case "1": ViewAllRecords(); break;
    case "2": InsertRecord("1/20/2025", 1); break;
    case "3": DeleteRecord("1"); break;
    case "4": UpdateRecord("2", "1/21/2025", 10); break;
}

async void UpdateRecord(string id, string date, double amount)
{
    await connection.OpenAsync();
    await using var command = new SqliteCommand($"""
        UPDATE HABITS
        SET DATE = "{date}",
            AMOUNT = {amount}
        WHERE ID = {id}
        """, connection);
    await using var reader = await command.ExecuteReaderAsync();
    await connection.CloseAsync();
    Console.WriteLine($"Record [{id}] updated.");
}

async void DeleteRecord(string id)
{
    await connection.OpenAsync();
    await using var command = new SqliteCommand($"""
        DELETE FROM HABITS WHERE ID = {id}
        """, connection);
    await using var reader = await command.ExecuteReaderAsync();
    await connection.CloseAsync();
    Console.WriteLine($"Record [{id}] deleted.");
}

async void InsertRecord(string date, double amount)
{
    await connection.OpenAsync();
    await using var command = new SqliteCommand($"""
        INSERT INTO HABITS(DATE, AMOUNT)
        VALUES ("{date}", {amount});
        """, connection);
    await using var reader = await command.ExecuteReaderAsync();
    await connection.CloseAsync();
    Console.WriteLine($"Record inserted.");
}

async void ViewAllRecords()
{
    await connection.OpenAsync();
    await using var command = new SqliteCommand("""
    SELECT *
    FROM HABITS;
    """, connection);
    await using var reader = await command.ExecuteReaderAsync();

    Console.WriteLine("""
        ID  |   DATE          |   AMOUNT
        """);

    while (await reader.ReadAsync())
    {
        Console.WriteLine($"""
            {reader["ID"]}   |   {reader["DATE"]}     |   {reader["AMOUNT"]}
            """);
    }

    await connection.CloseAsync();
}

