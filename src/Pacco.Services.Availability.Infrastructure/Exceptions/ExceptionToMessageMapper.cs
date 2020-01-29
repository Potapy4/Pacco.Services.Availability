using System;
using Convey.MessageBrokers.RabbitMQ;
using Pacco.Services.Availability.Application.Events.Rejected;
using Pacco.Services.Availability.Application.Exceptions;
using Pacco.Services.Availability.Core.Exceptions;

namespace Pacco.Services.Availability.Infrastructure.Exceptions
{
    internal sealed class ExceptionToMessageMapper : IExceptionToMessageMapper
    {
        public object Map(Exception exception, object message)
            => message switch
            {
                ResourceAlreadyExistsException ex => new AddResourceRejected(ex.ResourceId, ex.Message, ex.Code),
                MissingResourceTagsException ex => new AddResourceRejected(Guid.Empty, ex.Message, ex.Code),
                InvalidResourceTagsException ex => new AddResourceRejected(Guid.Empty, ex.Message, ex.Code),
                _ => null
            };
    }
}