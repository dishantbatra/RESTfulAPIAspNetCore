using Library.API.DTO;
using Library.API.Helpers;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace Library.API.Controllers
{
    [Route("api/[controller]")]
    public class AuthorsController:Controller
    {
        private ILibraryRepository _libraryRepository;
        public AuthorsController(ILibraryRepository libraryRepository)
        {
            this._libraryRepository = libraryRepository;
        }
        [HttpGet()]
        public IActionResult GetAuthors()
        {
            
                throw new Exception("Random Exception For Testing Purposes");
                var authorsFromRepos = this._libraryRepository.GetAuthors();
                var items = Mapper.Map<IEnumerable<AuthorDTO>>(authorsFromRepos);
                return Ok(items);
           
        }

        [HttpGet("{id}",Name ="GetAuthor")]
        public IActionResult GetAuthor(Guid id)
        {
            var authorResult = _libraryRepository.GetAuthor(id);
            if (authorResult==null)
            {
                return NotFound("Author Not Found");
            }
            
            return  Ok(Mapper.Map<AuthorDTO>(authorResult));
;        }

        [HttpPost]
        public IActionResult CreateAuthor([FromBody]AuthorForCreationDTO author)
        {
            //To Check Whether Client Is Able to Serialize the Request
            if (author == null)
            {
                return BadRequest();
            }
            var authorEntity = Mapper.Map<Entities.Author>(author);
            _libraryRepository.AddAuthor(authorEntity);
            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an author field on save");
            }

            var authorToReturn = Mapper.Map<AuthorDTO>(authorEntity);
            return CreatedAtAction("GetAuthor", new { authorEntity.Id },authorToReturn);

        }

        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (_libraryRepository.AuthorExists(id))
            {
                return StatusCode(StatusCodes.Status409Conflict);
            }
            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);
            if(authorFromRepo == null)
            {
                return NotFound();
            }
            _libraryRepository.DeleteAuthor(authorFromRepo);
            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting author  {id} failed on save");
            }
            return NoContent();
        }
    }
}
