using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FeedMe.Model;
using FeedMe.ViewModel;

namespace FeedMe.Views
{
    /// <summary>  
    /// Interaction logic for HomePage.xaml  
    /// </summary>  
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            Logic.InitPage(UserComboBox, MenuPanel, Cal1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            string user = UserComboBox.Text;

            if (user.Length == 0 || user == null)
            {
                MessageBox.Show("Please select an user. ");
                return;
            }

            StackPanel stpnl = (StackPanel)VisualTreeHelper.GetParent((Button)sender);
            Border brdr = (Border)VisualTreeHelper.GetParent(stpnl);
            Grid topLevel = (Grid)VisualTreeHelper.GetParent(brdr);
            StackPanel spMenu = (StackPanel)LogicalTreeHelper.FindLogicalNode(topLevel, "MenuPanel");
            Logic.SetLastOrder(user, spMenu);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void But1_Click(object sender, EventArgs e)
        {
            StackPanel stpnl = (StackPanel)VisualTreeHelper.GetParent((Button)sender);
            Grid grd = (Grid)VisualTreeHelper.GetParent(stpnl);
            StackPanel topLevel = (StackPanel)VisualTreeHelper.GetParent(grd);

            bool mainRow = MenuView.GetIfExtras(grd); 
            int cat = MenuView.GetCategoryID(grd); 

            int quan = MenuView.GetQuantity(grd); 

            if (DAL.HasExtra(cat) && mainRow && quan == 1)
            {
                int temp = MenuView.GetGridRow(grd);
                topLevel.Children.RemoveAt(temp);
            }
            else
            {

                if (quan > 0)
                {
                    quan--;
                    MenuView.SetQuantity(grd, quan);
                }
            }
            MenuView.SetTotalPrice(topLevel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void But2_Click(object sender, EventArgs e)
        {
            var callingButton = (Button)sender;

            StackPanel stpnl = (StackPanel)VisualTreeHelper.GetParent(callingButton);
            Grid grd = (Grid)VisualTreeHelper.GetParent(stpnl);
            StackPanel SPmenu = (StackPanel)VisualTreeHelper.GetParent(grd);

            bool mainRow = MenuView.GetIfExtras(grd); 
            int cat = MenuView.GetCategoryID(grd); 

            if (DAL.HasExtra(cat) && !mainRow)
            {
                //Create new row if main row, else, increase quantity
                string[] sendData = new string[5];
                sendData[0] = ""; //j.Text;
                sendData[1] = MenuView.GetCategoryID(grd).ToString();
                sendData[2] = MenuView.GetDish(grd);
                sendData[3] = MenuView.GetDishID(grd).ToString();
                sendData[4] = MenuView.GetPrice(grd);
                int row = MenuView.GetGridRow(grd);
                row++;
                Logic.InsertRow(row, SPmenu, sendData);
            }
            else
            {
                int quan = MenuView.GetQuantity(grd);
                quan++;
                MenuView.SetQuantity(grd, quan);
            }
            MenuView.SetTotalPrice(SPmenu);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan start = new TimeSpan(10, 0, 0);
            TimeSpan now = DateTime.Now.TimeOfDay;
            if (start < now && Cal1.SelectedDate == DateTime.Today)
            {
                MessageBox.Show("No more orders are accepted after 10:00. Your order has not been sent.");
                return;
            }

            string user = UserComboBox.Text;
            DateTime date = Cal1.SelectedDate.Value;
            string comment = CommentTb.Text;
            StackPanel menu = MenuPanel;

            if (user.Length == 0 || user == null)
            {
                MessageBox.Show("Please select an user. Your order has not been sent.");
                return;
            }
            Logic.SaveOrders(user, date, comment, menu);
        }
    }
}