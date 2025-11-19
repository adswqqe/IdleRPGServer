using IdleRPG.Application.Auth.Services;
using IdleRPG.Application.Interfaces;
using IdleRPG.Application.Tokens.Services;
using IdleRPG.Infrastructure.Authentication;
using IdleRPG.Infrastructure.Data;
using IdleRPG.Infrastructure.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation;

// Npgsql DateTime 처리 설정 (UTC DateTime을 timestamp without time zone에 허용)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// MiniProfiler 추가 (N+1 쿼리 감지)
builder.Services.AddMiniProfiler(options =>
{
    options.RouteBasePath = "/profiler"; // UI 경로
    options.PopupRenderPosition = StackExchange.Profiling.RenderPosition.BottomLeft;
    options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;
}).AddEntityFramework();

builder.Services.AddControllers();

// MediatR 등록 (Command/Query Handler 자동 스캔)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(IdleRPG.Application.AssemblyMarker).Assembly));

// JWT 설정 바인딩
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

// JWT 인증 추가
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero // 시간 오차 없음
        };
        // SignalR을 위한 토큰 추출 (쿼리 파라미터에서)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/gamehub") || path.StartsWithSegments("/chat")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// 서비스 등록
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IdleRPG.Application.Character.Services.ICharacterService, IdleRPG.Infrastructure.Service.CharacterService>();

// ✅ Combat System Refactoring - Phase 1, 2, 3, 4
builder.Services.AddScoped<IdleRPG.Application.Combat.Services.ICombatService, IdleRPG.Infrastructure.Service.CombatService>();
builder.Services.AddScoped<IdleRPG.Application.BattleLog.Services.IBattleLogService, IdleRPG.Infrastructure.Service.BattleLogService>();
builder.Services.AddScoped<IdleRPG.Application.Interfaces.IStageService, IdleRPG.Infrastructure.Service.StageService>();
builder.Services.AddScoped<IdleRPG.Application.Interfaces.ISpecialDungeonService, IdleRPG.Infrastructure.Service.SpecialDungeonService>();

// Legacy Services (Phase 3에서 제거 예정)
builder.Services.AddScoped<IdleRPG.Application.Interfaces.IBattleService, IdleRPG.Infrastructure.Service.BattleService>();
builder.Services.AddScoped<IdleRPG.Application.Interfaces.IDungeonService, IdleRPG.Infrastructure.Service.DungeonService>();

builder.Services.AddScoped<IdleRPG.Application.Interfaces.IMonsterService, IdleRPG.Infrastructure.Service.MonsterService>();
builder.Services.AddScoped<IdleRPG.Application.Interfaces.IOfflineRewardService, IdleRPG.Infrastructure.Services.OfflineRewardService>();
builder.Services.AddScoped<IdleRPG.Application.Interfaces.IEquipmentService, IdleRPG.Infrastructure.Service.EquipmentService>();
builder.Services.AddScoped<IdleRPG.Application.Services.ISkillService, IdleRPG.Infrastructure.Services.SkillService>();
builder.Services.AddScoped<IdleRPG.Application.Services.IPetService, IdleRPG.Infrastructure.Services.PetService>();
builder.Services.AddScoped<IdleRPG.Application.Services.IChatService, IdleRPG.Infrastructure.Services.ChatService>();

// Memory Cache (Chat 쿨다운 관리용)
builder.Services.AddMemoryCache();

// SignalR 추가 (실시간 채팅용)
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 1024 * 100; // 100KB
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.KeepAliveInterval = TimeSpan.FromSeconds(30);
})
.AddNewtonsoftJsonProtocol(options =>
{
    // Unity 클라이언트와 호환성을 위해 Newtonsoft.Json + camelCase 사용
    options.PayloadSerializerSettings.ContractResolver =
        new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
});

// CORS 설정 (Unity 클라이언트 허용)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUnity", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // 모든 Origin 허용 (개발 환경용)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // SignalR requires credentials
    });
});

// Domain Services (순수 비즈니스 로직)
builder.Services.AddScoped<IdleRPG.Domain.Services.GachaLogicService>();
builder.Services.AddScoped<IdleRPG.Domain.Services.PetGachaService>();
builder.Services.AddScoped<IdleRPG.Domain.Services.LootCalculator>();
builder.Services.AddScoped<IdleRPG.Domain.Services.EloRatingService>(); // PVP Arena
builder.Services.AddSingleton<IdleRPG.Domain.Services.IRandomProvider, IdleRPG.Infrastructure.Services.SystemRandomProvider>();

// Unit of Work 등록 (모든 Repository를 내부에서 관리)
builder.Services.AddScoped<IUnitOfWork, IdleRPG.Infrastructure.UnitOfWork.UnitOfWork>();

// Character Repository (PvpController에서 사용)
builder.Services.AddScoped<IdleRPG.Domain.Repositories.ICharacterRepository, IdleRPG.Infrastructure.Repositories.CharacterRepository>();

// PVP Arena Repositories
builder.Services.AddScoped<IdleRPG.Domain.Repositories.IPvpSeasonRepository, IdleRPG.Infrastructure.Repositories.PvpSeasonRepository>();
builder.Services.AddScoped<IdleRPG.Domain.Repositories.IPvpRankingRepository, IdleRPG.Infrastructure.Repositories.PvpRankingRepository>();
builder.Services.AddScoped<IdleRPG.Domain.Repositories.IPvpMatchRepository, IdleRPG.Infrastructure.Repositories.PvpMatchRepository>();

// PVP Arena Application Services
builder.Services.AddScoped<IdleRPG.Application.Services.IPvpService, IdleRPG.Application.Services.PvpService>();
builder.Services.AddScoped<IdleRPG.Application.Services.IPvpSeasonService, IdleRPG.Application.Services.PvpSeasonService>();

// PVP Arena Infrastructure Services
builder.Services.AddScoped<IdleRPG.Application.Services.IPvpMatchmakingService, IdleRPG.Infrastructure.Services.PvpMatchmakingService>();
builder.Services.AddScoped<IdleRPG.Application.Services.IRedisCacheService, IdleRPG.Infrastructure.Services.RedisCacheService>();

// Redis 연결 (PVP 랭킹 캐시용, Singleton)
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
    return StackExchange.Redis.ConnectionMultiplexer.Connect(connectionString);
});

// FluentValidation (PVP Arena Validators 포함)
builder.Services.AddValidatorsFromAssemblyContaining<IdleRPG.Application.Validators.PvpMatchRequestValidator>();

// 🔥 이 부분이 꼭 필요함! (테스트 환경에서는 CustomWebApplicationFactory에서 In-Memory DB로 교체)
if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
{
    Console.WriteLine($"[Program.cs] 현재 환경: {builder.Environment.EnvironmentName}");
}

if (!builder.Environment.IsEnvironment("Testing"))
{
    if (builder.Environment.IsDevelopment())
    {
        Console.WriteLine("[Program.cs] PostgreSQL DbContext 등록");
    }
    builder.Services.AddDbContext<GameDBContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}
else
{
    Console.WriteLine("[Program.cs] Testing 환경 감지 - PostgreSQL DbContext 등록 Skip");
}

// Loot System DI 등록 (순수 확률 계산 로직만)
// LootCalculator는 나중에 Domain으로 이동 후 등록

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Swagger에서 JWT 테스트할 수 있도록 설정
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
var app = builder.Build();

// Seed Data 초기화 (모든 환경에서 실행, idempotent 설계로 안전)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<GameDBContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // 던전 스테이지 Seed Data 생성
        var dungeonSeeder = new IdleRPG.Infrastructure.Data.Seeders.DungeonStageSeeder(
            context,
            services.GetRequiredService<ILogger<IdleRPG.Infrastructure.Data.Seeders.DungeonStageSeeder>>());

        await dungeonSeeder.SeedAsync();

        // 스킬 템플릿 Seed Data 생성
        var skillSeeder = new IdleRPG.Infrastructure.Data.Seeders.SkillTemplateSeeder(
            context,
            services.GetRequiredService<ILogger<IdleRPG.Infrastructure.Data.Seeders.SkillTemplateSeeder>>());

        await skillSeeder.SeedAsync();

        // LootTable Seed Data 생성 (던전 보상 테이블)
        // ⚠️ DISABLED: LootTables/LootItems 테이블이 아직 구현되지 않음 (Drop System 미완성)
        // var lootTableSeeder = new IdleRPG.Infrastructure.Data.Seeders.LootTableSeeder(
        //     context,
        //     services.GetRequiredService<ILogger<IdleRPG.Infrastructure.Data.Seeders.LootTableSeeder>>());
        //
        // await lootTableSeeder.SeedAsync();

        // 펫 템플릿 Seed Data 생성
        var petTemplateSeeder = new IdleRPG.Infrastructure.Data.Seeders.PetTemplateSeeder(
            context,
            services.GetRequiredService<ILogger<IdleRPG.Infrastructure.Data.Seeders.PetTemplateSeeder>>());

        await petTemplateSeeder.SeedAsync();

        // PVP 시즌 Seed Data 생성
        var pvpSeasonSeeder = new IdleRPG.Infrastructure.Data.Seeders.PvpSeasonSeeder(
            context,
            services.GetRequiredService<ILogger<IdleRPG.Infrastructure.Data.Seeders.PvpSeasonSeeder>>());

        await pvpSeasonSeeder.SeedAsync();

        logger.LogInformation("Seed Data 초기화 완료");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Seed Data 초기화 중 오류 발생");
    }
}

// Configure the HTTP request pipeline.

// MiniProfiler 미들웨어 (가장 먼저 실행되어야 정확한 측정)
app.UseMiniProfiler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // 개발 환경에서만 HTTPS 리디렉션 (로컬 인증서 사용)
    app.UseHttpsRedirection();
}
else
{
    // 프로덕션에서도 Swagger 활성화 (필요시 제거)
    app.UseSwagger();
    app.UseSwaggerUI();
    // 프로덕션: HTTPS 리디렉션 비활성화 (리버스 프록시에서 처리)
    // 또는 Let's Encrypt 인증서 설정 후 활성화 가능
}

// CORS 미들웨어 (인증 전에 실행)
app.UseCors("AllowUnity");

// 인증/인가 미들웨어 (순서 중요!)
app.UseAuthentication(); // 먼저 인증
app.UseAuthorization();  // 그 다음 권한 체크

// Controllers 매핑
app.MapControllers();

// SignalR Hub 매핑 (인증 필요)
app.MapHub<IdleRPG.API.Hubs.ChatHub>("/chat").RequireAuthorization();

app.Run();

// ✅ WebApplicationFactory를 위한 public partial class 선언 (통합 테스트용)
public partial class Program { }
