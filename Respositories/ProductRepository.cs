using CarvedRockFitness.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CarvedRockFitness.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly string? _connectionString;
    private readonly bool _useSampleData;

    public ProductRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? Environment.GetEnvironmentVariable("DefaultConnection");
        _useSampleData = string.IsNullOrEmpty(_connectionString);
    }

    public async Task<IEnumerable<Product?>> GetAllAsync()
    {
        if (_useSampleData)
        {
            return GetSampleData();
        }

        await InitializeDatabaseIfEmpty();
        var products = new List<Product>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = new SqlCommand("SELECT Id, Name, Description, ImageUrl, Price, Category FROM Products", connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            products.Add(new Product
            {
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("Name"),
                Description = reader.GetString("Description"),
                ImageUrl = reader.GetString("ImageUrl"),
                Price = reader.GetDecimal("Price"),
                Category = reader.GetString("Category")
            });
        }

        return products;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        if (_useSampleData)
        {
            return GetSampleData().FirstOrDefault(p => p.Id == id);
        }

        await InitializeDatabaseIfEmpty();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("SELECT Id, Name, Description, ImageUrl, Price, Category FROM Products WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Product
            {
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("Name"),
                Description = reader.GetString("Description"),
                ImageUrl = reader.GetString("ImageUrl"),
                Price = reader.GetDecimal("Price"),
                Category = reader.GetString("Category")
            };
        }
        return null;
    }

    public async Task<IEnumerable<Product?>> GetByCategoryAsync(string? category)
    {
        if (_useSampleData)
        {
            var sampleData = GetSampleData();
            return string.IsNullOrEmpty(category) ? sampleData : sampleData.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        await InitializeDatabaseIfEmpty();
        var products = new List<Product>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = string.IsNullOrEmpty(category)
        ? "SELECT Id, Name, Description, ImageUrl, Price, Category FROM Products"
        : "SELECT Id, Name, Description, ImageUrl, Price, Category FROM Products WHERE Category = @Category";
        var command = new SqlCommand(query, connection);
        if (!string.IsNullOrEmpty(category))
        {
            command.Parameters.AddWithValue("@Category", category);
        }

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(new Product
            {
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("Name"),
                Description = reader.GetString("Description"),
                ImageUrl = reader.GetString("ImageUrl"),
                Price = reader.GetDecimal("Price"),
                Category = reader.GetString("Category")
            });
        }

        return products;
    }

    private async Task InitializeDatabaseIfEmpty()
    {
        if (string.IsNullOrEmpty(_connectionString)) return;

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // Check if Products table exists
        var checkTableCommand = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products'", connection);
        int tableCount = (int)await checkTableCommand.ExecuteScalarAsync();

        if (tableCount == 0)
        {
            // Load and execute CreateData.sql content
            string createDataSql = @"
                CREATE TABLE Products (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    Name NVARCHAR(100) NOT NULL,
                    Description NVARCHAR(500),
                    Category NVARCHAR(100),
                    ImageUrl NVARCHAR(200),
                    Price DECIMAL(10,2) NOT NULL
                );

                CREATE TABLE CartItems (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    UserId NVARCHAR(128),
                    ProductId INT NOT NULL,
                    ProductName NVARCHAR(100) NOT NULL,
                    Price DECIMAL(10,2) NOT NULL,
                    Quantity INT NOT NULL,
                    AddedAt DATETIME NOT NULL,
                    FOREIGN KEY (ProductId) REFERENCES Products(Id)
                );

                INSERT INTO Products (Name, Description, Category, ImageUrl, Price)
                VALUES 
                    ('PeakPulse Hiking Boots', 'Durable brown leather hiking boots with rugged soles, perfect for tough trails. Features ankle support and breathable lining.', 'Footwear', 'images/products/boots/shutterstock_66842440.jpg', 79.99),
                    ('TrailTrek Hiking Sandals', 'Lightweight tan hiking sandals with ventilated design and adjustable straps, ideal for warm-weather adventures.', 'Footwear', 'images/products/boots/shutterstock_222721876.jpg', 59.99),
                    ('SummitStrider Trail Shoes', 'Sturdy gray trail shoes with pink accents, offering excellent grip and flexibility for all-terrain hikes.', 'Footwear', 'images/products/boots/shutterstock_1121278055.jpg', 69.99),
                    ('ClimbForce Climbing Shoes', 'Precision-fit climbing shoes with sticky rubber soles, designed for enhanced grip on rocky surfaces.', 'Footwear', 'images/products/boots/shutterstock_475046062.jpg', 89.99),
                    ('PeakLock Carabiner', 'Lightweight gold carabiner with screw-lock gate, rated for 23kN, ideal for secure climbing connections.', 'Equipment', 'images/products/climbing gear/shutterstock_362174360.jpg', 19.99),
                    ('SafePeak Helmet White', 'Ventilated white climbing helmet with adjustable straps, offering lightweight protection for all-day comfort.', 'Equipment', 'images/products/climbing gear/shutterstock_569026084.jpg', 49.99),
                    ('SafePeak Helmet Yellow', 'Bright yellow climbing helmet with durable shell and ventilation, perfect for visibility and safety on climbs.', 'Equipment', 'images/products/climbing gear/shutterstock_569026084.jpg', 49.99),
                    ('SafePeak Helmet Red', 'Red climbing helmet with integrated headlamp, designed for caving and night climbs with impact resistance.', 'Equipment', 'images/products/climbing gear/shutterstock_64998481.jpg', 59.99),
                    ('ClimbSafe Gear Set', 'Complete climbing kit with belay device, figure-eight descender, quickdraws, and carabiners for secure ascents.', 'Equipment', 'images/products/climbing gear/shutterstock_279617825.jpg', 99.99),
                    ('IceGrip Crampons', 'Durable steel crampons with adjustable straps, designed for secure footing on icy and steep terrain.', 'Equipment', 'images/products/climbing gear/shutterstock_362683778.jpg', 79.99),
                    ('FrostBite Ice Axe', 'Lightweight ice axe with ergonomic grip, perfect for ice climbing and mountaineering stability.', 'Equipment', 'images/products/climbing gear/shutterstock_236845636.jpg', 89.99),
                    ('PeakPulse Backpack', '50L orange and black climbing backpack with multiple compartments, ideal for carrying gear on long expeditions.', 'Equipment', 'images/products/climbing gear/shutterstock_48040747.jpg', 129.99);";

            using var command = new SqlCommand(createDataSql, connection);
            await command.ExecuteNonQueryAsync();
        }
    }

    private IEnumerable<Product> GetSampleData()
    {
        return new List<Product>
        {
            new Product { Id = 1, Name = "Sample Product 1", Description = "Sample Product Description 1", ImageUrl = "images/products/no-sign_red.png", Price = 9.99m, Category = "Clothing" },
            new Product { Id = 2, Name = "Sample Product 2", Description = "Sample Product Description 2", ImageUrl = "images/products/no-sign_red.png", Price = 19.99m, Category = "Clothing" },
            new Product { Id = 3, Name = "Sample Product 3", Description = "Sample Product Description 3", ImageUrl = "images/products/no-sign_red.png", Price = 29.99m, Category = "Clothing" },
            new Product { Id = 4, Name = "Sample Product 4", Description = "Sample Product Description 4", ImageUrl = "images/products/no-sign_red.png", Price = 39.99m, Category = "Footwear" },
            new Product { Id = 5, Name = "Sample Product 5", Description = "Sample Product Description 5", ImageUrl = "images/products/no-sign_red.png", Price = 49.99m, Category = "Footwear" },
            new Product { Id = 6, Name = "Sample Product 6", Description = "Sample Product Description 6", ImageUrl = "images/products/no-sign_red.png", Price = 59.99m, Category = "Footwear" },
            new Product { Id = 7, Name = "Sample Product 7", Description = "Sample Product Description 7", ImageUrl = "images/products/no-sign_red.png", Price = 69.99m, Category = "Equipment" },
            new Product { Id = 8, Name = "Sample Product 8", Description = "Sample Product Description 8", ImageUrl = "images/products/no-sign_red.png", Price = 79.99m, Category = "Equipment" },
            new Product { Id = 9, Name = "Sample Product 9", Description = "Sample Product Description 9", ImageUrl = "images/products/no-sign_red.png", Price = 89.99m, Category = "Equipment" }
        };
    }
}