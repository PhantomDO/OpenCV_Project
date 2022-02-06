using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Aruco;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Ocl;
using Emgu.CV.Util;
using UnityEngine;
using UnityEngine.UI;

namespace TD3
{
    public class FaceDetection : MonoBehaviour
    {
        [SerializeField] private bool _useVideo;
        [SerializeField] private int _videoIndex = 1;
        [SerializeField] private UnityEngine.UI.RawImage _rawImage;

        private VideoCapture _webcam;

        private Image<Bgr, byte> _currentFrameBgr;
        private bool _hasGrabImage; 
        
        private int _currentVideoIndex;
        private string _videoPath;

        private int GetVideoIndex(int newer)
        {
            var devices = WebCamTexture.devices;
            for (int i = 0; i < devices.Length; i++)
            {
                Debug.Log($"Devices[{i}]: {devices[i].name}");
            }

            return devices.Length < newer ? devices.Length - 1 : newer;
        }

        private void DestroyVideoCapture(ref VideoCapture currentCapture)
        {
            if (currentCapture != null)
            {
                currentCapture.ImageGrabbed -= HandleWebcamQueryFrame;
                currentCapture.Stop();
                currentCapture.Dispose();
                currentCapture = null;
            }
        }

        private void GetNewVideoCapture(ref VideoCapture currentCapture, string videoPath)
        {
            if (currentCapture != null) DestroyVideoCapture(ref currentCapture);

            currentCapture = new VideoCapture(videoPath, VideoCapture.API.DShow);
            Debug.Log(videoPath);

            if (currentCapture != null)
            {
                currentCapture.ImageGrabbed += HandleWebcamQueryFrame;
                currentCapture.Start();
            }
        }

        private void GetNewVideoCapture(ref VideoCapture currentCapture, int videoIndex = 0)
        {
            if (currentCapture != null) DestroyVideoCapture(ref currentCapture);

            _currentVideoIndex = videoIndex;

            int framerate = -1;
            Vector2Int resolution = Vector2Int.zero;
            if (WebcamManager.Instance)
            {
                var caps = WebcamManager.Instance.cameras[_currentVideoIndex].caps;
                foreach (var info in caps)
                {
                    if (resolution.magnitude < info.Size.magnitude)
                    {
                        framerate = (int) info.Fps;
                        resolution = info.Size;
                    }
                }

                Debug.Log($"Framerate: {framerate}, Resolution: {resolution}");
                currentCapture = new VideoCapture(_currentVideoIndex, VideoCapture.API.DShow, new Tuple<CapProp, int>[]
                {
                    new Tuple<CapProp, int>(CapProp.Fps, framerate),
                    new Tuple<CapProp, int>(CapProp.FrameWidth, resolution.x),
                    new Tuple<CapProp, int>(CapProp.FrameWidth, resolution.y)
                });

                if (currentCapture != null)
                {
                    currentCapture.ImageGrabbed += HandleWebcamQueryFrame;
                    currentCapture.Start();
                }
            }
        }
        
        // Start is called before the first frame update
        void Start()
        {
            if (_useVideo) GetNewVideoCapture(ref _webcam, $"{Application.dataPath}{@"/Videos/demo.mp4"}");
            else GetNewVideoCapture(ref _webcam, GetVideoIndex(_videoIndex));
        }

        void OnValidate()
        {
            if (_videoIndex != _currentVideoIndex)
            {
                GetNewVideoCapture(ref _webcam, GetVideoIndex(_videoIndex));
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_webcam == null || !_webcam.IsOpened)
            {
                Debug.LogWarning($"{_webcam} is not open.");
            }
        }

        // OnDestroy is called at the end of the object (when Destroy(gameObject) is called)
        void OnDestroy()
        {
            DestroyVideoCapture(ref _webcam);
        }
        
        // HandleWebcamQueryFrame is called when acquisition event occurs.
        // Get the current frame and face detection
        void HandleWebcamQueryFrame(object sender, System.EventArgs eventArgs)
        {
            if (_currentFrameBgr == null)
            {
                _currentFrameBgr = new Image<Bgr, byte>(_webcam.Width, _webcam.Height);
            }

            //Debug.Log($"Size of image from the main thread: {_currentFrameBgr.Size}");
            _hasGrabImage = _webcam.Retrieve(_currentFrameBgr);
            GetMarkers(_currentFrameBgr.Mat);

            if (UnityMainThreadDispatcher.Exists())
            {
                UnityMainThreadDispatcher.Instance().Enqueue(DisplayFrameOnPlane());
            }
        }
        
        // DisplayFrameOnPlane
        // Load the current frame in a texture draw on plane
        IEnumerator DisplayFrameOnPlane()
        {
            if (_hasGrabImage)
            {
                // LINK : https://docs.opencv.org/3.4/df/d0d/tutorial_find_contours.html
                //var currentFrameGray = new Mat(_currentFrameBgr.Size, DepthType.Cv8U, 1);
                //CvInvoke.CvtColor(_currentFrameBgr, currentFrameGray, ColorConversion.Bgr2Gray);

                //Mat cannyOutput = new Mat(_currentFrameBgr.Size, DepthType.Cv8U, 3);
                //CvInvoke.Canny(currentFrameGray, cannyOutput, 125, 250);
                

                //var contours = new VectorOfVectorOfPoint();
                //var hierarchy = new Mat();
                //CvInvoke.FindContours(cannyOutput, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

                //var display = Mat.Zeros(cannyOutput.Width, cannyOutput.Height, DepthType.Cv8U, 3);
                
                //for (int i = 0; i < contours.Size; i++)
                //{
                //    MCvScalar color = new MCvScalar(
                //        new RNG().Uniform(0, 256), 
                //        new RNG().Uniform(0, 256),
                //        new RNG().Uniform(0, 256));
                //    CvInvoke.DrawContours(display, contours, i, color, 2, LineType.EightConnected, hierarchy, 0);
                //}
                
                _rawImage.rectTransform.sizeDelta = new Vector2(_currentFrameBgr.Width, _currentFrameBgr.Height);
                _rawImage.texture = _currentFrameBgr.ToTexture2D(); 
            }
            else
            {
                Debug.LogError($"Can't grab image.");
            }

            yield return null;
        }

        private void GetMarkers(Mat mat)
        {
            VectorOfInt ids = new VectorOfInt();
            VectorOfVectorOfPointF acceptedCorners = new VectorOfVectorOfPointF();
            VectorOfVectorOfPointF ignoreCorners = new VectorOfVectorOfPointF();
            DetectorParameters detectorParameters = new DetectorParameters();
            detectorParameters = DetectorParameters.GetDefault();

            Dictionary dictMarkers = new Dictionary(Dictionary.PredefinedDictionaryName.Dict6X6_250);
            Mat grayFrame = new Mat(mat.Width, mat.Height, DepthType.Cv8U, 1);
            CvInvoke.CvtColor(mat, grayFrame, ColorConversion.Bgr2Gray);
            ArucoInvoke.DetectMarkers(mat, dictMarkers, acceptedCorners, ids, detectorParameters, ignoreCorners);

            Mat display = new Mat(mat.Width, mat.Height, DepthType.Cv8U, 3);
            mat.CopyTo(display);

            if (ids.Size > 0)
            {
                ArucoInvoke.DrawDetectedMarkers(display, acceptedCorners, ids, new MCvScalar(0,255,0));
            }

            CvInvoke.Imshow("ArucoDisplay", display);
        }
    }
}
