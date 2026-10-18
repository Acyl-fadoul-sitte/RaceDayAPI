

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDayAPI.Controllers;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using Xunit;

namespace RaceDayAPI.Tests;

public class UnitTest1
{
    // =========================================================
    // TEST HELPERS
    // =========================================================

    private static (RaceDayContext Context, AuthController Controller)
        CreateController()
    {
        var options = new DbContextOptionsBuilder<RaceDayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new RaceDayContext(options);

        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "ThisIsATestOnlySecretKeyThatIsLongEnough123!",
            ["Jwt:Issuer"] = "RaceDayAPI",
            ["Jwt:Audience"] = "RaceDayAPIUsers"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var controller = new AuthController(context, configuration)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return (context, controller);
    }

    private static (RaceDayContext Context, EventsController Controller)
        CreateEventsController(int userId, string role)
    {
        var options = new DbContextOptionsBuilder<RaceDayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new RaceDayContext(options);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, "Test"));

        var controller = new EventsController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            }
        };

        return (context, controller);
    }

    private static (RaceDayContext Context, EnrolmentsController Controller)
        CreateEnrolmentsController(int userId, string role)
    {
        var options = new DbContextOptionsBuilder<RaceDayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new RaceDayContext(options);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, "Test"));

        var controller = new EnrolmentsController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            }
        };

        return (context, controller);
    }

    private static (RaceDayContext Context, CategoriesController Controller)
        CreateCategoriesController(int userId, string role)
    {
        var options = new DbContextOptionsBuilder<RaceDayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new RaceDayContext(options);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, "Test"));

        var controller = new CategoriesController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            }
        };

        return (context, controller);
    }

    private static (RaceDayContext Context, ResultsController Controller)
        CreateResultsController(int userId, string role)
    {
        var options = new DbContextOptionsBuilder<RaceDayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new RaceDayContext(options);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, "Test"));

        var controller = new ResultsController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            }
        };

        return (context, controller);
    }

    // =========================================================
    // AUTHENTICATION TESTS: 1-6
    // =========================================================

    // TEST 1: A valid participant can register.
    [Fact]
    public async Task Register_WithValidParticipant_ReturnsCreated()
    {
        var (context, controller) = CreateController();

        try
        {
            var result = await controller.Register(new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant@example.com",
                Password = "TestPassword123!",
                PhoneNumber = "0123456789",
                Role = "Participant"
            });

            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(1, await context.Users.CountAsync());

            var user = await context.Users.FirstAsync();

            Assert.Equal("Participant", user.Role);
            Assert.NotEqual("TestPassword123!", user.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify(
                "TestPassword123!", user.PasswordHash));
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 2: Registration rejects an invalid role.
    [Fact]
    public async Task Register_WithInvalidRole_ReturnsBadRequest()
    {
        var (context, controller) = CreateController();

        try
        {
            var result = await controller.Register(new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "invalidrole@example.com",
                Password = "TestPassword123!",
                Role = "Administrator"
            });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(await context.Users.ToListAsync());
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 3: Correct login credentials return a JWT.
    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsToken()
    {
        var (context, controller) = CreateController();

        try
        {
            context.Users.Add(new User
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    "TestPassword123!"),
                PhoneNumber = "0123456789",
                Role = "Participant"
            });

            await context.SaveChangesAsync();

            var result = await controller.Login(new LoginRequest
            {
                Email = "participant@example.com",
                Password = "TestPassword123!"
            });

            var response = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(response.Value);

            var token = response.Value.GetType()
                .GetProperty("token")!
                .GetValue(response.Value) as string;

            Assert.False(string.IsNullOrWhiteSpace(token));
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 4: An incorrect password is rejected.
    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var (context, controller) = CreateController();

        try
        {
            context.Users.Add(new User
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    "CorrectPassword123!"),
                PhoneNumber = "0123456789",
                Role = "Participant"
            });

            await context.SaveChangesAsync();

            var result = await controller.Login(new LoginRequest
            {
                Email = "participant@example.com",
                Password = "WrongPassword123!"
            });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 5: Duplicate registration email is rejected.
    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var (context, controller) = CreateController();

        try
        {
            context.Users.Add(new User
            {
                FirstName = "First",
                LastName = "User",
                Email = "duplicate@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                PhoneNumber = "0123456789",
                Role = "Participant"
            });

            await context.SaveChangesAsync();

            var result = await controller.Register(new RegisterRequest
            {
                FirstName = "Second",
                LastName = "User",
                Email = "duplicate@example.com",
                Password = "AnotherPassword123!",
                PhoneNumber = "0123456788",
                Role = "Participant"
            });

            Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(1, await context.Users.CountAsync());
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 6: Login rejects a missing email.
    [Fact]
    public async Task Login_WithMissingEmail_ReturnsBadRequest()
    {
        var (context, controller) = CreateController();

        try
        {
            var result = await controller.Login(new LoginRequest
            {
                Email = "",
                Password = "TestPassword123!"
            });

            Assert.IsType<BadRequestObjectResult>(result);
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // =========================================================
    // EVENT TESTS: 7-10
    // =========================================================

    // TEST 7: An organiser can create an event.
    [Fact]
    public async Task CreateEvent_WithValidDetails_ReturnsCreated()
    {
        var (context, controller) = CreateEventsController(1, "Organiser");

        try
        {
            context.Users.Add(new User
            {
                Id = 1,
                FirstName = "Test",
                LastName = "Organiser",
                Email = "organiser@example.com",
                PasswordHash = "test-hash",
                PhoneNumber = "0123456789",
                Role = "Organiser"
            });

            await context.SaveChangesAsync();

            var result = await controller.CreateEvent(new EventRequest
            {
                Name = "Test Running Event",
                Description = "A test running event",
                EventDate = DateTime.UtcNow.Date.AddDays(30),
                Location = "Pretoria",
                EventType = "Running",
                DistanceKm = 10
            });

            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(1, await context.Events.CountAsync());

            var savedEvent = await context.Events.FirstAsync();

            Assert.Equal("Test Running Event", savedEvent.Name);
            Assert.Equal(1, savedEvent.OrganiserId);
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 8: An event with zero distance is rejected.
    [Fact]
    public async Task CreateEvent_WithZeroDistance_ReturnsBadRequest()
    {
        var (context, controller) = CreateEventsController(1, "Organiser");

        try
        {
            context.Users.Add(new User
            {
                Id = 1,
                FirstName = "Test",
                LastName = "Organiser",
                Email = "organiser@example.com",
                PasswordHash = "test-hash",
                PhoneNumber = "0123456789",
                Role = "Organiser"
            });

            await context.SaveChangesAsync();

            var result = await controller.CreateEvent(new EventRequest
            {
                Name = "Invalid Event",
                Description = "An event with invalid distance",
                EventDate = DateTime.UtcNow.Date.AddDays(30),
                Location = "Pretoria",
                EventType = "Running",
                DistanceKm = 0
            });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(await context.Events.ToListAsync());
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 9: An organiser cannot update another organiser's event.
    [Fact]
    public async Task UpdateEvent_WhenNotOwner_ReturnsForbidden()
    {
        var (context, controller) = CreateEventsController(1, "Organiser");

        try
        {
            context.Events.Add(new RaceDayAPI.Models.Event
            {
                Id = 10,
                Name = "Another Organiser Event",
                Description = "An event owned by another organiser",
                EventDate = DateTime.UtcNow.Date.AddDays(30),
                Location = "Pretoria",
                EventType = "Running",
                DistanceKm = 10,
                OrganiserId = 2
            });

            await context.SaveChangesAsync();

            var result = await controller.UpdateEvent(10, new EventRequest
            {
                Name = "Attempted Change",
                Description = "Trying to change another organiser's event",
                EventDate = DateTime.UtcNow.Date.AddDays(30),
                Location = "Pretoria",
                EventType = "Running",
                DistanceKm = 10
            });

            Assert.IsType<ForbidResult>(result);

            var savedEvent = await context.Events.FindAsync(10);

            Assert.NotNull(savedEvent);
            Assert.Equal("Another Organiser Event", savedEvent.Name);
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 10: An organiser cannot delete another organiser's event.
    [Fact]
    public async Task DeleteEvent_WhenNotOwner_ReturnsForbidden()
    {
        var (context, controller) = CreateEventsController(1, "Organiser");

        try
        {
            context.Events.Add(new RaceDayAPI.Models.Event
            {
                Id = 10,
                Name = "Another Organiser Event",
                Description = "An event owned by another organiser",
                EventDate = DateTime.UtcNow.Date.AddDays(30),
                Location = "Pretoria",
                EventType = "Running",
                DistanceKm = 10,
                OrganiserId = 2
            });

            await context.SaveChangesAsync();

            var result = await controller.DeleteEvent(10);

            Assert.IsType<ForbidResult>(result);
            Assert.Equal(1, await context.Events.CountAsync());
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // =========================================================
    // ENROLMENT TESTS: 11-13
    // =========================================================

    // TEST 11: A participant can enrol in a valid event category.
    [Fact]
    public async Task CreateEnrolment_WithValidDetails_SavesEnrolment()
    {
        var (context, controller) =
            CreateEnrolmentsController(2, "Participant");

        try
        {
            context.Users.AddRange(
                new User
                {
                    Id = 2,
                    FirstName = "Test",
                    LastName = "Participant",
                    Email = "participant@example.com",
                    PasswordHash = "test-hash",
                    PhoneNumber = "0123456789",
                    Role = "Participant"
                },
                new User
                {
                    Id = 1,
                    FirstName = "Test",
                    LastName = "Organiser",
                    Email = "organiser@example.com",
                    PasswordHash = "test-hash",
                    PhoneNumber = "0123456788",
                    Role = "Organiser"
                });

            context.Events.Add(new RaceDayAPI.Models.Event
            {
                Id = 10,
                Name = "Test Race",
                Description = "Test race description",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Pretoria",
                EventType = "Running",
                DistanceKm = 10,
                OrganiserId = 1
            });

            context.Categories.Add(new Category
            {
                Id = 20,
                EventId = 10,
                Name = "10 km",
                Description = "10 kilometre category",
                EntryFee = 100,
                MaxParticipants = 100
            });

            await context.SaveChangesAsync();

            var result = await controller.CreateEnrolment(
                new EnrolmentRequest
                {
                    EventId = 10,
                    CategoryId = 20
                });

            Assert.IsType<CreatedAtActionResult>(result);

            var saved = await context.Enrolments.SingleAsync();

            Assert.Equal(2, saved.ParticipantId);
            Assert.Equal(10, saved.EventId);
            Assert.Equal(20, saved.CategoryId);
            Assert.Equal("Active", saved.Status);
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 12: A category belonging to another event is rejected.
    [Fact]
    public async Task CreateEnrolment_WhenCategoryBelongsToAnotherEvent_ReturnsBadRequest()
    {
        var (context, controller) =
            CreateEnrolmentsController(2, "Participant");

        try
        {
            context.Users.Add(new User
            {
                Id = 2,
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant@example.com",
                PasswordHash = "test-hash",
                PhoneNumber = "0123456789",
                Role = "Participant"
            });

            context.Events.AddRange(
                new RaceDayAPI.Models.Event
                {
                    Id = 10,
                    Name = "First Race",
                    Description = "First race",
                    EventDate = DateTime.UtcNow.AddDays(30),
                    Location = "Pretoria",
                    EventType = "Running",
                    DistanceKm = 10,
                    OrganiserId = 1
                },
                new RaceDayAPI.Models.Event
                {
                    Id = 11,
                    Name = "Second Race",
                    Description = "Second race",
                    EventDate = DateTime.UtcNow.AddDays(40),
                    Location = "Johannesburg",
                    EventType = "Walking",
                    DistanceKm = 5,
                    OrganiserId = 1
                });

            context.Categories.Add(new Category
            {
                Id = 20,
                EventId = 11,
                Name = "Walking",
                Description = "Walking category",
                EntryFee = 50,
                MaxParticipants = 100
            });

            await context.SaveChangesAsync();

            var result = await controller.CreateEnrolment(
                new EnrolmentRequest
                {
                    EventId = 10,
                    CategoryId = 20
                });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(await context.Enrolments.ToListAsync());
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 13: A participant cannot enrol twice in the same event.
    [Fact]
    public async Task CreateEnrolment_WhenAlreadyEnrolled_ReturnsConflict()
    {
        var (context, controller) =
            CreateEnrolmentsController(2, "Participant");

        try
        {
            context.Users.Add(new User
            {
                Id = 2,
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant@example.com",
                PasswordHash = "test-hash",
                PhoneNumber = "0123456789",
                Role = "Participant"
            });

            context.Events.Add(new RaceDayAPI.Models.Event
            {
                Id = 10,
                Name = "Test Race",
                Description = "Test race description",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Pretoria",
                EventType = "Running",
                DistanceKm = 10,
                OrganiserId = 1
            });

            context.Categories.Add(new Category
            {
                Id = 20,
                EventId = 10,
                Name = "10 km",
                Description = "10 kilometre category",
                EntryFee = 100,
                MaxParticipants = 100
            });

            context.Enrolments.Add(new Enrolment
            {
                Id = 30,
                ParticipantId = 2,
                EventId = 10,
                CategoryId = 20,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Active"
            });

            await context.SaveChangesAsync();

            var result = await controller.CreateEnrolment(
                new EnrolmentRequest
                {
                    EventId = 10,
                    CategoryId = 20
                });

            Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(1, await context.Enrolments.CountAsync());
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // =========================================================
    // CATEGORY TESTS: 14-17
    // =========================================================

    // TEST 14: An organiser can create a category for their event.
    [Fact]
    public async Task CreateCategory_WithValidDetails_ReturnsCreated()
    {
        var (context, controller) =
            CreateCategoriesController(1, "Organiser");

        try
        {
            context.Users.Add(new User
            {
                Id = 1,
                FirstName = "Test",
                LastName = "Organiser",
                Email = "organiser@example.com",
                PasswordHash = "test-hash",
                PhoneNumber = "0123456789",
                Role = "Organiser"
            });

            context.Events.Add(new RaceDayAPI.Models.Event
            {
                Id = 10,
                Name = "Test Race",
                Description = "Test race description",
                EventDate = DateTime.UtcNow.Date.AddDays(30),
                Location = "Pretoria",
                EventType = "Running",
                DistanceKm = 10,
                OrganiserId = 1
            });

            await context.SaveChangesAsync();

            var result = await controller.CreateCategory(
                new CategoryRequest
                {
                    EventId = 10,
                    Name = "10 km",
                    Description = "10 kilometre category",
                    EntryFee = 100,
                    MaxParticipants = 100
                });

            Assert.IsType<CreatedAtActionResult>(result);

            var saved = await context.Categories.SingleAsync();

            Assert.Equal(10, saved.EventId);
            Assert.Equal("10 km", saved.Name);
            Assert.Equal(100, saved.EntryFee);
            Assert.Equal(100, saved.MaxParticipants);
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 15: A negative category entry fee is rejected.
    [Fact]
    public async Task CreateCategory_WithNegativeEntryFee_ReturnsBadRequest()
    {
        var (context, controller) =
            CreateCategoriesController(1, "Organiser");

        try
        {
            var result = await controller.CreateCategory(
                new CategoryRequest
                {
                    EventId = 10,
                    Name = "10 km",
                    Description = "10 kilometre category",
                    EntryFee = -10,
                    MaxParticipants = 100
                });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(await context.Categories.ToListAsync());
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 16: An organiser cannot create a category for another
    // organiser's event.
    [Fact]
    public async Task CreateCategory_WhenEventBelongsToAnotherOrganiser_ReturnsForbidden()
    {
        var (context, controller) =
            CreateCategoriesController(1, "Organiser");

        try
        {
            context.Events.Add(new RaceDayAPI.Models.Event
            {
                Id = 10,
                Name = "Another Organiser's Race",
                Description = "A race owned by another organiser",
                EventDate = DateTime.UtcNow.Date.AddDays(30),
                Location = "Pretoria",
                EventType = "Running",
                DistanceKm = 10,
                OrganiserId = 2
            });

            await context.SaveChangesAsync();

            var result = await controller.CreateCategory(
                new CategoryRequest
                {
                    EventId = 10,
                    Name = "10 km",
                    Description = "10 kilometre category",
                    EntryFee = 100,
                    MaxParticipants = 100
                });

            Assert.IsType<ForbidResult>(result);
            Assert.Empty(await context.Categories.ToListAsync());
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 17: A category cannot be created for a missing event.
    [Fact]
    public async Task CreateCategory_WhenEventDoesNotExist_ReturnsNotFound()
    {
        var (context, controller) =
            CreateCategoriesController(1, "Organiser");

        try
        {
            var result = await controller.CreateCategory(
                new CategoryRequest
                {
                    EventId = 999,
                    Name = "10 km",
                    Description = "10 kilometre category",
                    EntryFee = 100,
                    MaxParticipants = 100
                });

            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Empty(await context.Categories.ToListAsync());
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // =========================================================
    // RESULTS TESTS: 18-22
    // =========================================================

    // TEST 18: An organiser can record a valid result.
    [Fact]
    public async Task CreateResult_WithValidDetails_ReturnsCreated()
    {
        var (context, controller) =
            CreateResultsController(1, "Organiser");

        try
        {
            var eventItem = new RaceDayAPI.Models.Event
            {
                Id = 10,
                Name = "Test Race",
                Description = "Test race description",
                EventDate = DateTime.UtcNow.Date.AddDays(30),
                Location = "Pretoria",
                EventType = "Running",
                DistanceKm = 10,
                OrganiserId = 1
            };

            context.Events.Add(eventItem);

            context.Enrolments.Add(new Enrolment
            {
                Id = 30,
                ParticipantId = 2,
                EventId = 10,
                CategoryId = 20,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Active",
                Event = eventItem
            });

            await context.SaveChangesAsync();

            var result = await controller.CreateResult(
                30,
                new ResultRequest
                {
                    FinishTime = TimeSpan.FromMinutes(45),
                    Position = 1,
                    ResultStatus = "Finished"
                });

            Assert.IsType<CreatedAtActionResult>(result);

            var saved = await context.Results.SingleAsync();

            Assert.Equal(30, saved.EnrolmentId);
            Assert.Equal(TimeSpan.FromMinutes(45), saved.FinishTime);
            Assert.Equal(1, saved.Position);
            Assert.Equal("Finished", saved.ResultStatus);
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 19: An invalid result status is rejected.
    [Fact]
    public async Task CreateResult_WithInvalidStatus_ReturnsBadRequest()
    {
        var (context, controller) =
            CreateResultsController(1, "Organiser");

        try
        {
            var result = await controller.CreateResult(
                30,
                new ResultRequest
                {
                    FinishTime = TimeSpan.FromMinutes(45),
                    Position = 1,
                    ResultStatus = "Invalid"
                });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(await context.Results.ToListAsync());
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 20: A finished result cannot have zero finish time.
    [Fact]
    public async Task CreateResult_WithZeroFinishTime_ReturnsBadRequest()
    {
        var (context, controller) =
            CreateResultsController(1, "Organiser");

        try
        {
            var result = await controller.CreateResult(
                30,
                new ResultRequest
                {
                    FinishTime = TimeSpan.Zero,
                    Position = 1,
                    ResultStatus = "Finished"
                });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(await context.Results.ToListAsync());
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 21: A duplicate result for an enrolment is rejected.
    [Fact]
    public async Task CreateResult_WhenResultAlreadyExists_ReturnsConflict()
    {
        var (context, controller) =
            CreateResultsController(1, "Organiser");

        try
        {
            var eventItem = new RaceDayAPI.Models.Event
            {
                Id = 10,
                Name = "Test Race",
                Description = "Test race description",
                EventDate = DateTime.UtcNow.Date.AddDays(30),
                Location = "Pretoria",
                EventType = "Running",
                DistanceKm = 10,
                OrganiserId = 1
            };

            context.Events.Add(eventItem);

            var enrolment = new Enrolment
            {
                Id = 30,
                ParticipantId = 2,
                EventId = 10,
                CategoryId = 20,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Active",
                Event = eventItem
            };

            context.Enrolments.Add(enrolment);

            context.Results.Add(new Result
            {
                Id = 40,
                EnrolmentId = 30,
                FinishTime = TimeSpan.FromMinutes(45),
                Position = 1,
                ResultStatus = "Finished",
                Enrolment = enrolment
            });

            await context.SaveChangesAsync();

            var result = await controller.CreateResult(
                30,
                new ResultRequest
                {
                    FinishTime = TimeSpan.FromMinutes(50),
                    Position = 2,
                    ResultStatus = "Finished"
                });

            Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(1, await context.Results.CountAsync());
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    // TEST 22: An organiser cannot record a result for another
    // organiser's event.
    [Fact]
    public async Task CreateResult_WhenEventBelongsToAnotherOrganiser_ReturnsForbidden()
    {
        var (context, controller) =
            CreateResultsController(1, "Organiser");

        try
        {
            var eventItem = new RaceDayAPI.Models.Event
            {
                Id = 10,
                Name = "Another Organiser's Race",
                Description = "A race owned by another organiser",
                EventDate = DateTime.UtcNow.Date.AddDays(30),
                Location = "Pretoria",
                EventType = "Running",
                DistanceKm = 10,
                OrganiserId = 2
            };

            context.Events.Add(eventItem);

            context.Enrolments.Add(new Enrolment
            {
                Id = 30,
                ParticipantId = 3,
                EventId = 10,
                CategoryId = 20,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Active",
                Event = eventItem
            });

            await context.SaveChangesAsync();

            var result = await controller.CreateResult(
                30,
                new ResultRequest
                {
                    FinishTime = TimeSpan.FromMinutes(45),
                    Position = 1,
                    ResultStatus = "Finished"
                });

            Assert.IsType<ForbidResult>(result);
            Assert.Empty(await context.Results.ToListAsync());
        }
        finally
        {
            await context.DisposeAsync();
        }
    }
}
