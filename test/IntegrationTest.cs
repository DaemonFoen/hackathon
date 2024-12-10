using contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using hrdirector;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using Type = contracts.Type;

namespace Test
{
    public class TestServerFixture : IDisposable
    {
        private readonly SqliteConnection _connection;
        public IDbContextFactory<ApplicationDbContext> DbContextFactory { get; }

        public TestServerFixture()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var serviceProvider = new ServiceCollection()
                .AddDbContextFactory<ApplicationDbContext>(options =>
                    options.UseSqlite(_connection))
                .BuildServiceProvider();

            DbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

            using var context = DbContextFactory.CreateDbContext();
            context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _connection.Close();
        }
    }

    public class HarmonyServiceTests(TestServerFixture fixture) : IClassFixture<TestServerFixture>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory = fixture.DbContextFactory;

        [Fact(DisplayName = "Запись информации о мероприятии в БД")]
        public void CanAddHackathonAndPreferences()
        {
            // PREPARE
            using var context = _dbContextFactory.CreateDbContext();
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            const int hackathonId = 1234;
            var team = new Team(new Developer(1, Type.Junior), new Developer(1, Type.TeamLead));
            var preferences1 = new Preferences(hackathonId, new Developer(1, Type.Junior), [1]);
            var preferences2 = new Preferences(hackathonId, new Developer(1, Type.TeamLead), [1]);

            var service = new HarmonyService(_dbContextFactory, Mock.Of<IPublishEndpoint>(),Mock.Of<ILogger<HarmonyService>>());
            service.AddTeams(hackathonId, [team]);
            service.AddPreferences(hackathonId, preferences1);
            service.AddPreferences(hackathonId, preferences2);

            // ACTION
            var hackathon = context.Entities.FirstOrDefault(e => e.HackathonId == hackathonId);

            // ASSERT
            Assert.NotNull(hackathon);
            Assert.Equal(hackathonId, hackathon.HackathonId);
            Assert.Contains(hackathon.Teams, t => t.Junior.Id == 1);
            Assert.Contains(hackathon.Preferences, p => p.Developer.Id == 1);
        }

        [Fact(DisplayName = "Чтение информации о мероприятии из БД")]
        public void CanReadHackathonById()
        {
            // PREPARE
            using var context = _dbContextFactory.CreateDbContext();
            const int hackathonId = 5678;
            var entity = new Entity { HackathonId = hackathonId };
            context.Entities.Add(entity);
            context.SaveChanges();

            var service = new HarmonyService(_dbContextFactory, Mock.Of<IPublishEndpoint>(),Mock.Of<ILogger<HarmonyService>>());

            // ACTION
            var retrievedHackathon = service.GetHackathonById(hackathonId);

            // ASSERT
            Assert.NotNull(retrievedHackathon);
            Assert.Equal(hackathonId, retrievedHackathon!.HackathonId);
        }

        [Fact(DisplayName = "Расчёт и запись среднего гармонического")]
        public void CanCalculateAndSetHarmony()
        {
            // PREPARE
            using var context = _dbContextFactory.CreateDbContext();
            const int hackathonId = 3235;
            var team = new Team(new Developer(1, Type.Junior), new Developer(2, Type.TeamLead));
            var preferences1 = new Preferences(hackathonId, team.Junior, [team.TeamLead.Id]);
            var preferences2 = new Preferences(hackathonId, team.TeamLead, [team.Junior.Id]);

            var entity = new Entity
            {
                HackathonId = hackathonId,
                Teams = [team],
                Preferences = [preferences1, preferences2]
            };
            context.Entities.Add(entity);
            context.SaveChanges();

            var service = new HarmonyService(_dbContextFactory,Mock.Of<IPublishEndpoint>(), Mock.Of<ILogger<HarmonyService>>());

            // ACTION
            service.SetHarmony(hackathonId);

            // ASSERT
            var harmony = service.GetHackathonById(hackathonId)!.Harmony;
            Assert.Equal(1, harmony);
        }
    }
}