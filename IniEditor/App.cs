using System;
using System.IO;
using Redux;

namespace IniEditor
{
    public partial class App : Redux<AppModel>
    {
        private App() : base(new AppModel())
        {
        }

        public static App Instance = new App();

        public void Init()
        {
            AutoSave();
            Analyze();
        }

        public void Close()
        {
            foreach (var disposable in Model.Disposables)
            {
                disposable?.Dispose();
            }

            foreach (var disposer in Model.Disposers)
            {
                disposer?.Invoke();
            }
        }
    }
}
