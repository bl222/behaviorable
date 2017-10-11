using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Behaviorable.Attributes
{
    /// <summary>
    /// An attribued used to define a custom find in a business or behavior. Simply attach this
    /// attribute to the custom find method and specify a type
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BusinessFind : Attribute
    {
        public string Type { get; set; }

        public BusinessFind(string type)
        {
            Type = type;   
        }


    }
}