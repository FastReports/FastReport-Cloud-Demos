using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudApiWebDemo.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public string Alias { get; set; }
        public DateTime ReleaseDate { get; set; }
        public double Price { get; set; }
    }
}
