using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/courses")]
    [ResponseCache(CacheProfileName = "240SecondsCacheProfile")] // We could apply it at action level as well 
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

        [HttpGet(Name = "GetCoursesForAuthor")]
        public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId)
        {
            if (!repository.AuthorExists(authorId))
                return NotFound();

            var coursesForAuthorFromRepo = repository.GetCourses(authorId);
            return Ok(mapper.Map<IEnumerable<CourseDto>>(coursesForAuthorFromRepo));
        }

        [HttpGet("{courseId}", Name = "GetCourseForAuthor")]
        [ResponseCache(Duration = 120)] // well override the controller level cache attribute
        public ActionResult<CourseDto> GetCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!repository.AuthorExists(authorId))
                return NotFound();

            var courseForAuthorFromRepo = repository.GetCourse(authorId, courseId);

            if (courseForAuthorFromRepo == null)
                return NotFound();

            return Ok(mapper.Map<CourseDto>(courseForAuthorFromRepo));
        }

        [HttpPost(Name = "CreateCourseForAuthor")]
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
        public IActionResult UpdateCourseForAuthor(Guid authorId, Guid courseId,
            CourseForUpdateDto course)
        {
            if (!repository.AuthorExists(authorId))
                return NotFound();

            var courseForAuthorFromRepo = repository.GetCourse(authorId, courseId);

            // if the course not found, new resource will be created (upserting with PUT)
            if (courseForAuthorFromRepo == null)
            {
                var courseToAdd = mapper.Map<Course>(course);
                courseToAdd.Id = courseId;

                repository.AddCourse(authorId, courseToAdd);
                repository.Save();

                var courseToReturn = mapper.Map<CourseDto>(courseToAdd);
                return CreatedAtRoute("GetCourseForAuthor",
                    new { authorId, courseId = courseToAdd.Id },
                    courseToReturn);
            }

            // map the entity to a CourseForUpdateDto
            // apply the updated field values to the dto
            // map the CourseForUpdateDto back to an entity
            mapper.Map(course, courseForAuthorFromRepo);

            repository.UpdateCourse(courseForAuthorFromRepo);

            repository.Save();

            //return Ok(mapper.Map<CourseDto>(courseForAuthorFromRepo));

            //or
            // return NoContent(); 204 status code with return type of ActionResult
            // both are valid

            return NoContent();


            //return Ok(new
            //{
            //    Message = "No updates has happend",
            //    CourseDetails = mapper.Map<CourseDto>(courseForAuthorFromRepo)
            //});
        }

        [HttpPatch("{courseId}")]
        public ActionResult PartiallyUpdateCourseForAuthor(Guid authorId,
            Guid courseId,
            JsonPatchDocument<CourseForUpdateDto> patchDocument)
        {
            if (!repository.AuthorExists(authorId))
                return NotFound();

            var courseForAuthorFromRepo = repository.GetCourse(authorId, courseId);

            // if the course not found, new resource will be created (upserting with PATCH)
            if (courseForAuthorFromRepo == null)
            {
                var courseDto = new CourseForUpdateDto();
                patchDocument.ApplyTo(courseDto, ModelState);

                if (!TryValidateModel(courseDto))
                    return ValidationProblem(ModelState);

                var courseToAdd = mapper.Map<Course>(courseDto);
                courseToAdd.Id = courseId;


                repository.AddCourse(authorId, courseToAdd);
                repository.Save();

                var courseToReturn = mapper.Map<CourseDto>(courseToAdd);
                CreatedAtRoute("GetCourseForAuthor",
                    new { authorId, courseId = courseToReturn.Id },
                    courseToReturn);
            }

            var courseToPatch = mapper.Map<CourseForUpdateDto>(courseForAuthorFromRepo);

            patchDocument.ApplyTo(courseToPatch, ModelState);

            if (!TryValidateModel(courseToPatch))
                return ValidationProblem(ModelState);

            mapper.Map(courseToPatch, courseForAuthorFromRepo);

            repository.UpdateCourse(courseForAuthorFromRepo);
            repository.Save();

            return NoContent();
        }

        [HttpDelete("{courseId}")]
        public ActionResult DeleteCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!repository.AuthorExists(authorId))
                return NotFound();

            var courseForAuthorFromRepo = repository.GetCourse(authorId, courseId);

            if (courseForAuthorFromRepo == null)
                return NotFound();

            repository.DeleteCourse(courseForAuthorFromRepo);
            repository.Save();

            return NoContent();
        }

        public override ActionResult ValidationProblem(
            [ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices
                .GetRequiredService<IOptions<ApiBehaviorOptions>>();

            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);

            //return base.ValidationProblem(modelStateDictionary);
        }
    }
}