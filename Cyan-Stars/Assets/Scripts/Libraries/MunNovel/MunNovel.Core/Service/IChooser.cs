using System.Threading.Tasks;

namespace MunNovel.Service
{
    public interface IChooser
    {
        ValueTask ShowOptionAsync(string text, string selectId, object selectedResult);
        Task<ChooseResult> GetSelectResultAsync();
    }

    public readonly struct ChooseResult
    {
        public readonly bool IsSucceed;
        public readonly string SelectId;
        public readonly object Data;

        private ChooseResult(bool isSucceed, string selectId, object data)
        {
            this.IsSucceed = isSucceed;
            this.SelectId = selectId;
            this.Data = data;
        }

        public static ChooseResult Fail()
        {
            return default;
        }

        public static ChooseResult Success(string selectedId, object data)
        {
            return new ChooseResult(true, selectedId, data);
        }
    }
}
