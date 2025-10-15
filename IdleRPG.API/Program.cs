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

// Unit of Work 등록 (모든 Repository를 내부에서 관리)
builder.Services.AddScoped<IUnitOfWork, IdleRPG.Infrastructure.UnitOfWork.UnitOfWork>();

// 🔥 이 부분이 꼭 필요함!
builder.Services.AddDbContext<GameDBContext>(options =>
                                                 options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
