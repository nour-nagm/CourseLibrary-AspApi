using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;

namespace CourseLibrary.API.ActionConstraints
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
    {
        private readonly MediaTypeCollection mediaTypes = new();
        private readonly string requestHeaderToMatch;

        public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch,
            string mediaType, params string[] otherMediaTypes)
        {
            this.requestHeaderToMatch = requestHeaderToMatch
               ?? throw new ArgumentNullException(nameof(requestHeaderToMatch));

            // check if the inputted media types are valid media types
            // and add them to the mediaTypes collection                     

            if (MediaTypeHeaderValue.TryParse(mediaType,
                out MediaTypeHeaderValue parsedMediaType))
            {
                mediaTypes.Add(parsedMediaType);
            }
            else
                throw new ArgumentException(nameof(mediaType));
            

            foreach (var otherMediaType in otherMediaTypes)
            {
                if (MediaTypeHeaderValue.TryParse(otherMediaType,
                   out MediaTypeHeaderValue parsedOtherMediaType))
                {
                    mediaTypes.Add(parsedOtherMediaType);
                }
                else
                    throw new ArgumentException(nameof(otherMediaTypes));
                
            }

        }

        public int Order => 0; // The order property decides which stage the constraint
                               // is part of. Action constraints run in groups based
                               // on the order. All of the framework-provided HTTP
                               // method attributes use the same Order value so that
                               // they run in the same stage. And what we want is
                               // for this constraint to run in that same stage as
                               // well, and that's stage 0.

        public bool Accept(ActionConstraintContext context)
        {
            var requestHeader = context.RouteContext.HttpContext.Request.Headers;
            
            if (!requestHeader.ContainsKey(requestHeaderToMatch))
                return false;

            var parsedRequestMediaType = new MediaType(requestHeader[requestHeaderToMatch]);

            //if one the media types matches, return true
            foreach (var mediaType in mediaTypes)
            {
                var parsedMediaType = new MediaType(mediaType);

                if (parsedRequestMediaType.Equals(parsedMediaType))
                    return true;
            }
            return false;
        }
    }
}
