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
        [Authorize]
        public IActionResult Create() 
        {
            return View();
        }

        // POST: /Movies/Create
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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

        [Authorize]
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
                var user = new User()
                {
                    FirstName = data.FirstName,
                    LastName = data.LastName,
                    Email = data.Email,
                    Mobile = (long)data.Mobile!,
                    Password = data.Password,

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
                var user = _context.Users.Where(u => u.Email == data.Email).SingleOrDefault();
                if (user != null)
                {
                    bool UserIsValid = (user.Email == data.Email && user.Password == data.Password);
                    if (UserIsValid)
                    {
                        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, data.Email!) }, CookieAuthenticationDefaults.AuthenticationScheme);
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

        [Authorize]
        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

    }
}
