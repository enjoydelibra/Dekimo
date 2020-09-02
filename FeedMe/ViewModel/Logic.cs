using System;
using System.Collections.Generic;
using FeedMe.Model;
using System.Windows.Controls;

using FeedMe.Views;
using System.Windows;

/*
 * 
 * WORKING NOTES
 * 
 * 
 int or int32?
 => int is a primitive type allowed by the C# compiler, whereas Int32 is the Framework Class Library type 
 (available across languages that abide by CLS). In fact, int translates to Int32 during compilation.

 Int32.parse or int32.TryParse 
 -> We want the error when no int passed or conversion failed. All passed ints are created by software and should be OK.
    Otherwise there is an error in the code.
 -> Use tryParse when input from end user expected and failure is more likely.

 Every return must return an error message
 -> Used Try catch everywhere to catch errors

 Check what usings necessary
 -> Apparently Visual studio greys them out automatically, handy!

 Done 
    check what private and what public

 TODO 
    bedenkingen meer klasses gebruiken! ipv sql zoeken
    ipv alles in formulier menutabel op te slaan, beter klasse voor elk lijntje/dish maken!
*/

namespace FeedMe.ViewModel
{
    public class Logic
    {

        /// <summary>
        /// Called to initialize the page the first time when FeedMe is started.
        /// </summary>
        /// <param name="CBUsers">Combobox containing the users</param>
        /// <param name="SPMenu">Stackpanel containing the Grids for the menuSP</param>
        /// <param name="DPCalendar">Calender where the date is picked</param>
        public static void InitPage(ComboBox CBUsers, StackPanel SPMenu,DatePicker DPCalendar)
        {
            PopulateUsers(CBUsers);
            PopulateMenu(SPMenu);
            SetCalendar(DPCalendar);
        }

        /// <summary>
        /// Fill combobox with all users in database
        /// </summary>
        /// <param name="CBUsers">Combobox that will be filled in</param>
        private static void PopulateUsers(ComboBox CBUsers)
        {
            try
            {
                string[] userArray = DAL.GetDataArray("SELECT [FullName] From [Users]", "FullName");
                foreach (string str in userArray)
                {
                    CBUsers.Items.Add(str);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }

        /// <summary>
        /// Fills in stackpanel with all menu items from database
        /// </summary>
        /// <param name="SPMenu">Stackpanel containing menu</param>
        private static void PopulateMenu(StackPanel SPMenu)
        {
            try
            {
                AddHeader(SPMenu);
                String[,] menuTab = DAL.CreateMenuTable();
                int menuLen = menuTab.GetLength(0);
                for (int i = 0; i < menuLen; i++)
                {
                    String[] sendData = new String[5];
                    int rowLen = menuTab.GetLength(1);
                    for (int j = 0; j < rowLen; j++)
                    {
                        sendData[j] = menuTab[i, j];
                    }
                    AddRow(SPMenu, sendData);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }

        /// <summary>
        /// Add a single row at the end of the menu overview
        /// </summary>
        /// <param name="menuSP">Stackpanel containing menu</param>
        /// <param name="menuRow">String array containing in this order: 
        /// Category name, 
        /// CategoryID, 
        /// Dish name, 
        /// Dish ID, 
        /// Price</param>
        private static void AddRow(StackPanel menuSP, string[] menuRow)
        {
            try
            {
                Grid grd = GetGridRow(menuRow, false);
                menuSP.Children.Add(grd);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }

        /// <summary>
        /// Insert a single row in the menu overview
        /// </summary>
        /// <param name="row">Insert row here</param>
        /// <param name="menuSP">Insert at this stackpanel</param>
        /// <param name="menuRow">Row data</param>
        public static void InsertRow(int row, StackPanel menuSP, string[] menuRow, string orderDishID = null, int quan = 0)
        {
            try
            {
                Grid grd;
                if (orderDishID == null)
                {
                    grd = GetGridRow(menuRow, true);
                }
                else
                {
                    grd = GetGridRow(menuRow, false);
                    SetExistingExtras(grd, orderDishID, MenuView.GetCategoryID(grd).ToString());
                    MenuView.SetQuantity(grd, quan);
                }
                menuSP.Children.Insert(row, grd);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }

        /// <summary>
        /// Creates a grid, filled in with columns to insert in menu
        /// </summary>
        /// <param name="menuRow">Fills in grid with this array. Contains: 
        /// Category name, 
        /// Category ID, 
        /// Dish name, 
        /// Dish ID, 
        /// Price </param>
        /// <param name="fillExtrasCol">True if first time the menu is created. For menu items no need to fill extra's column yet.
        /// The extra's column is only filled in if the quantity of a main-row of a dish (with extra's) is clicked. </param>
        /// <returns>Grid ready to be added to the main menu stackpanel</returns>
        private static Grid GetGridRow(string[] menuRow, bool fillExtrasCol)
        {
            Grid ret = new Grid() { Name = "menuSP", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            try
            {
                /*----------------Init the grid in the correct format------------------*/
                
                ret.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
                ret.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });
                ret.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
                ret.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
                ret.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(140) });
                ret.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });

                /*--------------------------Define column content----------------------*/

                /*-----Column 1: Category-----*/
                TextBlock TBCategory = new TextBlock
                {
                    Name = "ColCategory",
                    Text = menuRow[0],
                    Tag = menuRow[1]
                };

                /*-----Column 2: Dish-----*/
                TextBlock TBDish = new TextBlock
                {
                    Name = "ColDish",
                    Text = menuRow[2],
                    Tag = menuRow[3]
                };

                /*-----Column 3: Price-----*/
                //Price will initially be in Money format (4 digits after comma)
                //We will display price with onle 2 digits after the comma
                string tempStr = menuRow[4];
                int tempInt = tempStr.IndexOf(",");
                TextBlock TBPrice = new TextBlock
                {
                    Name = "ColPrice",
                    Text = menuRow[4].Substring(0, tempInt + 3),
                };

                /*-----Column 4: Quantity-----*/
                //This column will have a - button, the quantity textblock, and a + button
                //These 3 items are stored in another stackpanel
                StackPanel SPQuantity = new StackPanel
                {
                    Name = "ColQuantity",
                    Orientation = Orientation.Horizontal
                };
                Button ButMinus = new Button
                {
                    Name = "butMin",
                    Content = " - ",
                    FontSize = 14,
                    Height = 20
                };
                Button ButPlus = new Button
                {
                    Name = "butPlus",
                    Content = " + ",
                    FontSize = 14,
                    Height = 20
                };
                ButMinus.Click += new RoutedEventHandler(HomePage.But1_Click);
                ButPlus.Click += new RoutedEventHandler(HomePage.But2_Click);

                //Init means new row at creation of table, quantity is default '0'
                //If the row is added afterwards, it means it is a new row with extra's
                //The plus button of the original line has been clicked, and this item receives this quantity '1'
                if (fillExtrasCol)
                {
                    tempStr = "1";
                }
                else
                {
                    tempStr = "0";
                }
                TextBlock TBQuantity = new TextBlock
                {
                    Name = "ColQuantityTxt",
                    Text = tempStr,
                    Margin = new Thickness(10, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                /*-----Column 5: Extra's-----*/
                int category = Int32.Parse(menuRow[1]);
                StackPanel SPExtras;
                //The extra's column is only filled in if the quantity of a main-row of a dish (with extra's) is clicked 
                //The tag of the stackpanel containing all radiobuttons and checkboxes, is indicator if this column is filled in
                //If tag is false, it means it is a dish without extra options, or a main-init row of a dish with extra's, with no extra's filled in
                //If tag is true, it is a dish with extra's filled in
                if (fillExtrasCol && DAL.HasExtra(category))
                {
                    SPExtras = new StackPanel
                    {
                        Name = "ColExtra",
                        Orientation = Orientation.Vertical,
                        Tag = "true"
                    };

                    //extras is jagged array, each row contains options of which only 1 can be selected
                    //each value contains Extra ID, Extra name, Default value
                    (int, string, bool)[][] extras = DAL.GetExtras(category);
                    int len = extras.GetLength(0);
                    for (int j = 0; j < len; j++)
                    {
                        int lenRow = extras[j].Length;

                        //Use radiobutton if more than 1 item in row
                        if (lenRow > 1)
                        {
                            foreach ((int, string, bool) ext in extras[j])
                            {
                                //string str = ext.Item2;
                                SPExtras.Children.Add(new RadioButton()
                                {
                                    Tag = ext.Item1,
                                    Content = ext.Item2,
                                    IsChecked = ext.Item3
                                });
                            }
                        }

                        //Use checkbox if only 1 item in row
                        else
                        {
                            SPExtras.Children.Add(new CheckBox()
                            {
                                Tag = extras[j][0].Item1,
                                Content = extras[j][0].Item2,
                                IsChecked = extras[j][0].Item3
                            });
                        }
                    }
                }
                //No need to fill in column extra's, no extra's or is main-init row of dish with extra's
                else
                {
                    SPExtras = new StackPanel
                    {
                        Name = "ColExtra",
                        Orientation = Orientation.Vertical,
                        Tag = "false"
                    };
                }

                /*---------------------Fill in colums---------------------*/
                Grid.SetColumn(TBCategory, 0);
                Grid.SetColumn(TBDish, 1);
                Grid.SetColumn(TBPrice, 2);
                Grid.SetColumn(SPQuantity, 3);
                Grid.SetColumn(SPExtras, 4);

                ret.Children.Add(TBCategory);
                ret.Children.Add(TBDish);
                ret.Children.Add(TBPrice);
                SPQuantity.Children.Add(ButMinus);
                SPQuantity.Children.Add(TBQuantity);
                SPQuantity.Children.Add(ButPlus);
                ret.Children.Add(SPQuantity);
                ret.Children.Add(SPExtras);

            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return ret;
        }


        /// <summary>
        /// Add the header row of the menu
        /// </summary>
        /// <param name="SPMenu">Stackpanel containing menu</param>
        private static void AddHeader(StackPanel SPMenu)
        {
            Grid thisRow = new Grid() { Name = "grdHeader", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            try
            {
                /*----------------Init the grid in the correct format------------------*/

                thisRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
                thisRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });
                thisRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
                thisRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
                thisRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
                thisRow.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });

                /*--------------------------Define column content----------------------*/

                /*-----Column 1: Category-----*/
                TextBlock TBCategory = new TextBlock
                {
                    Name = "CategoryHeader",
                    Text = "Category",
                    FontWeight = FontWeights.Bold
                };
                /*-----Column 2: Dish-----*/
                TextBlock TBDish = new TextBlock
                {
                    Name = "DishHeader",
                    Text = "Dish",
                    FontWeight = FontWeights.Bold
                };
                /*-----Column 3: Price-----*/
                TextBlock TBPrice = new TextBlock
                {
                    Name = "PriceHeader",
                    Text = "Price",
                    FontWeight = FontWeights.Bold
                };
                /*-----Column 4: Quantity-----*/
                TextBlock TBQuantity = new TextBlock
                {
                    Name = "QuantityHeader",
                    Text = "Quantity",
                    FontWeight = FontWeights.Bold
                };
                /*-----Column 5: Extra's-----*/
                TextBlock TBExtras = new TextBlock
                {
                    Name = "ExtrasHeader",
                    Text = "Extra's",
                    FontWeight = FontWeights.Bold
                };

                /*---------------------Fill in colums---------------------*/
                Grid.SetColumn(TBCategory, 0);
                Grid.SetColumn(TBDish, 1);
                Grid.SetColumn(TBPrice, 2);
                Grid.SetColumn(TBQuantity, 3);
                Grid.SetColumn(TBExtras, 4);
                thisRow.Children.Add(TBCategory);
                thisRow.Children.Add(TBDish);
                thisRow.Children.Add(TBPrice);
                thisRow.Children.Add(TBQuantity);
                thisRow.Children.Add(TBExtras);

                /*---------------------Add to stackpanel---------------------*/
                SPMenu.Children.Add(thisRow);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }

        /// <summary>
        /// Initialize calendar
        /// </summary>
        /// <param name="calendar">What datepicker to initialize</param>
        private static void SetCalendar(DatePicker calendar)
        {
            try
            {
                TimeSpan start = new TimeSpan(10, 0, 0);
                TimeSpan now = DateTime.Now.TimeOfDay;
                if (start > now)
                {
                    calendar.SelectedDate = DateTime.Today;
                    calendar.DisplayDateStart = DateTime.Today;
                }
                else
                {
                    calendar.SelectedDate = DateTime.Today.AddDays(1);
                    calendar.DisplayDateStart = DateTime.Today;
                }
                calendar.DisplayDateEnd = DateTime.Today.AddDays(14);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }

        /// <summary>
        /// Save the current form in the DB
        /// </summary>
        /// <param name="user"></param>
        /// <param name="date"></param>
        /// <param name="comment"></param>
        /// <param name="stPnl"></param>
        public static void SaveOrders(string user, DateTime date, string comment, StackPanel stPnl)
        {
            try
            {
                int userId = DAL.GetUserId(user);
                int orderId = DAL.SetOrders(userId, date, comment);
                if (orderId == 0 || orderId == -1)
                {
                    return;
                }

                foreach (object child in stPnl.Children)
                {
                    string childname;
                    StackPanel currStPnl;
                    TextBlock currTxtBl;
                    int orderDish;
                    int orderDishDetail;
                    if (child is FrameworkElement)
                    {
                        childname = (child as Grid).Name;
                        if (childname != null && childname != "grdHeader")
                        {
                            Grid grd = (Grid)child;
                            currStPnl = (StackPanel)LogicalTreeHelper.FindLogicalNode(grd, "ColQuantity");
                            currTxtBl = (TextBlock)LogicalTreeHelper.FindLogicalNode(currStPnl, "ColQuantityTxt");
                            int thisQuan = Int32.Parse(currTxtBl.Text);
                            if (thisQuan > 0)
                            {
                                currTxtBl = (TextBlock)LogicalTreeHelper.FindLogicalNode(grd, "ColDish");
                                int dishId = Int32.Parse(currTxtBl.Tag.ToString());
                                orderDish = DAL.SetOrderDish(orderId, dishId, thisQuan);
                                if (orderDish != 0 && orderDish != -1)
                                {
                                    List<int> extras = GetExtras(grd);
                                    foreach (int i in extras)
                                    {
                                        orderDishDetail = DAL.SetOrderDishDetails(i, orderDish);
                                        if (orderDishDetail == 0 || orderDishDetail == -1)
                                        {
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            MessageBox.Show("Your order has been made succesfully.");
            // TODO reset form and comment

        }

        /// <summary>
        /// Returns a list with the IDs of the selected extra's in the last column of the grid.
        /// </summary>
        /// <param name="grd"></param>
        /// <returns></returns>
        private static List<int> GetExtras(Grid grd)
        {
            List<int> ret = new List<int>();
            try
            {
                StackPanel stPnl = (StackPanel)LogicalTreeHelper.FindLogicalNode(grd, "ColExtra");
                foreach (object child in stPnl.Children)
                {
                    if (child is RadioButton)
                    {
                        RadioButton rdBut = child as RadioButton;
                        if ((bool)rdBut.IsChecked)
                        {
                            ret.Add(Int32.Parse(rdBut.Tag.ToString()));
                        }
                    }
                    else if (child is CheckBox)
                    {
                        CheckBox rdBut = child as CheckBox;
                        if ((bool)rdBut.IsChecked)
                        {
                            ret.Add(Int32.Parse(rdBut.Tag.ToString()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return ret;
        }
        
        /// <summary>
        /// Set the menu table according to the last order of the selected user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="sPmenu"></param>
        public static void SetLastOrder(string user, StackPanel sPmenu)
        {
            try
            {
                sPmenu.Children.Clear();
                PopulateMenu(sPmenu);
                int userId = DAL.GetUserId(user);
                string SQLstr = "SELECT [orderID]" +
                     " FROM [Orders]" +
                     " WHERE [User] = " + userId +
                     " ORDER BY Date desc, orderid desc";
                string[] orderId = DAL.GetDataArray(SQLstr, "orderID");
                if (orderId.Length == 0)
                {
                    MessageBox.Show("No previous order found.");
                    return;
                }
                SQLstr = "SELECT [OrderDishID],[Quantity],[Dish]" +
                    " FROM [OrderDishes]" +
                    " WHERE [Order] = " + Int32.Parse(orderId[0]);
                string[,] orderDishes = DAL.GetMultiDataArray(SQLstr, "OrderDishID", "Quantity", "Dish");

                string currOrderId;
                int grdCnt = 0;
                foreach (Object obj in sPmenu.Children)
                {
                    grdCnt++;
                }

                //For each saved dish of the last order
                for (int j = 0; j < orderDishes.GetLength(0); j++)
                {
                    for (int i = 0; i < grdCnt; i++)
                    {
                        string childName;

                        //find corresponding grid
                        if (sPmenu.Children[i] is Grid grid)
                        {
                            childName = (sPmenu.Children[i] as Grid).Name;
                            if (childName != null && childName != "grdHeader")
                            {
                                Grid grd = (Grid)sPmenu.Children[i];
                                currOrderId = MenuView.GetDishID(grd).ToString();
                                if (orderDishes[j, 2] == currOrderId)
                                {
                                    bool hasExtras = DAL.HasExtra(MenuView.GetCategoryID(grd)); // MenuView.GetIfExtras(grd);
                                    if (hasExtras)
                                    {
                                        string[] sendData = new string[5];
                                        sendData[0] = ""; //j.Text;
                                        sendData[1] = MenuView.GetCategoryID(grd).ToString();
                                        sendData[2] = MenuView.GetDish(grd);
                                        sendData[3] = MenuView.GetDishID(grd).ToString();
                                        sendData[4] = MenuView.GetPrice(grd);
                                        int row = MenuView.GetGridRow(grd);
                                        row++;
                                        InsertRow(row, sPmenu, sendData, orderDishes[j, 0], Int32.Parse(orderDishes[j, 1]));
                                        grdCnt++;
                                        i++;
                                    }
                                    else
                                    {
                                        MenuView.SetQuantity(grd, Int32.Parse(orderDishes[j, 1]));
                                    }
                                }
                            }
                        }
                    }
                }
                MenuView.SetTotalPrice(sPmenu);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }


        /// <summary>
        /// Sets the extra's column of the displayed menu table according to an existing order
        /// </summary>
        /// <param name="grd">For what grid</param>
        /// <param name="orderDish"></param>
        /// <param name="category"></param>
        private static void SetExistingExtras(Grid grd, string orderDish, string category)
        {
            try
            {
                StackPanel SPExtras = (StackPanel)LogicalTreeHelper.FindLogicalNode(grd, "ColExtra");
                SPExtras.Tag = true;

                //extras is jagged array, each row contains options of which only 1 can be selected
                //each value contains Extra ID, Extra name, Default value
                (int, string, bool)[][] extras = DAL.GetExtras(Int32.Parse(category));
                //Correct set values of extras
                string[,] orderDishExtras = DAL.GetMultiDataArray("SELECT [OrderDishDetailID],[DishExtra] FROM [OrderDishDetails] WHERE [OrderDish] = " + orderDish, "OrderDishDetailID", "DishExtra");
                int len = extras.GetLength(0);
                int ctr = 0;
                for (int j = 0; j < len; j++)
                {
                    for (int k = 0; k < extras[j].Length; k++)
                    {
                        if (Int32.Parse(orderDishExtras[ctr, 1]) == extras[j][k].Item1)
                        {
                            extras[j][k].Item3 = true;
                            ctr++;
                        }
                        else
                        {
                            extras[j][k].Item3 = false;
                        }
                    }
                }
                for (int j = 0; j < len; j++)
                {
                    int lenRow = extras[j].Length;

                    //Use radiobutton if more than 1 item in row
                    if (lenRow > 1)
                    {
                        foreach ((int, string, bool) ext in extras[j])
                        {
                            SPExtras.Children.Add(new RadioButton()
                            {
                                Tag = ext.Item1,
                                Content = ext.Item2,
                                IsChecked = ext.Item3
                            });
                        }
                    }

                    //Use checkbox if only 1 item in row
                    else
                    {
                        SPExtras.Children.Add(new CheckBox()
                        {
                            Tag = extras[j][0].Item1,
                            Content = extras[j][0].Item2,
                            IsChecked = extras[j][0].Item3
                        });
                    }
                }
                Grid.SetColumn(SPExtras, 4);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }
    }
}