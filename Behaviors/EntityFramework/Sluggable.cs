using Behaviorable.Attributes;
using Behaviorable.Businesses;
using Behaviorable.Businesses.EntityFramework;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Behaviorable.Behaviors.EntityFramework
{

    public interface ISluggable
    {
        string Slug { get; set; }
    }

    public class SluggableBehavior<Poco, Db, Business> : EFBehavior<Poco, Db, Business>
        where Poco : class, IBasePoco, ISluggable
        where Db : System.Data.Entity.DbContext
        where Business : EFBusiness<Poco, Db>
    {
        protected string[] SluggedProperties { get; set; }
        public SluggableBehavior(Business b, string[] sluggedProperties = null)
            : base(b)
        {
            SluggedProperties = sluggedProperties;
            if(SluggedProperties == null)
            {
                SluggedProperties = new string[] { "Title" };
            }
        }

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

        public override bool? BeforeSave(Poco toSave, BusinessParameters parameters)
        {
            toSave.Slug = "";
            foreach(string propertyName in SluggedProperties)
            {
                string title = toSave.GetType().GetProperty(propertyName).GetValue(toSave).ToString();
                toSave.Slug += GenerateSlug(title) + "-";
            }

            toSave.Slug = toSave.Slug.Remove(toSave.Slug.Length - 1);

            string tmpSlug = toSave.Slug;
            bool slugIsDuplicated;
            do
            {
                var alreadyUsed = (from m in business.Table
                                   where m.Slug == toSave.Slug
                                   select new { ID = m.ID, Slug = m.Slug }).FirstOrDefault();

                slugIsDuplicated = alreadyUsed != null && !string.IsNullOrEmpty(alreadyUsed.Slug) && toSave.ID != alreadyUsed.ID;
                
                if (slugIsDuplicated)
                {
                    var splitted = alreadyUsed.Slug.Split(new string[] { "__" }, StringSplitOptions.RemoveEmptyEntries);

                    string endSlug = splitted.Last();

                    int slugId = 1;
                    int.TryParse(endSlug, out slugId);
                    ++slugId;
                    toSave.Slug = tmpSlug +  "__" + slugId;

                }
            } while (slugIsDuplicated);

            return true;
        }

        public Poco FindBySlug(string slug)
        {
            return this.FindBySlug(new BusinessParameters() {
                { "slug", slug }
            }).FirstOrDefault();
        }

        [BusinessFind("slug")]
        public IQueryable<Poco> FindBySlug(BusinessParameters parameters)
        {  
            if(!parameters.Keys.Contains("slug"))
            {
                return null;
            }


            string slug = parameters["slug"];
            return from m in this.business.Table
                   where m.Slug == slug
                   select m;

        }
    }
}