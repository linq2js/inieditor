namespace IniEditor
{
    public class InlineEditData
    {
        public string Text { get; private set; }
        public bool Changed { get; set; }
        public Deferred<InlineEditData> Closed { get; }
        public AData.Location Location { get; }

        public InlineEditData(string text, AData.Location location)
        {
            Text = text;
            Location = location;
            Closed = new Deferred<InlineEditData>();
        }

        public void Change(string text)
        {
            if (text == Text) return;
            Changed = true;
            Text = text;
        }

        public void Close()
        {
            if (Closed.State == Deferred.Statuses.Resolved) return;
            Closed.Resolve(this);
        }
    }
}
