using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using Orleans.Concurrency;

namespace App.Core
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    [Immutable]
    public class MessageInfo
    {
        public string TypeCode { get; set; }
        public byte[] Data { get; set; }
    }
}
