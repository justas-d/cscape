using CScape.Models.Data;
using CScape.Models.Game.Entity;

namespace CScape.Models.Tests.Mock
{
    // TODO : better way of specifying which implementation to test
    public interface IModelImplementation
    {
        IEntityHandle CreateEntity(string name = "test entity");
        IConfigurationService GetConfig();
    }
}