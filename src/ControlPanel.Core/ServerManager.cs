﻿using ControlPanel.Data;
using ControlPanel.Data.Models;
using Microsoft.EntityFrameworkCore;
using ControlPanel.Core.Models;

namespace ControlPanel.Core
{
    public class ServerManager
    {
        private readonly AppDbContext _authContext;
        public ServerManager(AppDbContext appDbContext)
        {
            _authContext = appDbContext;

        }
        public async Task<bool> AddServer(ServerData serverData)
        {
            try
            {
                if (serverData is null || string.IsNullOrWhiteSpace(serverData.link))
                {
                    return false;
                }

                var metricsUrl = $"http://{serverData.link}:9100/metrics";
                var httpClient = new HttpClient();
                var metricsResponse = await httpClient.GetAsync(metricsUrl);

                if (metricsResponse.IsSuccessStatusCode)
                {
                    var metricsData = await metricsResponse.Content.ReadAsStringAsync();
                    serverData.Data = metricsData;
                }

                await _authContext.Machines.AddAsync(serverData);
                await _authContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Произошла ошибка при добавлении сервера: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CheckMachinelExist(string Link)
        {
            return await _authContext.Machines.AnyAsync(l => l.link == Link);
        }
        public async Task<List<ControlPanel.Data.Models.ServerData>> GetServers()
        {
            var machines = await _authContext.Machines.ToListAsync();
            return machines;
        }

        public async Task<UpdateResult> UpdateMachineData(ServerData serverData)
        {
            var result = new UpdateResult();

            var machine = await _authContext.Machines.FirstOrDefaultAsync(m => m.link == serverData.link);

            if (machine is null)
            {
                result.Success = false;
                result.Message = "Machine is not exist";
                return result;
            }

            var metricsUrl = $"http://{machine.link}:9100/metrics";
            var httpClient = new HttpClient();
            var metricsResponse = await httpClient.GetAsync(metricsUrl);
            if (metricsResponse.IsSuccessStatusCode)
            {
                var metricsData = await metricsResponse.Content.ReadAsStringAsync();
                machine.Data = metricsData;
                _authContext.Update(machine);
                await _authContext.SaveChangesAsync();
                result.Success = true;
                result.Message = "Updated";
                result.Data = machine.Data;
            }
            else
            {
                result.Success = false;
                result.Message = "Failed to update machine data";
            }

            return result;
        }



    }
}
