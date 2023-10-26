using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using Serilog;

public class CsvRecord
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string StreetNumber { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }
    public string PhoneNumber { get; set; }
    public string EmailAddress { get; set; }
    public string Date { get; set; }
}
public class Program
{
    public static int ValidRowCount = 0;
    public static int SkippedRowCount = 0;
    public static List<CsvRecord> AllRecords = new List<CsvRecord>();

    public static void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        string rootDirectory = @"Sample Data";

        var watch = System.Diagnostics.Stopwatch.StartNew();

        TraverseDirectory(rootDirectory);

        // Write all records to a single CSV file
        using (var writer = new StreamWriter("output.csv"))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
        {
            csv.WriteRecords(AllRecords);
        }

        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;

        Log.Information($"Total Execution Time: {elapsedMs} ms");
        Log.Information($"Total Valid Rows: {ValidRowCount}");
        Log.Information($"Total Skipped Rows: {SkippedRowCount}");
    }

    public static void ProcessCsvFile(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
        {
            try
            {
                var records = csv.GetRecords<CsvRecord>();
                foreach (var record in records)
                {
                    if (IsValid(record))
                    {
                        ValidRowCount++;
                        AllRecords.Add(record); 
                    }
                    else
                    {
                        SkippedRowCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error processing file: {filePath}");
            }
        }
    }

    public static bool IsValid(CsvRecord record)
    {
        return !string.IsNullOrWhiteSpace(record.FirstName) &&
               !string.IsNullOrWhiteSpace(record.LastName) &&
               !string.IsNullOrWhiteSpace(record.StreetNumber) &&
               !string.IsNullOrWhiteSpace(record.Street) &&
               !string.IsNullOrWhiteSpace(record.City) &&
               !string.IsNullOrWhiteSpace(record.Province) &&
               !string.IsNullOrWhiteSpace(record.Country) &&
               !string.IsNullOrWhiteSpace(record.PostalCode) &&
               !string.IsNullOrWhiteSpace(record.PhoneNumber) &&
               !string.IsNullOrWhiteSpace(record.EmailAddress);
    }

    public static void TraverseDirectory(string directoryPath)
    {
        string[] files = Directory.GetFiles(directoryPath, "*.csv");
        foreach (string file in files)
        {
            ProcessCsvFile(file);
        }

        string[] subDirectories = Directory.GetDirectories(directoryPath);
        foreach (string subDirectory in subDirectories)
        {
            TraverseDirectory(subDirectory);
        }
    }
}
