using System.Globalization;
using PetWorld.Core.Entities;

namespace PetWorld.Infrastructure.Data;

/// <summary>
/// Seeds product catalog from CSV file.
/// WHY CSV: Easy to update by non-technical staff, version-controllable.
/// </summary>
public static class ProductSeeder
{
    public static void LoadFromCsv(AppDbContext db)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "products.csv");
        if (!File.Exists(path))
        {
            Console.WriteLine("[Seeder] products.csv not found. Skipping seed.");
            return;
        }

        // DEMO BEHAVIOR: Reset products on each startup
        // PRODUCTION: Use migrations and conditional seeding
        if (db.Products.Any())
        {
            db.Products.RemoveRange(db.Products);
            db.SaveChanges();
        }

        var lines = File.ReadAllLines(path);
        var products = new List<Product>();

        foreach (var raw in lines)
        {
            var line = raw?.Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Skip CSV header
            if (line.StartsWith("Name;", StringComparison.OrdinalIgnoreCase))
                continue;

            var parts = line.Split(';');
            if (parts.Length < 3)
                continue;

            var name = parts[0].Trim();
            var priceText = parts[1].Trim().Replace(',', '.');
            var category = parts[2].Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category))
                continue;

            if (!decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                continue;

            products.Add(new Product
            {
                Name = name,
                Price = price,
                Category = category
            });
        }

        if (products.Any())
        {
            db.Products.AddRange(products);
            db.SaveChanges();
            Console.WriteLine($"[Seeder] Loaded {products.Count} products from CSV.");
        }
    }
}