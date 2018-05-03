using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;

namespace IniEditor
{
    public class MenuRenderer : ToolStripProfessionalRenderer
    {
        private readonly MaterialSkinManager _skinManager;

        public MenuRenderer(MaterialSkinManager skinManager) : base(new MenuColors(skinManager))
        {
            _skinManager = skinManager;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            // is top menu
            if (e.Item.OwnerItem == null)
            {
                
                if (!e.Item.Pressed)
                {
                    e.TextColor = _skinManager.ColorScheme.TextColor;
                }
            }
            else
            {
                if (e.Item.Pressed || e.Item.Selected)
                {
                    e.TextColor = _skinManager.ColorScheme.TextColor;
                }
            }
            
            base.OnRenderItemText(e);
        }
    }

    public class MenuColors : ProfessionalColorTable
    {
        private readonly MaterialSkinManager _skinManager;

        public MenuColors(MaterialSkinManager skinManager)
        {
            _skinManager = skinManager;
        }

        public override Color MenuItemSelected => _skinManager.ColorScheme.PrimaryColor;
        public override Color MenuItemSelectedGradientBegin => _skinManager.ColorScheme.PrimaryColor;
        public override Color MenuItemSelectedGradientEnd => _skinManager.ColorScheme.PrimaryColor;
    }
}
