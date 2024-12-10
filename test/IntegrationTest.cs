using contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using hrdirector;
using hrdirector.Entities;
using MassTransit;
using Microsoft.Data.Sqlite;
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
            using var context = _dbContextFactory.CreateDbContext();
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            var service = new HarmonyService(_dbContextFactory, Mock.Of<IPublishEndpoint>());
            service.StartHackathonsAsync(1, new TimeSpan(0)).GetAwaiter().GetResult();
            var hackathonId = context.Entities.First().HackathonId;
            var team = new Team(new Developer(1, Type.Junior), new Developer(1, Type.TeamLead));
            var preferences1 = new Preferences(hackathonId, new Developer(1, Type.Junior), [1]);
            var preferences2 = new Preferences(hackathonId, new Developer(1, Type.TeamLead), [1]);
            service.AddPreferences(hackathonId, preferences1);
            service.AddPreferences(hackathonId, preferences2);
            service.AddTeams(hackathonId, [team]);

            var hackathon = context.Entities.FirstOrDefault(e => e.HackathonId == hackathonId);

            Assert.NotNull(hackathon);
            Assert.Equal(hackathonId, hackathon.HackathonId);
            Assert.Contains(hackathon.Teams, t => t.Junior.Id == 1);
            Assert.Contains(hackathon.Preferences, p => p.Developer.Id == 1);
        }

        [Fact(DisplayName = "Чтение информации о мероприятии из БД")]
        public void CanReadHackathonById()
        {
            using var context = _dbContextFactory.CreateDbContext();
            const int hackathonId = 5678;
            var hackathon  = new Hackathon { HackathonId = hackathonId };
            context.Entities.Add(hackathon);
            context.SaveChanges();
            var service = new HarmonyService(_dbContextFactory, Mock.Of<IPublishEndpoint>());

            var retrievedHackathon = service.GetHackathonById(hackathonId);

            Assert.NotNull(retrievedHackathon);
            Assert.Equal(hackathonId, retrievedHackathon!.HackathonId);
        }

        [Fact(DisplayName = "Расчёт и запись среднего гармонического")]
        public void CanCalculateAndSetHarmony()
        {
            using var context = _dbContextFactory.CreateDbContext();
            const int hackathonId = 3235;
            var team = new Team(new Developer(1, Type.Junior), new Developer(2, Type.TeamLead));
            var preferences1 = new Preferences(hackathonId, team.Junior, [team.TeamLead.Id]);
            var preferences2 = new Preferences(hackathonId, team.TeamLead, [team.Junior.Id]);
            var hackathon = new Hackathon()
            {
                HackathonId = hackathonId,
                Teams = [team],
                Preferences = [preferences1, preferences2]
            };
            context.Entities.Add(hackathon);
            context.SaveChanges();
            var service = new HarmonyService(_dbContextFactory,Mock.Of<IPublishEndpoint>());

            service.SetHarmony(hackathonId);

            var harmony = service.GetHackathonById(hackathonId)!.Harmony;
            Assert.Equal(1, harmony);
        }
    }
}