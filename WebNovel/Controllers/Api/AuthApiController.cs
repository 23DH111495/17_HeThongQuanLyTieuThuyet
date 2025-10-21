using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using WebNovel.Data;
using WebNovel.Models;
using WebNovel.Models.ApiModels;

namespace WebNovel.Controllers.Api
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        public AuthController()
        {
            Debug.WriteLine($"AuthController constructor called at {DateTime.Now}");
        }

        // Multiple GET endpoints to test routing
        [HttpGet]
        [Route("")]
        public IHttpActionResult Test()
        {
            Debug.WriteLine($"GET api/auth called at {DateTime.Now}");

            return Ok(new
            {
                message = "Auth API is working",
                timestamp = DateTime.Now,
                controller = "AuthController",
                action = "Test",
                method = Request.Method.Method,
                uri = Request.RequestUri.ToString()
            });
        }

        [HttpGet]
        [Route("test")]
        public IHttpActionResult TestRoute()
        {
            Debug.WriteLine($"GET api/auth/test called at {DateTime.Now}");
            return Ok(new { message = "Test route working", timestamp = DateTime.Now });
        }

        [HttpGet]
        [Route("test-register")]
        public IHttpActionResult TestRegister()
        {
            Debug.WriteLine("Testing register with dummy data");

            var request = new RegisterRequest
            {
                Username = "testuser" + DateTime.Now.Ticks,
                Email = "test" + DateTime.Now.Ticks + "@example.com",
                Password = "testpass123",
                FirstName = "Test",
                LastName = "User"
            };

            return Register(request);
        }

        [HttpGet]
        [Route("test-login")]
        public IHttpActionResult TestLogin()
        {
            Debug.WriteLine("Testing login with dummy data");

            // First, let's try to find any existing user
            var existingUser = db.Users.FirstOrDefault(u => u.IsActive);

            if (existingUser == null)
            {
                return BadRequest("No users found in database. Try test-register first.");
            }

            return Ok(new
            {
                message = "Found user for testing",
                username = existingUser.Username,
                instruction = "Use POST /api/auth/login with this username and the password you registered with"
            });
        }

        [HttpGet]
        [Route("list-users")]
        public IHttpActionResult ListUsers()
        {
            try
            {
                var users = db.Users
                    .Where(u => u.IsActive)
                    .Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.Email,
                        u.JoinDate
                    })
                    .Take(10)
                    .ToList();

                return Ok(new
                {
                    message = "Active users in database",
                    count = users.Count,
                    users = users
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }




        [HttpGet]
        [Route("debug-register-detailed")]
        public IHttpActionResult DebugRegisterDetailed()
        {
            try
            {
                Debug.WriteLine("=== Starting Detailed Debug Register Test ===");

                var timestamp = DateTime.Now.Ticks.ToString();
                var testUser = new RegisterRequest
                {
                    Username = "debuguser" + timestamp,
                    Email = "debug" + timestamp + "@example.com",
                    Password = "testpass123",
                    FirstName = "Debug",
                    LastName = "User"
                };

                // Step 1: Create and save user
                var user = new User
                {
                    Username = testUser.Username,
                    Email = testUser.Email,
                    FirstName = testUser.FirstName,
                    LastName = testUser.LastName,
                    PasswordHash = HashPassword(testUser.Password),
                    JoinDate = DateTime.Now,
                    IsActive = true,
                    EmailVerified = false
                };

                db.Users.Add(user);
                db.SaveChanges();
                Debug.WriteLine($"✓ User created with ID: {user.Id}");

                // Step 2: Try creating reader with detailed error catching
                try
                {
                    Debug.WriteLine("Creating Reader object...");
                    var reader = new Reader
                    {
                        UserId = user.Id,
                        IsPremium = false,
                        ReadingPreferences = "{}",
                        FavoriteGenres = "[]",
                        NotificationSettings = "{\"newChapters\":true,\"authorUpdates\":true}"
                    };

                    Debug.WriteLine($"Reader object created. UserId: {reader.UserId}");

                    db.Readers.Add(reader);
                    Debug.WriteLine("Reader added to context");

                    db.SaveChanges();
                    Debug.WriteLine("✓ Reader saved successfully");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    Debug.WriteLine("DbEntityValidationException occurred:");
                    var errorDetails = "";
                    foreach (var validationError in dbEx.EntityValidationErrors)
                    {
                        foreach (var error in validationError.ValidationErrors)
                        {
                            var detail = $"Property: {error.PropertyName}, Error: {error.ErrorMessage}";
                            Debug.WriteLine(detail);
                            errorDetails += detail + "; ";
                        }
                    }
                    return BadRequest($"Validation error when saving Reader: {errorDetails}");
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException dbUpdateEx)
                {
                    Debug.WriteLine($"DbUpdateException: {dbUpdateEx.Message}");
                    var innerEx = dbUpdateEx.InnerException;
                    var details = "";
                    while (innerEx != null)
                    {
                        details += innerEx.Message + " | ";
                        Debug.WriteLine($"Inner: {innerEx.Message}");
                        innerEx = innerEx.InnerException;
                    }
                    return BadRequest($"Database update error when saving Reader: {details}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"General exception when saving Reader: {ex.Message}");
                    var innerEx = ex.InnerException;
                    var details = ex.Message;
                    while (innerEx != null)
                    {
                        details += " | " + innerEx.Message;
                        Debug.WriteLine($"Inner: {innerEx.Message}");
                        innerEx = innerEx.InnerException;
                    }
                    return BadRequest($"Error saving Reader: {details}");
                }

                return Ok(new
                {
                    message = "User and Reader created successfully!",
                    userId = user.Id,
                    username = user.Username
                });

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Overall error: {ex.Message}");
                return BadRequest($"Overall error: {ex.Message}");
            }
        }
        [HttpGet]
        [Route("check-reader-table")]
        public IHttpActionResult CheckReaderTable()
        {
            try
            {
                // Try to get existing readers to see what the table looks like
                var existingReaders = db.Readers.Take(5).ToList();

                // Also check if we can create a minimal reader object
                var testUserId = db.Users.First().Id; // Get any existing user ID

                return Ok(new
                {
                    message = "Reader table check",
                    existingReadersCount = existingReaders.Count,
                    sampleReaders = existingReaders.Select(r => new
                    {
                        r.Id,
                        r.UserId,
                        r.IsPremium,
                        HasReadingPreferences = !string.IsNullOrEmpty(r.ReadingPreferences),
                        HasFavoriteGenres = !string.IsNullOrEmpty(r.FavoriteGenres),
                        HasNotificationSettings = !string.IsNullOrEmpty(r.NotificationSettings)
                    }).ToList(),
                    testUserId = testUserId
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error checking Reader table: {ex.Message} | Inner: {ex.InnerException?.Message}");
            }
        }



        [HttpGet]
        [Route("check-roles")]
        public IHttpActionResult CheckRoles()
        {
            try
            {
                var roles = db.UserRoles.ToList();

                return Ok(new
                {
                    message = "Available roles in database",
                    count = roles.Count,
                    roles = roles.Select(r => new
                    {
                        r.Id,
                        r.Name,
                        r.Description
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    error = "Error accessing UserRoles table: " + ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet]
        [Route("debug")]
        public IHttpActionResult DebugInfo()
        {
            Debug.WriteLine($"GET api/auth/debug called at {DateTime.Now}");

            var routeData = Request.GetRouteData();

            return Ok(new
            {
                controller = "AuthController",
                availableRoutes = new[] {
                    "GET /api/auth",
                    "GET /api/auth/test",
                    "GET /api/auth/debug",
                    "POST /api/auth/login",
                    "POST /api/auth/register",
                    "GET /api/auth/profile/{userId}"
                },
                requestInfo = new
                {
                    method = Request.Method.Method,
                    uri = Request.RequestUri.ToString(),
                    headers = Request.Headers.Select(h => new { key = h.Key, value = string.Join(", ", h.Value) }).ToList(),
                    routeData = routeData?.Values?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString()),
                    routeTemplate = routeData?.Route?.RouteTemplate
                },
                configInfo = new
                {
                    attributeRoutingEnabled = "Check if config.MapHttpAttributeRoutes() is called",
                    controllerNamespace = this.GetType().Namespace,
                    controllerName = this.GetType().Name
                },
                timestamp = DateTime.Now
            });
        }

        // POST: api/auth/login
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login([FromBody] LoginRequest request)
        {
            Debug.WriteLine($"POST api/auth/login called at {DateTime.Now}");
            Debug.WriteLine($"Request data: {request?.Username}, Password length: {request?.Password?.Length}");

            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                Debug.WriteLine("Login failed: Missing username or password");
                return BadRequest("Username and password are required");
            }

            try
            {
                // Find user by username or email
                var user = db.Users.FirstOrDefault(u =>
                    (u.Username == request.Username || u.Email == request.Username) &&
                    u.IsActive);

                if (user == null)
                {
                    Debug.WriteLine($"Login failed: User not found for '{request.Username}'");
                    return BadRequest("Invalid credentials");
                }

                if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    Debug.WriteLine($"Login failed: Invalid password for user '{user.Username}'");
                    return BadRequest("Invalid credentials");
                }

                Debug.WriteLine($"Login successful for user: {user.Username}");

                // Update last login
                user.LastLoginDate = DateTime.Now;
                db.SaveChanges();

                // Get user roles
                var userRoles = db.UserRoleAssignments
                    .Where(ura => ura.UserId == user.Id && ura.IsActive)
                    .Include(ura => ura.UserRole)
                    .Select(ura => ura.UserRole.Name)
                    .ToList();

                // Get reader info if exists
                var reader = db.Readers.FirstOrDefault(r => r.UserId == user.Id);
                var wallet = db.Wallets.FirstOrDefault(w => w.UserId == user.Id);

                var userProfile = new UserProfileDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    JoinDate = user.JoinDate,
                    IsPremium = reader?.IsPremium ?? false,
                    CoinBalance = wallet?.CoinBalance ?? 0,
                    Roles = userRoles
                };

                return Ok(new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = userProfile
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return InternalServerError(new Exception("Error during login: " + ex.Message));
            }
        }

        // POST: api/auth/register
        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register([FromBody] RegisterRequest request)
        {
            Debug.WriteLine($"POST api/auth/register called at {DateTime.Now}");

            if (request == null)
            {
                Debug.WriteLine("Register failed: No request data");
                return BadRequest("Registration data is required");
            }

            Debug.WriteLine($"Register request: Username={request.Username}, Email={request.Email}");

            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.Password))
            {
                Debug.WriteLine("Register failed: Missing required fields");
                return BadRequest("Username, email, and password are required");
            }

            try
            {
                // Check if username or email already exists
                if (db.Users.Any(u => u.Username == request.Username))
                {
                    Debug.WriteLine($"Register failed: Username '{request.Username}' already exists");
                    return BadRequest("Username already exists");
                }

                if (db.Users.Any(u => u.Email == request.Email))
                {
                    Debug.WriteLine($"Register failed: Email '{request.Email}' already exists");
                    return BadRequest("Email already exists");
                }

                Debug.WriteLine("Creating new user...");

                // Create new user
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PasswordHash = HashPassword(request.Password),
                    JoinDate = DateTime.Now,
                    IsActive = true,
                    EmailVerified = false
                };

                db.Users.Add(user);
                db.SaveChanges();

                Debug.WriteLine($"User created with ID: {user.Id}");

                // Create reader profile
                var reader = new Reader
                {
                    UserId = user.Id,
                    IsPremium = false,
                    ReadingPreferences = "{}",
                    FavoriteGenres = "[]",
                    NotificationSettings = "{\"newChapters\":true,\"authorUpdates\":true}"
                };
                db.Readers.Add(reader);

                // Create wallet
                var wallet = new Wallet
                {
                    UserId = user.Id,
                    CoinBalance = 100, // Welcome bonus
                    TotalCoinsEarned = 100
                };
                db.Wallets.Add(wallet);

                // Assign Reader role
                var readerRole = db.UserRoles.FirstOrDefault(r => r.Name == "Reader");
                if (readerRole != null)
                {
                    var roleAssignment = new UserRoleAssignment
                    {
                        UserId = user.Id,
                        RoleId = readerRole.Id,
                        IsActive = true
                    };
                    db.UserRoleAssignments.Add(roleAssignment);
                    Debug.WriteLine("Reader role assigned");
                }
                else
                {
                    Debug.WriteLine("Warning: Reader role not found in database");
                }

                // Record welcome bonus transaction
                var welcomeTransaction = new CoinTransaction
                {
                    UserId = user.Id,
                    TransactionType = "Bonus",
                    Amount = 100,
                    BalanceBefore = 0,
                    BalanceAfter = 100,
                    Description = "Welcome bonus"
                };
                db.CoinTransactions.Add(welcomeTransaction);

                db.SaveChanges();

                Debug.WriteLine("Registration completed successfully");

                var userProfile = new UserProfileDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    JoinDate = user.JoinDate,
                    IsPremium = false,
                    CoinBalance = 100,
                    Roles = new List<string> { "Reader" }
                };

                return Ok(new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Message = "Registration successful. Welcome bonus of 100 coins added!",
                    Data = userProfile
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Register error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return InternalServerError(new Exception("Error during registration: " + ex.Message));
            }
        }

        // GET: api/auth/profile/{userId}
        [HttpGet]
        [Route("profile/{userId:int}")]
        public IHttpActionResult GetProfile(int userId)
        {
            Debug.WriteLine($"GET api/auth/profile/{userId} called at {DateTime.Now}");

            try
            {
                var user = db.Users.Find(userId);
                if (user == null || !user.IsActive)
                {
                    Debug.WriteLine($"User not found or inactive: {userId}");
                    return NotFound();
                }

                Debug.WriteLine($"Profile found for user: {user.Username}");

                var reader = db.Readers.FirstOrDefault(r => r.UserId == userId);
                var wallet = db.Wallets.FirstOrDefault(w => w.UserId == userId);

                var userRoles = db.UserRoleAssignments
                    .Where(ura => ura.UserId == userId && ura.IsActive)
                    .Include(ura => ura.UserRole)
                    .Select(ura => ura.UserRole.Name)
                    .ToList();

                var userProfile = new UserProfileDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    JoinDate = user.JoinDate,
                    IsPremium = reader?.IsPremium ?? false,
                    CoinBalance = wallet?.CoinBalance ?? 0,
                    Roles = userRoles
                };

                return Ok(new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Data = userProfile
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetProfile error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return InternalServerError(new Exception("Error retrieving profile: " + ex.Message));
            }
        }

        [HttpGet]
        [Route("debug-full-register")]
        public IHttpActionResult DebugFullRegister()
        {
            try
            {
                Debug.WriteLine("=== Starting Full Registration Debug ===");

                var timestamp = DateTime.Now.Ticks.ToString();
                var testUser = new RegisterRequest
                {
                    Username = "fulltest" + timestamp,
                    Email = "fulltest" + timestamp + "@example.com",
                    Password = "testpass123",
                    FirstName = "Full",
                    LastName = "Test"
                };

                // Step 1: Create User
                var user = new User
                {
                    Username = testUser.Username,
                    Email = testUser.Email,
                    FirstName = testUser.FirstName,
                    LastName = testUser.LastName,
                    PasswordHash = HashPassword(testUser.Password),
                    JoinDate = DateTime.Now,
                    IsActive = true,
                    EmailVerified = false
                };

                db.Users.Add(user);
                db.SaveChanges();
                Debug.WriteLine($"✓ Step 1: User created with ID: {user.Id}");

                // Step 2: Create Reader
                try
                {
                    var reader = new Reader
                    {
                        UserId = user.Id,
                        IsPremium = false,
                        ReadingPreferences = "{}",
                        FavoriteGenres = "[]",
                        NotificationSettings = "{\"newChapters\":true,\"authorUpdates\":true}"
                    };
                    db.Readers.Add(reader);
                    db.SaveChanges();
                    Debug.WriteLine($"✓ Step 2: Reader created with ID: {reader.Id}");
                }
                catch (Exception ex)
                {
                    return BadRequest($"Step 2 (Reader) failed: {ex.Message} | Inner: {ex.InnerException?.Message}");
                }

                // Step 3: Create Wallet
                try
                {
                    var wallet = new Wallet
                    {
                        UserId = user.Id,
                        CoinBalance = 100,
                        TotalCoinsEarned = 100
                    };
                    db.Wallets.Add(wallet);
                    db.SaveChanges();
                    Debug.WriteLine($"✓ Step 3: Wallet created with ID: {wallet.UserId}");
                }
                catch (Exception ex)
                {
                    return BadRequest($"Step 3 (Wallet) failed: {ex.Message} | Inner: {ex.InnerException?.Message}");
                }

                // Step 4: Assign Reader Role
                try
                {
                    var readerRole = db.UserRoles.FirstOrDefault(r => r.Name == "Reader");
                    if (readerRole == null)
                    {
                        return BadRequest("Reader role not found in database");
                    }

                    var roleAssignment = new UserRoleAssignment
                    {
                        UserId = user.Id,
                        RoleId = readerRole.Id,
                        IsActive = true
                    };
                    db.UserRoleAssignments.Add(roleAssignment);
                    db.SaveChanges();
                    Debug.WriteLine($"✓ Step 4: Role assignment created with ID: {roleAssignment.Id}");
                }
                catch (Exception ex)
                {
                    return BadRequest($"Step 4 (Role Assignment) failed: {ex.Message} | Inner: {ex.InnerException?.Message}");
                }

                // Step 5: Create Coin Transaction
                try
                {
                    var welcomeTransaction = new CoinTransaction
                    {
                        UserId = user.Id,
                        TransactionType = "Bonus",
                        Amount = 100,
                        BalanceBefore = 0,
                        BalanceAfter = 100,
                        Description = "Welcome bonus"
                    };
                    db.CoinTransactions.Add(welcomeTransaction);
                    db.SaveChanges();
                    Debug.WriteLine($"✓ Step 5: Coin transaction created with ID: {welcomeTransaction.Id}");
                }
                catch (Exception ex)
                {
                    return BadRequest($"Step 5 (Coin Transaction) failed: {ex.Message} | Inner: {ex.InnerException?.Message}");
                }

                return Ok(new
                {
                    message = "Full registration process completed successfully!",
                    userId = user.Id,
                    username = user.Username,
                    allStepsCompleted = true
                });

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Overall error: {ex.Message}");
                return BadRequest($"Overall error: {ex.Message} | Inner: {ex.InnerException?.Message}");
            }
        }

        // Add these enhanced debug methods to your AuthController

        [HttpGet]
        [Route("test-register-step-by-step")]
        public IHttpActionResult TestRegisterStepByStep()
        {
            try
            {
                Debug.WriteLine("=== Step-by-Step Registration Test ===");

                var timestamp = DateTime.Now.Ticks.ToString();
                var testUser = new RegisterRequest
                {
                    Username = "steptest" + timestamp,
                    Email = "steptest" + timestamp + "@example.com",
                    Password = "testpass123",
                    FirstName = "Step",
                    LastName = "Test"
                };

                Debug.WriteLine($"Test user data: {testUser.Username}, {testUser.Email}");

                // Step 1: Check if username/email already exists
                Debug.WriteLine("Step 1: Checking for existing users...");
                var existingUsername = db.Users.Any(u => u.Username == testUser.Username);
                var existingEmail = db.Users.Any(u => u.Email == testUser.Email);

                if (existingUsername || existingEmail)
                {
                    return Content(HttpStatusCode.BadRequest, $"User already exists: Username={existingUsername}, Email={existingEmail}");
                }
                Debug.WriteLine("✓ No existing user conflicts");

                // Step 2: Create user object
                Debug.WriteLine("Step 2: Creating user object...");
                var user = new User
                {
                    Username = testUser.Username,
                    Email = testUser.Email,
                    FirstName = testUser.FirstName,
                    LastName = testUser.LastName,
                    PasswordHash = HashPassword(testUser.Password),
                    JoinDate = DateTime.Now,
                    IsActive = true,
                    EmailVerified = false
                };
                Debug.WriteLine($"✓ User object created: {user.Username}");

                // Step 3: Save user
                Debug.WriteLine("Step 3: Saving user to database...");
                db.Users.Add(user);
                db.SaveChanges();
                Debug.WriteLine($"✓ User saved with ID: {user.Id}");

                // Step 4: Check if Reader role exists
                Debug.WriteLine("Step 4: Checking for Reader role...");
                var readerRole = db.UserRoles.FirstOrDefault(r => r.Name == "Reader");
                if (readerRole == null)
                {
                    Debug.WriteLine("⚠ Reader role not found - creating minimal response");
                    return Ok(new
                    {
                        message = "User created but Reader role missing",
                        userId = user.Id,
                        username = user.Username,
                        issue = "Reader role not found in UserRoles table"
                    });
                }
                Debug.WriteLine($"✓ Reader role found with ID: {readerRole.Id}");

                // Step 5: Create reader profile
                Debug.WriteLine("Step 5: Creating reader profile...");
                var reader = new Reader
                {
                    UserId = user.Id,
                    IsPremium = false,
                    ReadingPreferences = "{}",
                    FavoriteGenres = "[]",
                    NotificationSettings = "{\"newChapters\":true,\"authorUpdates\":true}"
                };

                db.Readers.Add(reader);
                db.SaveChanges();
                Debug.WriteLine($"✓ Reader created");

                // Step 6: Create wallet
                Debug.WriteLine("Step 6: Creating wallet...");
                var wallet = new Wallet
                {
                    UserId = user.Id,
                    CoinBalance = 100,
                    TotalCoinsEarned = 100
                };

                db.Wallets.Add(wallet);
                db.SaveChanges();
                Debug.WriteLine($"✓ Wallet created");

                // Step 7: Assign role
                Debug.WriteLine("Step 7: Assigning reader role...");
                var roleAssignment = new UserRoleAssignment
                {
                    UserId = user.Id,
                    RoleId = readerRole.Id,
                    IsActive = true
                };

                db.UserRoleAssignments.Add(roleAssignment);
                db.SaveChanges();
                Debug.WriteLine($"✓ Role assigned");

                // Step 8: Create welcome transaction
                Debug.WriteLine("Step 8: Creating welcome bonus transaction...");
                var welcomeTransaction = new CoinTransaction
                {
                    UserId = user.Id,
                    TransactionType = "Bonus",
                    Amount = 100,
                    BalanceBefore = 0,
                    BalanceAfter = 100,
                    Description = "Welcome bonus"
                };

                db.CoinTransactions.Add(welcomeTransaction);
                db.SaveChanges();
                Debug.WriteLine($"✓ Transaction created");

                Debug.WriteLine("=== Registration completed successfully ===");

                return Ok(new
                {
                    message = "Registration completed successfully!",
                    userId = user.Id,
                    username = user.Username,
                    email = user.Email,
                    coinBalance = 100,
                    allStepsCompleted = true
                });

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in step-by-step registration: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                return Content(HttpStatusCode.BadRequest, new
                {
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace?.Split('\n').Take(5).ToArray() // First 5 lines
                });
            }
        }

        [HttpGet]
        [Route("test-database-connection")]
        public IHttpActionResult TestDatabaseConnection()
        {
            try
            {
                Debug.WriteLine("Testing database connection and table access...");

                var results = new Dictionary<string, object>();

                // Test Users table
                try
                {
                    var userCount = db.Users.Count();
                    results["Users"] = new { count = userCount, status = "OK" };
                }
                catch (Exception ex)
                {
                    results["Users"] = new { error = ex.Message, status = "FAILED" };
                }

                // Test UserRoles table
                try
                {
                    var roleCount = db.UserRoles.Count();
                    var roles = db.UserRoles.Select(r => r.Name).ToList();
                    results["UserRoles"] = new { count = roleCount, roles = roles, status = "OK" };
                }
                catch (Exception ex)
                {
                    results["UserRoles"] = new { error = ex.Message, status = "FAILED" };
                }

                // Test Readers table
                try
                {
                    var readerCount = db.Readers.Count();
                    results["Readers"] = new { count = readerCount, status = "OK" };
                }
                catch (Exception ex)
                {
                    results["Readers"] = new { error = ex.Message, status = "FAILED" };
                }

                // Test Wallets table
                try
                {
                    var walletCount = db.Wallets.Count();
                    results["Wallets"] = new { count = walletCount, status = "OK" };
                }
                catch (Exception ex)
                {
                    results["Wallets"] = new { error = ex.Message, status = "FAILED" };
                }

                // Test UserRoleAssignments table
                try
                {
                    var assignmentCount = db.UserRoleAssignments.Count();
                    results["UserRoleAssignments"] = new { count = assignmentCount, status = "OK" };
                }
                catch (Exception ex)
                {
                    results["UserRoleAssignments"] = new { error = ex.Message, status = "FAILED" };
                }

                // Test CoinTransactions table
                try
                {
                    var transactionCount = db.CoinTransactions.Count();
                    results["CoinTransactions"] = new { count = transactionCount, status = "OK" };
                }
                catch (Exception ex)
                {
                    results["CoinTransactions"] = new { error = ex.Message, status = "FAILED" };
                }

                return Ok(new
                {
                    message = "Database connection test completed",
                    results = results
                });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = "Database connection failed",
                    details = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet]
        [Route("minimal-test-register")]
        public IHttpActionResult MinimalTestRegister()
        {
            try
            {
                Debug.WriteLine("=== Minimal Registration Test (User Only) ===");

                var timestamp = DateTime.Now.Ticks.ToString();

                // Create minimal user
                var user = new User
                {
                    Username = "minimal" + timestamp,
                    Email = "minimal" + timestamp + "@example.com",
                    FirstName = "Minimal",
                    LastName = "Test",
                    PasswordHash = HashPassword("testpass123"),
                    JoinDate = DateTime.Now,
                    IsActive = true,
                    EmailVerified = false
                };

                db.Users.Add(user);
                db.SaveChanges();

                return Ok(new
                {
                    message = "Minimal user created successfully",
                    userId = user.Id,
                    username = user.Username,
                    note = "Only User table entry created - no Reader, Wallet, etc."
                });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = "Minimal registration failed",
                    details = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet]
        [Route("debug-reader-creation")]
        public IHttpActionResult DebugReaderCreation()
        {
            try
            {
                Debug.WriteLine("=== Debugging Reader Creation Issue ===");

                // First, let's examine an existing reader to see the structure
                var existingReader = db.Readers.FirstOrDefault();
                if (existingReader != null)
                {
                    Debug.WriteLine($"Existing Reader example: ID={existingReader.Id}, UserId={existingReader.UserId}");
                    Debug.WriteLine($"  IsPremium: {existingReader.IsPremium}");
                    Debug.WriteLine($"  ReadingPreferences: '{existingReader.ReadingPreferences}'");
                    Debug.WriteLine($"  FavoriteGenres: '{existingReader.FavoriteGenres}'");
                    Debug.WriteLine($"  NotificationSettings: '{existingReader.NotificationSettings}'");
                }

                // Get the user we just created
                var testUser = db.Users.OrderByDescending(u => u.Id).FirstOrDefault();
                if (testUser == null)
                {
                    return Content(HttpStatusCode.BadRequest, new { error = "No test user found" });
                }

                Debug.WriteLine($"Using test user: ID={testUser.Id}, Username={testUser.Username}");

                // Check if this user already has a reader profile
                var existingReaderForUser = db.Readers.FirstOrDefault(r => r.UserId == testUser.Id);
                if (existingReaderForUser != null)
                {
                    return Ok(new
                    {
                        message = "User already has a reader profile",
                        userId = testUser.Id,
                        readerId = existingReaderForUser.Id
                    });
                }

                // Try creating reader with detailed error catching
                try
                {
                    Debug.WriteLine("Creating Reader object...");
                    var reader = new Reader
                    {
                        UserId = testUser.Id,
                        IsPremium = false,
                        ReadingPreferences = "{}",
                        FavoriteGenres = "[]",
                        NotificationSettings = "{\"newChapters\":true,\"authorUpdates\":true}"
                    };

                    Debug.WriteLine($"Reader object created. UserId: {reader.UserId}");

                    db.Readers.Add(reader);
                    Debug.WriteLine("Reader added to context");

                    // Try to save with detailed validation error catching
                    try
                    {
                        db.SaveChanges();
                        Debug.WriteLine("✓ Reader saved successfully");

                        return Ok(new
                        {
                            message = "Reader created successfully",
                            userId = testUser.Id,
                            readerId = reader.Id
                        });
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException validationEx)
                    {
                        Debug.WriteLine("=== DbEntityValidationException Details ===");
                        var validationErrors = new List<object>();

                        foreach (var entityError in validationEx.EntityValidationErrors)
                        {
                            Debug.WriteLine($"Entity: {entityError.Entry.Entity.GetType().Name}");
                            foreach (var error in entityError.ValidationErrors)
                            {
                                Debug.WriteLine($"  Property: {error.PropertyName}, Error: {error.ErrorMessage}");
                                validationErrors.Add(new
                                {
                                    entity = entityError.Entry.Entity.GetType().Name,
                                    property = error.PropertyName,
                                    error = error.ErrorMessage
                                });
                            }
                        }

                        return Content(HttpStatusCode.BadRequest, new
                        {
                            error = "Entity validation failed",
                            validationErrors = validationErrors,
                            fullMessage = validationEx.Message
                        });
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException updateEx)
                    {
                        Debug.WriteLine("=== DbUpdateException Details ===");
                        Debug.WriteLine($"Message: {updateEx.Message}");

                        var innerException = updateEx.InnerException;
                        var innerMessages = new List<string>();

                        while (innerException != null)
                        {
                            Debug.WriteLine($"Inner: {innerException.Message}");
                            innerMessages.Add(innerException.Message);
                            innerException = innerException.InnerException;
                        }

                        return Content(HttpStatusCode.BadRequest, new
                        {
                            error = "Database update failed",
                            message = updateEx.Message,
                            innerMessages = innerMessages
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error creating Reader object: {ex.Message}");
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        error = "Failed to create Reader object",
                        message = ex.Message,
                        innerError = ex.InnerException?.Message
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Overall error: {ex.Message}");
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = "Overall debug error",
                    message = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet]
        [Route("check-reader-constraints")]
        public IHttpActionResult CheckReaderConstraints()
        {
            try
            {
                Debug.WriteLine("=== Checking Reader Table Constraints ===");

                // Get the latest test user
                var testUser = db.Users.OrderByDescending(u => u.Id).FirstOrDefault();
                if (testUser == null)
                {
                    return Content(HttpStatusCode.BadRequest, new { error = "No test user found" });
                }

                // Try creating readers with different field combinations to isolate the issue
                var results = new List<object>();

                // Test 1: Minimal reader (only required fields)
                try
                {
                    var minimalReader = new Reader
                    {
                        UserId = testUser.Id,
                        IsPremium = false
                    };

                    db.Readers.Add(minimalReader);
                    db.SaveChanges();

                    results.Add(new { test = "Minimal reader", status = "SUCCESS", id = minimalReader.Id });

                    // Clean up
                    db.Readers.Remove(minimalReader);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    results.Add(new { test = "Minimal reader", status = "FAILED", error = ex.Message });
                }

                // Test 2: Reader with empty strings
                try
                {
                    var emptyStringReader = new Reader
                    {
                        UserId = testUser.Id,
                        IsPremium = false,
                        ReadingPreferences = "",
                        FavoriteGenres = "",
                        NotificationSettings = ""
                    };

                    db.Readers.Add(emptyStringReader);
                    db.SaveChanges();

                    results.Add(new { test = "Empty string fields", status = "SUCCESS", id = emptyStringReader.Id });

                    // Clean up
                    db.Readers.Remove(emptyStringReader);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    results.Add(new { test = "Empty string fields", status = "FAILED", error = ex.Message });
                }

                // Test 3: Reader with null fields
                try
                {
                    var nullFieldReader = new Reader
                    {
                        UserId = testUser.Id,
                        IsPremium = false,
                        ReadingPreferences = null,
                        FavoriteGenres = null,
                        NotificationSettings = null
                    };

                    db.Readers.Add(nullFieldReader);
                    db.SaveChanges();

                    results.Add(new { test = "Null fields", status = "SUCCESS", id = nullFieldReader.Id });

                    // Clean up
                    db.Readers.Remove(nullFieldReader);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    results.Add(new { test = "Null fields", status = "FAILED", error = ex.Message });
                }

                return Ok(new
                {
                    message = "Reader constraint testing completed",
                    testUserId = testUser.Id,
                    results = results
                });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    error = "Constraint checking failed",
                    message = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }



        // Helper methods
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + "YourSaltHere"; // Use a proper salt in production
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }




}