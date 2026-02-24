using System.Threading.Tasks;
using Blazored.LocalStorage;

namespace TestBlazor.Client.Services
{
    public class CounterService
    {
        private readonly ILocalStorageService _localStorage;
        private const string Key = "CounterValue";

        public CounterService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public int Count { get; private set; }
        public bool IsLoaded { get; private set; }

        public event Action? OnCountChanged;

        public async Task InitializeAsync()
        {
            if (IsLoaded) return;
            Count = await _localStorage.GetItemAsync<int>(Key);
            IsLoaded = true;
            OnCountChanged?.Invoke();
        }

        public async Task IncrementCountAsync()
        {
            Count++;
            await _localStorage.SetItemAsync(Key, Count);
            OnCountChanged?.Invoke();
        }
    }
}
