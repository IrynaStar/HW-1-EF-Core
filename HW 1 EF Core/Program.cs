using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using static HW_1_EF_Core.Program;

namespace HW_1_EF_Core;

public class Train
{
    public int Id { get; set; }
    public string TrainNumber { get; set; }
    public string DepartureStation { get; set; }
    public string ArrivalStation { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }

    public static Train[] Trains() => new Train[]
    {
        new Train
        {
            TrainNumber = "789C",
            DepartureStation = "Одесса",
            ArrivalStation = "Киев",
            DepartureTime = DateTime.Now.AddHours(2),
            ArrivalTime = DateTime.Now.AddHours(6)
        },
        new Train
        {
            TrainNumber = "562T",
            DepartureStation = "Мариуполь",
            ArrivalStation = "Львов",
            DepartureTime = DateTime.Now.AddHours(3),
            ArrivalTime = DateTime.Now.AddHours(8)
        }
    };
}
public class ApplicationContext : DbContext
{
    public DbSet<Train> Trains { get; set; } = null!;
    public ApplicationContext(DbContextOptions options) : base(options)
    {

    }
}

public class DatabaseService
{
    private DbContextOptions<ApplicationContext> GetConnectionOptions()
    {
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory());
        builder.AddJsonFile("appsettings.json");
        var config = builder.Build();
        string connestionString = config.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
        return optionsBuilder.UseSqlServer(connestionString).Options;
    }

    public async Task EnsurePopulate()
    {
        using (ApplicationContext db = new ApplicationContext(GetConnectionOptions()))
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Trains.AddRange(Train.Trains());
            await db.SaveChangesAsync();
        }
    }

    public async Task<Train> GetTrainById(int id)
    {
        using (ApplicationContext db = new ApplicationContext(GetConnectionOptions()))
        {
            return await db.Trains.FirstOrDefaultAsync(e => e.Id == id);
        }
    }

    public async Task AddTrain(Train train)
    {
        using (ApplicationContext db = new ApplicationContext(GetConnectionOptions()))
        {
            db.Trains.Add(train);
            await db.SaveChangesAsync();
        }
    }

    public async Task UpdateTrain(Train train)
    {
        using (ApplicationContext db = new ApplicationContext(GetConnectionOptions()))
        {
            db.Trains.Update(train);
            await db.SaveChangesAsync();
        }
    }

    public async Task RemoveTrain(Train train)
    {
        using (ApplicationContext db = new ApplicationContext(GetConnectionOptions()))
        {
            db.Trains.Remove(train);
            await db.SaveChangesAsync();
        }
    }

}

class Program
    {
    static async Task Main()
    {
        DatabaseService databaseService = new DatabaseService();
        await databaseService.EnsurePopulate();

        await databaseService.AddTrain(new Train
        {
            TrainNumber = "416У",
            DepartureStation = "Варшава",
            ArrivalStation = "Познань",
            DepartureTime = DateTime.Now.AddHours(3),
            ArrivalTime = DateTime.Now.AddHours(8)
        });

        var currentTrain = await databaseService.GetTrainById(3);
        if (currentTrain != null)
        {
            currentTrain.TrainNumber += "333TP";
            await databaseService.UpdateTrain(currentTrain);
        }

        var currentTraintoDelete = await databaseService.GetTrainById(2);
        if (currentTraintoDelete != null)
        {
            await databaseService.RemoveTrain(currentTraintoDelete);
        }
    }
}







