using System;
using System.Windows.Controls;
using System.Windows;

namespace FeedMe.ViewModel
{
    static class MenuView
    {

        public static string GetCategory(Grid grd)
        {
            TextBlock currTB = (TextBlock)LogicalTreeHelper.FindLogicalNode(grd, "ColCategory");
            string ret = currTB.Text;
            return ret;
        }
        public static int GetCategoryID(Grid grd)
        {
            TextBlock currTB = (TextBlock)LogicalTreeHelper.FindLogicalNode(grd, "ColCategory");
            int ret = Int32.Parse(currTB.Tag.ToString());
            return ret;
        }

        public static string GetDish(Grid grd)
        {
            TextBlock currTB = (TextBlock)LogicalTreeHelper.FindLogicalNode(grd, "ColDish");
            string ret = currTB.Text;
            return ret;
        }

        public static int GetDishID(Grid grd)
        {
            TextBlock currTB = (TextBlock)LogicalTreeHelper.FindLogicalNode(grd, "ColDish");
            int ret = Int32.Parse(currTB.Tag.ToString());
            return ret;
        }

        public static string GetPrice(Grid grd)
        {
            TextBlock currTB = (TextBlock)LogicalTreeHelper.FindLogicalNode(grd, "ColPrice");
            string ret = currTB.Text;
            return ret;
        }
        public static int GetQuantity(Grid grd)
        {
            TextBlock currTB = (TextBlock)LogicalTreeHelper.FindLogicalNode(grd, "ColQuantityTxt");
            int ret = Int32.Parse((currTB.Text).ToString());
            return ret;
        }

        public static void SetQuantity(Grid grd, int quantity)
        {
            TextBlock currTB = (TextBlock)LogicalTreeHelper.FindLogicalNode(grd, "ColQuantityTxt");
            currTB.Text = quantity.ToString();
        }

        public static bool GetIfExtras(Grid grd)
        {
            StackPanel currSP = (StackPanel)LogicalTreeHelper.FindLogicalNode(grd, "ColExtra");
            bool ret = bool.Parse(currSP.Tag.ToString());
            return ret;
        }

        public static void SetTotalPrice(StackPanel SPMenu)
        {
            double totPrice = 0;
            TextBlock txtBlck;
            foreach (object child in SPMenu.Children)
            {
                string childname = null;
                if (child is Grid grid)
                {
                    childname = (child as Grid).Name;
                    if (childname != null && childname != "grdHeader")
                    {
                        Grid grd = grid;
                        double thisQuan = GetQuantity(grid); 
                        double thisPrice = Double.Parse(GetPrice(grid));
                        double temp = thisQuan * thisPrice;
                        totPrice += temp;
                    }
                }
            }
            Border brdr = (Border)LogicalTreeHelper.GetParent(SPMenu);
            Grid topLevel = (Grid)LogicalTreeHelper.GetParent(brdr);
            txtBlck = (TextBlock)LogicalTreeHelper.FindLogicalNode(topLevel, "TotalPrice");
            txtBlck.Text = string.Format("{0:N2}", totPrice);
        }

        public static int GetGridRow(Grid grd)
        {
            StackPanel topLevel = (StackPanel)LogicalTreeHelper.GetParent(grd);
            int ret = topLevel.Children.IndexOf(grd);
            return ret;
        }
    }
}
