using CScape.Models.Data;
using CScape.Models.Game.Entity;

namespace CScape.Dev.Tests.Mock
{
    public interface IModelImplementation
    {
        IEntityHandle CreateEntity(string name = "test entity");
        IConfigurationService GetConfig();
    }
}