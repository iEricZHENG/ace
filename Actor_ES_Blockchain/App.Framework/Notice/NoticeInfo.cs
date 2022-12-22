using System.Collections.Generic;
using ProtoBuf;
using App.Core;
using App.Core.Notice;

namespace App.Framework.Notice
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class NoticeInfo
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string TargetId { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public LangType LangType { get; set; }
        public bool IsVoice { get; set; }
        public Dictionary<string, string> Values { get; set; }
    }
}
