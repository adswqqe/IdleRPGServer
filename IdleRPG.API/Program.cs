using IdleRPG.Application.Auth.Services;
using IdleRPG.Application.Interfaces;
using IdleRPG.Application.Services;
using IdleRPG.Application.Tokens.Services;
using IdleRPG.Infrastructure.Authentication;
using IdleRPG.Infrastructure.Caching;
using IdleRPG.Infrastructure.Data;
using IdleRPG.Infrastructure.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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
                    path.StartsWithSegments("/gamehub"))
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
builder.Services.AddScoped<IdleRPG.Application.Interfaces.IBattleService, IdleRPG.Infrastructure.Service.BattleService>();
builder.Services.AddScoped<IdleRPG.Application.Interfaces.IMonsterService, IdleRPG.Infrastructure.Service.MonsterService>();
builder.Services.AddScoped<IdleRPG.Application.Interfaces.IOfflineRewardService, IdleRPG.Infrastructure.Services.OfflineRewardService>();
builder.Services.AddScoped<IdleRPG.Application.Interfaces.IEquipmentService, IdleRPG.Infrastructure.Service.EquipmentService>();
builder.Services.AddScoped<IdleRPG.Application.Interfaces.IDungeonService, IdleRPG.Infrastructure.Service.DungeonService>();
builder.Services.AddScoped<ISkillService, IdleRPG.Infrastructure.Services.SkillService>();

// Domain Services (가챠 로직)
builder.Services.AddScoped<IdleRPG.Domain.Services.GachaLogicService>();
builder.Services.AddSingleton<IdleRPG.Domain.Services.IRandomProvider, IdleRPG.Infrastructure.Services.SystemRandomProvider>();

// Unit of Work 등록 (모든 Repository를 내부에서 관리)
builder.Services.AddScoped<IUnitOfWork, IdleRPG.Infrastructure.UnitOfWork.UnitOfWork>();

// 🔥 이 부분이 꼭 필요함!
builder.Services.AddDbContext<GameDBContext>(options =>
                                                 options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== Drop System DI 등록 =====
// [학습 포인트] IMemoryCache 등록 (ASP.NET Core 내장)
builder.Services.AddMemoryCache();  // Singleton으로 자동 등록됨

// [학습 포인트] 캐시 구현체 선택 (현재: InMemory, 미래: Redis)
builder.Services.AddScoped<ILootTableCache, InMemoryLootTableCache>();
// Redis 전환 시: builder.Services.AddScoped<ILootTableCache, RedisLootTableCache>();

// [학습 포인트] 순수 함수는 Transient (상태 없음, 매번 새 인스턴스)
builder.Services.AddTransient<IDropCalculator, DropCalculator>();

// [학습 포인트] 비즈니스 로직은 Scoped (요청당 1개 인스턴스)
builder.Services.AddScoped<IDropService, DropService>();

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

        logger.LogInformation("Seed Data 초기화 완료");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Seed Data 초기화 중 오류 발생");
    }
}

// Configure the HTTP request pipeline.
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

// 인증/인가 미들웨어 (순서 중요!)
app.UseAuthentication(); // 먼저 인증
app.UseAuthorization();  // 그 다음 권한 체크
app.MapControllers();
app.Run();
