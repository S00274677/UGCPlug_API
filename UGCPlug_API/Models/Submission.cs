//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UGCPlug_API.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Submission
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public Nullable<System.DateTime> DateTaken { get; set; }
        public string FileUrl { get; set; }
        public bool ConsentGiven { get; set; }
        public string BusinessId { get; set; }
        public System.DateTime DateSubmitted { get; set; }
    }
}
