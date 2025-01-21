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
Func<string, string> readAllQuery = (table) => $"SELECT * FROM {table};";
Func<string, string, double, string> insertQuery = (table, date, amount) =>
{
    return $"""
        INSERT INTO {table}(DATE, AMOUNT)
        VALUES("{date}", {amount});
    """;
};
Func<string, string, string, double, string> updateQuery = (table, id, date, amount) =>
{
    return $"""
        UPDATE {table}
        SET DATE = "{date}",
            AMOUNT = {amount}
        WHERE ID = {id};
    """;
};
Func<string, string, string> deleteQuery = (table, id) => $"DELETE FROM {table} WHERE ID = {id};";

switch(userChoice)
{
    case "0": System.Environment.Exit(0); break;
    case "1": executeQuery(readAllQuery("HABITS"), "READ"); break;
    case "2": executeQuery(insertQuery("HABITS", "1/20/2025", 1), "INSERT"); break;
    case "3": executeQuery(deleteQuery("HABITS", "1"), "DELETE"); break;
    case "4": executeQuery(updateQuery("HABITS", "3", "1/21/2025", 10), "UPDATE"); break;
}

async void executeQuery(string query, string queryType)
{
    await connection.OpenAsync();

    //TODO: Check query validity

    //Run Query
    await using var command = new SqliteCommand(query, connection);
    await using var reader = await command.ExecuteReaderAsync();

    //Print prompt IF query was succesfully executed.
    //TODO: Check if query execution is a success.
    switch(queryType.ToLower())
    {
        case "update": Console.WriteLine("Record updated."); break;
        case "delete": Console.WriteLine("Record deleted."); break;
        case "insert": Console.WriteLine("Record added."); break;
        case "read":
            Console.WriteLine("""
            ID  |   DATE          |   AMOUNT
            """);

            while (await reader.ReadAsync())
            {
                Console.WriteLine($"""
            {reader["ID"]}   |   {reader["DATE"]}     |   {reader["AMOUNT"]}
            """);
            }

            break;
    }
    await connection.CloseAsync();
}

