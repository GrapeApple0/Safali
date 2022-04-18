using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Safali.View
{
    /// <summary>
    /// TabControlEx.xaml の相互作用ロジック
    /// </summary>
    /// <summary>
    /// A TabControl with large tabs. 
    /// </summary>
    public partial class TabControlEx : TabControl
    {
        public TabControlEx()
        {
           
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.Template != null)
            {
                UniformGrid X = this.Template.FindName("headerPanel", this) as UniformGrid;
                if (X != null) X.Columns = this.Items.Count;
            }
        }
    }
}
