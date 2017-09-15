namespace Golem.Common.Interfaces
{
    public interface ISpawnable
    {
        void OnBeforeSpawn();
        void OnAfterSpawn();

        ISpawner Spawner { get; set; }
    }
}