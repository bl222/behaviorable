using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcMovie.Models.Behaviorable.Behaviors.EntityFramework
{
    public interface IBasePoco
    {
        int? ID { get; set; }
    }

    public interface ISoftDeletable
    {
        DateTime? DeletedAt { get; set; }
    }

    public interface ISluggable
    {
        string Slug { get; set; }
    }

    public interface ITimeStampable
    {
        DateTime? CreatedAt { get; set; }
        DateTime? ModifiedAt { get; set; }

    }
}
