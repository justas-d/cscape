namespace CScape.Model
{
    public interface IPlayerForeignModel : IForeignModelObject<string, PlayerModel> { }

    public interface IForeignModelObject<TId, TModel>
    {
        TId ForeignKey { get; set; }
        TModel Model { get; set; }
    }
}