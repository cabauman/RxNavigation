using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ReactiveUI;
using Splat;
using Windows.UI.Xaml.Controls;

namespace Sample.Native.UWP.Views
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public MainPage()
        {
            InitializeComponent();
            List<Task<int>> tasks = new List<Task<int>>();
            for (int i = 0; i < 4; ++i)
            {
                tasks.Add(DoWork(i * 1000));
            }
            tasks.ToObservable().SelectMany(x => x).ToList()
                .Subscribe(
                    x =>
                    {
                        System.Diagnostics.Debug.WriteLine(x);
                    },
                    onCompleted: () =>
                    {
                        System.Diagnostics.Debug.WriteLine("Completed");
                    });
            System.Diagnostics.Debug.WriteLine("Subsciption done.");
        }

        private async Task<int> DoWork(int ms)
        {
            await Task.Delay(ms);
            System.Diagnostics.Debug.WriteLine(DateTime.Now.TimeOfDay);
            return ms;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
