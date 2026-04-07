#nullable enable

namespace HyperCasualGame.Scripts.Features.Meta.Persistence
{
    using Cysharp.Threading.Tasks;
    using GameFoundationCore.Scripts.Models.Interfaces;
    using GameFoundationCore.Scripts.Utilities.UserData;

    public sealed class LocalSaveFacade
    {
        private readonly IHandleUserDataServices handleUserDataServices;

        public LocalSaveFacade(IHandleUserDataServices handleUserDataServices)
        {
            this.handleUserDataServices = handleUserDataServices;
        }

        public UniTask<T> LoadAsync<T>() where T : class, ILocalData
        {
            return this.handleUserDataServices.Load<T>();
        }

        public UniTask SaveAsync<T>(T data, bool force = false) where T : class, ILocalData
        {
            return this.handleUserDataServices.Save(data, force);
        }

        public UniTask SaveAllAsync()
        {
            return this.handleUserDataServices.SaveAll();
        }
    }
}
