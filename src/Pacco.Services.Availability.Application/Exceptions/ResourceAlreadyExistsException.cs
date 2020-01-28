using System;

namespace Pacco.Services.Availability.Application.Exceptions
{
    public class ResourceAlreadyExistsException : AppException
    {
        public override string Code => "resource_already_exists";
        public Guid ResourceId { get; }
        
        public ResourceAlreadyExistsException(Guid resourceId) 
            : base($"Resource with ID: '{resourceId}' already exists.")
        {
            ResourceId = resourceId;
        }
    }
}