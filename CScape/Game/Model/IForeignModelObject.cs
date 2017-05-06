namespace CScape.Game.Model
{
    public interface IPlayerForeignModel : IForeignModelObject<string, PlayerModel> { }

    public interface IForeignModelObject<TId, TModel>
    {
        int Id { get; }
        TId ForeignKey { get; set; }
        TModel Model { get; set; }
    }
}