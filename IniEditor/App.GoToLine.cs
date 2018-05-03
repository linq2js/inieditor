using IniEditor.GoTo;

namespace IniEditor
{
    public partial class App
    {
        private GoToDialog _gotoDialog;
        public void GoToLine()
        {
            // nothing to show if no document loaded
            if (Model.Document == null) return;

            if (_gotoDialog == null)
            {
                _gotoDialog = new GoToDialog();
            }

            _gotoDialog.Show(Model.Document.Editor.FindForm());
        }
    }
}
