//using System;
//using System.Text;

//namespace IniEditor
//{
//    public class IniWriter
//    {
//        private readonly StringBuilder _contents = new StringBuilder();

//        public IniWriter Section(string value)
//        {
//            _contents.AppendLine($"[{value}]");
//            return this;
//        }

//        public IniWriter Comment(string value)
//        {
//            foreach (var line in value.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries))
//            {
//                _contents.AppendLine(";" + line);
//            }
//            return this;
//        }

//        public IniWriter Property(string name, object value)
//        {
//            _contents.AppendLine($"");
//            return this;
//        }

//        public override string ToString()
//        {
//            return _contents.ToString();
//        }
//    }
//}
