using ABI.Robotics.Model;
using AForge.Imaging;
using AForge.Imaging.Filters;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Accord.Imaging;
using æ.Imaging;
using æ.Imaging.Hermary;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


namespace EnglishMuffinVision_AForge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {

        Bitmap GrayScaleImage;
        bool imageLoaded = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLoadimage_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Image files (*.png;*.jpeg;*.bmp)|*.png;*.jpeg;*.bmp|All files (*.*)|*.*",
                //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //InitialDirectory = "C:\\Users\\BrianWang\\Desktop\\English Muffin Scan\\Dec 1 2017"
                InitialDirectory = "C:\\Users\\kai23\\Projects\\ABI\\EnglishMuffinVision_AForge\\Images\\English Muffin\\Batch 1\\All Top"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    lblfilename.Content = filename;

                    int startPos = filename.LastIndexOf("SK Foods On-Site Scan") + "SK Foods On-Site Scan".Length + 1;
                    int length = filename.IndexOf("æ") - startPos - 1;
                    string sub = filename.Substring(startPos, length);

                    lblFolder.Content = sub;

                    GrayScaleImage = AForge.Imaging.Image.FromFile(filename);
                    imageLoaded = true;
                }
            }
            if (imageLoaded)
            {
                AForge.Imaging.UnmanagedImage unmanagedImage1 = AForge.Imaging.UnmanagedImage.FromManagedImage(GrayScaleImage);
                Bitmap managedImage = unmanagedImage1.ToManagedImage();
                BitmapImage GrayImage_temp = ToBitmapImage(managedImage);
                imgGray.Source = GrayImage_temp;
            }

        }

        private static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }

        }

        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            if (imageLoaded)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                AForge.Imaging.UnmanagedImage unmanagedImage1 = AForge.Imaging.UnmanagedImage.FromManagedImage(GrayScaleImage);
                AForge.Imaging.BlobCounter bc = new AForge.Imaging.BlobCounter
                {
                    CoupledSizeFiltering = true,
                    FilterBlobs = true,
                    MinHeight = 30,
                    MinWidth = 30,
                    MaxHeight = 100,
                    MaxWidth = 100
                };

                bc.ProcessImage(GrayScaleImage);

                lblBlobCount.Content = bc.ObjectsCount;

                Bitmap indexMap = AForge.Imaging.Image.Clone(GrayScaleImage);

                for (int x = 0; x < indexMap.Width; x++)
                {
                    for (int y = 0; y < indexMap.Height; y++)
                    {
                        indexMap.SetPixel(x, y, System.Drawing.Color.Black);
                    }
                }

                System.Drawing.Rectangle[] rects = bc.GetObjectsRectangles();
                // process blobs
                BreadBlob[] breadBlob1 = new BreadBlob[bc.ObjectsCount];
                int blobArrayIndex = 0;
                int blobPt = Convert.ToInt16(txbBlobNum.Text);
                int blobThreshold = Convert.ToInt16(txbBlobThreshold.Text);
                if (blobPt >= bc.ObjectsCount)
                {
                    blobPt = bc.ObjectsCount - 1;
                }
                StaticsCalculator MuffinStatistics = new StaticsCalculator();

                Graphics g = Graphics.FromImage(indexMap);
                foreach (System.Drawing.Rectangle rect in rects)
                {
                    //initialize Object
                    breadBlob1[blobArrayIndex] = new BreadBlob();
                    breadBlob1[blobArrayIndex].TopDownThreshold = blobThreshold;
                    byte[,] blobArray = new byte[rect.Width, rect.Height];

                    for (int x = rect.Left; x < rect.Right; x++)
                    {
                        for (int y = rect.Top; y < rect.Bottom; y++)
                        {
                            System.Drawing.Color tempPixelColor = GrayScaleImage.GetPixel(x, y);
                            blobArray[x - rect.Left, y - rect.Top] = tempPixelColor.G;
                        }
                    }

                    breadBlob1[blobArrayIndex].PixelArray = blobArray;
                    breadBlob1[blobArrayIndex].X = rect.X;
                    breadBlob1[blobArrayIndex].Y = rect.Y;
                    MuffinStatistics.Add(breadBlob1[blobArrayIndex].Variance.QAverage);

                    if (blobArrayIndex == blobPt)
                    {
                        System.Drawing.Rectangle tempRect = rect;
                        tempRect.X -= 1;
                        tempRect.Y -= 1;
                        tempRect.Width += 2;
                        tempRect.Height += 2;

                        AForge.Imaging.Drawing.Rectangle(unmanagedImage1, tempRect, System.Drawing.Color.Yellow);
                    }

                    if (breadBlob1[blobArrayIndex].IsTop())
                    {
                        AForge.Imaging.Drawing.Rectangle(unmanagedImage1, rect, System.Drawing.Color.Green);
                    }
                    else
                    {
                        AForge.Imaging.Drawing.Rectangle(unmanagedImage1, rect, System.Drawing.Color.Red);
                    }

                    RectangleF rectf = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);

                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawString(Convert.ToString(blobArrayIndex), new Font("Arial", 5), System.Drawing.Brushes.White, rectf);

                    lblBlobHeight.Content = rect.Height;
                    lblBlobWidth.Content = rect.Width;

                    blobArrayIndex++;
                }

                BitmapImage indexMap_temp = ToBitmapImage(indexMap);
                g.Flush();
                // conver to managed image if it is required to display it at some point of time
                Bitmap managedImage = unmanagedImage1.ToManagedImage();

                // create filter
                Add filter = new Add(indexMap);
                // apply the filter
                Bitmap resultImage = filter.Apply(managedImage);
                BitmapImage GrayImage_temp = ToBitmapImage(resultImage);

                imgGray.Source = GrayImage_temp;

                stopwatch.Stop();
                lblTime.Content = stopwatch.ElapsedMilliseconds;



                lbl9var_1.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S1);
                lbl9var_2.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S2);
                lbl9var_3.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S3);
                lbl9var_4.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S4);
                lbl9var_5.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S5);
                lbl9var_6.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S6);
                lbl9var_7.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S7);
                lbl9var_8.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S8);
                lbl9var_9.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S9);
                lbl9var_avg.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Savg);

                lblLib.Content = "AForge";
                lblVariance.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.All);
                lblX.Content = breadBlob1[blobPt].X;
                lblY.Content = breadBlob1[blobPt].Y;
                lblQ1Variance.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Q1);
                lblQ2Variance.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Q2);
                lblQ3Variance.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Q3);
                lblQ4Variance.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Q4);
                lblQAverage.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.QAverage);
                lblAllMuffinStat.Content = MuffinStatistics.StandardDeviation;


                // System.IO.File.WriteAllLines(@"C:\Users\Public\TestFolder\Histogram.txt", GrayImage1Histogram_str);
                // E:\Brian\Project 3 - English Muffin Onsite Data Gathering\Data Analysis
                //System.IO.File.WriteAllLines(@"E:\Brian\Project 3 - English Muffin Onsite Data Gathering\Data Analysis\Histogram.txt", GrayImage1Histogram_str);
                bool fileExist = File.Exists("C:\\Users\\kai23\\Projects\\ABI\\EnglishMuffinVision_AForge\\Data Analysis\\Data.csv");
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\kai23\Projects\ABI\EnglishMuffinVision_AForge\Data AnalysisData.csv", true))
                {
                    if (!fileExist)
                    {
                        file.WriteLine("File Info" +
                            "Variance All," +
                            "Vari Q1:," +
                            "Vari Q2:," +
                            "Vari Q3:," +
                            "Vari Q4:," +
                            "Variance Average:");
                    }

                    file.WriteLine(Convert.ToString(lblFolder.Content) + "," +
                        Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.All)) + "," +
                        Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Q1)) + "," +
                        Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Q2)) + "," +
                        Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Q3)) + "," +
                        Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Q4)) + "," +
                        Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.QAverage)));

                }
            }
        }

        

        private void BtnObjectTest_Click(object sender, RoutedEventArgs e)
        {
            Board board1 = new Board();
            Bread bread1 = new Bread();
            Cut cut1 = new Cut();
            BreadBlob breadBlob1 = new BreadBlob();
            StaticsCalculator statCalc = new StaticsCalculator();

            byte[,] blobFound = new byte[50, 50];

            breadBlob1.PixelArray = blobFound;
            lblCount.Content = breadBlob1.Variance.All;

            //board1.Size.Height;
            board1.breadList.Add(bread1);
            bread1.cutList.Add(cut1);

            //lblCount.Content = board1.breadList.Count;

            board1.breadList[0].cutList[0].Angle_Plow = 1;
            //board1.Location.X = 5;

            statCalc.Add(9);
            statCalc.Add(2);
            statCalc.Add(5);
            statCalc.Add(4);
            statCalc.Add(12);
            statCalc.Add(7);
            statCalc.Add(8);
            statCalc.Add(11);
            statCalc.Add(9);
            statCalc.Add(3);
            statCalc.Add(7);
            statCalc.Add(4);
            statCalc.Add(12);
            statCalc.Add(5);
            statCalc.Add(4);
            statCalc.Add(10);
            statCalc.Add(9);
            statCalc.Add(6);
            statCalc.Add(9);
            statCalc.Add(4);
            List<byte> tempList;
            tempList = statCalc.NumberList;
            //lblAllMuffinStat.Content = statCalc.Variance;
            lblAllMuffinStat.Content = tempList.Count;
            lblAccordStdDev.Content = statCalc.CalculateWhat();


        }

        private void imgGray_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void imgGray_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void imgGray_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void BtnCalcAccord_Click(object sender, RoutedEventArgs e)
        {
            
            if (imageLoaded)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                Accord.Imaging.UnmanagedImage unmanagedImage1 = Accord.Imaging.UnmanagedImage.FromManagedImage(GrayScaleImage);
                Accord.Imaging.BlobCounter bc = new Accord.Imaging.BlobCounter
                {
                    BackgroundThreshold = Color.Black,
                    CoupledSizeFiltering = true,
                    FilterBlobs = true,
                    MinHeight = 30,
                    MinWidth = 30,
                    MaxHeight = 100,
                    MaxWidth = 100
                };
                

                bc.ProcessImage(GrayScaleImage);

                
                Bitmap indexMap = AForge.Imaging.Image.Clone(GrayScaleImage);
                
                for (int x = 0; x < indexMap.Width; x++)
                {
                    for (int y = 0; y < indexMap.Height; y++)
                    {
                        
                        indexMap.SetPixel(x, y, System.Drawing.Color.Black);
                        
                    }
                    
                }
                
                
                System.Drawing.Rectangle[] rects = bc.GetObjectsRectangles();
                // process blobs
                BreadBlob[] breadBlob1 = new BreadBlob[bc.ObjectsCount];
                int blobArrayIndex = 0;
                int blobPt = Convert.ToInt16(txbBlobNum.Text);
                int blobThreshold = Convert.ToInt16(txbBlobThreshold.Text);
                if (blobPt > bc.ObjectsCount)
                {
                    blobPt = bc.ObjectsCount - 1;
                }
                StaticsCalculator MuffinStatistics = new StaticsCalculator();

                Graphics g = Graphics.FromImage(indexMap);

                List<Accord.Imaging.Blob> blobList = new List<Accord.Imaging.Blob>();
                
                
                foreach (Accord.Imaging.Blob blob in bc.GetObjects(GrayScaleImage,false))
                {
                    blobList.Add(blob);

                    breadBlob1[blobArrayIndex] = new BreadBlob();
                    breadBlob1[blobArrayIndex].TopDownThreshold = blobThreshold;
                    byte[,] blobArray = new byte[blob.Rectangle.Width, blob.Rectangle.Height];

                    for (int x = blob.Rectangle.Left; x < blob.Rectangle.Right; x++)
                    {
                        for (int y = blob.Rectangle.Top; y < blob.Rectangle.Bottom; y++)
                        {
                            System.Drawing.Color tempPixelColor = GrayScaleImage.GetPixel(x, y);
                            blobArray[x - blob.Rectangle.Left, y - blob.Rectangle.Top] = tempPixelColor.G;
                        }
                    }
                    
                    breadBlob1[blobArrayIndex].PixelArray = blobArray;
                    
                    breadBlob1[blobArrayIndex].X = blob.Rectangle.X;
                    breadBlob1[blobArrayIndex].Y = blob.Rectangle.Y;

                    if (blobArrayIndex == blobPt)
                    {
                        System.Drawing.Rectangle tempRect = blob.Rectangle;
                        tempRect.X -= 1;
                        tempRect.Y -= 1;
                        tempRect.Width += 2;
                        tempRect.Height += 2;

                        Accord.Imaging.Drawing.Rectangle(unmanagedImage1, tempRect, System.Drawing.Color.Yellow);
                    }

                    if (breadBlob1[blobArrayIndex].IsTop())
                    {
                        Accord.Imaging.Drawing.Rectangle(unmanagedImage1, blob.Rectangle, System.Drawing.Color.Green);
                    }
                    else
                    {
                        Accord.Imaging.Drawing.Rectangle(unmanagedImage1, blob.Rectangle, System.Drawing.Color.Red);
                    }

                    RectangleF rectf = new RectangleF(blob.Rectangle.X, blob.Rectangle.Y, blob.Rectangle.Width, blob.Rectangle.Height);

                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawString(Convert.ToString(blob.ID-1), new Font("Arial", 5), System.Drawing.Brushes.White, rectf);

                    lblBlobHeight.Content = blob.Rectangle.Height;
                    lblBlobWidth.Content = blob.Rectangle.Width;

                    blobArrayIndex++;
                    
                }
                
                lblAccordStdDev.Content = blobList[blobPt].ColorStdDev.B ;
                BitmapImage indexMap_temp = ToBitmapImage(indexMap);
                g.Flush();
                // conver to managed image if it is required to display it at some point of time
                Bitmap managedImage = unmanagedImage1.ToManagedImage();
                
                // create filter
                Add filter = new Add(indexMap);
                // apply the filter
                Bitmap resultImage = filter.Apply(managedImage);
                BitmapImage GrayImage_temp = ToBitmapImage(resultImage);

                imgGray.Source = GrayImage_temp;

                stopwatch.Stop();
                lblTime.Content = stopwatch.ElapsedMilliseconds;

                lblBlobCount.Content = bc.ObjectsCount;
                

                lblLib.Content = "Accord";
                lblVariance.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.All);
                lblX.Content = breadBlob1[blobPt].X;
                lblY.Content = breadBlob1[blobPt].Y;
                lblQ1Variance.Content = "";
                lblQ2Variance.Content = "";
                lblQ3Variance.Content = "";
                lblQ4Variance.Content = "";
                lblQAverage.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.QAverage);
                //lblAllMuffinStat.Content = MuffinStatistics.StandardDeviation;

                
                
            }
        }

        private void btnMassAnalysis_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for(int index = 0; index < 6; index++)
            {
                string folderSettingText = "";

                switch (index)
                {
                    case 0:
                        folderSettingText = "Batch 1\\All Top";
                        break;
                    case 1:
                        folderSettingText = "Batch 2\\All Top";
                        break;
                    case 2:
                        folderSettingText = "Batch 3\\All Top";
                        break;
                    case 3:
                        folderSettingText = "Batch 1\\All Bottom";
                        break;
                    case 4:
                        folderSettingText = "Batch 2\\All Bottom";
                        break;
                    case 5:
                        folderSettingText = "Batch 3\\All Bottom";
                        break;
                    default:
                        folderSettingText = "";
                        break;
                }
                string[] filename = { "" };
                //string path = @"E:\Brian\Project 3 - English Muffin Onsite Data Gathering\SK Foods On-Site Scan\English Muffin\Batch 3\All Bottom";
                

                string path = "C:\\Users\\kai23\\Projects\\ABI\\EnglishMuffinVision_AForge\\Images\\English Muffin\\" + folderSettingText;
                string searchPattern = "æKatanaScoring_CameraImageGray1*";
                try
                {
                    filename = Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
                }
                catch (UnauthorizedAccessException)
                {
                
                }

                foreach (string f in filename)
                {
                    if(f != null)
                    {
                        GrayScaleImage = AForge.Imaging.Image.FromFile(f);

                        int startPos = f.LastIndexOf("SK Foods On-Site Scan") + "SK Foods On-Site Scan".Length + 1;
                        int length = f.IndexOf("æ") - startPos - 1;
                        string sub = f.Substring(startPos, length);

                        lblFolder.Content = sub;
                        //AForge.Imaging.UnmanagedImage unmanagedImage1 = AForge.Imaging.UnmanagedImage.FromManagedImage(GrayScaleImage);
                        //Bitmap managedImage = unmanagedImage1.ToManagedImage();
                        //BitmapImage GrayImage_temp = ToBitmapImage(managedImage);
                        //imgGray.Source = GrayImage_temp;

                        //Stopwatch stopwatch = new Stopwatch();
                        //stopwatch.Start();

                        AForge.Imaging.UnmanagedImage unmanagedImage1 = AForge.Imaging.UnmanagedImage.FromManagedImage(GrayScaleImage);
                        AForge.Imaging.BlobCounter bc = new AForge.Imaging.BlobCounter
                        {
                            CoupledSizeFiltering = true,
                            FilterBlobs = true,
                            MinHeight = 30,
                            MinWidth = 30,
                            MaxHeight = 100,
                            MaxWidth = 100
                        };

                        bc.ProcessImage(GrayScaleImage);

                        lblBlobCount.Content = bc.ObjectsCount;

                        Bitmap indexMap = AForge.Imaging.Image.Clone(GrayScaleImage);

                        for (int x = 0; x < indexMap.Width; x++)
                        {
                            for (int y = 0; y < indexMap.Height; y++)
                            {
                                indexMap.SetPixel(x, y, System.Drawing.Color.Black);
                            }
                        }

                        System.Drawing.Rectangle[] rects = bc.GetObjectsRectangles();
                        // process blobs
                        BreadBlob[] breadBlob1 = new BreadBlob[bc.ObjectsCount];
                        int blobArrayIndex = 0;
                        int blobPt = Convert.ToInt16(txbBlobNum.Text);
                        int blobThreshold = Convert.ToInt16(txbBlobThreshold.Text);
                        if (blobPt >= bc.ObjectsCount)
                        {
                            blobPt = bc.ObjectsCount - 1;
                        }
                        StaticsCalculator MuffinStatistics = new StaticsCalculator();

                        Graphics g = Graphics.FromImage(indexMap);
                        foreach (System.Drawing.Rectangle rect in rects)
                        {
                            //initialize Object
                            breadBlob1[blobArrayIndex] = new BreadBlob();
                            breadBlob1[blobArrayIndex].TopDownThreshold = blobThreshold;
                            byte[,] blobArray = new byte[rect.Width, rect.Height];

                            for (int x = rect.Left; x < rect.Right; x++)
                            {
                                for (int y = rect.Top; y < rect.Bottom; y++)
                                {
                                    System.Drawing.Color tempPixelColor = GrayScaleImage.GetPixel(x, y);
                                    blobArray[x - rect.Left, y - rect.Top] = tempPixelColor.G;
                                }
                            }

                            breadBlob1[blobArrayIndex].PixelArray = blobArray;
                            breadBlob1[blobArrayIndex].X = rect.X;
                            breadBlob1[blobArrayIndex].Y = rect.Y;
                            MuffinStatistics.Add(breadBlob1[blobArrayIndex].Variance.QAverage);

                            if (blobArrayIndex == blobPt)
                            {
                                System.Drawing.Rectangle tempRect = rect;
                                tempRect.X -= 1;
                                tempRect.Y -= 1;
                                tempRect.Width += 2;
                                tempRect.Height += 2;

                                AForge.Imaging.Drawing.Rectangle(unmanagedImage1, tempRect, System.Drawing.Color.Yellow);
                            }

                            if (breadBlob1[blobArrayIndex].IsTop())
                            {
                                AForge.Imaging.Drawing.Rectangle(unmanagedImage1, rect, System.Drawing.Color.Green);
                            }
                            else
                            {
                                AForge.Imaging.Drawing.Rectangle(unmanagedImage1, rect, System.Drawing.Color.Red);
                            }

                            RectangleF rectf = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);

                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            g.DrawString(Convert.ToString(blobArrayIndex), new Font("Arial", 5), System.Drawing.Brushes.White, rectf);

                            lblBlobHeight.Content = rect.Height;
                            lblBlobWidth.Content = rect.Width;

                            blobArrayIndex++;
                        }

                        BitmapImage indexMap_temp = ToBitmapImage(indexMap);
                        g.Flush();
                        // conver to managed image if it is required to display it at some point of time
                        Bitmap managedImage = unmanagedImage1.ToManagedImage();

                        // create filter
                        Add filter = new Add(indexMap);
                        // apply the filter
                        Bitmap resultImage = filter.Apply(managedImage);
                        BitmapImage GrayImage_temp = ToBitmapImage(resultImage);

                        imgGray.Source = GrayImage_temp;

                        

                    

                        lblLib.Content = "AForge";
                        lblVariance.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.All);
                        lblX.Content = breadBlob1[blobPt].X;
                        lblY.Content = breadBlob1[blobPt].Y;
                        lblQ1Variance.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Q1);
                        lblQ2Variance.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Q2);
                        lblQ3Variance.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Q3);
                        lblQ4Variance.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Q4);
                        lblQAverage.Content = breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.QAverage);
                        lblAllMuffinStat.Content = MuffinStatistics.StandardDeviation;


                        // System.IO.File.WriteAllLines(@"C:\Users\Public\TestFolder\Histogram.txt", GrayImage1Histogram_str);
                        // E:\Brian\Project 3 - English Muffin Onsite Data Gathering\Data Analysis
                        //System.IO.File.WriteAllLines(@"E:\Brian\Project 3 - English Muffin Onsite Data Gathering\Data Analysis\Histogram.txt", GrayImage1Histogram_str);

                        bool fileExist = File.Exists("C:\\Users\\kai23\\Projects\\ABI\\EnglishMuffinVision_AForge\\Data Analysis\\Data.csv");

                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\kai23\Projects\ABI\EnglishMuffinVision_AForge\Data Analysis\Data.csv", true))
                        {
                            if (!fileExist)
                            {
                                file.WriteLine("File Info," +
                                    "Variance All," +
                                    "Vari Q1:," +
                                    "Vari Q2:," +
                                    "Vari Q3:," +
                                    "Vari Q4:," +
                                    "Variance Average:," +
                                    "S1," +
                                    "S2," +
                                    "S3," +
                                    "S4," +
                                    "S5," +
                                    "S6," +
                                    "S7," +
                                    "S8," +
                                    "S9," +
                                    "Savg," +
                                    "L1," +
                                    "L2," +
                                    "L3," +
                                    "L4," +
                                    "L5," +
                                    "L6," +
                                    "L7," +
                                    "L8," +
                                    "L9," +
                                    "L10," +
                                    "L11," +
                                    "L12," +
                                    "L13," +
                                    "L14," +
                                    "L15," +
                                    "L16," +
                                    "Lavg");
                            }

                            file.WriteLine(Convert.ToString(lblFolder.Content) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.All)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Q1)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Q2)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Q3)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Q4)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.QAverage)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S1)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S2)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S3)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S4)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S5)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S6)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S7)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S8)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.S9)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Savg)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.L1)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.L2)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.L3)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.L4)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.L5)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.L6)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.L7)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.L8)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.L9)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.L10)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.L11)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.L12)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.L13)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.L14)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.L15)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.L16)) + "," +
                                Convert.ToString(breadBlob1[blobPt].GetVariance(BreadBlob.VarianceType.Lavg)));

                        }

                        //lblFolder.Content = "";
                    }
                }


            }
            stopwatch.Stop();
            lblTime.Content = stopwatch.ElapsedMilliseconds;

        }

        public double MathNetTest()
        {
            List<double> myListDouble = new List<double>();
            myListDouble.Add(1);
            myListDouble.Add(234);
            myListDouble.Add(12);
            myListDouble.Add(4444);
            myListDouble.Add(43234);
            myListDouble.Add(432);
            double vari = MathNet.Numerics.Statistics.Statistics.Variance(myListDouble);
            btnMathTest.Content = vari;

            Matrix<double> A = DenseMatrix.OfArray(new double[,] {
                                                        {1,1,1,1},
                                                        {1,2,3,4},
                                                        {4,3,2,1}});
            return 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MathNetTest();
        }
    }

}
