using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdleRPG.Infrastructure.Data;

/// <summary>
/// EF Core Design-time DbContext Factory
/// EF Core CLI (dotnet ef migrations add/update)가 GameDBContext 인스턴스를 생성할 수 있도록 지원
/// </summary>
public class GameDBContextFactory : IDesignTimeDbContextFactory<GameDBContext>
{
    public GameDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GameDBContext>();

        // Design-time 연결 문자열 (로컬 개발 환경)
        // 실제 연결 문자열은 appsettings.json에서 읽어야 하지만,
        // EF Core CLI는 appsettings.json을 읽을 수 없으므로 하드코딩
        var connectionString = "Host=localhost;Database=idlerpg;Username=postgres;Password=CHANGE_ME";

        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsAssembly("IdleRPG.Infrastructure");
        });

        return new GameDBContext(optionsBuilder.Options);
    }
}
