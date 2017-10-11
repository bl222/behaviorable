using Behaviorable.Attributes;
using Behaviorable.Businesses;
using Behaviorable.Businesses.EntityFramework;
using MvcMovie.Models.Behaviorable.Behaviors.EntityFramework;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Behaviorable.Behaviors.EntityFramework
{

    /// <summary>
    /// In order for the sluggable behavior to be able to manipulate an entity framework Poco object,
    /// the Poco in question must implement the ISluggable interface.
    /// </summary>
    public interface ISluggable
    {
        /// <summary>
        /// The table represented by the Poco must have a Slug column to save the slug.
        /// </summary>
        string Slug { get; set; }
    }

    /// <summary>
    /// A sluggable behavior. When attached to a business, the sluggable behavior creates a slug that can be used
    /// to identify the row uniquely
    /// </summary>
    /// <typeparam name="Poco">The Poco type managed by the business the behavior is attached to</typeparam>
    /// <typeparam name="Db">The type of the database context used to access the database</typeparam>
    /// <typeparam name="Business">The type of the business the behavior is attadched to</typeparam>
    public class SluggableBehavior<Poco, Db, Business> : EFBehavior<Poco, Db, Business>
        where Poco : class, IBasePoco, ISluggable
        where Db : System.Data.Entity.DbContext
        where Business : EFBusiness<Poco, Db>
    {
        /// <summary>
        /// An array of string indicating which table colllumns are concategnated to form the slug
        /// </summary>
        protected string[] SluggedProperties { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="b">The business the behavior is attached to</param>
        /// <param name="sluggedProperties">An array of string, each entryin the array specifies a table collumn
        /// that is concategnated to create the slug. If null, the slug is created from a collumn named title</param>
        public SluggableBehavior(Business b, string[] sluggedProperties = null)
            : base(b)
        {
            SluggedProperties = sluggedProperties;
            if(SluggedProperties == null)
            {
                SluggedProperties = new string[] { "Title" };
            }
        }

        /// <summary>
        /// Creates a slug based on a string
        /// </summary>
        /// <param name="text">The string that be turned into a slug</param>
        /// <returns>The slug</returns>
        public string GenerateSlug(string text)
        {
            string str = RemoveDiacritics(text).ToLower();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }

        /// <summary>
        /// Remove RemoveDiacritics from the text that will become a slug
        /// </summary>
        /// <param name="text">The text from which th e RemoveDiacritics are removed</param>
        /// <returns>The same text that was passed, but without diacritics</returns>
        public string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// In the BeforeSave callback, the slug is created and added to the Poco object that is being saved
        /// </summary>
        /// <param name="toSave">The Poco object that is being saved</param>
        /// <param name="parameters">Unused for now</param>
        /// <returns></returns>
        public override bool? BeforeSave(Poco toSave, BusinessParameters parameters)
        {
            // Create the slug based on the SluggedProperties
            toSave.Slug = "";
            foreach(string propertyName in SluggedProperties)
            {
                string title = toSave.GetType().GetProperty(propertyName).GetValue(toSave).ToString();
                toSave.Slug += GenerateSlug(title) + "-";
            }

            // Remove the trailing - from the slug
            toSave.Slug = toSave.Slug.Remove(toSave.Slug.Length - 1);

            //Slugs must be unique so this loop check for duplicates and appends a number to the slug until
            // the slug is unique
            string tmpSlug = toSave.Slug;
            bool slugIsDuplicated;
            do
            {
                // Try to find a row with the current slug
                var alreadyUsed = (from m in business.Table
                                   where m.Slug == toSave.Slug
                                   select new { ID = m.ID, Slug = m.Slug }).FirstOrDefault();

                slugIsDuplicated = alreadyUsed != null && !string.IsNullOrEmpty(alreadyUsed.Slug) && toSave.ID != alreadyUsed.ID;
                
                if (slugIsDuplicated)
                {
                    var splitted = alreadyUsed.Slug.Split(new string[] { "__" }, StringSplitOptions.RemoveEmptyEntries);

                    string endSlug = splitted.Last();

                    //  The TryParse means the slugId will be 1 if this is the first duplicate found,
                    // if not the first duplicate found, the slugId will have the value of endSlug
                    int slugId = 1;
                    int.TryParse(endSlug, out slugId);
                    ++slugId;
                    toSave.Slug = tmpSlug +  "__" + slugId;

                }
            } while (slugIsDuplicated);

            return true;
        }

        /// <summary>
        /// A custom find method that is used to find a row by slug. Can be use with Find("slug") or FindFirst("slug"), but
        /// since slugs are unique should be used with FindFirst("slug")
        /// </summary>
        /// <param name="parameters">The slug is saved under the slug key</param>
        /// <returns>A collection containing the row identified by the given slug or null if the slug isn't found</returns>
        [BusinessFind("slug")]
        public IQueryable<Poco> FindBySlug(BusinessParameters parameters)
        {
            if (!parameters.Keys.Contains("slug"))
            {
                return null;
            }

            string slug = parameters["slug"];
            return from m in this.business.Table
                   where m.Slug == slug
                   select m;

        }

        /// <summary>
        /// A convinience method that uses the FindBySlug custom find method. It's a little easier to use then the custom save
        /// since you have less boiler plate code to write
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public Poco FindBySlug(string slug)
        {
            return this.FindBySlug(new BusinessParameters() {
                { "slug", slug }
            }).FirstOrDefault();
        }


    }
}