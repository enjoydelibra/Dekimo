using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace FeedMe.Model
{
    /// <summary>
    /// Data Access Layer
    /// According to wikipedia a layer of a computer program which provides simplified access to 
    /// data stored in persistent storage of some kind, such as an entity-relational database.
    /// </summary>
    public class DAL
    {
        private const string ConnectStr = @"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = FeedMeDB; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";

        /// <summary>
        /// Execute a query on the local database
        /// </summary>
        /// <param name="query">An SQL query to be executed</param>
        /// <returns>Returns the number of rows affected </returns>
        public static int ExecuteQuery(string query)
        {
            int rowCount;
            SqlConnection sqlConnection = new SqlConnection(ConnectStr);
            SqlCommand sqlCommand = new SqlCommand();

            try
            {
                sqlCommand.CommandText = query;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.Connection = sqlConnection;
                sqlConnection.Open();
                rowCount = sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                sqlConnection.Close();
                throw ex;
            }
            return rowCount;
        }

        /// <summary>
        /// Returns the number of results from a SQL query
        /// </summary>
        /// <param name="query">Query to check</param>
        /// <param name="col">Database column to be checked</param>
        /// <returns>Number of records</returns>
        public static int GetArrayLength(string query, string col)
        {
            string[] ret;
            ret = GetDataArray(query, col);
            return ret.Length;
        }

        /// <summary>
        /// Gets an array based on the SQL query
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <param name="col">What column from the database is returned</param>
        /// <returns></returns>
        public static string[] GetDataArray(string query, string col)
        {
            List<string> ret = new List<string>();
            SqlConnection sqlConnection = new SqlConnection(ConnectStr);
            SqlCommand sqlCommand = new SqlCommand();
            try
            {
                sqlCommand.CommandText = query;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandTimeout = 60;
                sqlConnection.Open();
                SqlDataReader sqlReader = sqlCommand.ExecuteReader();
                while (sqlReader.Read())
                {
                    ret.Add(sqlReader[col].ToString());
                }
                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                sqlConnection.Close();
                throw ex;
            }
            return ret.ToArray();
        }

        /// <summary>Class <c>CreateMenuTable</c> returns an table in following format: 
        /// Name category, CategoryID, Name dish, DishID, Price</summary>
        public static string[,] CreateMenuTable()
        {
            List<string> retList = new List<string>();
            bool first;
            int arrLen;
            string SQLstr = "SELECT [CategoryID], [CategoryName]" +
                " FROM [DishCategories]" +
                " ORDER BY [CategoryID] ASC";
            String[,] categories = GetMultiDataArray(SQLstr, "CategoryID", "CategoryName");
            String[,] dishNPrice;
            String[,] ret;
            int rowCtr = 0;

            try
            {
                // Fill list with all data returned
                for (int i = 0; i < categories.GetLength(0); ++i)
                {
                    first = true;
                    SQLstr = "SELECT [DishName], [DishID], [Price]" +
                        " FROM [Dishes]" +
                        " WHERE [Category] = " + categories[i, 0] +
                        " ORDER BY [DishID] ASC";
                    dishNPrice = GetMultiDataArray(SQLstr, "DishName", "DishID", "Price");
                    arrLen = dishNPrice.GetLength(0);
                    for (int j = 0; j < arrLen; ++j)
                    {
                        if (first)
                        {
                            retList.Add(categories[i, 1]);
                            retList.Add(categories[i, 0]);
                            retList.Add(dishNPrice[j, 0]);
                            retList.Add(dishNPrice[j, 1]);
                            retList.Add(dishNPrice[j, 2]);
                            first = false;
                        }
                        else
                        {
                            retList.Add("");
                            retList.Add(categories[i, 0]);
                            retList.Add(dishNPrice[j, 0]);
                            retList.Add(dishNPrice[j, 1]);
                            retList.Add(dishNPrice[j, 2]);
                        }
                    }
                }
                arrLen = retList.Count;
                                              
                //convert list to multidim array
                ret = new string[arrLen / 5, 5];
                for (int k = 0; k < arrLen; k += 5)
                {
                    ret[rowCtr, 0] = retList[k];
                    ret[rowCtr, 1] = retList[k + 1];
                    ret[rowCtr, 2] = retList[k + 2];
                    ret[rowCtr, 3] = retList[k + 3];
                    ret[rowCtr, 4] = retList[k + 4];
                    rowCtr++;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }

        /// <summary>
        /// Gets a multidimensional array from the database, based on the query provided
        /// </summary>
        /// <param name="query">In the database we look for this query</param>
        /// <param name="col1">1st column from database to be returned</param>
        /// <param name="col2">2nd column from database to be returned</param>
        /// <param name="col3">3rd column from database to be returned</param>
        /// <param name="col4">4th column from database to be returned</param>
        /// <returns></returns>
        public static string[,] GetMultiDataArray(string query, string col1, string col2, string col3 = "", string col4 = "")
        {
            List<string> retList = new List<string>();
            SqlConnection sqlConnection = new SqlConnection(ConnectStr);
            SqlCommand sqlCommand = new SqlCommand();
            bool thirdCol;
            bool fourthCol;
            // lenOpt for tracking how many columns the multidim array contains
            int lenOpt = 2;
            int len;
            String[,] ret;
            int rowCtr = 0;

            if (col3 == "")
            {
                thirdCol = false;
                fourthCol = true;
            }
            else
            {
                thirdCol = true;
                if (col4 == "")
                {
                    fourthCol = false;
                }
                else
                {
                    fourthCol = true;
                }
            }

            // Fill list with all data returned
            try
            {
                sqlCommand.CommandText = query;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandTimeout = 60; 
                sqlConnection.Open();

                SqlDataReader sqlReader = sqlCommand.ExecuteReader();

                
                while (sqlReader.Read())
                {
                    retList.Add(sqlReader[col1].ToString());
                    retList.Add(sqlReader[col2].ToString());
                    if (thirdCol)
                    {
                        retList.Add(sqlReader[col3].ToString());
                        if (fourthCol)
                        {
                            retList.Add(sqlReader[col4].ToString());
                        }
                    }
                }
                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                sqlConnection.Close();
                throw ex;
            }

            //Convert the list to a multidimensional array
            try
            {
                if (thirdCol)
                {
                    if (fourthCol)
                    {
                        lenOpt = 4;
                    }
                    else
                    {
                        lenOpt = 3;
                    }
                }
                len = retList.Count;
                ret = new string[len / lenOpt, lenOpt];
                for (int k = 0; k < len; k += lenOpt)
                {
                    ret[rowCtr, 0] = retList[k];
                    ret[rowCtr, 1] = retList[k + 1];
                    if (thirdCol)
                    {
                        ret[rowCtr, 2] = retList[k + 2];
                        if (fourthCol)
                        {
                            ret[rowCtr, 3] = retList[k + 3];
                        }
                    }
                    rowCtr++;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }

        /// <summary>
        /// Checks if the category has any extra options in the database
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static bool HasExtra(int category)
        {
            string SQLstr = "SELECT DishExtraID" +
                " FROM DishExtras" +
                " WHERE Category = " + category;
            SqlConnection Con = new SqlConnection(ConnectStr);
            SqlCommand Com = new SqlCommand(SQLstr, Con);
            SqlDataAdapter sda = new SqlDataAdapter(Com);
            //IMPROVE use datatable-structure like below to improve GetArray methods?
            DataTable dt = new DataTable();
            sda.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get extra's for category and their default value
        /// </summary>
        /// <param name="category">Get extra's for what category</param>
        /// <returns>Jagged array, each row contains options of which only 1 can be selected,
        /// based on the group column of the DB
        /// each value contains Extra ID, Extra name, Default value</returns>
        public static (int, string, bool)[][] GetExtras(int category)
        {
            (int, string, bool)[][] ret;
            int len;
            string[,] extrasDB;
            //The groups column in DB will decide how many elements are in this row of the jagged array
            int prevGrp;
            int jArrCtr;
            //tempList used to store all elements if jagged array has more than 1 element in row
            List<(int, string, bool)> tempLst;
            int currGrp;

            try
            {
                string SQLstr = "SELECT DISTINCT [Group]" +
                    " FROM [DishExtras]" +
                    " WHERE [Category] = " + category + " AND [Group] != 0";
                len = DAL.GetArrayLength(SQLstr, "Group");
                SQLstr = "SELECT [DishExtraID]" +
                    " FROM [DishExtras]" +
                    " WHERE [Category] = " + category + " AND [Group] = 0";
                len += DAL.GetArrayLength(SQLstr, "DishExtraID");

                ret = new (int, string, bool)[len][];

                SQLstr = "SELECT *" +
                    " FROM [DishExtras]" +
                    " WHERE [Category] = " + category +
                    " ORDER BY [Group] DESC";
                extrasDB = DAL.GetMultiDataArray(SQLstr, "Group", "DishExtraID", "DishExtraName", "Default");
                len = extrasDB.GetLength(0);
                prevGrp = Int32.Parse(extrasDB[0, 0]);
                jArrCtr = 0;
                tempLst = new List<(int, string, bool)>();

                for (int i = 0; i < len; i++)
                {
                    currGrp = Int32.Parse(extrasDB[i, 0]);
                    if (currGrp != prevGrp)
                    {
                        prevGrp = Int32.Parse(extrasDB[i, 0]);
                        ret[jArrCtr] = tempLst.ToArray();
                        tempLst.Clear();
                        jArrCtr++;
                    }
                    //If group = 0 it means this extra is not part of any group of options
                    //Add single column to jagged array
                    if (currGrp == 0)
                    {
                        prevGrp = Int32.Parse(extrasDB[i, 0]);
                        tempLst.Add((Int32.Parse(extrasDB[i, 1]), extrasDB[i, 2], bool.Parse(extrasDB[i, 3])));
                        ret[jArrCtr] = tempLst.ToArray();
                        tempLst.Clear();
                        jArrCtr++;
                    }
                    else
                    {
                        tempLst.Add((Int32.Parse(extrasDB[i, 1]), extrasDB[i, 2], bool.Parse(extrasDB[i, 3])));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }

        /// <summary>
        /// Adds new order in DB
        /// </summary>
        /// <param name="user"></param>
        /// <param name="date"></param>
        /// <param name="comment"></param>
        /// <returns>The ID of the newly created order</returns>
        public static int SetOrders(int user, DateTime date, string comment)
        {
            string[] ret;
            try
            {
                string dateStr = date.ToString("yyyy-MM-dd");
                string SQLstr = "SELECT [orderID]" +
                    " FROM [Orders]" +
                    " WHERE [User] = '" + user + "' AND [Date] = '" + dateStr + "'" +
                    " ORDER BY orderID desc";
                ret = DAL.GetDataArray(SQLstr, "orderID");

                if (ret.Length == 0)
                {
                    SQLstr = "INSERT INTO Orders ([User],[Date],[Comment]) VALUES (" +
                        user + ",'" + dateStr + "','" + comment + "')";
                    DAL.ExecuteQuery(SQLstr);
                    SQLstr = "SELECT [orderID]" +
                        " FROM [Orders]" +
                        " WHERE [User] = " + user + " AND [Date] = '" + dateStr + "'" +
                        " ORDER BY orderID desc";
                    ret = DAL.GetDataArray(SQLstr, "orderID");
                }
                return Int32.Parse(ret[0]);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return 0;
        }

        /// <summary>
        /// Adds a new OrderDish in the DB
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="dishId"></param>
        /// <param name="quantity"></param>
        /// <returns>newly made OrderDish ID</returns>
        public static int SetOrderDish(int orderId, int dishId, int quantity)
        {
            try
            {
                string SQLstr = "SELECT [orderDishID]" +
                    " FROM [OrderDishes]" +
                    " WHERE [Order] = " + orderId + " AND [Dish] = " + dishId +
                    " ORDER BY orderDishID DESC";
                string[] ret = DAL.GetDataArray(SQLstr, "orderDishID");

                if (ret.Length == 0)
                {
                    SQLstr = "INSERT INTO [OrderDishes] ([Order],[Dish],[Quantity]) VALUES (" +
                    orderId + "," + dishId + "," + quantity + ")";
                    DAL.ExecuteQuery(SQLstr);
                    SQLstr = "SELECT [orderDishID]: " +
                        " FROM [OrderDishes]" +
                        " WHERE [Order] = " + orderId + " AND [Dish] = " + dishId +
                        " ORDER BY orderDishID DESC";
                    ret = DAL.GetDataArray(SQLstr, "orderDishID");
                }
                return Int32.Parse(ret[0]);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return 0;
        }

        /// <summary>
        /// Adds OrderDishDetails to DB
        /// </summary>
        /// <param name="DishExtra"></param>
        /// <param name="OrderDish"></param>
        /// <returns>Returns the ID of the newly created orderDishDetail</returns>
        public static int SetOrderDishDetails(int DishExtra, int OrderDish)
        {
            string[] ret;
            try
            {
                string SQLstr = "SELECT [OrderDishDetailID]" +
                    " FROM [OrderDishDetails]" +
                    " WHERE [DishExtra] = " + DishExtra + " AND [OrderDish] = " + OrderDish +
                    " ORDER BY OrderDishDetailID DESC";
                ret = DAL.GetDataArray(SQLstr, "OrderDishDetailID");
                if (ret.Length == 0)
                {
                    SQLstr = "INSERT INTO OrderDishDetails (DishExtra,OrderDish)" +
                        " VALUES (" + DishExtra + "," + OrderDish + ")";
                    DAL.ExecuteQuery(SQLstr);
                    ret = DAL.GetDataArray("SELECT [OrderDishDetailID] FROM [OrderDishDetails] WHERE [DishExtra] = " + DishExtra + " AND [OrderDish] = " + OrderDish + " ORDER BY OrderDishDetailID DESC", "OrderDishDetailID");
                }
                return Int32.Parse(ret[0]);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return 0;
        }

        /// <summary>
        /// Returns ID of user from DB
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public static int GetUserId(string fullName)
        {
            string SQLstr = "SELECT UserID FROM Users" +
                " WHERE FullName ='" + fullName + "'";
            string[] userIDstr = DAL.GetDataArray(SQLstr, "UserID");
            int ret = Int32.Parse(userIDstr[0]);
            return ret;
        }

        /// <summary>
        /// Print all orders of today to a txt file on the D-drive
        /// </summary>
        /// TODO: Could use some brushing up, output is not as nice as it could be
        /// Improve outlining/use table format, insert logo, ... 
        public static void GetOrders(DateTime day)
        {
            //string path = Directory.GetCurrentDirectory();
            string path = @"D:\FeedMe";
            string fileName = path + @"\" + day.ToString("yyMMdd") + "Order.txt";
            string[] orders;
            string SQLstr;
            string[,] dishes;
            string line;
            string[] details;
            string[] temp;

            try
            {

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    //Header
                    sw.WriteLine("Dekimo, Industrielaan 6/3, 8520 Kuurne");
                    sw.WriteLine("Orders for : " + day.ToString("dd/MM/yyyy") + Environment.NewLine);
                    
                    SQLstr = "SELECT [OrderID]" +
                        " FROM [Orders]" +
                        " WHERE [Date] = '" + day.ToString("yyyyMMdd") + "'";
                    orders = GetDataArray(SQLstr, "OrderID");

                    //For each order
                    for (int i = 0; i < orders.Length; i++)
                    {
                        SQLstr = "SELECT OD.[OrderDishID], D.[DishName], OD.[Quantity], DC.[CategoryName]" +
                        " FROM [OrderDishes] OD" +
                        " JOIN [Dishes] D ON OD.[Dish] = D.[DishID]" +
                        " JOIN [DishCategories] DC ON DC.[CategoryID] = D.[Category]" +
                        " WHERE OD.[Order] = " + orders[i];
                        dishes = GetMultiDataArray(SQLstr, "OrderDishID", "DishName", "Quantity", "CategoryName"); //"SELECT [OrderDishes].[OrderDishID], [Dishes].[Name], [OrderDishes].[Quantity] FROM [OrderDishes] LEFT JOIN [Dishes] ON [OrderDishes].[Dish] = [Dishes].[DishID] WHERE [OrderDishes].[Order] = " + orders[i], "OrderDishID", "Name", "Quantity");

                        //For each dish print the line
                        for (int j = 0; j < dishes.GetLength(0); j++)
                        {
                            line = dishes[j, 3] + "\t";
                            line += dishes[j, 2] + "\t";
                            line += dishes[j, 1] + "\t";
                            SQLstr = "SELECT [DishExtraName] FROM [OrderDishDetails] " +
                                " LEFT JOIN [DishExtras] ON [OrderDishDetails].[DishExtra] = [DishExtras].[DishExtraID] " +
                                " WHERE [OrderDishDetails].[OrderDish] = " + dishes[j, 0];
                            details = GetDataArray(SQLstr, "DishExtraName");

                            for (int k = 0; k < details.Length; k++)
                            {
                                line += details[k].ToString() + ", ";
                            }
                            sw.WriteLine(line);
                        }

                        //Add comment of order
                        SQLstr = "SELECT Orders.Comment" +
                            " FROM Orders" +
                            " WHERE Orderid = " + orders[i];
                        temp = GetDataArray(SQLstr, "Comment");
                        if (temp[0].Length != 0)
                        {
                            line = "Comment: " + temp[0];
                            sw.WriteLine(line);
                            sw.WriteLine();
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Print all orders of a user to a txt file on the D-drive
        /// </summary>
        /// <param name="ID">UserID</param>
        /// <param name="month">For what month</param>
        /// <param name="year">For what year</param>
        /// TODO: Could use some brushing up, output is not as nice as it could be
        /// Improve outlining/use table format, insert logo, ... 
        public static void GetMonthlyOverview(string ID, int month, int year)
        {
            string SQLstr;
            SQLstr = "SELECT Initials FROM Users" +
                " WHERE UserID = " + ID;
            string[] userInit = GetDataArray(SQLstr, "Initials");
            SQLstr = "SELECT FullName FROM Users" +
                " WHERE UserID = " + ID;
            string[] userFullName = GetDataArray(SQLstr, "FullName");
            double totalPrice = 0;

            DateTime first = new DateTime(year, month, 1);
            DateTime last = first.AddMonths(1);
            last = last.AddDays(-1);
            string[,] orders;
            string path = @"D:\FeedMe";
            string fileName;
            string[,] dishes;
            string line;

            try
            {
                SQLstr = "SELECT [OrderID], [Date]" +
                        " FROM [Orders]" +
                        " WHERE [User] = " + ID +
                        " AND [Date] >= '" + first.ToString("yyyy/MM/dd") + "'" +
                        " AND [Date] <= '" + last.ToString("yyyy/MM/dd") + "'" +
                        " ORDER BY [Date] ASC";
                orders = GetMultiDataArray(SQLstr, "OrderID", "Date");

                if (orders.Length == 0)
                { return; }
                
                fileName = path + @"\" + first.ToString("yyMM") + "Order" + userInit[0] + ".txt";

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    //Header
                    sw.WriteLine("Dekimo, Industrielaan 6/3, 8520 Kuurne");
                    sw.WriteLine();
                    sw.WriteLine("Orders for : " + first.ToString("yyyy") + " " + first.ToString("MMMM") + " " +
                        userFullName[0] + Environment.NewLine);

                    //For each order
                    for (int i = 0; i < orders.GetLength(0); i++)
                    {
                        SQLstr = "SELECT D.[Price], D.[DishName], OD.[Quantity], DC.[CategoryName]" +
                        " FROM [OrderDishes] OD" +
                        " JOIN [Dishes] D ON OD.[Dish] = D.[DishID]" +
                        " JOIN [DishCategories] DC ON DC.[CategoryID] = D.[Category]" +
                        " WHERE OD.[Order] = " + orders[i, 0];
                        dishes = GetMultiDataArray(SQLstr, "Price", "DishName", "Quantity", "CategoryName"); //"SELECT [OrderDishes].[OrderDishID], [Dishes].[Name], [OrderDishes].[Quantity] FROM [OrderDishes] LEFT JOIN [Dishes] ON [OrderDishes].[Dish] = [Dishes].[DishID] WHERE [OrderDishes].[Order] = " + orders[i], "OrderDishID", "Name", "Quantity");
                        sw.WriteLine(Convert.ToDateTime(orders[i, 1]).ToString("dd/MM/yyyy"));

                        for (int j = 0; j < dishes.GetLength(0); j++)
                        {
                            line = dishes[j, 3] + "\t";
                            line += dishes[j, 1] + "\t";
                            line += dishes[j, 2] + "\t";
                            line += dishes[j, 0] + "\t";

                            sw.WriteLine(line);
                            totalPrice += Double.Parse(dishes[j, 0]) * Int32.Parse(dishes[j, 2]);
                        }
                        sw.WriteLine();
                    }

                    //Footer
                    sw.WriteLine();
                    sw.WriteLine("Total Price: € " + String.Format("{0:0.00}", totalPrice));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}