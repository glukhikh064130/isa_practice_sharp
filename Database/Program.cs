using Database.entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Database;

public enum Commands
{
    List,
    Create,
    Read,
    Update,
    Delete,
    Deal,
    Unknown,
    Exit,
}

public enum CommandsResult
{
    Continue,
    Exit,
}

public enum Entities
{
    Product,
    Customer,
    Deal,
    Unknown,
}

public class Program
{
    private const string CommandPrompt = $"{Help}. Enter the command:";
    private const string Help = "Commands list: [c,r,u,d,l,e]";

    public static void Main(string[] args)
    {
        var opts = GetDatabaseOpts<AppCtx>();
        var ctx = new AppCtx(opts);

        CommandsResult res;
        do
        {
            Console.WriteLine(CommandPrompt);
            var cmd = ParseCommand(Console.ReadLine());
            res = DoCommand(ctx, cmd);
        } while (res != CommandsResult.Exit);

        Console.WriteLine("See you later!");
    }

    private static DbContextOptions<T> GetDatabaseOpts<T>() where T : DbContext
    {
        var cfgBuilder = new ConfigurationBuilder();
        cfgBuilder.SetBasePath(Directory.GetCurrentDirectory());
        cfgBuilder.AddJsonFile("config.json");
        var cfg = cfgBuilder.Build();

        var connString = cfg.GetConnectionString("DefaultConnection");

        var optsBuilder = new DbContextOptionsBuilder<T>();
        return optsBuilder.UseNpgsql(connString).Options;
    }

    private static Commands ParseCommand(string? cmd)
    {
        if (cmd == null) return Commands.Unknown;

        return NormalizeInput(cmd) switch
        {
            "l" => Commands.List,
            "c" => Commands.Create,
            "r" => Commands.Read,
            "u" => Commands.Update,
            "d" => Commands.Delete,
            "deal" => Commands.Deal,
            "e" => Commands.Exit,
            _ => Commands.Unknown
        };
    }

    private static Entities ParseEntity(string? entity)
    {
        if (entity == null) return Entities.Unknown;

        return NormalizeInput(entity) switch
        {
            "p" => Entities.Product,
            "c" => Entities.Customer,
            "d" => Entities.Deal,
            _ => Entities.Unknown
        };
    }

    private static CommandsResult DoCommand<TContext>(TContext ctx, Commands cmd) where TContext : DbContext
    {
        switch (cmd)
        {
            case Commands.List:
                ListCommand(ctx);
                break;
            case Commands.Create:
                CreateCommand(ctx);
                break;
            case Commands.Read:
                ReadCommand(ctx);
                break;
            case Commands.Update:
                UpdateCommand(ctx);
                break;
            case Commands.Delete:
                DeleteCommand(ctx);
                break;
            case Commands.Deal:
                DealCommand(ctx);
                break;
            case Commands.Unknown:
                Console.WriteLine("Unknown command. {0}", Help);
                break;
            case Commands.Exit:
            default:
                return CommandsResult.Exit;
        }

        return CommandsResult.Continue;
    }

    private static void DealCommand<TContext>(TContext ctx) where TContext : DbContext
    {
        Console.WriteLine("Enter your customer ID:");
        var id = int.Parse(Console.ReadLine() ?? string.Empty);

        var customer = ctx.Find<Customer>(id);
        if (customer == null)
        {
            Console.WriteLine($"Customer #{id} not found");
            return;
        }
        
        Console.WriteLine($"Hello {customer.Name}! Your deals amount is: {customer.Deals.Count}");
        
        Console.WriteLine("Our store has these products:");
        foreach (var product in ctx.Set<Product>())
        {
            Console.WriteLine(product.ToString());
        }
        
        Console.WriteLine("What do you want to buy:");
        var productId = int.Parse(Console.ReadLine() ?? string.Empty);
        var productToBuy = ctx.Find<Product>(productId);
        if (productToBuy == null)
        {
            Console.WriteLine($"Product #{id} not found");
            return;
        }
        
        customer.Deals.Add(new Deal { Product = productToBuy, Amount = 1, Date = new DateOnly() });
        ctx.SaveChanges();
    }

    private static void ListCommand<TContext>(TContext ctx) where TContext : DbContext
    {
        Console.WriteLine("What do you want to see [p=products,c=customers,d=deals]:");
        var entity = ParseEntity(Console.ReadLine());

        switch (entity)
        {
            case Entities.Product:
                foreach (var product in ctx.Set<Product>())
                {
                    Console.WriteLine(product.ToString());
                }
                break;
            case Entities.Customer:
                foreach (var customer in ctx.Set<Customer>())
                {
                    Console.WriteLine(customer.ToString());
                }
                break;
            case Entities.Deal:
                foreach (var deal in ctx.Set<Deal>())
                {
                    Console.WriteLine(deal.ToString());
                }
                break;
            case Entities.Unknown:
            default:
                Console.WriteLine("Unknown entity");
                break;
        }
    }

    private static void CreateCommand<TContext>(TContext ctx) where TContext : DbContext
    {
        Console.WriteLine("What do you want to create [p=products,c=customers]:");
        var entity = ParseEntity(Console.ReadLine());
        
        switch (entity)
        {
            case Entities.Product:
                var product = new Product();
                Console.WriteLine("Enter good:");
                product.Good = Console.ReadLine() ?? "something";
                Console.WriteLine("Enter price:");
                product.Price = Double.Parse(Console.ReadLine() ?? "100.0");
                Console.WriteLine("Enter category:");
                product.Category = Console.ReadLine() ?? "all";

                ctx.Add(product);
                ctx.SaveChanges();
                break;
            case Entities.Customer:
                var customer = new Customer();
                Console.WriteLine("Enter name:");
                customer.Name = Console.ReadLine() ?? "anybody";

                ctx.Add(customer);
                ctx.SaveChanges();
                break;
            case Entities.Unknown:
            default:
                Console.WriteLine("Unknown entity");
                break;
        }
    }

    private static void ReadCommand<TContext>(TContext ctx) where TContext : DbContext
    {
        Console.WriteLine("What do you want to see [p=products,c=customers]:");
        var entity = ParseEntity(Console.ReadLine());
        Console.WriteLine("Enter ID:");
        var id = int.Parse(Console.ReadLine() ?? string.Empty);
        
        switch (entity)
        {
            case Entities.Product:
                var product = ctx.Find<Product>(id);
                Console.WriteLine(product?.ToString());
                break;
            case Entities.Customer:
                var customer = ctx.Find<Customer>(id);
                Console.WriteLine(customer?.ToString());
                break;
            case Entities.Unknown:
            default:
                Console.WriteLine("Unknown entity");
                break;
        }
    }

    private static void UpdateCommand<TContext>(TContext ctx) where TContext : DbContext
    {
        Console.WriteLine("What do you want to update [p=products,c=customers]:");
        var entity = ParseEntity(Console.ReadLine());
        Console.WriteLine("Enter ID:");
        var id = int.Parse(Console.ReadLine() ?? string.Empty);
        
        switch (entity)
        {
            case Entities.Product:
                var products = ctx.Set<Product>();
                var product = products.Find(id);
                if (product != null)
                {
                    Console.WriteLine($"Current product: {product}");
                    Console.WriteLine("Enter new good [empty = without changes]:");
                    var newGood = Console.ReadLine() ?? "";
                    if (newGood != "")
                    {
                        product.Good = newGood;
                    }

                    Console.WriteLine("Enter new price [empty = without changes]:");
                    var newPrice = Console.ReadLine() ?? "";
                    if (newPrice != "")
                    {
                        product.Price = Double.Parse(newPrice);
                    }
                    
                    Console.WriteLine("Enter new category [empty = without changes]:");
                    var newCategory = Console.ReadLine() ?? "";
                    if (newCategory != "")
                    {
                        product.Category = newCategory;
                    }

                    ctx.SaveChanges();
                }
                else
                {
                    Console.WriteLine($"Product #{id} not found");
                }
                break;
            case Entities.Customer:
                var customers = ctx.Set<Customer>();
                var customer = customers.Find(id);
                if (customer != null)
                {
                    Console.WriteLine($"Current customer: {customer}");
                    Console.WriteLine("Enter new name [empty = without changes]:");
                    var newName = Console.ReadLine() ?? "";
                    if (newName != "")
                    {
                        customer.Name = newName;
                    }

                    ctx.SaveChanges();
                }
                else
                {
                    Console.WriteLine($"Customer #{id} not found");
                }
                break;
            case Entities.Unknown:
            default:
                Console.WriteLine("Unknown entity");
                break;
        }
    }

    private static void DeleteCommand<TContext>(TContext ctx) where TContext : DbContext
    {
        Console.WriteLine("What do you want to remove [p=products,c=customers]:");
        var entity = ParseEntity(Console.ReadLine());
        Console.WriteLine("Enter ID:");
        var id = int.Parse(Console.ReadLine() ?? string.Empty);
        
        switch (entity)
        {
            case Entities.Product:
                var products = ctx.Set<Product>();
                var removedProduct = products.Find(id);
                if (removedProduct != null)
                {
                    products.Remove(removedProduct);
                    ctx.SaveChanges();
                    Console.WriteLine($"Product #{removedProduct.Id} has been removed!");
                }
                else
                {
                    Console.WriteLine($"Product #{id} is not exist!");
                }
                break;
            case Entities.Customer:
                var customers = ctx.Set<Customer>();
                var removedCustomer = customers.Find(id);
                if (removedCustomer != null)
                {
                    customers.Remove(removedCustomer);
                    ctx.SaveChanges();
                    Console.WriteLine($"Customer #{removedCustomer.Id} has been removed!");
                }
                else
                {
                    Console.WriteLine($"Customer #{id} is not exist!");
                }
                break;
            case Entities.Unknown:
            default:
                Console.WriteLine("Unknown entity");
                break;
        }
    }

    private static string? NormalizeInput(string? input)
    {
        return input?.Trim().ToLower();
    }
}
