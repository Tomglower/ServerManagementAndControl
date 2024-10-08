﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ControlPanel.Data.Models
{
    public class User
    {
        [Key]
        [JsonIgnore]
        public int id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        [JsonIgnore]
        public virtual ICollection<ServerData> Servers { get; set; }
    }
}
