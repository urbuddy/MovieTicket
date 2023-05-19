using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieTicket.Data;
using MovieTicket.Models;
using System.Security.Claims;

namespace MovieTicket.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly MovieTicketContext _context;

        public FeedbackController(MovieTicketContext context) 
        {
            _context = context;
        }

        // GET: /Feedback/Id?
        public IActionResult Index(int? id)
        {
            if (id == null)
            {
                return Problem("Entity set 'MovieTicketContext.Feedback' is null.");
            }
            var movie = _context.Movies.Find(id);
            TempData["movieName"] = movie!.Title;
            var feedbacks = from f in _context.Feedbacks.Include(fd => fd.Movie).Include(fdb => fdb.User)
                            select f;

            feedbacks = feedbacks.Where(f => f.Movie!.Id == id);
            if (feedbacks == null)
            {
                return NotFound();
            }
            return View(feedbacks);
        }

        // GET: /Feedback/Create/MovieId?
        [Authorize]
        public IActionResult Add(int? id)
        {
            var movie = _context.Movies.Find(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var feed = _context.Feedbacks.Include(x => x.User).Include(x => x.Movie).Where(f => f.User!.Id == Convert.ToInt32(userId)).Where(f => f.Movie!.Id == id).SingleOrDefault();
            if (feed != null)
            {
                TempData["updateMsg"] = "You already submitted your review for this movie! Update it from here.";
                return RedirectToAction("Index", new { @id = id });
            }
            if (movie == null)
            {
                return Problem("Entity set 'MvcMovieContext.Movie' data not found.");
            }

            var RRVM = new RatingReviewViewModel
            {
                Movies = movie,
            };
            return View(RRVM);
        }

        // POST: /Feedback/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RatingReviewViewModel data)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _context.Users.FirstOrDefault(u => u.Id == Convert.ToInt32(userId));
                var movie = _context.Movies.FirstOrDefault(m => m.Id == data.Movies!.Id);
                var feedback = new Feedback()
                {
                    Rating = data.Rating,
                    Review = data.Review,
                    Movie = movie,
                    User = user,
                    UploadedAt = DateTime.UtcNow
                };
                _context.Feedbacks.Add(feedback);
                _context.SaveChanges(); 
                return RedirectToAction("Index", new { @id = movie!.Id });
            }
            return View(data);
        }

        // GET: /Feedback/Edit/Id?
        [Authorize]
        public async Task<ActionResult> Edit(int? id) 
        {
            if (id == null || _context.Feedbacks == null)
            {
                return NotFound();
            }
            var feedback = await _context.Feedbacks.Include(x => x.Movie).Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            TempData["movieId"] = feedback!.Movie!.Id;
            if (feedback == null)  
            {
                return NotFound();
            }
            return View(feedback);
        }

        // POST: /Feedback/Edit/Id?
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Edit(int? id, Feedback data) 
        {
            if (id == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    data.UploadedAt = DateTime.UtcNow;
                    _context.Feedbacks.Update(data);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!(_context.Feedbacks?.Any(e => e.Id == data.Movie!.Id)).GetValueOrDefault())
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", new { @id = TempData["movieId"] });
            }
            return View(data);
        }

        // GET: /Feedback/Delete/Id?
        [Authorize]
        public async Task<ActionResult> Delete(int? id)  
        {
            if (id == null)
            {
                return NotFound();
            }
            var feedback = await _context.Feedbacks.Include(x => x.Movie).Include(x => x.User).FirstOrDefaultAsync(f => f.Id == id);
            if (feedback == null)
            {
                return NotFound();
            }
            return View(feedback);
        }

        //POST: Feedback/Delete/Id/
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, Feedback data)
        {
            if (_context.Feedbacks == null)
            {
                return NotFound();
            }
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { @id = data.Movie!.Id });
        }
    }
}
