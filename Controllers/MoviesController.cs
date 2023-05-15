using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using MovieTicket.Data;
using MovieTicket.Models;
using System.Security.Claims;

namespace MovieTicket.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MovieTicketContext _context;
        public MoviesController(MovieTicketContext context)
        {
            _context = context;
        }

        // GET: /Movies
        public async Task<IActionResult> Index(string movieGenre, string searchString)
        {
            if (_context.Movies == null)
            {
                return Problem("Entity set 'MvcMovieContext.Movie'  is null.");
            }
            IQueryable<string> genreQuery = from m in _context.Movies
                                            orderby m.Genre
                                            select m.Genre;
            var movies = from m in _context.Movies
                         select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(s => s.Title!.Contains(searchString));
            }
            if (!string.IsNullOrEmpty(movieGenre))
            {
                movies = movies.Where(x => x.Genre == movieGenre);
            }
            var movieGenreVM = new MovieGenreViewModel
            {
                Genres = new SelectList(await genreQuery.Distinct().ToListAsync()),
                Movies = await movies.ToListAsync()
            };
            return View(movieGenreVM);
        }

        // GET: /Movies/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create() 
        {
            return View();
        }

        // POST: /Movies/Create
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ReleaseDate,Genre,Price")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: /Movies/Details/Id?
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FirstOrDefaultAsync(x => x.Id == id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // GET: /Movies/Edit/Id?
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null || _context.Movies == null)
            {
                return NotFound();
            }
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: /Movie/Edit/Id?
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,Title,Description,ReleaseDate,Genre,Price")] Movie movie)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: /Movies/Delete/Id?
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var movie = await _context.Movies.FirstOrDefaultAsync(x => x.Id == id); 
            if (movie == null || _context.Movies == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: /Movies/Delete/Id?
        [Authorize(Roles = "admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (_context.Movies == null)
            {
                return Problem("Entity set 'MovieTicketContext.Movie'  is null.");
            }
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "admin")]
        private bool MovieExists(int id)
        {
            return (_context.Movies?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // GET: /Movies/SignUp
        public IActionResult SignUp()
        {
            return View();
        }

        // POST: /Movies/SignUp
        [HttpPost]
        public IActionResult SignUp(SignInSignUpViewModel data)
        {
            if (ModelState.IsValid)
            {
                var firstRole = _context.Roles.FirstOrDefault(r => r.Name == "customer");
                var user = new User()
                {
                    FirstName = data.FirstName,
                    LastName = data.LastName,
                    Email = data.Email,
                    Mobile = (long)data.Mobile!,
                    Password = data.Password,
                    Role = firstRole
                };
                _context.Users.Add(user);
                _context.SaveChanges();
                TempData["SuccessMsg"] = "Registered Successfully!";
                return RedirectToAction("SignIn");
            }
            else
            {
                TempData["MainError"] = "Empty form can't be submitted";
                return View(data);
            }
        }

        // GET: /Movies/SignIn
        public IActionResult SignIn()
        {
            return View();
        }

        // POST: /Movies/SignIn
        [HttpPost]
        public IActionResult SignIn(SignInViewModel data)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.Include(x => x.Role).Where(u => u.Email == data.Email).SingleOrDefault();
                if (user != null)
                {
                    bool UserIsValid = (user.Email == data.Email && user.Password == data.Password);
                    if (UserIsValid)
                    {
                        var identity = new ClaimsIdentity(new[] {
                            new Claim(ClaimTypes.Name, (user.FirstName!+" "+user.LastName!)),
                            new Claim(ClaimTypes.Role, user.Role!.Name!)
                        }, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principle = new ClaimsPrincipal(identity);
                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principle);
                        HttpContext.Session.SetString("Email", data.Email!);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["passwordErrorMsg"] = "Invaid Password";
                        return View(data);
                    }
                }
                else
                {
                    TempData["emailErrorMsg"] = "User Not Found!";
                    return View(data);

                }
            }
            else
            {
                return View(data);
            }
        }

        // GET: /Movies/ResetPassword
        public IActionResult ResetPassword()
        {
            return View();
        }

        [AcceptVerbs("Post", "Get")]
        public IActionResult EmailIsNotExists(string? email)
        {
            var user = _context.Users.Where(e => e.Email == email).SingleOrDefault();
            if (user != null)
            {
                return Json($"Email is already in use!");
            }
            else
            {
                return Json(true);
            }
        }

        [Authorize(Roles = "admin,customer")]
        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        // GET: /Movies/CreateRole
        [Authorize(Roles = "admin")]
        public IActionResult CreateRole()
        {
            return View();
        }

        // POST: /Movies/CreateRole
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole([Bind("Name")] Role data)
        {
            if (ModelState.IsValid)
            {
                _context.Add(data);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(RoleIndex));
            }
            return View(data);
        }

        [Authorize(Roles = "admin")]
        public IActionResult RoleIndex()
        {
            if (_context.Roles == null)
            {
                return Problem("Entity set 'MvcMovieContext.Role' is null.");
            }
            
            var roles = from r in _context.Roles
                         select r;

            return View(roles);
        }

        [Authorize(Roles = "admin")]
        public IActionResult UserIndex()
        {
            if (_context.Roles == null)
            {
                return Problem("Entity set 'MvcMovieContext.Role' is null.");
            }

            //var users = from r in _context.Users
            //            select r;

            var users = _context.Users.Include(x => x.Role).ToList();

            return View(users);
        }


        [Authorize(Roles = "admin")]
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: /Movies/CreateRole
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User data)
        {
            if (ModelState.IsValid)
            {
                var firstRole = _context.Roles.Where(r => r.Name == data.Role!.Name).SingleOrDefault();
                var user = new User()
                {
                    FirstName = data.FirstName,
                    LastName = data.LastName,
                    Email = data.Email,
                    Mobile = (long)data.Mobile!,
                    Password = data.Password,
                    Role = firstRole
                };
                _context.Users.Add(user);
                _context.SaveChanges();
                TempData["SuccessMsg"] = "Account created Successfully!";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(UserIndex));
            }
            return View(data);
        }

        // GET: /Movies/UserDetails/Id?
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UserDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // GET: /Movies/EditUser/Id?
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EditUser(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: /Movie/EditUser/Id?
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int? id, User data)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(data);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!(_context.Users?.Any(e => e.Id == id)).GetValueOrDefault())
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(UserIndex));
            }
            return View(data);
        }

        // GET: /Movies/DeleteUser/Id?
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUser(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = await _context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == id);
            if (user == null || _context.Users == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: /Movies/DeleteUser/Id?
        [Authorize(Roles = "admin")]
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(int? id)
        {
            if (_context.Movies == null)
            {
                return Problem("Entity set 'MovieTicketContext.User'  is null.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(UserIndex));
        }
    }
}