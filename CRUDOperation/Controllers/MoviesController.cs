using CRUDOperation.DataServer;
using CRUDOperation.Models;
using CRUDOperation.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CRUDOperation.Controllers
{
	public class MoviesController : Controller
	{
		private readonly ApplicationDbContext _applicationDbContext;
		private readonly IToastNotification _toastNotification;
		public MoviesController(ApplicationDbContext applicationDbContext  ,IToastNotification toastNotification)
		{
			_applicationDbContext	= applicationDbContext;
			_toastNotification		= toastNotification;
		}
		public async Task<IActionResult> Index()
		{
			var movies = await _applicationDbContext.movies.OrderByDescending(rate => rate.Rate).ToListAsync();
			return View(movies);
		}
		public async Task<IActionResult> Create()
		{
			var viewModelCreate = new MovieFormVM()
			{
				Genres = await _applicationDbContext.genres.OrderBy(m=> m.Name).ToListAsync()
			};
			return View("MovieForm", viewModelCreate);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(MovieFormVM movieFormVM)
		{
			movieFormVM.Genres = await _applicationDbContext.genres.OrderBy(m => m.Name).ToListAsync();
			if (!ModelState.IsValid)
			{
				return View("MovieForm", movieFormVM);
			}
			var Files = Request.Form.Files;
			if(!Files.Any())
			{

				ModelState.AddModelError("Poster","Please Select Movie Poster!");
				return View("MovieForm",movieFormVM);
			}
			var poster				= Files.FirstOrDefault();
			var AllowedExtensions	= new List<string> { ".jpg",".png"};
			if (!AllowedExtensions.Contains(Path.GetExtension(poster.FileName).ToLower()))
			{

				ModelState.AddModelError("Poster", "Only .PNG, .JPG Images Extentions Are Allowed!");
				return View("MovieForm", movieFormVM);
			}
			if (poster.Length > 1048576)
			{
				ModelState.AddModelError("Poster", "Poster Image Cannot be more than 1 MB");
				return View("MovieForm", movieFormVM);
			}


			using var dataStream = new MemoryStream();
			await poster.CopyToAsync(dataStream);
			// Map Data
			var movieData = new Movie()
			{
				Title			= movieFormVM.Title,
				Rate			= movieFormVM.Rate,
				Poster			= dataStream.ToArray(),
				Year			= movieFormVM.Year,
				StoreLine		= movieFormVM.StoreLine,
				GenreId			= movieFormVM.GenreId
			};
			_applicationDbContext.Add(movieData);
			_applicationDbContext.SaveChanges();
			_toastNotification.AddSuccessToastMessage("Movie Created Successfully :)");
			return RedirectToAction(nameof(Index));
		}
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
				return BadRequest();
			var MovieID = await _applicationDbContext.movies.FindAsync(id);
			if (MovieID == null)
				return NotFound();
			// Render All Data From DB "Initialized"
			var viewModelEdit = new MovieFormVM()
			{
				Id				= MovieID.Id,
				Title			= MovieID.Title,
				Year			= MovieID.Year,
				Genres			= await _applicationDbContext.genres.OrderBy(m=>m.Name).ToListAsync(),
				GenreId			= MovieID.GenreId,
				Rate			= MovieID.Rate,
				StoreLine		= MovieID.StoreLine,
				Poster			= MovieID.Poster,

			};
			return View("MovieForm", viewModelEdit);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(MovieFormVM editMovieFormVM)
		{
			editMovieFormVM.Genres = await _applicationDbContext.genres.OrderBy(m => m.Name).ToListAsync();
			if (!ModelState.IsValid)
				return View("MovieForm",editMovieFormVM);
			var movie = await _applicationDbContext.movies.FindAsync(editMovieFormVM.Id);
			if (movie == null)
				return NotFound();
			var Files = Request.Form.Files;
			if(Files.Any())
			{
				var poster = Files.FirstOrDefault();
				var AllowedExtensions = new List<string> { ".jpg", ".png" };
				using var dataStream = new MemoryStream();
				await poster.CopyToAsync(dataStream);
				editMovieFormVM.Poster = dataStream.ToArray();
				if (!AllowedExtensions.Contains(Path.GetExtension(poster.FileName).ToLower()))
				{

					ModelState.AddModelError("Poster", "Only .PNG, .JPG Images Extentions Are Allowed!");
					return View("MovieForm", editMovieFormVM);
				}
				if (poster.Length > 1048576)
				{
					ModelState.AddModelError("Poster", "Poster Image Cannot be more than 1 MB");
					return View("MovieForm", editMovieFormVM);
				}
				movie.Poster = editMovieFormVM.Poster;
			}

			movie.Title			= editMovieFormVM.Title;
			movie.Year			= editMovieFormVM.Year;
			movie.GenreId		= editMovieFormVM.GenreId;
			movie.Rate			= editMovieFormVM.Rate;
			movie.StoreLine		= editMovieFormVM.StoreLine;
			_applicationDbContext.SaveChanges();
			_toastNotification.AddSuccessToastMessage("Movie Updated Successfully :)");
			return RedirectToAction(nameof(Index));
		}
		public async Task<IActionResult> Details (int? id)
		{
			if (id == null)
				return BadRequest();
			var movieID		= await _applicationDbContext.movies.Include(m=>m.Genre).SingleOrDefaultAsync(m=>m.Id==id);
			if (movieID == null)
				return NotFound();
			return View(movieID);
			
		}
		[HttpPost]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
				return BadRequest();
			var movieID = await _applicationDbContext.movies.FindAsync(id);
			if (movieID == null)
				return NotFound();
			_applicationDbContext.movies.Remove(movieID);
			_applicationDbContext.SaveChanges();
			return Ok();

		}
	}
}
