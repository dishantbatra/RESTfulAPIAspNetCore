using System;
using System.Collections.Generic;

namespace Library.API.DTO
{
    public class AuthorForCreationDTO
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTimeOffset DateOfBirth { get; set; }

        public string Genre { get; set; }

        public ICollection<BookForCreationDTO> Books { get; set; }
    }
}