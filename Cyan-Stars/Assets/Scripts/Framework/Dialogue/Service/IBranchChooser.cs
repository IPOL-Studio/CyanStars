using System.Collections.Generic;
using System.Threading.Tasks;

namespace CyanStars.Framework.Dialogue
{
    public interface IBranchChooser : IService
    {
        Task ShowOptionsAsync(IList<BranchOption> options);
        Task<BranchOption> GetSelectOptionAsync();
    }
}
