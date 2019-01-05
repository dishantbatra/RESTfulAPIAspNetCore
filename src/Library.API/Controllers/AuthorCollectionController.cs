using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Library.API.Controllers
{
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : Controller
    {
        private ILibraryRepository _libraryRepository;
        public AuthorCollectionsController(ILibraryRepository libraryRepository)
        {
            this._libraryRepository = libraryRepository;
        }

        public IActionResult CreateAuthorCollection([FromBody]ICollection<DTO.AuthorForCreationDTO> authorCollectionDTO)
        {
            if (authorCollectionDTO == null)
            {
                return BadRequest();
            }

            var authorEntities = Mapper.Map<IEnumerable<Entities.Author>>(authorCollectionDTO);

            foreach (var author in authorEntities)
            {
                _libraryRepository.AddAuthor(author);
            }

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an authot collection failed on save");
            }
            //return Ok();
            var authorCollection = Mapper.Map<IEnumerable<DTO.AuthorDTO>>(authorEntities);
            var idsAsString = string.Join(",", authorCollection.Select(x => x.Id));

            return CreatedAtRoute("GetAuthorCollection", new { ids = idsAsString },authorCollection);
        }

        [HttpGet("{ids}",Name ="GetAuthorCollection")]
        public IActionResult GetAuthorCollection([ModelBinder(BinderType = typeof(ArrayModelBinder<Guid>))]IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }
            var authorEntities = _libraryRepository.GetAuthors(ids);

            if(ids.Count()!= authorEntities.Count())
            {
                return NotFound();
            }

            var authorToReturn = Mapper.Map<IEnumerable<DTO.AuthorDTO>>(authorEntities);
            return Ok(authorToReturn);
        }

    }
}
