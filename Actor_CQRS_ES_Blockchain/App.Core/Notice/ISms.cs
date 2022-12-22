using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Notice
{
    public interface ISms
    {
        Task<TResult> Send(string mobile, string content);
    }
}
