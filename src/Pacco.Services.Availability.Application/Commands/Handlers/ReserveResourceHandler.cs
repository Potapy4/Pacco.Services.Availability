using System;
using System.Net.Http;
using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Newtonsoft.Json;
using Pacco.Services.Availability.Application.DTO;
using Pacco.Services.Availability.Application.Exceptions;
using Pacco.Services.Availability.Application.Services;
using Pacco.Services.Availability.Core.Repositories;
using Pacco.Services.Availability.Core.ValueObjects;

namespace Pacco.Services.Availability.Application.Commands.Handlers
{
    public class ReserveResourceHandler : ICommandHandler<ReserveResource>
    {
        private readonly IResourcesRepository _resourcesRepository;
        private readonly IEventProcessor _eventProcessor;
        private readonly IHttpClientFactory _httpClientFactory;

        public ReserveResourceHandler(IResourcesRepository resourcesRepository, IEventProcessor eventProcessor,
            IHttpClientFactory httpClientFactory)
        {
            _resourcesRepository = resourcesRepository;
            _eventProcessor = eventProcessor;
            _httpClientFactory = httpClientFactory;
        }
        
        public async Task HandleAsync(ReserveResource command)
        {
            var resource = await _resourcesRepository.GetAsync(command.ResourceId);
            if (resource is null)
            {
                throw new ResourceNotFoundException(command.ResourceId);
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(
                $"http://localhost:5002/customers/{command.CustomerId}/state");
            if (!response.IsSuccessStatusCode)
            {
                throw new CustomerNotFoundException(command.CustomerId);
            }

            var content = await response.Content.ReadAsStringAsync();
            var state = JsonConvert.DeserializeObject<CustomerStateDto>(content);
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