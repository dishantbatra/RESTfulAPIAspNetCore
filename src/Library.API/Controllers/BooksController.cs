using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Library.API.DTO;
using Microsoft.AspNetCore.JsonPatch;

namespace Library.API.Controllers
{    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private ILibraryRepository _libraryRepository;
        public BooksController(ILibraryRepository libraryRepository)
        {
            this._libraryRepository = libraryRepository;
        }
        [HttpGet()]
        public IActionResult GetBooksForAuthors(Guid authorId)
        {
            var booksForAuthorFromRepository = _libraryRepository.GetBooksForAuthor(authorId);
            if (booksForAuthorFromRepository == null || booksForAuthorFromRepository.Count()==0)
            {
                return NotFound();
            }
            var bookForAuthors = Mapper.Map<IEnumerable<BookDTO>>(booksForAuthorFromRepository);
            return Ok(bookForAuthors);
        }
        [HttpGet("{bookId}",Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId ,Guid bookId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, bookId);
            if(bookForAuthorFromRepo == null)
            {
                return NotFound();
            }
            var bookForAuthor = Mapper.Map<BookDTO>(bookForAuthorFromRepo);
            return Ok(bookForAuthor);
        }

        [HttpPost]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody]BookForCreationDTO bookForCreationDTO)
        {
            if (bookForCreationDTO == null)
            {
                return BadRequest();
            }
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookToCreate = Mapper.Map<Entities.Book>(bookForCreationDTO);

            _libraryRepository.AddBookForAuthor(authorId, bookToCreate);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Creating a book for author {authorId} failed on save changes");
            }

            var bookTorReturn = bookToCreate;
            var bookCreated = Mapper.Map<BookDTO>(bookToCreate);
            return CreatedAtAction("GetBookForAuthor", new { authorId=authorId, bookId=bookCreated.Id }, bookCreated);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBookForAuthor(Guid authorId,Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteBook(bookForAuthorFromRepo);
            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting book {id} for author {authorId} failed on save");
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody]BookForUpdateDTO bookForUpdateDTO)
        {

            if (bookForUpdateDTO == null)
            {
                return BadRequest();
            }
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null) // Upsert the resource
            {
                var bookToAdd = Mapper.Map<Entities.Book>(bookForUpdateDTO);
                bookToAdd.Id = id;
                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);
                if (!_libraryRepository.Save())
                {
                    throw new Exception($"Upserting book {id} for Author {authorId} failed on save.");
                }
                var bookToReturn = Mapper.Map<BookDTO>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, bookId = bookToReturn.Id }, bookToReturn);
            }
            //map
            Mapper.Map(bookForUpdateDTO, bookForAuthorFromRepo);
            //apply update
            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);
            //map back entity

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Updating book {id} for author {authorId} failed on save changes");
            }

            return NoContent();
        }
        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id, [FromBody]JsonPatchDocument<BookForUpdateDTO> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookFromRepository = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookFromRepository == null)//Upsert
            {
                var bookForUpdate = new BookForUpdateDTO();
                patchDoc.ApplyTo(bookForUpdate);
                var bookToAdd = Mapper.Map<Entities.Book>(bookForUpdate);
                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);
                if (!_libraryRepository.Save())
                {
                    throw new Exception($"upsering book for author {authorId} failed on save.");
                }

                var bookForAuthor = Mapper.Map<BookDTO>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", new {authorId= authorId,bookId=bookToAdd.Id },bookForAuthor);
            }

            var bookToPatch = Mapper.Map<BookForUpdateDTO>(bookFromRepository);
            patchDoc.ApplyTo(bookToPatch);
            Mapper.Map(bookToPatch, bookFromRepository);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"patching book {id} for author {authorId} failed on save.");
            }
            return NoContent();
        }
    }
}