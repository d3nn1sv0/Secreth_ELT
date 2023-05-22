using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class WeatherDataContext : DbContext
{
    public DbSet<WeatherData> WeatherData { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = "Server = (localdb)\\mssqllocaldb; Database = BigDataWeather; Trusted_Connection = True; ";
        optionsBuilder.UseSqlServer(connectionString);
    }
}