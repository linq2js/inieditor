using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace IniEditor
{

    public partial class SectionDiagram : UserControl
    {
        private SectionBlock _mainBlock;
        private SectionBlock _linksBlock;
        private readonly IList<SectionBlock> _normalBlocks = new List<SectionBlock>();
        private readonly IList<SectionBlock> _allBlocks = new List<SectionBlock>();
        private readonly Stack<IEnumerable<SectionBlock.Data>> _stack = new Stack<IEnumerable<SectionBlock.Data>>();
        private object _hoverItem;

        public SectionDiagram()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            foreach (var block in _allBlocks)
            {
                block.Draw(e);
            }

            Debug.WriteLine("Drawing");
        }

        public void AddBlocks(IEnumerable<SectionBlock.Data> blocks, bool noStack = false)
        {
            if (!noStack)
            {
                _stack.Push(blocks);
            }
            
            _mainBlock = null;
            _linksBlock = null;
            _allBlocks.Clear();
            _normalBlocks.Clear();

            foreach (var block in blocks)
            {
                AddBlock(block);
            }

            ArrangeBlocks();
        }

        private void AddBlock(SectionBlock.Data data)
        {
            var block = new SectionBlock(data);

            if (block.Type == BlockType.Main)
            {
                _mainBlock = block;
            }
            else if (block.Type == BlockType.Links)
            {
                _linksBlock = block;
            }
            else
            {
                _normalBlocks.Add(block);
            }

            _allBlocks.Add(block);

            block.LeftClick += BlockLeftClick;
            block.RightClick += BlockRightClick;
        }


        private void HandleBlockClick(object data, Action<string, AData.Location> moveAction)
        {
            if (data is AData.LocationGroup group)
            {
                if (group.Locations.Length > 1)
                {
                    locationMenu.Items.Clear();
                    // create menu item for locations
                    foreach (var location in group.Locations)
                    {
                        var filePath = App.FilePath(location.FileId);
                        var item = new ToolStripMenuItem($"{Path.GetFileName(filePath)} ({Path.GetDirectoryName(filePath)})");
                        item.Click += delegate
                        {
                            moveAction(group.Key, location);
                        };
                        locationMenu.Items.Add(item);
                    }

                    locationMenu.Show(MousePosition);
                }
                else
                {
                    moveAction(group.Key, group.Locations.First());
                }
            }
        }

        private void BlockRightClick(object sender, EventArgs e)
        {
            HandleBlockClick(sender, async (word, location) =>
            {
                await App.Instance.Diagram(word, location.FileId, blocks => AddBlocks(blocks));
            });
        }

        private void BlockLeftClick(object sender, EventArgs e)
        {
            HandleBlockClick(sender, (word, location) =>
            {
                Redux.Timer.Timeout(100, () => App.Instance.GoTo(location));
                FindForm()?.Close();
            });
        }

        private void ArrangeBlocks()
        {
            if (_mainBlock == null || _allBlocks.Count == 0) return;
            
            var left = 0;
            var top = 0;

            var wideBlocks = new List<SectionBlock>
            {
                _mainBlock
            };

            if (_linksBlock != null)
            {
                wideBlocks.Add(_linksBlock);
            }

            wideBlocks.AddRange(_normalBlocks.Where(x => x.Count > 15));

            var maxBottom = 0;

            foreach (var block in wideBlocks)
            {
                block.Top = top;
                block.Left = left;
                block.Width = Width;
                maxBottom = top = block.Bottom + SectionBlock.Spacing;
            }

            // arrange rest blocks
            var amountOfColumn = Math.Max(1, (Width - left - SectionBlock.Spacing) / (SectionBlock.DefaultWidth + SectionBlock.Spacing));
            var columns = Enumerable.Range(0, amountOfColumn).Select(x => new List<SectionBlock>()).ToArray();
            

            foreach (var block in _normalBlocks.Except(wideBlocks).OrderByDescending(x => x.Style.Order).ThenBy(x => x.Count))
            {
                // find out column has min bottom
                var minBottom = int.MaxValue;
                var i = -1;
                var colIndex = 0;
                List<SectionBlock> foundColumn = null;
                foreach (var column in columns)
                {
                    i++;

                    if (column.Count == 0)
                    {
                        foundColumn = column;
                        colIndex = i;
                        minBottom = top - SectionBlock.Spacing;
                        break;
                    }
                    var bottom = column.Last().Bottom;

                    if (bottom < minBottom)
                    {
                        foundColumn = column;
                        minBottom = bottom;
                        colIndex = i;
                    }
                }

                foundColumn?.Add(block);

                block.Left = left + colIndex * (SectionBlock.DefaultWidth + SectionBlock.Spacing);
                block.Top = minBottom + SectionBlock.Spacing;
                if (block.Bottom > maxBottom)
                {
                    maxBottom = block.Bottom;
                }
            }

            Height = maxBottom + 5;

            Refresh();
        }

        public void Back()
        {
            if (_stack.Count == 1) return;
            _stack.Pop();
            AddBlocks(_stack.Peek(), true);
        }

        private void SectionDiagram_Resize(object sender, EventArgs e)
        {
            ArrangeBlocks();
        }

        private void SectionDiagram_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (var block in _allBlocks)
            {
                if (block.HitTest(e))
                {
                    if (block.SizeChanged)
                    {
                        block.SizeChanged = false;
                        ArrangeBlocks();
                    }
                    break;
                }
            }
        }

        private void SectionDiagram_MouseMove(object sender, MouseEventArgs e)
        {
            var found = false;
            foreach (var block in _allBlocks)
            {
                var i = block.Hover(e);
                
                if (i != null)
                {
                    if (block.Type != BlockType.Main)
                    {
                        Cursor = Cursors.Hand;
                    }
                    var form = FindForm();
                    var point = PointToScreen(new Point(e.X - form.Left, e.Y - form.Top));
                    point.Offset(10, 10);
                    tooltip.Show(i.ToString(), form, point, 1000);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Cursor = DefaultCursor;
                tooltip.Hide(FindForm());
            }
        }
    }
}
