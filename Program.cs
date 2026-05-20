using SighApp.Data;
using SighApp.Repository;
using SighApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register ADO.NET SqlDatabaseConnection
builder.Services.AddSingleton<SqlDatabaseConnection>();

// Register Repositories
builder.Services.AddScoped<IEspecialidadRepository, EspecialidadRepository>();
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IMedicoRepository, MedicoRepository>();
builder.Services.AddScoped<ICitaRepository, CitaRepository>();
builder.Services.AddScoped<IDiagnosticoRepository, DiagnosticoRepository>();
builder.Services.AddScoped<ITratamientoRepository, TratamientoRepository>();

// Register HttpClient & External API Service
builder.Services.AddHttpClient<IExternalApiService, ExternalApiService>();

// Register Business Services
builder.Services.AddScoped<ICitaService, CitaService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Auto-create database tables on startup if they don't exist
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbConnection = scope.ServiceProvider.GetRequiredService<SighApp.Data.SqlDatabaseConnection>();
        using var conn = dbConnection.CreateConnection();
        conn.Open();
        
        using var checkCmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Pacientes'", conn);
        int count = (int)checkCmd.ExecuteScalar();
        if (count == 0)
        {
            var sqlPath = System.IO.Path.Combine(builder.Environment.ContentRootPath, "Data", "SighDatabase.sql");
            if (System.IO.File.Exists(sqlPath))
            {
                var lines = System.IO.File.ReadAllLines(sqlPath);
                var sqlBlocks = new System.Collections.Generic.List<string>();
                var currentBlock = new System.Text.StringBuilder();
                
                // Skip first 18 lines to avoid creating database, just create tables
                for (int i = 19; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
                    {
                        sqlBlocks.Add(currentBlock.ToString());
                        currentBlock.Clear();
                    }
                    else
                    {
                        currentBlock.AppendLine(line);
                    }
                }
                if (currentBlock.Length > 0) sqlBlocks.Add(currentBlock.ToString());
                
                foreach (var block in sqlBlocks)
                {
                    if (!string.IsNullOrWhiteSpace(block))
                    {
                        using var cmd = new Microsoft.Data.SqlClient.SqlCommand(block, conn);
                        cmd.ExecuteNonQuery();
                    }
                }
                Console.WriteLine("Base de datos inicializada correctamente con las tablas.");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error al inicializar la base de datos: " + ex.Message);
    }
}

app.Run();