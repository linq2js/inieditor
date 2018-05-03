using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Redux
{
    public static class ControlExtensions
    {
        private class ControlKey
        {
            public ControlKey(object key)
            {
                Key = key;
            }

            public object Key { get; }
        }

        public static IEnumerable<TControl> RenderControlList<TModel, TControl>(this IEnumerable<TModel> models, Control.ControlCollection controls, Func<TModel, TControl> builder) where TControl : Control
        {
            return RenderControlList(models, controls, x => x, builder);
        }

        public static IEnumerable<TControl> RenderControlList<TModel, TKey, TControl>(this IEnumerable<TModel> models, Control.ControlCollection controls, Func<TModel, TKey> key, Func<TModel, TControl> builder) where TControl : Control
        {
            return RenderControlList(models, controls, (m, i) => key(m), (m, i) => builder(m));
        }

        public static IEnumerable<TControl> RenderControlList<TModel, TKey, TControl>(this IEnumerable<TModel> models, Control.ControlCollection controls, Func<TModel, int, TKey> key, Func<TModel, int, TControl> builder) where TControl : Control
        {
            var existingKeys = controls.OfType<Control>().Where(x => x.Tag is ControlKey).ToDictionary(x => ((ControlKey) x.Tag).Key);
            var index = 0;
            var result = new List<TControl>();

            foreach (var m in models)
            {
                var k = key(m, index);
                if (existingKeys.TryGetValue(k, out Control c))
                {
                    // not build again
                    existingKeys.Remove(k);
                }
                else
                {
                    c = builder(m, index);
                    c.Tag = new ControlKey(k);
                    controls.Add(c);
                }

                controls.SetChildIndex(c, index);
                index++;
                result.Add((TControl) c);
            }

            // remove unused controls
            foreach (var value in existingKeys.Values)
            {
                controls.Remove(value);
            }

            return result;
        }

        public static TControl ChangeText<TControl>(this TControl control, string text) where TControl : Control
        {
            if (control.Text != text)
            {
                control.Text = text;
            }
            return control;
        }
    }
}
