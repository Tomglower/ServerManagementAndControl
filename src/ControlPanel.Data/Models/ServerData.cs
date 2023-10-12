using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlPanel.Data.Models
{
    public class ServerData
    {
        [Key]
        public int id { get; set; }
        public string link { get; set; }
        public string Data { get; set; }

    }
}
