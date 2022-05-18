using CRUDOperation.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CRUDOperation.ViewModels
{
	public class MovieFormVM
	{
		public int Id { get; set; }
		[Required, StringLength(100)]
		public string Title { get; set; }
		public int Year { get; set; }
		[Range(1,10)]
		public double Rate { get; set; }
		[Required, MaxLength(2500)]
		public string StoreLine { get; set; }
		[Display(Name ="Select Poster")]
		public byte[] Poster { get; set; }
		[Display(Name ="Genre")]
		public byte GenreId { get; set; }
		public IEnumerable<Genre> Genres { get; set; }

	}
}
