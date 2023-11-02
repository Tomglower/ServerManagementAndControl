using ControlPanel.Data;
using ControlPanel.Data.Models;
using Microsoft.EntityFrameworkCore;
using ControlPanel.Core.Models;
using Microsoft.AspNetCore.Mvc;
using ControlPanel.Core.Result;

namespace ControlPanel.Core
{
    public class ServerManager
    {
        private readonly AppDbContext _authContext;
        public ServerManager(AppDbContext appDbContext)
        {
            _authContext = appDbContext;

        }
        public async Task<bool> AddServer(ServerData serverData, int UserId)
        {
            try
            {
                if (serverData is null || string.IsNullOrWhiteSpace(serverData.link) || UserId <= 0)
                {
                    return false;
                }

                var checkResult = await CheckMachine(serverData.link);

                if (checkResult.Exists)
                {
                    Console.WriteLine(checkResult.Message);
                    return false;
                }
                if (serverData.link.ToLower() == "localhost")
                {
                    serverData.link = "127.0.0.1"; 
                }
                serverData.UserId = UserId;

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
        public async Task<List<ControlPanel.Data.Models.ServerData>> GetServers(int userId)
        {
            var machines = await _authContext.Machines.Where(m => m.UserId == userId).ToListAsync();
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


        public async Task<CheckMachineResult> CheckMachine(string link)
        {
            var result = new CheckMachineResult();

            var existingMachine = await _authContext.Machines.FirstOrDefaultAsync(m => m.link == link);

            if (existingMachine != null)
            {
                result.Exists = true;
                result.Message = "Сервер с таким адресом уже существует.";
            }
            else
            {
                result.Exists = false;
                result.Message = "Сервер с таким адресом не существует ";
            }

            return result;
        }
        public async Task<DeleteMachineResult> DeleteMachine(int id)
        {
            var result = new DeleteMachineResult();
            var deletingMachine = await _authContext.Machines.FirstOrDefaultAsync(i => i.id == id); 
            if(deletingMachine != null) 
            {
                _authContext.Machines.Remove(deletingMachine);
                await _authContext.SaveChangesAsync();
                result.Exists = true;
                result.Message = "Сервер удален";
            }
            else
            {
                result.Exists = false;
                result.Message = "Данной машины не существует";
            }
            return result;
        }
        

    }
}
