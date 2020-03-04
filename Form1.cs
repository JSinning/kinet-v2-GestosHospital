using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        KinectSensor myKinect = null;
        MultiSourceFrameReader reader = null;

        VisualGestureBuilderDatabase vgbd;
        VisualGestureBuilderFrameSource vgbfs;
        VisualGestureBuilderFrameReader vgbr;

        Gesture Gesture;        //Gesture oreja;
        Gesture Gesture2;       //Gesto estomago

        BodyFrameReader bfr;



        public Form1()
        {
            myKinect = KinectSensor.GetDefault();
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           //Kinect = KinectSensor.GetDefault();
            if(myKinect != null)
            {
                myKinect.Open();
            }
            reader = myKinect.OpenMultiSourceFrameReader(FrameSourceTypes.Color);
            reader.MultiSourceFrameArrived += reader_MultiSourceFrameArrived;
        }

        private void reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if(frame != null)
                {
                    var width = frame.FrameDescription.Width;   
                    var hight = frame.FrameDescription.Height;
                    var data = new byte[width * hight * 64 /16];
                    frame.CopyConvertedFrameDataToArray(data, ColorImageFormat.Bgra);
                    var bitmap = new Bitmap(width, hight, PixelFormat.Format32bppRgb);
                    var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.WriteOnly, bitmap.PixelFormat);
                    Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);
                    bitmap.UnlockBits(bitmapData);
                    pictureBox1.Image = bitmap;
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            bfr = myKinect.BodyFrameSource.OpenReader();
            bfr.FrameArrived += bfr_FrameArrived;

            vgbd = new VisualGestureBuilderDatabase("C:/Users/INGENIERO POL/Documents/Poryectos visual/WindowsFormsApp1/WindowsFormsApp1/DB/D_cabeza.gba");
            vgbfs = new VisualGestureBuilderFrameSource(KinectSensor.GetDefault(), 0);

            foreach (Gesture g in vgbd.AvailableGestures)
            {
           
                if (g.Name.Equals("D_cabeza"))
                {
                    Gesture = g;
                    vgbfs.AddGesture(Gesture);
                }
                if (g.Name.Equals("D_garganta"))
                {
                    Gesture2 = g;
                    vgbfs.AddGesture(Gesture2);
                }

             }
            vgbr = vgbfs.OpenReader();
            vgbfs.GetIsEnabled(Gesture);
            vgbfs.GetIsEnabled(Gesture2);
            vgbr.FrameArrived += vgbr_FrameArrived;
            myKinect.Open();

        }

        private void vgbr_FrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (vgbfs.IsTrackingIdValid)
                    {
                        IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResult = frame.DiscreteGestureResults;
                        if (discreteResult != null)
                        {
                            foreach (Gesture gesture in this.vgbfs.Gestures)
                            {
                                if (gesture.Name.Equals(" D_cabeza") && gesture.GestureType == GestureType.Discrete)
                                {
                                    DiscreteGestureResult result = null;
                                    discreteResult.TryGetValue(gesture, out result);
                                    if (result != null)
                                    {
                                        if (result.Detected && result.FirstFrameDetected)
                                        {
                                            // Console.WriteLine("Cabeza");
                                            label1.Text = " D_cabeza";
                                        }
                                    }
                                }
                                else if (gesture.Name.Equals("D_garganta") && gesture.GestureType == GestureType.Discrete)
                                {
                                    DiscreteGestureResult result = null;
                                    discreteResult.TryGetValue(gesture, out result);
                                    if (result != null)
                                    {
                                        if (result.Detected && result.FirstFrameDetected)
                                        {
                                            //Console.WriteLine("Oreja");
                                            label1.Text = "D_garganta";
                                        }
                                    }
                                }


                            }
                        }
                    }
                }
            }
        }

        private void bfr_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            if (!vgbfs.IsTrackingIdValid)
            {
                using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
                {
                    if (bodyFrame != null)
                    {
                        Body[] bodies = new Body[6];
                        bodyFrame.GetAndRefreshBodyData(bodies);
                        Body bodyclose = null;

                        foreach (Body b in bodies)
                        {
                            if (b.IsTracked)
                            {
                                if (bodyclose == null)
                                {
                                    bodyclose = b;
                                }
                                else
                                {
                                    Joint newhead = b.Joints[JointType.HandRight];
                                    Joint oldHead = bodyclose.Joints[JointType.HandRight];
                                    if (newhead.TrackingState == TrackingState.Tracked && newhead.Position.Z < oldHead.Position.Z)
                                    {
                                        bodyclose = b;
                                        Console.WriteLine(bodyclose);
                                    }
                                }
                            }

                        }
                        if (bodyclose != null)
                        {
                            vgbfs.TrackingId = bodyclose.TrackingId;
                        }
                    }
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
