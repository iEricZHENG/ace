using Orleans.Concurrency;
using ProtoBuf;

namespace App.Core
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [Immutable]
    public class TResult
    {
        private static readonly TResult successResponse = new TResult(true);
        public bool Result { get; set; }
        /// <summary>
        /// 代码是区分结果分类的不同。
        /// 0表示系统自动行为，无任何错误。
        /// 1表示人工主动行为
        /// 10以上的数字表示系统自动系统,对应业务错误
        /// </summary>
        public int Code { get; set; } = 0;
        public string Message { get; set; }
        public static TResult Success { get { return successResponse; } }
        public TResult()
        {
            this.Result = true;
            this.Code = 0;
            this.Message = null;
        }
        public TResult(bool result, int code = 0, string message = null)
        {
            this.Result = result;
            this.Code = code;
            this.Message = message;
        }
        public static TResult Fail(int code, string message = null, string detail = null)
        {
            return new TResult(false, code, message);
        }
        public static TResult<T> Succeed<T>(T result = default(T))
        {
            return new TResult<T>(true, result);
        }
        public static TResult Succeed(string message)
        {
            return new TResult() { Result = true, Message = message };
        }
        public static TResult<T> Fail<T>(int code, T data, string message = null)
        {
            return new TResult<T>(false, data, code, message);
        }
        public override string ToString()
        {
            return string.Format("{{Result:{0},Code:{1},Message:{2}}}", this.Result, this.Code, this.Message);
        }
    }
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TResult<T> : TResult
    {
        public T Data { get; set; }
        public TResult()
        {
            this.Result = true;
            this.Code = 0;
            this.Message = null;
        }
        public TResult(bool result, T data = default(T), int code = 0, string message = null)
             : base(result, code, message)
        {
            this.Data = data;
        }
    }
}
