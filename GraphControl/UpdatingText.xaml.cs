using GraphData;
using System.Windows.Controls;
using System.Windows.Media;

namespace GraphControl
{
    /// <summary>
    /// Interaction logic for UpdatingText.xaml
    /// </summary>
    public partial class UpdatingText : UserControl, IUpdating
    {
        public UpdatingText()
        {
            InitializeComponent();
        }

        public string Title
        {
            get
            {
                return TitleBlock.Text;
            }
            set
            {
                TitleBlock.Text = value;
            }
        }
        public Color Color
        {
            get
            {
                var br = TitleBlock.Foreground as SolidColorBrush;
                return br.Color;
            }
            set
            {
                Brush newBrush = new SolidColorBrush(value);
                TitleBlock.Foreground = newBrush;
                TextBlock.Foreground = newBrush;
            }
        }

        public void Update(GraphDataPacket text)
        {
            TextBlock.Text = text.GetTextData();
        }
    }
}
