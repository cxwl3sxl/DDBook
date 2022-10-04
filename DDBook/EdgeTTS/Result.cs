using System.IO;

namespace DDBook.EdgeTTS
{
    public enum ResultCode
    {
        Success,
        Fail,
        NetworkFail,
    }
    public class Result
    {
        public string Message { get; set; }

        public ResultCode Code { get; set; }

        public MemoryStream Data { get; set; }
    }
}