using System.Threading.Tasks;

namespace Core
{
    public interface IBuilder
    {
        Task Build(IBuildScheme scheme);
    }

    public interface IBuildScheme
    {
        Task Execute();
    }
}