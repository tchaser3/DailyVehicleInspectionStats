/* Title:           Daily Vehicle Inspection Stats
 * Date:            3-28-18
 * Author:          Terry Holmes
 * 
 * Description:     This program will compute mean and standard deviation for each vehicle */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NewEventLogDLL;
using NewVehicleDLL;
using DataValidationDLL;
using DateSearchDLL;
using InspectionsDLL;

namespace DailyVehicleInspectionStats
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //setting up the classes
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        VehicleClass TheVehicleClass = new VehicleClass();
        DataValidationClass TheDataValidationClass = new DataValidationClass();
        DateSearchClass TheDataSearchClass = new DateSearchClass();
        InspectionsClass TheInspectionClass = new InspectionsClass();
        
        //setting up the data
        FindActiveVehiclesDataSet TheFindActiveVehiclesDataSet = new FindActiveVehiclesDataSet();
        FindDailyVehicleInspectionByDateRangeDataSet TheFindDailyVehicleInspectionByDateRangeDataSet = new FindDailyVehicleInspectionByDateRangeDataSet();
        FindDailyVehicleInspectionByVehicleIDAndDateRangeDataSet TheFindDailyVehicleInspectionByVehicleIDandDateRangeDataSet = new FindDailyVehicleInspectionByVehicleIDAndDateRangeDataSet();
        VehicleLocalStatsDataSet TheVehicleLocalStatsDataSet = new VehicleLocalStatsDataSet();

        //setting global variables
        DateTime gdatStartDate;
        DateTime gdatEndDate;
        int gintVehicleID;
        decimal gdecTotalMean;
        decimal gdecVariance;
        decimal gdecStandardDeviation;
        decimal gdecLimiter;
        decimal gdecTotalMilage;
        int gintTotalItems;
        decimal gdecMileage;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int intVehicleCounter;
            int intVehicleNumberOfRecords;
            int intInspectionCounter;
            int intInspectionNumberOfRecords;
            int intLastOdometerReading;
            int intCurrentOdometerReading;
            decimal decMileage;
            int intCounter;
            int intBJCNumber;
            decimal decMean = 0;
            decimal decVariance;
            decimal decStandardDeviation;
            decimal decLimiter;
            int intStatsCounter;
            int intStatsUpperLimit;
            int intStatsSelectedIndex = 0;
            bool blnItemFound;
            
            try
            {
                TheFindActiveVehiclesDataSet = TheVehicleClass.FindActiveVehicles();

                gdatEndDate = DateTime.Now;

                gdatEndDate = TheDataSearchClass.RemoveTime(gdatEndDate);

                gdatStartDate = TheDataSearchClass.SubtractingDays(gdatEndDate, 365);

                TheFindDailyVehicleInspectionByDateRangeDataSet = TheInspectionClass.FindDailyVehicleInspectionByDateRange(gdatStartDate, gdatEndDate);

                intVehicleNumberOfRecords = TheFindActiveVehiclesDataSet.FindActiveVehicles.Rows.Count - 1;

                gdecTotalMilage = 0;

                gintTotalItems = 0;

                for (intVehicleCounter = 0; intVehicleCounter <= intVehicleNumberOfRecords; intVehicleCounter++)
                {
                    gintVehicleID = TheFindActiveVehiclesDataSet.FindActiveVehicles[intVehicleCounter].VehicleID;
                    intBJCNumber = TheFindActiveVehiclesDataSet.FindActiveVehicles[intVehicleCounter].BJCNumber;

                    TheFindDailyVehicleInspectionByVehicleIDandDateRangeDataSet = TheInspectionClass.FindDailyVehicleInspectionByVehicleIDAndDateRange(gintVehicleID, gdatStartDate, gdatEndDate);

                    intCounter = 0;
                    decMileage = 0;
                    intLastOdometerReading = 0;

                    intInspectionNumberOfRecords = TheFindDailyVehicleInspectionByVehicleIDandDateRangeDataSet.FindDailyVehicleInspectionsByVehicleIDAndDateRange.Rows.Count - 1;

                    if(intInspectionNumberOfRecords > -1)
                    {
                        for (intInspectionCounter = 0; intInspectionCounter <= intInspectionNumberOfRecords; intInspectionCounter++)
                        {
                            intCurrentOdometerReading = TheFindDailyVehicleInspectionByVehicleIDandDateRangeDataSet.FindDailyVehicleInspectionsByVehicleIDAndDateRange[intInspectionCounter].OdometerReading;

                            if (intLastOdometerReading == 0)
                            {
                                intLastOdometerReading = intCurrentOdometerReading;
                            }
                            else if (intCurrentOdometerReading < intLastOdometerReading + 3000)
                            {
                                if (intCurrentOdometerReading > intLastOdometerReading)
                                {
                                    decMileage += Convert.ToDecimal(intCurrentOdometerReading - intLastOdometerReading);

                                    intLastOdometerReading = intCurrentOdometerReading;

                                    intCounter++;
                                    gintTotalItems++;
                                }
                            }
                        }

                        if(intCounter > 0)
                        {
                            decMean = decMileage / intCounter;

                            decMean = Math.Round(decMean, 4);
                        }
                        else
                        {
                            decMean = 0;
                        }
                        

                        VehicleLocalStatsDataSet.vehiclestatsRow NewVehicleRow = TheVehicleLocalStatsDataSet.vehiclestats.NewvehiclestatsRow();

                        NewVehicleRow.BJCNumber = intBJCNumber;
                        NewVehicleRow.TotalMiles = Convert.ToInt32(decMileage);
                        NewVehicleRow.NoOfItems = intCounter;
                        NewVehicleRow.VehicleID = gintVehicleID;
                        NewVehicleRow.VehicleLimiter = 0;
                        NewVehicleRow.VehicleMean = decMean;
                        NewVehicleRow.VehicleSD = 0;
                        NewVehicleRow.VehicleVariance = 0;
                        NewVehicleRow.OdometerReading = intLastOdometerReading;

                        TheVehicleLocalStatsDataSet.vehiclestats.Rows.Add(NewVehicleRow);

                        gdecTotalMilage += decMileage;
                    }
                    
                }

                gdecTotalMean = gdecTotalMilage / gintTotalItems;

                gdecVariance = 0;

                intStatsUpperLimit = TheVehicleLocalStatsDataSet.vehiclestats.Rows.Count - 1;

                for (intVehicleCounter = 0; intVehicleCounter <= intVehicleNumberOfRecords; intVehicleCounter++)
                {
                    gintVehicleID = TheFindActiveVehiclesDataSet.FindActiveVehicles[intVehicleCounter].VehicleID;

                    decVariance = 0;

                    TheFindDailyVehicleInspectionByVehicleIDandDateRangeDataSet = TheInspectionClass.FindDailyVehicleInspectionByVehicleIDAndDateRange(gintVehicleID, gdatStartDate, gdatEndDate);

                    intCounter = 0;
                    decMileage = 0;
                    intLastOdometerReading = 0;

                    intInspectionNumberOfRecords = TheFindDailyVehicleInspectionByVehicleIDandDateRangeDataSet.FindDailyVehicleInspectionsByVehicleIDAndDateRange.Rows.Count - 1;

                    for(intInspectionCounter = 0; intInspectionCounter <= intInspectionNumberOfRecords; intInspectionCounter++)
                    {
                        intCurrentOdometerReading = TheFindDailyVehicleInspectionByVehicleIDandDateRangeDataSet.FindDailyVehicleInspectionsByVehicleIDAndDateRange[intInspectionCounter].OdometerReading;

                        if(intLastOdometerReading == 0)
                        {
                            intLastOdometerReading = intCurrentOdometerReading;
                        }
                        else if (intCurrentOdometerReading < intLastOdometerReading + 3000)
                        {
                            if(intCurrentOdometerReading > intLastOdometerReading)
                            {
                                gdecMileage = Convert.ToDecimal(intCurrentOdometerReading - intLastOdometerReading);
                                decMileage = Convert.ToDecimal(intCurrentOdometerReading - intLastOdometerReading);

                                for(intStatsCounter = 0; intStatsCounter <= intStatsUpperLimit; intStatsCounter++)
                                {
                                    if(gintVehicleID == TheVehicleLocalStatsDataSet.vehiclestats[intStatsCounter].VehicleID)
                                    {
                                        intStatsSelectedIndex = intStatsCounter;
                                        decMileage = decMileage - TheVehicleLocalStatsDataSet.vehiclestats[intStatsCounter].VehicleMean;

                                        decVariance += decMileage * decMileage;
                                    }
                                }

                                intLastOdometerReading = intCurrentOdometerReading;

                                gdecMileage = decMileage - gdecTotalMean;

                                gdecVariance += decMileage * decMileage;
                            }
                        }
                    }

                    gdecTotalMilage += decMileage;

                    if (TheVehicleLocalStatsDataSet.vehiclestats[intStatsSelectedIndex].NoOfItems > 0)
                    {
                        decVariance = decVariance / TheVehicleLocalStatsDataSet.vehiclestats[intStatsSelectedIndex].NoOfItems;

                        decVariance = Math.Round(decVariance, 4);
                        
                        decStandardDeviation = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(decVariance)));

                        decStandardDeviation = Math.Round(decStandardDeviation, 4);

                        TheVehicleLocalStatsDataSet.vehiclestats[intStatsSelectedIndex].VehicleSD = decStandardDeviation;
                        TheVehicleLocalStatsDataSet.vehiclestats[intStatsSelectedIndex].VehicleVariance = decVariance;
                        TheVehicleLocalStatsDataSet.vehiclestats[intStatsSelectedIndex].VehicleLimiter = TheVehicleLocalStatsDataSet.vehiclestats[intStatsSelectedIndex].VehicleMean + (5 * decStandardDeviation);
                    }
                }

                

                gdecVariance = gdecVariance / gintTotalItems;

                gdecStandardDeviation = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(gdecVariance)));

                gdecStandardDeviation = Math.Round(gdecStandardDeviation, 4);

                gdecTotalMean = Convert.ToDecimal(Convert.ToDouble(Math.Round(gdecTotalMean, 4)));

                txtVehiclesSD.Text = Convert.ToString(gdecStandardDeviation);

                txtVehiclesMean.Text = Convert.ToString(gdecTotalMean);

                

                dgrResults.ItemsSource = TheVehicleLocalStatsDataSet.vehiclestats;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Daily Vehicle Inspection Stats // Window Loaded " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
        }
    }
}
