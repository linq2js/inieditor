using System;
using System.Drawing;
using System.Windows.Forms;

namespace IniEditor
{
    public class SectionBlock
    {
        public const int DefaultWidth = 200;
        private const int HeadingHeight = 20;
        private const int ItemHeight = 20;
        public const int Spacing = 10;
        private const int MaxDisplayItems = 30;
        private static readonly object MoreObject = new object();
        private static readonly SectionStyle MainStyle = new SectionStyle
        {
            ForeColor = Color.Black,
            BackColor = Color.RoyalBlue,
            BorderColor = Color.RoyalBlue,
            HeadingColor = Color.White
        };
        private static readonly SectionStyle DefaultStyle = new SectionStyle
        {
            ForeColor = Color.Black,
            BackColor = Color.Silver,
            BorderColor = Color.Black,
            HeadingColor = Color.Black
        };

        public class Data
        {
            public SectionStyle Style;
            public string Text;
            public BlockType Type;
            public object[] Properties = new object[0];
        }

        public event EventHandler LeftClick;
        public event EventHandler RightClick;
        public BlockType Type { get; }
        public string Text { get;  }

        public int Width
        {
            get => _width;
            set
            {
                _width = value;
                UpdateHeight();
            }
        }

        

        public int Height { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public SectionStyle Style { get; }
        public int Right => Left + Width;
        public int Bottom => Top + Height;
        public int Count => _items.Length;
        public bool SizeChanged { get; set; }
        private readonly Font _headingFont = new Font("Verdana", 8, FontStyle.Bold);
        private readonly Font _bodyFont = new Font("Verdana", 8);
        private readonly Font _symbolFont = new Font("Segoe MDL2 Assets", 10);
        private readonly object[] _items = new object[0];
        private int _width;
        private bool _collapsed;

        public SectionBlock(Data data)
        {
            var defaultStyle = data.Type == BlockType.Main ? MainStyle : DefaultStyle;
            Type = data.Type;
            Text = data.Text;
            Style = data.Style ?? defaultStyle;


            // make sure no style prop can be null
            if (Style.BackColor == null)
            {
                Style.BackColor = defaultStyle.BackColor;
            }
            if (Style.ForeColor == null)
            {
                Style.ForeColor = defaultStyle.ForeColor;
            }
            if (Style.BorderColor == null)
            {
                Style.BorderColor = defaultStyle.BorderColor;
            }
            if (Style.HeadingColor == null)
            {
                Style.HeadingColor = defaultStyle.HeadingColor;
            }

            Width = Type == BlockType.Main ? 320 : DefaultWidth;

            _items = data.Properties;

            if (_items.Length > MaxDisplayItems && (Type == BlockType.Main || Type == BlockType.Usages))
            {
                _collapsed = true;
            }

            if (Type == BlockType.Main || Type == BlockType.Links)
            {
                // do not collapse
            }
            else if (data.Properties.Length == 0)
            {
                Height = HeadingHeight;
            }
            else
            {
                Height = data.Properties.Length * ItemHeight + HeadingHeight;
            }
        }

        private void Each(Action<object, Rectangle> action)
        {
            Each((i, b) =>
            {
                action(i, b);
                return false;
            });
        }

        private void UpdateHeight()
        {
            var bottom = 0;
            Each((o, b) =>
            {
                if (b.Bottom > bottom)
                {
                    bottom = b.Bottom;
                }
            });

            Height = bottom - Top;
        }

        private void Each(Func<object, Rectangle, bool> action)
        {
            var columns = (Width + Spacing) / (DefaultWidth + Spacing);
            var c = 0;
            var r = 0;
            var i = 0;
            foreach (var item in _items)
            {
                
                var width = columns == 1 ? Width : DefaultWidth;
                var bounds = new Rectangle(Left + c * (Spacing + width), Top + HeadingHeight + r * ItemHeight, width, ItemHeight);

                if (_collapsed && i >= MaxDisplayItems)
                {
                    action(MoreObject, bounds);
                    break;
                }

                var stop = action(item, bounds);
                if (stop) break;

                i++;
                c++;
                if (c >= columns)
                {
                    c = 0;
                    r++;
                }
            }
        }

        public void Draw(PaintEventArgs e)
        {
            var headingFillRect = new Rectangle(Left, Top, Width, HeadingHeight);
            var headingTextRect = new Rectangle(Left + 5, Top, Width - 10, HeadingHeight);
            var stringFormat = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };

            if (!e.ClipRectangle.IntersectsWith(new Rectangle(Left, Top, Width, Height)))
            {
                return;
            }

            // draw heading
            using (var headingFillBrush = new SolidBrush(Style.BackColor.Value))
            {
                e.Graphics.FillRectangle(headingFillBrush, headingFillRect);
            }

            using (var headingTextBrush = new SolidBrush(Style.HeadingColor.Value))
            {
                e.Graphics.DrawString(Text, _headingFont, headingTextBrush, headingTextRect, stringFormat);
            }

            using (var bodyTextBrush = new SolidBrush(Style.ForeColor.Value))
            {
                Each((o, b) =>
                {
                    if (!e.ClipRectangle.IntersectsWith(b))
                    {
                        return;
                    }

                    var bodyTextRect = new Rectangle(b.Left + 5, b.Top, b.Width - 10, b.Height);
                    var text = o == MoreObject ? "" : o?.ToString() ?? string.Empty;
                    e.Graphics.DrawString(text, o == MoreObject ? _symbolFont : _bodyFont, bodyTextBrush, bodyTextRect, stringFormat);
                });
            }

            using (var borderPend = new Pen(Style.BorderColor.Value))
            {
                e.Graphics.DrawRectangle(borderPend, Left, Top, Width - 1, Height - 1);
            }
        }

        public object Hover(MouseEventArgs e)
        {
            object found = null;
            Each((i, b) =>
            {


                if (b.Contains(e.Location))
                {
                    if (i == MoreObject)
                    {
                        return true;
                    }
                    found = i;
                    return true;
                }
                return false;
            });
            return found;
        }

        public bool HitTest(MouseEventArgs e)
        {
            var found = false;
            Each((i, b) =>
            {
                

                if (b.Contains(e.Location))
                {
                    if (i == MoreObject)
                    {
                        found = true;
                        _collapsed = !_collapsed;
                        SizeChanged = true;
                        UpdateHeight();
                        return true;
                    }

                    switch (e.Button)
                    {
                        case MouseButtons.Left:
                            LeftClick?.Invoke(i, new EventArgs());
                            break;
                        case MouseButtons.Right:
                            RightClick?.Invoke(i, new EventArgs());
                            break;
                    }
                    found = true;
                    return true;
                }
                return false;
            });
            return found;
        }
        
    }
}
