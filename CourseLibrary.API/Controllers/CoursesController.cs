using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/courses")]
    public class CoursesController : ControllerBase
    {
         private readonly ICourseLibraryRepository repository;
        private readonly IMapper mapper;

        public CoursesController(ICourseLibraryRepository repository, IMapper mapper)
        {
            this.repository = repository ?? 
                throw new ArgumentNullException(nameof(repository));
            this.mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId)
        {
            if(!repository.AuthorExists(authorId))
                return NotFound();
            
            var coursesForAuthorFromRepo = repository.GetCourses(authorId);
            return Ok(mapper.Map<IEnumerable<CourseDto>>(coursesForAuthorFromRepo));
        }

        [HttpGet("{courseId}", Name = "GetCourseForAuthor")]
        public ActionResult<CourseDto> GetCourseForAuthor(Guid authorId, Guid courseId)
        {
            if(!repository.AuthorExists(authorId))
                return NotFound();
            
            var courseForAuthorFromRepo = repository.GetCourse(authorId, courseId);

            if(courseForAuthorFromRepo == null)
                return NotFound();
            
            return Ok(mapper.Map<CourseDto>(courseForAuthorFromRepo));
        }

        [HttpPost]
        public ActionResult<CourseDto> CreateCourseForAuthor(Guid authorId, CourseForCreationDto course)
        {
            if (!repository.AuthorExists(authorId))
                return NotFound();

            var courseEntity = mapper.Map<Course>(course);
            repository.AddCourse(authorId, courseEntity);
            repository.Save();

            var courseToReturn = mapper.Map<CourseDto>(courseEntity);
            return CreatedAtRoute("GetCourseForAuthor",
                new { authorId, courseId = courseToReturn.Id },
                courseToReturn);
        }

        [HttpPut("{courseId}")]
        public ActionResult<CourseDto> UpdateCourseForAuthor(Guid authorId, Guid courseId,
            CourseForUpdateDto course)
        {
            if (!repository.AuthorExists(authorId))
                return NotFound();

            var courseForAuthorFromRepo = repository.GetCourse(authorId, courseId);

            if (courseForAuthorFromRepo == null)
                return NotFound();

            // map the entity to a CourseForUpdateDto
            // apply the updated field values to the dto
            // map the CourseForUpdateDto back to an entity
            mapper.Map(course, courseForAuthorFromRepo);
            
            repository.UpdateCourse(courseForAuthorFromRepo);

            repository.Save();
            
            return Ok(mapper.Map<CourseDto>(courseForAuthorFromRepo));
            //or
            // return NoContent(); 204 status code with return type of ActionResult
        }

    }
}