using System;
using System.Net.Http;
using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Pacco.Services.Availability.Application.Exceptions;
using Pacco.Services.Availability.Application.Services;
using Pacco.Services.Availability.Application.Services.Clients;
using Pacco.Services.Availability.Core.Repositories;
using Pacco.Services.Availability.Core.ValueObjects;

namespace Pacco.Services.Availability.Application.Commands.Handlers
{
    public class ReserveResourceHandler : ICommandHandler<ReserveResource>
    {
        private readonly IResourcesRepository _resourcesRepository;
        private readonly ICustomersServiceClient _customersServiceClient;
        private readonly IEventProcessor _eventProcessor;

        public ReserveResourceHandler(IResourcesRepository resourcesRepository,
            ICustomersServiceClient customersServiceClient, IEventProcessor eventProcessor)
        {
            _resourcesRepository = resourcesRepository;
            _customersServiceClient = customersServiceClient;
            _eventProcessor = eventProcessor;
        }
        
        public async Task HandleAsync(ReserveResource command)
        {
            var resource = await _resourcesRepository.GetAsync(command.ResourceId);
            if (resource is null)
            {
                throw new ResourceNotFoundException(command.ResourceId);
            }

            var state = await _customersServiceClient.GetStateAsync(command.CustomerId);
            if (state is null)
            {
                throw new CustomerNotFoundException(command.CustomerId);
            }

            if (!state.IsValid)
            {
                throw new InvalidCustomerStateException(command.CustomerId, state.State);
            }
            
            var reservation = new Reservation(command.DateTime, command.Priority);
            resource.AddReservation(reservation);
            await _resourcesRepository.UpdateAsync(resource);
            await _eventProcessor.ProcessAsync(resource.Events);
        }
    }
}