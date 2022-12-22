using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.IActor
{
    public interface IAccount : IGrainWithStringKey
    {
        public  Task Write();
        public Task<decimal> Read();
    }
}
