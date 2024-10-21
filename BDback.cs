using Microsoft.EntityFrameworkCore;
using MyApiApp.Data;

public static class BDback
{
    public static async Task InitializeUserDatabase(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync(@"CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY AUTOINCREMENT, 
            Username TEXT NOT NULL, 
            PasswordHash TEXT NOT NULL, 
            Salt TEXT NOT NULL, 
            Token TEXT)");
    }

    public static async Task InitializeContactDatabase(ContactDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync(@"CREATE TABLE IF NOT EXISTS Contacts (
            Id INTEGER PRIMARY KEY AUTOINCREMENT, 
            Name TEXT NOT NULL, 
            PhoneNumber TEXT NOT NULL, 
            Email TEXT, 
            Address TEXT, 
            UserAddId INTEGER)");
    }

    public static async Task InitializeHistoryDatabase(HistoryDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync(@"CREATE TABLE IF NOT EXISTS History (
            Id INTEGER PRIMARY KEY AUTOINCREMENT, 
            UserId INTEGER NOT NULL, 
            HttpMethod TEXT NOT NULL, 
            Path TEXT NOT NULL, 
            Timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, 
            QueryString TEXT,
            BodyContent TEXT)");
    }
}