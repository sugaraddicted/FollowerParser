using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FollowerParser.MVVM.Model
{
    internal class Follower
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public string? Link { get; set; }
    }
}
