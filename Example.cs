using Behaviorable.Attributes;
using Behaviorable.Behaviors.EntityFramework;
using Behaviorable.Businesses.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Behaviorable.Behaviors;
using MvcMovie.Models.Behaviorable.Behaviors.EntityFramework;

namespace MvcMovie.Models
{
    // A Poco object for a movie database, it implements several IPoco interface so
    // behaviors can be used
    public class Movie : IBasePoco, ISoftDeletable, ITimeStampable, ISluggable
    {
        public int? ID { get; set; }
       
        public string Title { get; set; }

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ReleaseDate { get; set; }
        public string Genre { get; set; }
        public decimal Price { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string Slug { get; set; }
    }

    //Db context
    public class MovieDbContext2 : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
    }

    /// <summary>
    /// A business that manages to Movies table
    /// </summary>
    public class MovieBusiness : EFBusiness<Movie, MovieDbContext2>
    {
        /// <summary>
        /// Behaviors attached to this business.
        /// </summary>
        public EFBehavior<Movie, MovieDbContext2, MovieBusiness> TimeStampable {get;set;}
        public SluggableBehavior<Movie, MovieDbContext2, MovieBusiness> Sluggable { get; set; }
        public SoftDeletableBehavior<Movie, MovieDbContext2, MovieBusiness> SoftDeletable { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">The Db Context</param>
        public MovieBusiness(MovieDbContext2 db)
            : base(db, db.Movies)
        {
            // Creates the behaviors
            TimeStampable = new TimeStampableBehavior<Movie, MovieDbContext2, MovieBusiness>(this);
            Sluggable = new SluggableBehavior<Movie, MovieDbContext2, MovieBusiness>(this, new string[] { "Title", "ReleaseDate", "Genre", "Price" });
            SoftDeletable = new SoftDeletableBehavior<Movie, MovieDbContext2, MovieBusiness>(this);
            
            // This must be called for the behaviors to work
            InitBehaviors();
        }

        public IQueryable<Movie> Search(string searchstring)
        {
            return FindSearch(new Dictionary<string, dynamic> { { "searchstring", searchstring } });
        }


        [BusinessFind("search")]
        public IQueryable<Movie> FindSearch(IDictionary<string, dynamic> parameters)
        {

            string s = parameters["searchstring"];
            return Table.Where(p => p.Title.Contains(s));
        }
    }
}