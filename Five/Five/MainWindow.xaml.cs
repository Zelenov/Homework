using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Five
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Field start = Field.RandomField();
        private Button[] buttons;
        private const int speed = 100;
        private Thread animThread;    
        public MainWindow()
        {
            InitializeComponent();
            buttons = new Button[] { button1, button2, button3, button4, button5, button6, button7, button8 };
        }
        private void Run(Func<Field,Tuple<List<Field>,int>> func)
        {
            //var solve = Solver<Field>.BFS(start, out pathsViewed);
            text.Text = start + "\n_________________\n";
            SetEditibleButtons(false);
            var solveTuple = func(start);
            var solve = solveTuple.Item1;
            var pathsViewed = solveTuple.Item2;
            text.Text += "Paths visited: "+pathsViewed;
            
            animThread = new Thread(() =>
                           {
                               int i = 0;
                               Field lastField=null;
                               foreach (var field in solve)
                               {
                                   i++;
                                   
                                   int i1 = i;
                                   Field field1 = field;
                                   Dispatcher.BeginInvoke(new Action(() =>
                                                                         {
                                                                             text.Text += "\n\n(" + i1 + ")\n" + field1;
                                                                         }));
                                   
                                   if (lastField != null)
                                   {
                                       Diff mov = Field.GetDifference(lastField, field);
                                       if (mov == null)
                                           return;
                                       Dispatcher.BeginInvoke(new Action(() =>AnimateButton(mov.movedNum, mov.dx, mov.dy)));
                                       Thread.Sleep(speed);  
                                   }
                                   lastField = field; 

                               }
                           }
                ){IsBackground = true};
            animThread.Start();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Run(st =>
                    {
                        int pathsViewed;
                        return new Tuple<List<Field>, int>(Solver<Field>.BFS(start, out pathsViewed),pathsViewed);
            });
            
        }
        private void Regen()
        {
            start = Field.RandomField();
            text.Text = start+"\n_________________\n";
            PlaceButtons();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Regen();
            SetEditibleButtons(true);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Regen();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Run(st =>
            {
                int pathsViewed;
                return
                    new Tuple<List<Field>, int>(
                        Solver<Field>.AStar(start, out pathsViewed, p => p.GetHeuristics1(), (a, b) => 1), pathsViewed);
            });
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Run(st =>
            {
                int pathsViewed;
                return new Tuple<List<Field>, int>(Solver<Field>.AStar(start, out pathsViewed, p => p.GetHeuristics2(), (a, b) => 1), pathsViewed);
            });
        }
        public void AnimateButton(int btnNum, int dx, int dy)
        {
          
            if (btnNum > buttons.Length || btnNum < 0)
                return;

            var button = buttons[btnNum-1];

            var anim = new ThicknessAnimation(button.Margin,
                                              new Thickness(button.Margin.Left + dx*button.Width,
                                                            button.Margin.Top + dy*button.Height, button.Margin.Right,
                                                            button.Margin.Bottom),
                                              new Duration(TimeSpan.FromMilliseconds(speed)));
            button.BeginAnimation(Button.MarginProperty, anim);
            
            
        }

        private void PlaceButtons()
        {
            start.Traverse((i, j, el) =>
                               {
                                   if (el==Field.emptyCell)
                                       return;
                                   var button = buttons[el-1];
                                   button.BeginAnimation(Button.MarginProperty, null);
                                   button.Margin = new Thickness(i*button.Width,
                                                                 j*button.Height,
                                                                 button.Margin.Right, button.Margin.Bottom);
                               });
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button) sender;
            var num = int.Parse(btn.Content.ToString());
            var clickedField = start.MovePiece(num);
            if (clickedField==null)
                return;
            Diff mov = Field.GetDifference(start, clickedField);
            if (mov == null)
                return;
            start = clickedField;
            text.Text = start + "\n_________________\n";
            AnimateButton(mov.movedNum, mov.dx, mov.dy);
            
        }
        public void SetEditibleButtons(bool enabled)
        {
            PlaceButtons();
            if (animThread!=null)
            {
                animThread.Abort();
                animThread = null;
            }
            foreach (var button in buttons)
            {
                button.IsEnabled = enabled;
            }
            
            restoreBtn.Visibility = enabled ? Visibility.Hidden : Visibility.Visible; 
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            SetEditibleButtons(true);
        }
    }
}
