using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Behaviorable.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BusinessFind : Attribute
    {
        public string Name { get; set; }

        public BusinessFind(string name)
        {
            Name = name;   
        }


    }
}